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

    public enum PET_STATUS
    {
        DEFAULT,
        DINNER,
        LUNCH,
        OVERTIME,
        SLEEP,
        DOZE,
        WORK,
        WALK,
        KUAI,
        END,

        SHOCKED,
        DRAG,        
        GOOD_APPLE,
        BAD_APPLE,
        COOK,
        COOK_FA,
        COOK_CRAB,
    }

    public class PetConfig
    {
        public double PetSize { get; set; } = 0.4;
        public bool AllowWalk { get; set; } = true;
        public bool AllowLunch { get; set; } = true;
        public bool AllowDinner { get; set; } = true;
        public bool AllowWork { get; set; } = true;       // 允許工作
        public bool AllowDoze { get; set; } = true;       // 允許瞌睡
        public bool AllowOverTime { get; set; } = true;   // 允許加班
        public bool AllowMad { get; set; } = true;        // 允許發瘋
        public bool AllowLeader { get; set; } = true;     // 允許教主
        public bool AllowUpdate { get; set; } = true;     // 允許更新
        public bool AllowAdvertise { get; set; } = true;  // 允許廣告
        public bool AllowIt { get; set; } = true;         // 哀踢通知
        public bool AllowHr { get; set; } = true;         // 人資通知
        public bool AllowAdm { get; set; } = true;        // 管理通知

        public int LunchStartHour { get; set; } = 11;
        public int LunchStartMinute { get; set; } = 45;
        public int LunchEndHour { get; set; } = 13;
        public int LunchEndMinute { get; set; } = 0;

        public int DinnerStartHour { get; set; } = 14;
        public int DinnerStartMinute { get; set; } = 20;
        public int DinnerEndHour { get; set; } = 15;
        public int DinnerEndMinute { get; set; } = 30;
    }
}
