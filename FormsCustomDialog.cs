using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace CopyLine
{
    class CustomDialog : Form
    {
        string Context =
            "試験問題入力システムへのコピー・ペーストを" +
            "省力化するプログラムです\n" +
            "先入れ先出し (First In First Out, FIFO) " +
            "機能を実装しています\n\n" +
            "キューに入った要素を1つずつペーストすることができます";
        string URL = "https://github.com/toshi-ara/CopyLine-cs";
        string TextFont = "MSゴシック";

        public CustomDialog()
        {
            this.Text = "このプログラムについて";
            this.Width = 520;
            this.Height = 280;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label versionLabel = new Label()
            {
                Text = $"CopyLine version {CopyLine.VersionNumber}",
                Font = new Font(TextFont, 18, FontStyle.Bold),
                ForeColor = Color.Blue,
                AutoSize = true,
                Top = 10,
                Left = 20
            };

            Label labelContext = new Label()
            {
                Text = Context,
                Font = new Font(TextFont, 11, FontStyle.Regular),
                AutoSize = true,
                Top = 60,
                Left = 20
            };

            LinkLabel linkLabel = new LinkLabel()
            {
                Text = "ウェブサイト",
                Font = new Font(TextFont, 11, FontStyle.Regular),
                AutoSize = true,
                Top = 160,
                Left = 20
            };

            // リンクをクリックしたときにブラウザで開く
            linkLabel.LinkClicked += (sender, e) => OpenUrl(URL);

            Button closeButton = new Button()
            {
                Text = "閉じる",
                Font = new Font(TextFont, 11, FontStyle.Regular),
                AutoSize = true,
                Top = 200,
            };
            closeButton.Click += (sender, e) => this.Close();

            Size buttonSize = closeButton.Size;  // ボタンのサイズを取得
            Size formSize = this.ClientSize;     // フォームのサイズを取得
            int x = (formSize.Width - buttonSize.Width) / 2;  // ボタンの位置を計算
            closeButton.Location = new Point(x, 200);         // ボタンの位置を設定

            this.Controls.Add(versionLabel);
            this.Controls.Add(labelContext);
            this.Controls.Add(linkLabel);
            this.Controls.Add(closeButton);
        }

        // OS に応じて適切な方法でURLを開く
        public static void OpenUrl(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url); // Linux 用
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url); // macOS 用
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("リンクを開けませんでした: " + ex.Message);
            }
        }
    }
}
/* class Program */
/* { */
/*     static void Main() */
/*     { */
/*         Application.EnableVisualStyles(); */
/*         Application.Run(new CustomDialog()); */
/*     } */
/* } */

