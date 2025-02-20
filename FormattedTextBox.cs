using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace CopyLine
{
    public class FormattedTextBox : TextBox
    {
        private const int WM_PASTE = 0x0302;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PASTE)
            {
                base.WndProc(ref m); // 通常の貼り付け処理
                ConvertNewLines();   // 貼り付け後の改行変換
                return;
            }
            base.WndProc(ref m);
        }

        private void ConvertNewLines()
        {
            string newText = this.Text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "\r\n");

            if (this.Text != newText)
            {
                int cursorPosition = this.SelectionStart;
                this.Text = newText;
                this.SelectionStart = cursorPosition;
            }
        }
    }
}
