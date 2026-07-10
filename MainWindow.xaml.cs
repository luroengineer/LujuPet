using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace LujuPet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 拖曳移動功能 + 判斷拖曳時間
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                // 拖曳前顯示拖曳 GIF
                ChangeGif("Images/petDrag.gif");

                this.DragMove(); // 阻塞直到拖曳結束

                sw.Stop();

                if (sw.Elapsed.TotalSeconds > 0.2)
                {
                    // 拖曳超過 0.2 秒 → 延遲 0.5 秒再換回原始 GIF
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                    });
                }
                else
                {
                    // 拖曳小於等於 0.2 秒 → 當作點擊
                    ChangeGif("Images/petClick.gif");

                    // 點擊後也延遲 0.5 秒再換回原始 GIF
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                    });
                }
            }
        }

        // 右鍵點擊 → 顯示選單
        private void PetImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem exitItem = new MenuItem { Header = "結束寵物" };
            exitItem.Click += (s, args) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            menu.IsOpen = true;
        }

        // 共用方法：切換 GIF
        private void ChangeGif(string path)
        {
            var uri = new Uri($"pack://application:,,,/" + path, UriKind.Absolute);
            var image = new BitmapImage(uri);
            ImageBehavior.SetAnimatedSource(PetImage, image);
        }

    }
}