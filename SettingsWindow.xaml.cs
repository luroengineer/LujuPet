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

            SaveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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
