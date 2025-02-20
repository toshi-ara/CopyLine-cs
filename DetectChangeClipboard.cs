using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CopyLine
{
// クリップボードの変化を検出
    partial class CopyLine : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private extern static void AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private extern static void RemoveClipboardFormatListener(IntPtr hwnd);

        private const int WM_CLIPBOARDUPDATE = 0x031D;

        protected override void OnLoad(EventArgs e)
        {
            AddClipboardFormatListener(Handle);
            base.OnLoad(e);
        }

        protected override void Dispose(bool disposing)
        {
            RemoveClipboardFormatListener(Handle);
            base.Dispose(disposing);
        }

        // function called in the detection of clipboard change
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                SynchronizationContext.Current?.Post(async _ =>
                {
                    await OnClipboardUpdate();
                }, null);
                m.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
