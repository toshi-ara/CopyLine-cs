using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace CopyLine
{
    // INIファイルを読み書きするためのクラス
    class IniFile
    {
        private string filePath;

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string defaultValue,
            StringBuilder retVal,
            int size,
            string filePath
        );

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern bool WritePrivateProfileString(
            string section,
            string key,
            string value,
            string filePath
        );

        public IniFile(string path)
        {
            filePath = path;
            // ファイルが存在しない場合は作成
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");  // 空のファイルを作成
            }
        }

        public string Read(string section, string key, string defaultValue = "")
        {
            var retVal = new StringBuilder(256);
            GetPrivateProfileString(
                section, key, defaultValue, retVal, retVal.Capacity, filePath
            );
            return retVal.ToString();
        }

        public bool Write(string section, string key, string value)
        {
            return WritePrivateProfileString(section, key, value, filePath);
        }
    }


    partial class CopyLine : Form
    {
        ////////////////////////////////////////
        // 設定ファイルの書き込み、読み込み
        ////////////////////////////////////////

        // 設定ファイルに書き込み
        private void SaveValuesINI()
        {
            IniFile ini = new IniFile(iniFilePath);
            ini.Write("Settings", "WaitTimeAfterPaste",
                WaitTimeAfterPaste.ToString());
            ini.Write("Settings", "QueueBackColor1", QueueBackColor[0]);
            ini.Write("Settings", "QueueBackColor2", QueueBackColor[1]);
            ini.Write("Settings", "ClipboardBackColorON", ClipboardBackColorON);
            ini.Write("Settings", "ClipboardBackColorOFF", ClipboardBackColorOFF);
            ini.Write("Settings", "TextFont", TextFont);
        }

        private void GetValuesINI()
        {
            if (!File.Exists(iniFilePath))
            {
                // INIファイルがない場合には作成して初期設定を書き込む
                SaveValuesINI();
            }
            else
            {
                // INIファイルから読み込む
                IniFile ini = new IniFile(iniFilePath);
                WaitTimeAfterPaste =
                    Convert.ToInt32(ini.Read("Settings", "WaitTimeAfterPaste"));
                QueueBackColor[0] = ini.Read("Settings", "QueueBackColor1");
                QueueBackColor[1] = ini.Read("Settings", "QueueBackColor2");
                ClipboardBackColorON = ini.Read("Settings", "ClipboardBackColorON");
                ClipboardBackColorOFF = ini.Read("Settings", "ClipboardBackColorOFF");
                TextFont = ini.Read("Settings", "TextFont");
            }
        }

        public string ConvertColorNameToHex(string colorName)
        {
            Color color = Color.FromName(colorName);
            return ColorTranslator.ToHtml(color);
        }


        ////////////////////////////////////////
        // ダイアログからテキストファイルを選択する
        ////////////////////////////////////////
        private void HandlerOpenFile(Object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "開くファイルを選択してください";
            op.Filter = "txtファイル|*.txt|Markdownファイル|*.md|すべてのファイル|*.*";

            if (op.ShowDialog() == DialogResult.OK)
            {
                OpenFile(op.FileName);
            }
        }
        private void OpenFile(string path)
        {
            try
            {
                string content = FileConvertToUtf8(path);
                string newText = ConvertToCRLF(content);
                textBoxInput.Text = newText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ファイルを開くことができません\n{ex.Message}");
            }
        }

        // ファイルがドラッグされたときの処理
        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        // ファイルがドロップされたときの処理
        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0)
            {
                return;
            }

            try
            {
                string content = FileConvertToUtf8(files[0]);
                string newText = ConvertToCRLF(content);
                textBoxInput.Text = newText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ファイルを開くことができません\n{ex.Message}");
            }
        }

        // ファイルを開き、Shift-JIS であれば utf-8 に変換する
        private string FileConvertToUtf8(string filePath)
        {
            Encoding encoding = DetectEncoding(filePath);

            if (encoding == Encoding.GetEncoding("Shift_JIS"))
            {
                string context = File.ReadAllText(filePath, Encoding.Default);
                byte[] shiftJisBytes = Encoding.GetEncoding("Shift_JIS").GetBytes(context);

                // CP932（Shift_JIS）から utf-8 へ変換
                byte[] utf8Bytes =
                    Encoding.Convert(Encoding.GetEncoding("Shift_JIS"),
                                    Encoding.UTF8, shiftJisBytes);

                // バイト配列を utf-8 の文字列に変換
                return Encoding.UTF8.GetString(utf8Bytes);
            }
            else
            {
                string context = File.ReadAllText(filePath);
                return context;
            }
        }

        // 改行コードを CRLF に変換する
        private string ConvertToCRLF(string content)
        {
            string newText = content
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "\r\n");
            return newText;
        }

        static Encoding DetectEncoding(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            // utf-8 の場合、BOM付きかどうかを確認
            if (bytes.Length >= 3 &&
                bytes[0] == 0xEF &&
                bytes[1] == 0xBB &&
                bytes[2] == 0xBF)
            {
                return Encoding.UTF8; // BOM付き utf-8
            }

            // utf-8 のバイトパターンをチェック
            bool isUtf8 = true;
            int i = 0;
            while (i < bytes.Length)
            {
                byte b = bytes[i];

                if ((b & 0x80) == 0x00)
                {
                    // ASCII文字（1バイト）
                    i++;
                }
                else if ((b & 0xE0) == 0xC0)
                {
                    // 2バイト utf-8
                    if (i + 1 >= bytes.Length || (bytes[i + 1] & 0xC0) != 0x80)
                    {
                        isUtf8 = false;
                        break;
                    }
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0)
                {
                    // 3バイト utf-8
                    if (i + 2 >= bytes.Length ||
                        (bytes[i + 1] & 0xC0) != 0x80 ||
                        (bytes[i + 2] & 0xC0) != 0x80)
                    {
                        isUtf8 = false;
                        break;
                    }
                    i += 3;
                }
                else if ((b & 0xF8) == 0xF0)
                {
                    // 4バイト utf-8
                    if (i + 3 >= bytes.Length ||
                        (bytes[i + 1] & 0xC0) != 0x80 ||
                        (bytes[i + 2] & 0xC0) != 0x80 ||
                        (bytes[i + 3] & 0xC0) != 0x80)
                    {
                        isUtf8 = false;
                        break;
                    }
                    i += 4;
                }
                else
                {
                    isUtf8 = false;
                    break;
                }
            }

            if (isUtf8)
            {
                return Encoding.UTF8;
            }

            return Encoding.GetEncoding("Shift_JIS");
        }
    }
}
