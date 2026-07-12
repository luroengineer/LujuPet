using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Text.Json;
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
        PetConfig petConfig = new PetConfig();

        private DispatcherTimer idleTimer;
        private DateTime lastInteraction;
        private Random random = new Random();

        private string configPath = "petConfig.json";

        bool isWalking = false;
        bool isDrag = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            ResizePet();
            InitIdleTimer();
            lastInteraction = DateTime.Now;
            CheckDatePass();
        }

        private void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                petConfig = JsonSerializer.Deserialize<PetConfig>(json);

                ResizePet();
            }
        }

        private void SaveConfig()
        {
            string json = JsonSerializer.Serialize(petConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }

        private bool CheckDatePass()
        {
            DateTime start = new DateTime(2026, 7, 10, 0, 0, 0);
            DateTime end = new DateTime(2026, 7, 12, 23, 59, 0);

            if (IsTimePeriod(start, end))
            {
                return true;
            }
            else
            {
                MessageBox.Show("試用版已結束啦~", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return false;
            }
        }

        // 重新設定寵物尺寸
        private void ResizePet()
        {
            PetImage.RenderTransform = new ScaleTransform(petConfig.PetSize, petConfig.PetSize);
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

        private bool isRandomRun(int probability)
        {
            int randomNum = random.Next(0, 100);
            return (randomNum < probability);
        }

        private PET_STATUS GetRandomStatus()
        {
            PET_STATUS status = PET_STATUS.DEFAULT; //(PET_STATUS)(random.Next(0, (int)PET_STATUS.END));

            if (petConfig.AllowAlarm)
            {
                DateTime now = DateTime.Now;
                if (now.Hour == petConfig.AlarmHour && now.Minute == petConfig.AlarmMinute)
                {
                    return PET_STATUS.ALARM;
                }
            }

            if (petConfig.AllowDinner)
            {
                DateTime start = DateTime.Today.AddHours(petConfig.DinnerStartHour).AddMinutes(petConfig.DinnerStartMinute);
                DateTime end = DateTime.Today.AddHours(petConfig.DinnerEndHour).AddMinutes(petConfig.DinnerEndMinute);

                if (IsTimePeriod(start, end))
                {
                    return PET_STATUS.DINNER;
                }
            }

            if (petConfig.AllowLunch)
            {
                DateTime start = DateTime.Today.AddHours(petConfig.LunchStartHour).AddMinutes(petConfig.LunchStartMinute);
                DateTime end = DateTime.Today.AddHours(petConfig.LunchEndHour).AddMinutes(petConfig.LunchEndMinute);

                if (IsTimePeriod(start, end))
                {
                    return PET_STATUS.LUNCH;
                }
            }

            if (petConfig.AllowOverTime)
            {
                DateTime start = DateTime.Today.AddHours(18).AddMinutes(30);
                DateTime end = DateTime.Today.AddHours(23).AddMinutes(59);

                if (IsTimePeriod(start, end) && isRandomRun(50))
                {
                    return PET_STATUS.OVERTIME;
                }
            }

            if (petConfig.AllowDoze)
            {
                if (isRandomRun(50))
                {
                    return PET_STATUS.DOZE;
                }
                if (isRandomRun(50))
                {
                    return PET_STATUS.SLEEP;
                }
            }

            if (petConfig.AllowWork)
            {
                DateTime start = DateTime.Today.AddHours(8).AddMinutes(30);
                DateTime end = DateTime.Today.AddHours(17).AddMinutes(30);

                if (IsTimePeriod(start, end) && isRandomRun(50))
                {
                    return PET_STATUS.WORK;
                }
            }

            if (petConfig.AllowWalk)
            {
                if (isRandomRun(80))
                {
                    return PET_STATUS.WALK;
                }
            }

            if (petConfig.AllowMad)
            {
                DateTime start = new DateTime(2026, 7, 10, 0, 0, 0);
                DateTime end = new DateTime(2026, 7, 12, 23, 0, 0);

                if (IsTimePeriod(start, end) && isRandomRun(12))
                {
                    return PET_STATUS.KUAI;
                }
            }

            return status;
        }

        bool isPetBusy = false;
        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            double idleTimer = random.Next(6, 10);
            double pastTime = (DateTime.Now - lastInteraction).TotalSeconds;

            if (pastTime > 30)
            {
                isPetBusy = false;
            }

            if (isPetBusy == true)
            {
                return;
            }

            if (pastTime > idleTimer)
            {
                isPetBusy = true;
                PET_STATUS status = GetRandomStatus();

                switch(status)
                {
                    case PET_STATUS.ALARM:
                        PetAlarm();
                        break;
                    case PET_STATUS.LUNCH:
                        HaveLunch();
                        break;
                    case PET_STATUS.DINNER:
                        HaveDinner();
                        break;
                    case PET_STATUS.OVERTIME:
                        PetOvertime();
                        break;
                    case PET_STATUS.SLEEP:
                        PetSleep();
                        break;
                    case PET_STATUS.DOZE:
                        PetDoze();
                        break;
                    case PET_STATUS.WORK:
                        PetWork();
                        break;
                    case PET_STATUS.KUAI:
                        PetKuai();
                        break;
                    case PET_STATUS.WALK:
                        StartRandomMove();
                        break;
                    default:
                        //PetDefault();
                        //return;
                        break;
                }
                lastInteraction = DateTime.Now; // 重置時間，避免持續觸發
                isPetBusy = false;
            }

        }
        private async void PetAlarm()
        {
            ChangeGif(PET_STATUS.ALARM);
        }

        private async void HaveDinner()
        {
            ChangeGif(PET_STATUS.DINNER);
        }

        private async void HaveLunch()
        {
            ChangeGif(PET_STATUS.LUNCH);
        }

        private async void PetSleep()
        {
            ChangeGif(PET_STATUS.SLEEP);
        }

        private async void PetDoze()
        {
            ChangeGif(PET_STATUS.DOZE);
        }

        private async void PetWork()
        {
            ChangeGif(PET_STATUS.WORK);
        }

        private async void PetOvertime()
        {
            ChangeGif(PET_STATUS.OVERTIME);
        }

        private async void PetKuai()
        {
            ChangeGif(PET_STATUS.KUAI);
        }

        private async void PetDance()
        {

            ChangeGif(PET_STATUS.DANCE);
            Task.Delay(1000).ContinueWith(_ =>
            {
                //Dispatcher.Invoke(() => PetDefault());
            });

            lastInteraction = DateTime.Now; // 更新互動時間
        }

        private async void PetCook()
        {
            if (isRandomRun(50))
            {
                ChangeGif(PET_STATUS.COOK_FA);
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => PetDefault());
                });
            }
            else
            {
                ChangeGif(PET_STATUS.COOK_CRAB);
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => PetDefault());
                });
            }

            lastInteraction = DateTime.Now; // 更新互動時間
        }

        private async void PetDefault()
        {
            ChangeGif(PET_STATUS.DEFAULT);
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
            ChangeGif(PET_STATUS.WALK);

            while (DateTime.Now < endTime)
            {
                if (isDrag)
                {
                    isWalking = false;
                    return;
                    //break;
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

                if ((petTopLeft.X) < 0)
                {
                    step = Math.Abs(step);
                    Console.WriteLine("撞牆往右");
                }
                else if ((petTopLeft.X) > screenWidth)
                {
                    step = (-1) * Math.Abs(step);
                    Console.WriteLine("撞牆往左");
                }

                this.Left = nextLeft;
                await Task.Delay(100); // 每 0.1 秒移動一次
            }

            // 移動結束後恢復原始 GIF
            PetDefault();

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

                ChangeGif(PET_STATUS.DRAG);
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
                    ChangeGif(PET_STATUS.SHOCKED);
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
            PetDefault();
        }

        private void FeedApple()
        {

            int randomSeed = random.Next(0, 100);

            if (randomSeed > 10)
            {
                ChangeGif(PET_STATUS.GOOD_APPLE);
                Task.Delay(500).ContinueWith(_ =>
                {
                    //Dispatcher.Invoke(() => ChangeGif("Images/pet.gif"));
                });
            }
            else
            {
                ChangeGif(PET_STATUS.BAD_APPLE);
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => PetDefault());
                });
            }

            lastInteraction = DateTime.Now; // 更新互動時間
        }

        private void FeedRiceBall()
        {
            {
                ChangeGif(PET_STATUS.RICE_BALL);
                Task.Delay(500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => PetDefault());
                });
            }

            lastInteraction = DateTime.Now; // 更新互動時間
        }

        // 右鍵點擊 → 顯示選單
        private void PetImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            lastInteraction = DateTime.Now; // 更新互動時間

            ContextMenu menu = new ContextMenu();

            // 在 PetImage_MouseRightButtonUp 內新增
            MenuItem feedbackItem = new MenuItem { Header = "填寫反饋" };
            feedbackItem.Click += (s, args) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://forms.gle/sxnt1Q9noYTng6w67",
                    UseShellExecute = true
                });
            };
            menu.Items.Add(feedbackItem);

            MenuItem exitItem = new MenuItem { Header = "結束滷豬" };
            exitItem.Click += (s, args) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            MenuItem sizeMenu = new MenuItem { Header = "滷豬尺寸" };
            for (int i = 10; i <= 100; i += 10)
            {
                MenuItem sizeItem = new MenuItem { Header = $"{i}%" };
                int scale = i;
                sizeItem.Click += (s, args) =>
                {
                    petConfig.PetSize = scale / 100.0;
                    ResizePet();
                };
                sizeMenu.Items.Add(sizeItem);
            }
            menu.Items.Add(sizeMenu);

            MenuItem feedAppleItem = new MenuItem { Header = "餵食蘋果" };
            feedAppleItem.Click += (s, args) =>
            {
                FeedApple();
            };
            menu.Items.Add(feedAppleItem);

            MenuItem feedRiceBallItem = new MenuItem { Header = "餵食飯糰" };
            feedRiceBallItem.Click += (s, args) =>
            {
                FeedRiceBall();
            };
            menu.Items.Add(feedRiceBallItem);

            MenuItem DanceItem = new MenuItem { Header = "扭動一下" };
            DanceItem.Click += (s, args) =>
            {
                PetDance();
            };
            menu.Items.Add(DanceItem);

            MenuItem cookAppleItem = new MenuItem { Header = "熬製滷汁" };
            cookAppleItem.Click += (s, args) =>
            {
                PetCook();
            };
            menu.Items.Add(cookAppleItem);

            MenuItem settingsItem = new MenuItem { Header = "開啟設定" };
            settingsItem.Click += (s, args) =>
            {
                SettingsWindow settingsWindow = new SettingsWindow(petConfig);
                if (settingsWindow.ShowDialog() == true)
                {
                    ResizePet();
                    SaveConfig();
                    LoadConfig();
                }
            };
            menu.Items.Add(settingsItem);

            menu.IsOpen = true;

            lastInteraction = DateTime.Now; // 更新互動時間
        }

        // 共用方法：切換 GIF
        private void ChangeGif(PET_STATUS status)
        {
            string path = "Images/" + status.ToString() + ".gif";
            var uri = new Uri($"pack://application:,,,/" + path, UriKind.Absolute);
            var image = new BitmapImage(uri);
            ImageBehavior.SetAnimatedSource(PetImage, image);
        }

    }
}
