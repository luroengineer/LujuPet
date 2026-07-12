using System;
using System.Text;
using System.Windows;

namespace LujuPet
{
    public partial class EncodeDecodeWindow : Window
    {
        public string key = "";

        public EncodeDecodeWindow(string _key)
        {
            InitializeComponent();
            this.key = _key;
            Label_key.Content = key;
        }

        private void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            string input = EncodeInputBox.Text;
            if (!string.IsNullOrEmpty(input))
            {
                // 這裡用 Base64 當範例編碼
                string encoded = EnDeCode.EncryptString(input, "pig");
                EncodeOutputBox.Text = encoded;
            }
        }

        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            string input = DecodeInputBox.Text;
            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    string decoded = EnDeCode.DecryptString(input, "pig");
                    DecodeOutputBox.Text = decoded;
                }
                catch
                {
                    DecodeOutputBox.Text = "解碼失敗";
                }
            }
        }
    }
}
