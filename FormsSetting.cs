using System;
using System.Drawing;
using System.Windows.Forms;


namespace CopyLine
{
    partial class CopyLine : Form
    {
        // 設定変更（仮）
        private void HandlerChangeSetting(object sender, EventArgs e)
        {
            SettingForm sf = new SettingForm(this);
            if (sf.ShowDialog() == DialogResult.OK)
            {
                int waitTime = sf.WaitTimeAfterPaste;
                WaitTimeAfterPaste = waitTime;

                for (int i = 0; i < 2; i++)
                {
                    QueueBackColor[i] = ColorTranslator.ToHtml(sf.QueueBackColor[i]);
                }

                SaveValuesINI();      // INIファイルに書き込み
                SetQueueBackColor();  // キューの背景色を変更
            }
        }
    }


    // 設定画面用のクラス
    class SettingForm : Form
    {
        private const int N = 2;
        private CopyLine _cl;
        public int WaitTimeAfterPaste { get; private set; }
        public Color[] QueueBackColor { get; private set; } = new Color[N];
        private TextBox textBoxWaitTime;
        private Label[] labelBackColor = new Label[N];
        private Label[] labelColor = new Label[N];
        private Button[] buttonChangeColor = new Button[N];
        private Button[] buttonResetColor = new Button[N];

        public SettingForm(CopyLine copyline)
        {
            _cl = copyline;
            this.Text = "設定画面";
            this.Width = 380;
            this.Height = 200;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int _waitTime = _cl.WaitTimeAfterPaste;
            WaitTimeAfterPaste = _waitTime;

            for (int i = 0; i < N; i++)
            {
                string tmp = _cl.QueueBackColor[i] ?? "White";
                QueueBackColor[i] = ColorTranslator.FromHtml(
                    _cl.ConvertColorNameToHex(tmp));
            }

            ////////////////////////////////////////
            // 待ち時間の設定
            ////////////////////////////////////////
            Label labelWaitTime = new Label
            {
                Text = "ペースト操作後の待ち時間 (msec)",
                Font = new Font(_cl.TextFont, 11, FontStyle.Regular),
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            textBoxWaitTime = new TextBox
            {
                Text = WaitTimeAfterPaste.ToString(),
                Font = new Font(_cl.TextFont, 11, FontStyle.Regular),
                Width = 50,
                Height = 16,
                Anchor = AnchorStyles.None
            };

            Panel panelWaitTime = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Dock = DockStyle.Top
            };

            panelWaitTime.Controls.Add(labelWaitTime);
            panelWaitTime.Controls.Add(textBoxWaitTime);
            panelWaitTime.Padding = new Padding(0, 10, 0, 10);


            ////////////////////////////////////////
            // キュー色の設定
            ////////////////////////////////////////
            for (int i = 0; i < 2; i++)
            {
                labelColor[i] = new Label
                {
                    Text = $"キュー色{i + 1}",
                    Font = new Font(_cl.TextFont, 10, FontStyle.Regular),
                    Width = 70,
                    Height = 16,
                    Anchor = AnchorStyles.None
                };

                labelBackColor[i] = new Label
                {
                    Text = "",
                    Width = 40,
                    Height = 20,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = QueueBackColor[i],
                    Anchor = AnchorStyles.None
                };

                buttonChangeColor[i] = new Button()
                {
                    Text = "色を変更する",
                    Font = new Font(_cl.TextFont, 10, FontStyle.Regular),
                    AutoSize = true,
                    Anchor = AnchorStyles.None
                };

                buttonResetColor[i] = new Button()
                {
                    Text = "初期設定に戻す",
                    Font = new Font(_cl.TextFont, 10, FontStyle.Regular),
                    AutoSize = true,
                    Anchor = AnchorStyles.None
                };
            }

            buttonChangeColor[0].Click += btnPickColor1_Click;
            buttonChangeColor[1].Click += btnPickColor2_Click;
            buttonResetColor[0].Click += btnResetColor1_Click;
            buttonResetColor[1].Click += btnResetColor2_Click;

            TableLayoutPanel tlpChangeColor = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            for (int i = 0; i < N; i++)
            {
                tlpChangeColor.Controls.Add(labelColor[i], 0, i);
                tlpChangeColor.Controls.Add(labelBackColor[i], 1, i);
                tlpChangeColor.Controls.Add(buttonChangeColor[i], 2, i);
                tlpChangeColor.Controls.Add(buttonResetColor[i], 3, i);
            }


            ////////////////////////////////////////
            // 設定の完了・キャンセル
            ////////////////////////////////////////
            Button buttonOK = new Button()
            {
                Text = "設定する",
                Font = new Font(_cl.TextFont, 10, FontStyle.Regular),
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };
            Button buttonCancel = new Button()
            {
                Text = "キャンセル",
                Font = new Font(_cl.TextFont, 10, FontStyle.Regular),
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };

            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Click += btnOK_Click;

            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Click += btnCancel_Click;


            TableLayoutPanel tlpButtonOKCancel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                Dock = DockStyle.Bottom
            };

            // 幅を均等にする
            tlpButtonOKCancel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlpButtonOKCancel.Controls.Add(buttonOK, 0, 0);
            tlpButtonOKCancel.Controls.Add(buttonCancel, 1, 0);


            ////////////////////////////////////////
            // 配置
            ////////////////////////////////////////
            tlpChangeColor.Parent = this;
            panelWaitTime.Parent = this;
            tlpButtonOKCancel.Parent = this;
        }


        ////////////////////////////////////////
        // キューの背景色の変更ボタン
        ////////////////////////////////////////

        // キュー背景色1の変更ボタン
        private void btnPickColor1_Click(object sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog
            {
                AllowFullOpen = true,
                Color = QueueBackColor[0]
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                QueueBackColor[0] = colorDialog.Color;
                labelBackColor[0].BackColor = QueueBackColor[0];
            }
        }

        // キュー背景色2の変更ボタン
        private void btnPickColor2_Click(object sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog
            {
                AllowFullOpen = true,
                Color = QueueBackColor[1]
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                QueueBackColor[1] = colorDialog.Color;
                labelBackColor[1].BackColor = QueueBackColor[1];
            }
        }

        // キュー背景色1のリセットボタン
        private void btnResetColor1_Click(object sender, EventArgs e)
        {
            QueueBackColor[0] = ColorTranslator.FromHtml(DefaultVal.QueueBackColor[0]);
            labelBackColor[0].BackColor = QueueBackColor[0];
        }

        // キュー背景色2のリセットボタン
        private void btnResetColor2_Click(object sender, EventArgs e)
        {
            QueueBackColor[1] = ColorTranslator.FromHtml(DefaultVal.QueueBackColor[1]);
            labelBackColor[1].BackColor = QueueBackColor[1];
        }


        ////////////////////////////////////////
        // 設定完了・キャンセルボタン
        ////////////////////////////////////////

        // 設定完了ボタン
        private void btnOK_Click(object sender, EventArgs e)
        {
            string waitTime = textBoxWaitTime.Text;
            if (int.TryParse(waitTime, out int num))
            {
                WaitTimeAfterPaste = num;
            }
            else
            {
                MessageBox.Show(
                    "文字列は整数に変換できません",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        // 設定キャンセルボタン
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

