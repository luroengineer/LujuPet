using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LujuPet
{
    public partial class LotteryWindow : Window
    {
        private Random random = new Random();
        int winningRate = 0;
        int randomSeed = 0;
        string inputString = "";

        string prizeAccount = "";
        string prizePassword = "";

        public LotteryWindow()
        {
            InitializeComponent();
        }

        public void SetLotteryMsg(string title)
        {
            ContentText.Content = title;
        }

        public void SetPrizeAcPw(string _prizeAccount, string _prizePassword)
        {
            prizeAccount = _prizeAccount;
            prizePassword = _prizePassword;
        }

        public void SetParameter(int _winningRate, int _randomSeed, string _inputString)
        {
            winningRate = _winningRate;
            randomSeed = _randomSeed;
            inputString = _inputString;

            Console.Write("rate: " + winningRate);
            Console.Write("seed: " + randomSeed);
            Console.Write("str: " + inputString);

            bool isWinner = EnDeCode.IsWinner(winningRate, randomSeed, inputString);
            Console.WriteLine("win: " + isWinner.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 取消關閉動作
            e.Cancel = true;

            // 隱藏視窗
            this.Visibility = Visibility.Hidden;
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            bool isWinner = EnDeCode.IsWinner(winningRate, randomSeed, inputString);

            if (isWinner)
            {
                TitleText.Text = "🎉 恭喜中獎！";

                // 修改按鈕
                JoinButton.Content = "如何領獎";
                JoinButton.Click += ClaimPrize_Click;

                // 建立浮水印文字 (重複多次)
                Grid parentGrid = LotteryImage.Parent as Grid;
                if (parentGrid != null)
                {
                    // 清除舊的浮水印，避免重複疊加
                    for (int i = parentGrid.Children.Count - 1; i >= 0; i--)
                    {
                        if (parentGrid.Children[i] is TextBlock)
                            parentGrid.Children.RemoveAt(i);
                    }

                    // 在圖片上斜角重複顯示
                    for (int i = 0; i < 3; i++) // 重複 6 次
                    {
                        Color color = Color.FromArgb(60, 0, 0, 0);
                        if (i == 0)
                        {
                            color = Color.FromArgb(80, 255, 0, 0); // 半透明紅色
                        }
                        else if (i == 1)
                        {
                            color = Color.FromArgb(80, 0, 255, 0);
                        }
                        else if (i == 2)
                        {
                            color = Color.FromArgb(80, 0, 0, 255);
                        }

                        TextBlock watermark = new TextBlock
                        {
                            Text = $"中獎帳號: {prizeAccount}  \n領獎密碼: {prizePassword}",
                            Foreground = new SolidColorBrush(color),
                            FontSize = 24,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        };

                        // 加上旋轉 (斜角)
                        watermark.RenderTransform = new RotateTransform(-30);

                        // 用 Margin 讓文字分散在圖片上
                        watermark.Margin = new Thickness(0, i * 150, 0, 0);

                        parentGrid.Children.Add(watermark);
                    }
                }
            }
            else
            {
                TitleText.Text = "😢 可惜沒中！";
                JoinButton.Content = "關閉視窗";
                this.Close();
            }

            // 放棄按鈕隱藏
            GiveUpButton.Visibility = Visibility.Collapsed;
        }



        private void ClaimPrize_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("請將中獎畫面拍照並傳送至LINE:\n@lutsaitu\n(請務必拍進「中獎帳號」「領獎密碼」)", "領獎方式", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GiveUpButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
