using System;
using System.Windows.Forms;


////////////////////////////////////////
// タスクトレイ用関数
////////////////////////////////////////

namespace CopyLine
{
    partial class CopyLine : Form
    {
        // 最小化時にタスクトレイに格納
        private void Form_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        // タスクトレイアイコンをクリックしたらウィンドウを表示
        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowForm();
            }
        }

        // 「開く」メニュークリック時の処理
        private void OnOpenClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        // 「終了」メニュークリック時の処理
        private void OnExitClick(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Application.Exit();
        }

        // ウィンドウを復元する処理
        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // 機能の有効/無効を切り替える
        private void OnFeatureToggleClick(object sender, EventArgs e)
        {
            // 状態を反転
            isEnableCopyLine = !isEnableCopyLine;

            // メニューのチェックを更新
            menuItemFeatureToggle.Checked = isEnableCopyLine;

            // チェックボックスを変更
            //   => 自動的に HandlerEnabledFunction 関数が呼ばれる
            checkBoxEnabled.Checked = isEnableCopyLine;
        }
    }
}
