using System.Diagnostics;
using System.IO;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        // 拖曳移動功能 + 切換 GIF
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 切換成拖曳 GIF
                ChangeGif("Images/petDrag.gif");
                this.DragMove();

                // 拖曳結束後換回原始 GIF
                ChangeGif("Images/pet.gif");
            }

            sw.Stop();

            // 判斷是否超過 0.2 秒
            if (sw.Elapsed.TotalSeconds < 0.2)
            {
                ChangeGif("Images/petClick.gif");
            }
        }


        // 右鍵點擊 → 顯示選單
        private void PetImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem exitItem = new MenuItem { Header = "結束寵物" };
            exitItem.Click += (s, args) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            menu.IsOpen = true;
        }

        // 共用方法：切換 GIF
        private void ChangeGif(string path)
        {
            var uri = new Uri($"pack://application:,,,/"+path, UriKind.Absolute);
            var image = new BitmapImage(uri);
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(PetImage, image);
        }

    }
}