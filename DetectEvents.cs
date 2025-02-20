// 以下のプログラムを参考にして一部変更した
// 1. コンストラクタで CopyLine クラスを格納
// 2. ペースト実行中は Ctrl-V を無視
// 
// Windowsのキーボードフックの最小サンプル
// https://qiita.com/okuhiiro/items/ab768819fe47c0ebba78

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace CopyLine
{
    class DetectEvents
    {
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);


        private readonly CopyLine _cl;
        private static IntPtr _hookID = IntPtr.Zero;
        private readonly HookProc _proc;

        // コンストラクタ
        public DetectEvents(CopyLine copyline)
        {
            _onPaste = false;
            _cl = copyline;
            // null にならないようにコンストラクタで必ず初期化
            _proc = HookCallback;
        }


        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        public void Hook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // フックを行う
                // 第1引数 フックするイベントの種類：13はキーボードフックを表す
                // 第2引数 フック時のメソッドのアドレス：フックメソッドを登録する
                // 第3引数 インスタンスハンドル：現在実行中のハンドルを渡す
                // 第4引数 スレッドID：0を指定すると、すべてのスレッドでフックされる
                _hookID = SetWindowsHookEx(
                            WH_KEYBOARD_LL,
                            _proc,
                            GetModuleHandle(curModule.ModuleName),
                            0
                        );
            }
        }


        static bool _onPaste;  // 排他的に動作させるためのフラグ

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    // Ctrl-V を検出した場合
                    if (Control.ModifierKeys == Keys.Control && vkCode == (int)Keys.V)
                    {
                        // ペースト実行中の場合にはフックしたキーを捨てる（Ctrl-Vを無視）
                        if (_onPaste) {
                            return (IntPtr)1;
                        }

                        SynchronizationContext.Current?.Post(async _ =>
                        {
                            _onPaste = true;
                            await _cl.DetectPaste();
                            _onPaste = false;
                        }, null);
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        public void HookEnd()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
    }
}
