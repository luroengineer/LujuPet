using System.Windows;
using System.Windows.Media.Imaging;

namespace LujuPet
{
    public partial class NoticeWindow : Window
    {
        public NoticeWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 設定視窗標題與圖片
        /// </summary>
        /// <param name="fileName">圖片檔案路徑</param>
        /// <param name="windowTitle">視窗標題</param>
        public static void ShowNotice(string fileName, string windowTitle)
        {
            NoticeWindow win = new NoticeWindow();

            // 設定視窗標題
            win.Title = windowTitle;

            // 設定圖片
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    win.NoticeImage.Source = new BitmapImage(new System.Uri(fileName, System.UriKind.Absolute));
                }
                catch
                {
                    MessageBox.Show("載入圖片失敗: " + fileName, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            bool? result = win.ShowDialog();
        }
    }
}
