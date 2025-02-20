namespace CopyLine
{
    public struct DefaultVal
    {
        public static int WaitTimeAfterPaste = 20;
        public static string[] QueueBackColor = {"SkyBlue", "PaleTurquoise"};
        public static string ClipboardBackColorON = "LavenderBlush";
        public static string ClipboardBackColorOFF = "White";
        public static string TextFont = "MSゴシック";
    }


    partial class CopyLine
    {
        // バージョン番号
        public static string VersionNumber = "1.0.0";

        // 共通のラベル
        private const string textEnabledCopyLine = "CopyLineの機能を有効にする";


        // iniファイル名
        private static readonly string fileNameINI = "CopyLine.ini";
    }
}
