using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace LujuPet
{
    public partial class MainWindow : Window
    {
        double petSize = 0.5;
        private DispatcherTimer idleTimer;
        private DateTime lastInteraction;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            ResizePet();
            InitIdleTimer();
            lastInteraction = DateTime.Now;
        }

        // 重新設定寵物尺寸
        private void ResizePet()
        {
            PetImage.RenderTransform = new ScaleTransform(petSize, petSize);
        }

        // 初始化閒置檢查計時器
        private void InitIdleTimer()
        {
            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromSeconds(1);
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastInteraction).TotalSeconds > 10)
            {
                StartRandomMove();
                lastInteraction = DateTime.Now; // 重置時間，避免持續觸發
            }
        }

        // 隨機左右移動，移動時顯示 petMove.gif，結束後恢復 pet.gif
        private async void StartRandomMove()
        {
            int duration = random.Next(5, 11); // 5~10 秒
            int direction = random.Next(0, 2) == 0 ? -1 : 1; // -1 左, 1 右
            double step = 5 * direction; // 每次移動的像素量
            DateTime endTime = DateTime.Now.AddSeconds(duration);

            // 切換成移動 GIF
            ChangeGif("Images/petMove.gif");

            while (DateTime.Now < endTime)
            {
                this.Left += step;
                await Task.Delay(100); // 每 0.1 秒移動一次
            }

            // 移動結束後恢復原始 GIF
            ChangeGif("Images/pet.gif");
        }

        // 拖曳移動功能 + 判斷拖曳時間
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastInteraction = DateTime.Now; // 更新互動時間

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                ChangeGif("Images/petDrag.gif");
                this.DragMove(); // 阻塞直到拖曳結束
                sw.Stop();

                if (sw.Elapsed.TotalSeconds > 0.2)
                {
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                    });
                }
                else
                {
                    ChangeGif("Images/petClick.gif");
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
            lastInteraction = DateTime.Now; // 更新互動時間

            ContextMenu menu = new ContextMenu();

            MenuItem exitItem = new MenuItem { Header = "結束寵物" };
            exitItem.Click += (s, args) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            MenuItem sizeMenu = new MenuItem { Header = "寵物尺寸" };
            for (int i = 10; i <= 100; i += 10)
            {
                MenuItem sizeItem = new MenuItem { Header = $"{i}%" };
                int scale = i;
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
