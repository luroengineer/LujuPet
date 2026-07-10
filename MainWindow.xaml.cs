using System.Diagnostics;
using System.IO;
using System.Numerics;
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
        double petSize = 0.5;
        public MainWindow()
        {
            InitializeComponent();
            ResizePet();
        }

        // 重新設定寵物尺寸
        private void ResizePet()
        {
            PetImage.RenderTransform = new ScaleTransform(petSize, petSize);
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

            // 結束寵物選項
            MenuItem exitItem = new MenuItem { Header = "結束寵物" };
            exitItem.Click += (s, args) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            // 尺寸選單
            MenuItem sizeMenu = new MenuItem { Header = "寵物尺寸" };

            // 建立 0% ~ 100% 的選項（每 20% 一格，可依需求調整）
            for (int i = 10; i <= 100; i += 10)
            {
                MenuItem sizeItem = new MenuItem { Header = $"{i}%" };
                int scale = i; // 捕捉迴圈變數
                sizeItem.Click += (s, args) =>
                {
                    petSize = scale / 100.0;
                    ResizePet();
                };
                sizeMenu.Items.Add(sizeItem);
            }

            menu.Items.Add(sizeMenu);

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