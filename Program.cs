using System;
using System.Threading;
using System.Windows.Forms;


namespace CopyLine
{
    class MainClass
    {

        // 多重起動を禁止するためのタグ
        static Mutex mutex = new Mutex(true, "toshiara_CopyLine");


        [STAThread]
        private static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                // 多重起動を禁止
                MessageBox.Show(
                    "アプリケーションはすでに実行されています。",
                    "警告",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            CopyLine cl = new CopyLine();

            // キーボードフックを設定
            DetectEvents myHook = new DetectEvents(cl);
            myHook.Hook();

            // アプリケーション開始
            Application.EnableVisualStyles();
            Application.Run(cl);

            // アプリ終了時にフックを解除
            myHook.HookEnd();
        }
    }
}
