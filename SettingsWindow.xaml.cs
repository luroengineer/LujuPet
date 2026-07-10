using System;
using System.Windows;

namespace LujuPet
{
    public partial class SettingsWindow : Window
    {
        public PetConfig petConfig = new PetConfig();
        public SettingsWindow(PetConfig _petConfig)
        {
            InitializeComponent();

            petConfig = _petConfig;
            this.DataContext = petConfig;
            // 初始化 UI
            //SizeBox.Text = petConfig.PetSize.ToString();
            //WalkBox.IsChecked = petConfig.AllowWalk;
            //LunchBox.IsChecked = petConfig.AllowLunch;


            SaveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
 /*           if (double.TryParse(SizeBox.Text, out double newSize))
            {
                PetSize = newSize;
            }
            else
            {
                PetSize = 0.4; // 預設值
            }

            AllowWalk = WalkBox.IsChecked == true;
            AllowLunch = LunchBox.IsChecked == true;
            */
            DialogResult = true;
            Close();
        }
    }

    public class PetConfig
    {
        public double PetSize { get; set; } = 0.4;
        public bool AllowWalk { get; set; } = true;
        public bool AllowLunch { get; set; } = true;
        public bool AllowDinner { get; set; } = true;
    }
}
