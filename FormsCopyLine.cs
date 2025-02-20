using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace CopyLine
{
    partial class CopyLine : Form
    {
        // タスクトレイ用
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuItemFeatureToggle; // チェック付きメニュー


        // コピー / ペースト時に使用する変数
        private bool isEnableCopyLine = true;  // CopyLine の機能のオン・オフ
        private bool onCopy = false;           // 手動でコピーした場合に true

        // クリップボードにペーストする内容を保持しておくための変数
        private string strClipboard = string.Empty;
        private string strClipboardManual = string.Empty;
        private string strClipboardBackUp = string.Empty;

        // キュー
        Queue<string> queue = new Queue<string>();

        // ユーザーホームフォルダ
        private static string homeFolder = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile
        );
        // iniファイルのパス
        private string iniFilePath = Path.Combine(homeFolder, fileNameINI);

        // GUI用
        private readonly MenuStrip ms;

        private readonly TableLayoutPanel tlp;
        private readonly Button buttonStart;
        private readonly Button buttonClear;
        private readonly CheckBox checkBoxEnabled;  // 機能オン/オフ用

        private readonly Label labelItemNumber;
        private readonly Button buttonQueueClearTop;
        private readonly Button buttonQueueClearAll;

        private readonly FormattedTextBox textBoxInput;  // 入力用
        private readonly TextBox textBoxClipboard;       // クリップボード用
        private readonly TableLayoutPanel tableQueue;    // FIFOリスト用
        private readonly Label[] queueLabel;
        private const int nQueueMax = 20;

        // デフォルト設定（iniファイルで上書きされる）
        public int WaitTimeAfterPaste = DefaultVal.WaitTimeAfterPaste;
        public string[] QueueBackColor = new string[2];
        public string ClipboardBackColorON = DefaultVal.ClipboardBackColorON;
        public string ClipboardBackColorOFF = DefaultVal.ClipboardBackColorOFF;
        public string TextFont = DefaultVal.TextFont;


        // コンストラクタ
        public CopyLine()
        {
            this.Text = $"Copy Line {VersionNumber}";
            this.ClientSize = new Size(770, 640);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            QueueBackColor[0] = DefaultVal.QueueBackColor[0];
            QueueBackColor[1] = DefaultVal.QueueBackColor[1];

            // 設定ファイルから変数読み込み
            GetValuesINI();

            ////////////////////////////////////////
            // アイコンの設定
            ////////////////////////////////////////
            // リソースからバイト配列を取得
            byte[] iconBytes = Properties.Resources.iconName;

            // MemoryStream を使って Icon に変換
            using (MemoryStream memstream = new MemoryStream(iconBytes))
            {
                this.Icon = new Icon(memstream);
            }

            ////////////////////////////////////////
            // Top-level menu bar
            ////////////////////////////////////////
            ms = new MenuStrip();

            var menuItemFile = new ToolStripMenuItem("ファイル(&F)");
            var menuItemSetting = new ToolStripMenuItem("設定(&S)");
            var menuItemHelp = new ToolStripMenuItem("ヘルプ(&H)");

            ms.Items.Add(menuItemFile);
            ms.Items.Add(menuItemSetting);
            ms.Items.Add(menuItemHelp);

            ////////////////////////////////////////
            // File
            ////////////////////////////////////////
            // Open File
            var menuItemOpenFile = new ToolStripMenuItem("ファイルを開く");
            menuItemFile.DropDownItems.Add(menuItemOpenFile);
            menuItemOpenFile.ShortcutKeys = Keys.Control | Keys.O;
            menuItemOpenFile.Click += this.HandlerOpenFile;

            // Quit
            var menuItemQuit = new ToolStripMenuItem("終了");
            menuItemFile.DropDownItems.Add(menuItemQuit);
            menuItemQuit.ShortcutKeys = Keys.Control | Keys.Q;
            menuItemQuit.Click += (s, e) => { this.Close(); };

            // Closing（アプリケーションを閉じるときのイベントを追加）
            this.Closing += new System.ComponentModel.CancelEventHandler(this.HandlerQuit);

            ////////////////////////////////////////
            // Setting
            ////////////////////////////////////////
            var menuItemChangeSetting = new ToolStripMenuItem("設定変更");
            menuItemSetting.DropDownItems.Add(menuItemChangeSetting);
            menuItemChangeSetting.Click += this.HandlerChangeSetting;

            ////////////////////////////////////////
            // Help
            ////////////////////////////////////////
            var menuItemVersionNumber =
                new ToolStripMenuItem("このプログラムについて");
            menuItemHelp.DropDownItems.Add(menuItemVersionNumber);
            menuItemVersionNumber.Click += this.HandlerDialogThisProgram;

            ////////////////////////////////////////
            // Placing
            ////////////////////////////////////////
            this.MainMenuStrip = ms;

            ////////////////////////////////////////
            // Layout
            ////////////////////////////////////////
            tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5
            };

            ////////////////////////////////////////
            // Left area
            ////////////////////////////////////////

            Panel panelItemClipboardArea = new FlowLayoutPanel
            {
                AutoSize = true
            };

            // CopyLineの機能のオン/オフ
            checkBoxEnabled = new CheckBox
            {
                Text = textEnabledCopyLine,
                Checked = true,
                AutoSize = true,
                Font = new Font(TextFont, 11, FontStyle.Regular)
            };
            checkBoxEnabled.CheckedChanged += HandlerEnabledFunction;

            // コントロールをPanelに追加
            panelItemClipboardArea.Controls.Add(checkBoxEnabled);

            // クリップボードの内容のためのテキストボックス
            textBoxClipboard = new TextBox
            {
                Text = string.Empty,
                Multiline = true,
                BackColor = ColorTranslator.FromHtml(
                    ConvertColorNameToHex(ClipboardBackColorOFF)
                ),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Size = new Size(360, 50),
                Font = new Font(TextFont, 12, FontStyle.Regular)
            };

            // キュー操作のボタン
            FlowLayoutPanel panelButtonQueueArea = new FlowLayoutPanel
            {
                AutoSize = true
            };

            labelItemNumber = new Label
            {
                Text = "要素数: 0",
                Font = new Font(TextFont, 12, FontStyle.Regular),
                Width = 150
            };

            buttonQueueClearTop = new Button
            {
                Text = "先頭を削除",
                Font = new Font(TextFont, 10, FontStyle.Regular),
                AutoSize = true
            };
            buttonQueueClearTop.Click += HandlerClearQueueTop;

            buttonQueueClearAll = new Button
            {
                Text = "すべて削除",
                Font = new Font(TextFont, 10, FontStyle.Regular),
                AutoSize = true
            };
            buttonQueueClearAll.Click += HandlerClearQueueAll;

            // コントロールをPanelに追加
            panelButtonQueueArea.Controls.Add(labelItemNumber);
            panelButtonQueueArea.Controls.Add(buttonQueueClearTop);
            panelButtonQueueArea.Controls.Add(buttonQueueClearAll);

            // キューの内容のためのテーブル
            tableQueue = new TableLayoutPanel
            {
                Size = new Size(360, 460),
                ColumnCount = 1,       // 列数
                RowCount = nQueueMax   // 行数
            };

            // 行の追加
            queueLabel = new Label[nQueueMax];
            for (int i = 0; i < nQueueMax; i++)
            {
                queueLabel[i] = new Label()
                {
                    Text = string.Empty,
                    Font = new Font(TextFont, 11, FontStyle.Regular),
                    Dock = DockStyle.Fill, 
                    AutoSize = true,
                    MaximumSize = new Size(0, 23),
                    MinimumSize = new Size(0, 23),
                };
                tableQueue.Controls.Add(queueLabel[i], 0, i); // 0列目のi行目に追加
            }
            SetQueueBackColor();  // キューの背景色を設定


            ////////////////////////////////////////
            // Right area
            ////////////////////////////////////////
            Panel panelButtonInputArea = new FlowLayoutPanel
            {
                AutoSize = true
            };

            buttonStart = new Button
            {
                Text = "キューに追加",
                Font = new Font(TextFont, 10, FontStyle.Bold),
                AutoSize = true
            };
            buttonStart.Click += HandlerSetQueue;

            buttonClear = new Button
            {
                Text = "クリア",
                Font = new Font(TextFont, 10, FontStyle.Regular),
                AutoSize = true
            };
            buttonClear.Click += HandlerClearInput;

            // コントロールをPanelに追加
            panelButtonInputArea.Controls.Add(buttonStart);
            panelButtonInputArea.Controls.Add(buttonClear);

            // 入力用テキストボックス（自動整形あり）
            textBoxInput = new FormattedTextBox
            {
                Text = string.Empty,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Size = new Size(400, 555),
                Font = new Font(TextFont, 13, FontStyle.Regular),
                AllowDrop = true
            };
            textBoxInput.DragEnter += TextBox_DragEnter;
            textBoxInput.DragDrop += TextBox_DragDrop;


            ////////////////////////////////////////
            // テーブルに配置
            ////////////////////////////////////////
            // Menubar
            tlp.Controls.Add(this.MainMenuStrip, 0, 0);
            tlp.SetColumnSpan(tlp.GetControlFromPosition(0, 0), 2);

            // Left area
            tlp.Controls.Add(panelItemClipboardArea, 0, 1);
            tlp.Controls.Add(textBoxClipboard, 0, 2);

            tlp.Controls.Add(panelButtonQueueArea, 0, 3);
            tlp.Controls.Add(tableQueue, 0, 4);

            // Right area
            tlp.Controls.Add(panelButtonInputArea, 1, 1);
            tlp.Controls.Add(textBoxInput, 1, 2);
            tlp.SetRowSpan(tlp.GetControlFromPosition(1, 2), 3);

            tlp.Parent = this;


            ////////////////////////////////////////
            // タスクトレイ
            ////////////////////////////////////////
            // コンテキストメニューの作成
            contextMenu = new ContextMenuStrip();
            menuItemFeatureToggle = new ToolStripMenuItem(
                textEnabledCopyLine, null, OnFeatureToggleClick)
            {
                Checked = isEnableCopyLine  // 初期状態を反映
            };
            contextMenu.Items.Add(menuItemFeatureToggle);
            contextMenu.Items.Add("設定変更", null, this.HandlerChangeSetting);
            contextMenu.Items.Add("このプログラムについて", null, this.HandlerDialogThisProgram);
            contextMenu.Items.Add("サイズを戻す", null, this.OnOpenClick);
            contextMenu.Items.Add("終了", null, this.OnExitClick);

            // NotifyIcon の設定
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = $"Copy Line {VersionNumber}";
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;

            // NotifyIcon のクリックイベント
            notifyIcon.MouseClick += NotifyIcon_MouseClick;

            // フォームのリサイズイベント（最小化時に隠す）
            this.Resize += Form_Resize;

            // アイコンの設定
            using (MemoryStream memstream = new MemoryStream(iconBytes))
            {
                notifyIcon.Icon = new Icon(memstream);
            }
        }
        // コンストラクタ（終了）



        ////////////////////////////////////////
        // イベントハンドラ
        ////////////////////////////////////////

        // 入力ボックスのクリア
        private void HandlerClearInput(Object sender, EventArgs e)
        {
            textBoxInput.Clear();
        }

        // チェックボックス（機能オン・オフ）
        private void HandlerEnabledFunction(Object sender, EventArgs e)
        {
            bool isChecked = ((CheckBox)sender).Checked;
            isEnableCopyLine = isChecked;

            buttonStart.Enabled = isChecked;
            buttonClear.Enabled = isChecked;
            textBoxInput.Enabled = isChecked;

            // トレイアイコンのメニューのチェックを更新
            menuItemFeatureToggle.Checked = isChecked;

            if (isChecked)
            {
                // 機能オン：退避した内容を戻す
                SynchronizationContext.Current?.Post(async _ =>
                {
                    await SetClipboard(strClipboardBackUp);
                }, null);
            }
            else
            {
                // 機能オフ：現在のクリップボードの内容を退避
                strClipboardBackUp = strClipboard;
            }
        }

        // キューの先頭要素のクリア
        private void HandlerClearQueueTop(Object sender, EventArgs e)
        {
            int N = queue.Count;
            if (N >= 2)
            {
                _ = queue.Dequeue();
                strClipboard = queue.Peek();
                SynchronizationContext.Current?.Post(async _ =>
                {
                    if (!onCopy)
                    {
                        await SetClipboard(strClipboard);
                    }
                }, null);
                UpdateQueueList();
            }
            else if (N == 1)
            {
                queue.Clear();
                strClipboard = string.Empty;
                SynchronizationContext.Current?.Post(async _ =>
                {
                    if (!onCopy)
                    {
                        await ClearClipboard();
                    }
                }, null);
                UpdateQueueList();
            }
        }

        // キューの全クリア
        private void HandlerClearQueueAll(Object sender, EventArgs e)
        {
            queue.Clear();
            UpdateQueueList();

            SynchronizationContext.Current?.Post(async _ =>
            {
                if (!onCopy)
                {
                    await ClearClipboard();
                }
            }, null);
        }

        // アプリケーション終了時の処理
        private void HandlerQuit(Object sender,
                                System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show(
                "終了しますか？",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }

            // アプリケーション終了時にタスクトレイアイコンを削除
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
        private void HandlerDialogThisProgram(Object sender, EventArgs e)
        {
            CustomDialog customDialog = new CustomDialog();
            customDialog.Show();
        }

        // 手動でクリップボードを更新したときに
        // クリップボードの内容を表示するテキストボックスの背景色を変更
        private void ToggleClipbordBoxColor()
        {
            textBoxClipboard.BackColor = onCopy
                ? ColorTranslator.FromHtml(ClipboardBackColorON)
                : ColorTranslator.FromHtml(ClipboardBackColorOFF);
        }


        ////////////////////////////////////////
        // その他の関数
        ////////////////////////////////////////

        // キューの背景色を設定
        private void SetQueueBackColor()
        {
            for (int i = 0; i < nQueueMax; i++)
            {
                queueLabel[i].BackColor =
                    ColorTranslator.FromHtml(QueueBackColor[i % 2]);
            }
        }
    }
}
