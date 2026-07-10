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
        double petSize = 0.4;
        bool allowWalk = true;
        bool allowLunch = true;
        
        private DispatcherTimer idleTimer;
        private DateTime lastInteraction;
        private Random random = new Random();

        bool isWalking = false;
        bool isDrag = false;

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

        private bool IsTimePeriod(DateTime dateTimeStart, DateTime dateTimeEnd)
        {
            DateTime now = DateTime.Now;

            // 檢查是否在區間內
            return now >= dateTimeStart && now <= dateTimeEnd;
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (allowLunch)
            {
                double idleTimer = random.Next(5, 10);

                DateTime start = DateTime.Today.AddHours(23);   // 今天早上 9 點
                DateTime end = DateTime.Today.AddHours(24);  // 今天下午 5 點
                if (IsTimePeriod(start, end) && (DateTime.Now - lastInteraction).TotalSeconds > idleTimer)
                {
                    HaveLunch();
                    lastInteraction = DateTime.Now;
                }
            }

            if (allowWalk)
            {
                double idleTimer = random.Next(3, 18);
                if ((DateTime.Now - lastInteraction).TotalSeconds > idleTimer)
                {
                    StartRandomMove();
                    lastInteraction = DateTime.Now; // 重置時間，避免持續觸發
                }
            }

        }

        private async void HaveLunch()
        {
            ChangeGif("Images/petEat.gif");
        }

        // 隨機左右移動，移動時顯示 petMove.gif，結束後恢復 pet.gif
        private async void StartRandomMove()
        {
            if (isWalking) //避免有重複的走動事件
            {
                return;
            }

            isWalking = true;

            int duration = random.Next(3, 7); // 3~6 秒
            int direction = random.Next(0, 2) == 0 ? -1 : 1; // -1 左, 1 右
            double step = 1 * direction; // 每次移動的像素量
            DateTime endTime = DateTime.Now.AddSeconds(duration);

            // 切換成移動 GIF
            ChangeGif("Images/petMove.gif");

            while (DateTime.Now < endTime)
            {
                if (isDrag)
                {
                    break;
                }

                // 計算下一個位置
                double nextLeft = this.Left + step;

                // 取得螢幕寬度
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                // 取得 PetImage 左上角在螢幕中的座標
                Point petTopLeft = PetImage.PointToScreen(new Point(0, 0));

                // 取得 PetImage 右下角在螢幕中的座標
                Point petBottomRight = PetImage.PointToScreen(new Point(PetImage.ActualWidth, PetImage.ActualHeight));

                Console.WriteLine(petTopLeft.X.ToString() + "," + petBottomRight.X.ToString());

                if ((petTopLeft.X - PetImage.Width / 2) < 0 || (petBottomRight.X - PetImage.Width / 2) > screenWidth)
                {
                    //MessageBox.Show("寵物超出螢幕範圍！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                }

                // 檢查是否超出邊界
                /*if (nextLeft < 0 || nextLeft + this.Width > screenWidth)
                {
                    string msg = nextLeft.ToString() + "," + screenWidth.ToString();
                    MessageBox.Show("寵物碰到螢幕邊緣了！" + msg, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break; // 停止移動
                }*/

                this.Left = nextLeft;
                await Task.Delay(100); // 每 0.1 秒移動一次
            }

            // 移動結束後恢復原始 GIF
            ChangeGif("Images/pet.gif");

            isWalking = false;
        }

        // 拖曳移動功能 + 判斷拖曳時間
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrag = true;
            lastInteraction = DateTime.Now; // 更新互動時間

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                ChangeGif("Images/petDrag.gif");
                this.DragMove(); // 阻塞直到拖曳結束
                sw.Stop();

                if (sw.Elapsed.TotalSeconds > 0.1)
                {
                    //Task.Delay(500).ContinueWith(_ =>
                    //{
                        //Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                    //});
                }
                else
                {
                    ChangeGif("Images/petClick.gif");
                    Task.Delay(300).ContinueWith(_ =>
                    {
                        //Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                    });
                }
            }
            isDrag = false;
        }


        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeGif("Images/pet.gif");
        }

        private void FeedPet()
        {
            int randomSeed = random.Next(0, 100);

            if (randomSeed > 10)
            {
                ChangeGif("Images/petGoodApple.gif");
                Task.Delay(1000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                });
            }
            else
            {
                ChangeGif("Images/petBadApple.gif");
                Task.Delay(100).ContinueWith(_ =>
                {
                    //Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                });
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

            MenuItem feedAppleItem = new MenuItem { Header = "餵食" };
            feedAppleItem.Click += (s, args) =>
            {
                FeedPet();
            };
            menu.Items.Add(feedAppleItem);

            menu.IsOpen = true;

            lastInteraction = DateTime.Now; // 更新互動時間
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
