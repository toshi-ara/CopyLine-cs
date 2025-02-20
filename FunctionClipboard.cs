using System;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CopyLine
{
    // メンバー関数（コピー・ペースト関連）
    partial class CopyLine : Form
    {
        static bool onChangeClipboard = false;

        ////////////////////////////////////////
        // テキストボックスの内容を処理してキューに格納する関数
        ////////////////////////////////////////
        async private void HandlerSetQueue(Object sender, EventArgs e)
        {
            string strInput = textBoxInput.Text ?? string.Empty;
            if (strInput == string.Empty)
            {
                return;
            }

            // 改行文字で分割
            string[] del = {"\n", "\r", "\r\n"};  // 改行を示す特殊文字
            string[] arr =  strInput.Split(del, StringSplitOptions.None);

            for (int i = 0; i < arr.Length; i++)
            {
                // ここで正規表現で切り出す
                string result = ExtractString.GetExtractString(arr[i]);

                // 有効な文字列の場合のみキューに登録
                if (result != string.Empty)
                {
                    queue.Enqueue(result);
                }
            }

            if (queue.Count == 0)
            {
                return;
            }

            // 手動でクリップボードを変更した*後でない*場合には
            // キューの先頭の要素をクリップボードに設定
            // if (!onCopy && N == 0)
            if (!onCopy)
            {
                strClipboard = queue.Peek();
                await SetClipboard(strClipboard);
            }
            UpdateQueueList();
        }

        async private Task SetClipboard(string str)
        {
            onChangeClipboard = true;
            if (str == string.Empty)
            {
                Clipboard.Clear();
            }
            else
            {
                Clipboard.SetText(str);
            }
            textBoxClipboard.Text = str;

            await Task.Delay(10);
            onChangeClipboard = false;
        }

        async private Task ClearClipboard()
        {
            onChangeClipboard = true;
            Clipboard.Clear();
            textBoxClipboard.Text = string.Empty;

            await Task.Delay(10);
            onChangeClipboard = false;
        }


        ////////////////////////////////////////
        // キュー表示の更新
        ////////////////////////////////////////
        private void UpdateQueueList()
        {
            int N = queue.Count;
            if (N == 0) {
                labelItemNumber.Text = "要素数: 0";
                foreach (Label label in queueLabel)
                {
                    label.Text = string.Empty;
                }
                return;
            }

            // 最大nMax個の要素を表示する
            int i = 0;
            foreach (var item in queue)
            {
                queueLabel[i].Text = item ?? string.Empty;
                i++;
                if (i == nQueueMax)
                {
                    break;
                }
            }

            if (N < nQueueMax)
            {
                // 最後の要素があった場所のラベルを消す
                queueLabel[N].Text = string.Empty;
            }

            labelItemNumber.Text = $"要素数: {N}";
        }


        ////////////////////////////////////////
        // クリップボードの書き換えを検出した場合
        ////////////////////////////////////////
        async private Task OnClipboardUpdate()
        {
            // プログラム内でクリップボードを書き換えた場合は無視
            if (onChangeClipboard)
            {
                return;
            }

            // 手動でクリップボードを書き換えた場合は onCopy = true
            // 2回目以降も true のまま
            strClipboardManual = Clipboard.GetText();
            await SetClipboard(strClipboardManual);

            if (isEnableCopyLine)
            {
                onCopy = true;
                ToggleClipbordBoxColor();
            }
        }


        ////////////////////////////////////////
        // Ctrl-V が押された場合の処理
        // 右クリックメニューから貼り付けを選択した場合には対応していない
        ////////////////////////////////////////
        async public Task DetectPaste()
        {
            if (isEnableCopyLine)
            {
                await Task.Delay(WaitTimeAfterPaste);  // この待ち時間の間に貼り付け

                int N = queue.Count;
                // 貼付け後の操作
                if (!onCopy)  // Ctrl-C なしで貼り付けた場合
                {
                    // キューに要素が2個以上存在する場合には先頭の要素を捨てて
                    // 2番目の要素をクリップボード用変数に設定する
                    if (N >= 2)
                    {
                        try {
                            _ = queue.Dequeue();          // 先頭の要素（捨てる）
                            strClipboard = queue.Peek();  // 2番目の要素
                            await SetClipboard(strClipboard);
                        } catch {
                            strClipboard = string.Empty;
                            await ClearClipboard();
                        }
                    }
                    else  // N = 1, 0
                    {
                        if (N == 1)
                        {
                            _ = queue.Dequeue();  // 先頭の要素を捨てる
                        }
                        strClipboard = string.Empty;
                        await ClearClipboard();
                    }
                    UpdateQueueList();
                }
                else  // 手動で Ctrl-C を行った直後の場合 (onCopy == true)
                {
                    // クリップボードには strClipboardManual の内容が設定されている
                    // 貼り付け後にキューの内容を変更せずにクリップボードの操作だけを行う
                    //   setClipboard にキューの先頭の内容が格納されている
                    if (N >= 1)
                    {
                        await SetClipboard(strClipboard);
                    }
                    else  // N == 0
                    {
                        // キューが空の場合には貼り付け後にクリップボード用変数を空にする
                        // もともと 実行されているように思えるが念のため
                        await ClearClipboard();
                    }
                    // クリップボード書き換え後のフラグ設定
                    onCopy = false;
                    ToggleClipbordBoxColor();
                }
            }
        }
    }
}
