using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LujuPet
{
    public partial class MainWindow : Window
    {
        PetConfig petConfig = new PetConfig();

        private DispatcherTimer idleTimer;
        private DateTime lastInteraction;
        private Random random = new Random();

        private string configPath = "petConfig.json";
        private string rootFolderName = @"C:\簡報資料公用區\暫存區\";
        LotteryWindow lotteryWindow = new LotteryWindow();
        string userName = Environment.UserName;
        string key = "pig";

        string nowStringDate;
        string enCodeDate;
        string deCodeDate;
        string nowStringTime;
        string enCodeTime;
        string deCodeTime;



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
            SaveLog(LOG_TYPE.OPEN_PET);
        }

        private void ShowLotteryWindow()
        {
            if (lotteryWindow == null)
            {
                lotteryWindow = new LotteryWindow();
            }

            lotteryWindow.Show();
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
            DateTime end = new DateTime(2026, 8, 31, 23, 59, 0);

            if (IsTimePeriod(start, end))
            {
                return true;
            }
            else
            {
                MessageBox.Show("這個版本使用期間結束啦~\n請去下載新版本!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void UpdateDateTimeString()
        {
            nowStringDate = EnDeCode.GetNowString(false);
            enCodeDate = EnDeCode.EncryptString(nowStringDate, key);
            Console.WriteLine("EN: " + enCodeDate);
            deCodeDate = EnDeCode.DecryptString(enCodeDate, key);
            Console.WriteLine("DE: " + deCodeDate);

            nowStringTime = EnDeCode.GetNowString(true);
            enCodeTime = EnDeCode.EncryptString(nowStringTime, key);
            Console.WriteLine("EN: " + enCodeTime);
            deCodeTime = EnDeCode.DecryptString(enCodeTime, key);
            Console.WriteLine("DE: " + deCodeTime);
        }

        private PET_STATUS GetRandomStatus()
        {
            PET_STATUS status = PET_STATUS.DEFAULT; //(PET_STATUS)(random.Next(0, (int)PET_STATUS.END));
            List<string> gdiskLines = null;

            if (petConfig.AllowAlarm)
            {
                DateTime now = DateTime.Now;
                if (now.Hour == petConfig.AlarmHour && now.Minute == petConfig.AlarmMinute)
                {
                    return PET_STATUS.ALARM;
                }
            }

            UpdateDateTimeString();
            List<int> checkMinuteBox = new List<int>();
            checkMinuteBox.Add(0);
            checkMinuteBox.Add(30);
            if (checkMinuteBox.Contains(DateTime.Now.Minute)) 
            {
                gdiskLines = GdiskHandle();
                if (gdiskLines != null)
                {
                    if (petConfig.AllowLottery)
                    {
                        LotteryHandle(gdiskLines);
                    }

                    if (petConfig.AllowLeader)
                    {
                        NoticeHandle(gdiskLines, NOTICE_TYPE.LEADER);
                    }

                    if (petConfig.AllowUpdate)
                    {
                        NoticeHandle(gdiskLines, NOTICE_TYPE.UPDATE);
                    }

                    if (petConfig.AllowAdvertise)
                    {
                        NoticeHandle(gdiskLines, NOTICE_TYPE.ADVERTISE);
                    }

                    if (petConfig.AllowIt)
                    {
                        NoticeHandle(gdiskLines, NOTICE_TYPE.IT);
                    }

                    if (petConfig.AllowHr)
                    {
                        NoticeHandle(gdiskLines, NOTICE_TYPE.HR);
                    }

                    if (petConfig.AllowAdm)
                    {
                        NoticeHandle(gdiskLines, NOTICE_TYPE.ADM);
                    }
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

                //Console.WriteLine(petTopLeft.X.ToString() + "," + petBottomRight.X.ToString());

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

        DateTime lastNotice = DateTime.Now;
        private bool NoticeHandle(List<string> lines, NOTICE_TYPE noticeType)
        {
            for (int cntLine = 0; cntLine < lines.Count; cntLine++)
            {
                string lineOri = lines[cntLine];
                string lineDecode = EnDeCode.DecryptString(lineOri, key);

                if ((cntLine == 0))
                {
                    if (lineDecode.Contains(noticeType.ToString()) && lineDecode.Contains(nowStringDate))
                    {
                        Console.WriteLine(noticeType.ToString() + ": 檢查到此分類正確");

                        DateTime nowNotice = DateTime.Now;
                        if ((lastNotice.Date == nowNotice.Date)
                            && (lastNotice.Hour == nowNotice.Hour)
                            && (lastNotice.Minute == nowNotice.Minute))
                        {
                            Console.WriteLine("此notice已顯示過");
                            return false;
                        }
                        lastNotice = DateTime.Now;

                        string fileNameDecode = EnDeCode.DecryptString(lines[1], key);
                        string titleDecode = EnDeCode.DecryptString(lines[2], key);

                        NoticeWindow.ShowNotice(fileNameDecode, titleDecode);
                    }
                    else
                    {
                        Console.WriteLine(noticeType.ToString() + ": 第一行檢查異常");
                        return false;
                    }
                }
            }
             
            return false;
        }

        private bool LotteryHandle(List<string> lotteryString)
        {
            int seed = 0;
            int prob = 0;
            string lotteryMsg = "";

            for (int cntLine = 0; cntLine < lotteryString.Count; cntLine++)
            {
                string lineOri = lotteryString[cntLine];
                string lineDecode = EnDeCode.DecryptString(lineOri, key);


                Console.WriteLine("lottery第" + cntLine + "行檢查：" + lineOri + "\t" + lineDecode);
                //第一行檢查是不是含有lutsaitu和當天日期
                //第二行檢查機率是不是 0 ~ 100
                //第三行檢查有沒有介於19940706~20201109
                if ((cntLine == 0))
                {
                    if (lineDecode.Contains("LOTTERY") && lineDecode.Contains(nowStringDate))
                    {

                    }
                    else
                    {
                        Console.WriteLine("第一行檢查異常");
                        return false;
                    }
                }

                if ((cntLine == 1))
                {
                    prob = int.Parse(lineDecode);
                    if ((prob >= 0) && (prob <= 100))
                    {

                    }
                    else
                    {
                        Console.WriteLine("第二行檢查異常");
                        return false;
                    }
                }

                if ((cntLine == 2))
                {
                    seed = int.Parse(lineDecode);
                    if ((seed >= 19940706) && (seed <= 20201109))
                    {

                    }
                    else
                    {
                        Console.WriteLine("第三行檢查異常");
                        return false;
                    }
                }

                if ((cntLine == 3))
                {
                    lotteryMsg = lineDecode;
                    if (lotteryMsg != null)
                    {

                    }
                    else
                    {
                        Console.WriteLine("第四行檢查異常");
                        return false;
                    }
                }
            }


            DateTime nowNotice = DateTime.Now;
            if ((lastNotice.Date == nowNotice.Date)
                && (lastNotice.Hour == nowNotice.Hour)
                && (lastNotice.Minute == nowNotice.Minute))
            {
                Console.WriteLine("此notice已顯示過");
                return false;
            }
                ShowLotteryWindow();
            if (lotteryWindow != null)
            {
                lotteryWindow.SetParameter(prob, seed, userName);
                lotteryWindow.SetLotteryMsg(lotteryMsg);
                string lotteryPasswordInput = nowStringTime + userName;
                string lotteryPasswordOutput = EnDeCode.EncryptString(lotteryPasswordInput, key);
                Console.WriteLine("樂透密碼: " + lotteryPasswordInput + "\t" + lotteryPasswordOutput);
                lotteryWindow.SetPrizeAcPw(userName, lotteryPasswordOutput);
            }
            return true;
        }

        private List<string> GdiskHandle()
        {
            try
            {
                string momentFolderName = rootFolderName + enCodeTime + @"\";
                bool isFolderExist = EnDeCode.IfFolderExist(momentFolderName);
                if (isFolderExist)
                {
                    Console.WriteLine("找到了資料夾: " + momentFolderName);

                }
                else
                {
                    Console.WriteLine("資料夾不存在: " + momentFolderName);
                    return null;
                }

                string momentFileName = EnDeCode.EncryptString(enCodeTime, key);
                List<string> momentString = EnDeCode.CheckFileAndGetString(momentFolderName + momentFileName + ".txt");
                
                return momentString; 

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static string ShowInputDialog(string prompt, string title = "輸入文字")
        {
            string defaultValue = DateTime.Now.ToString("yyMMdd_HHmm");
            return Interaction.InputBox(prompt, title, defaultValue);
        }

        public List<string> ShowFourInputs()
        {
            // 建立視窗
            Window win = new Window
            {
                Title = "輸入四個字串",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // 建立 StackPanel
            StackPanel panel = new StackPanel { Margin = new Thickness(10) };


            // 建立四個輸入框 + 標題
#if false
            TextBox tb1 = new TextBox { Margin = new Thickness(5), Text = "LOTTERY" + EnDeCode.GetNowString(false)};
            TextBox tb2 = new TextBox { Margin = new Thickness(5), Text = "7" };
            TextBox tb3 = new TextBox { Margin = new Thickness(5), Text = "20201109" };
            TextBox tb4 = new TextBox { Margin = new Thickness(5), Text = "抽點什麼但項目不明" };
#else
            TextBox tb1 = new TextBox { Margin = new Thickness(5), Text = "LEADER" + EnDeCode.GetNowString(false) };
            TextBox tb2 = new TextBox { Margin = new Thickness(5), Text = rootFolderName + @"luju\adv.jpg" };
            TextBox tb3 = new TextBox { Margin = new Thickness(5), Text = "傳教時間" };
            TextBox tb4 = new TextBox { Margin = new Thickness(5), Text = "抽點什麼但項目不明" };
#endif
            panel.Children.Add(new TextBlock { Text = "檢碼", Margin = new Thickness(5) });
            panel.Children.Add(tb1);
            panel.Children.Add(new TextBlock { Text = "機率", Margin = new Thickness(5) });
            panel.Children.Add(tb2);
            panel.Children.Add(new TextBlock { Text = "種子", Margin = new Thickness(5) });
            panel.Children.Add(tb3);
            panel.Children.Add(new TextBlock { Text = "項目", Margin = new Thickness(5) });
            panel.Children.Add(tb4);

            // 確定按鈕
            Button okButton = new Button
            {
                Content = "確定",
                Width = 80,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => { win.DialogResult = true; };

            panel.Children.Add(okButton);
            win.Content = panel;

            // 顯示視窗
            bool? result = win.ShowDialog();

            if (result == true)
            {
                return new List<string> { tb1.Text, tb2.Text, tb3.Text, tb4.Text };
            }
            else
            {
                return null; // 使用者取消
            }
        }

        private void GeneratePasswordFile()
        {
            string inputTime = EnDeCode.GetBasicString() + ShowInputDialog("yyMMdd_HHmm");
            string _enCodeTime = EnDeCode.EncryptString(inputTime, key);
            string momentFolderName = rootFolderName + _enCodeTime + @"\";
            bool isFolderExist = EnDeCode.IfFolderExist(momentFolderName);

            // 建立資料夾
            Directory.CreateDirectory(momentFolderName);
            Console.WriteLine("資料夾已建立或已存在: " + momentFolderName);

            // 自行輸入input
            List<string> linesOri = ShowFourInputs();
            string momentFileName = EnDeCode.EncryptString(_enCodeTime, key);
            linesOri.Add(inputTime + "\t" + _enCodeTime);
            linesOri.Add(momentFolderName + "\t");
            File.WriteAllLines(momentFolderName + _enCodeTime + "(" + inputTime + ")" + ".txt", linesOri);
            
            List<string> linesEncode = new List<string>();
            foreach (string line in linesOri)
            {
                linesEncode.Add(EnDeCode.EncryptString(line, key));
            }
            File.WriteAllLines(momentFolderName + momentFileName + ".txt", linesEncode);
        }

        private void SaveLog(LOG_TYPE logType)
        {
            string inputName = "TestLog" + DateTime.Now.ToString("yyMMdd");
            string logFolderName = rootFolderName + inputName + @"\";
            bool isFolderExist = EnDeCode.IfFolderExist(logFolderName);

            
            List<string> lines = new List<string>();
            string userNameEncode = EnDeCode.EncryptString(userName, key);
            string timeEncode = EnDeCode.EncryptString(DateTime.Now.ToString("yyMMdd_HHmmss"), key);
            string logTypeEncode = EnDeCode.EncryptString(logType.ToString(), key);

            lines.Add(userNameEncode + "\t" + timeEncode + "\t" + logTypeEncode);

            string logFileName = logFolderName + userNameEncode + ".txt";

            if (isFolderExist)
            {
                File.AppendAllLines(logFileName, lines);
            }
            else
            {
                return;
            }
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
#if DEBUG
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
#endif
            MenuItem exitItem = new MenuItem { Header = "結束滷豬" };
            exitItem.Click += (s, args) =>
            {
                SaveLog(LOG_TYPE.CLOSE_PET);
                Application.Current.Shutdown();
            };
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
                    SaveConfig();
                    LoadConfig();
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
#if DEBUG
            MenuItem advTestItem = new MenuItem { Header = "廣告測試" };
            advTestItem.Click += (s, args) =>
            {
                NoticeWindow.ShowNotice("C:\\簡報資料公用區\\暫存區\\adv.png", "滷豬滷豬真可愛");
            };
            menu.Items.Add(advTestItem);

            MenuItem enCodeItem = new MenuItem { Header = "密碼測試" };
            enCodeItem.Click += (s, args) =>
            {
                EncodeDecodeWindow encodeDecodeWindow = new EncodeDecodeWindow(key);
                if (encodeDecodeWindow.ShowDialog() == true)
                {

                }
            };
            menu.Items.Add(enCodeItem);

            MenuItem deCodeItem = new MenuItem { Header = "生成密碼" };
            deCodeItem.Click += (s, args) =>
            {
                GeneratePasswordFile();
            };
            menu.Items.Add(deCodeItem);
#endif
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
