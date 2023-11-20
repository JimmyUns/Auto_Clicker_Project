using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace AutoClickerProject
{
    public partial class MainWindow : Window
    {   
        private enum mouseeventFlags
        {
            leftDown = 2,
            leftUp = 4,
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        private DispatcherTimer clickTimer = new DispatcherTimer();

        private bool isClicking;

        public MainWindow()
        {
            InitializeComponent();
            stopButton_Click(this, null);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {   if (isClicking) return;
            isClicking = true;
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            clickTimer = new DispatcherTimer();
            clickTimer.Interval = TimeSpan.FromSeconds(1);
            clickTimer.Tick += Click_Timer_Tick;
            clickTimer.Start();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            clickTimer.Stop();
            isClicking = false;
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
        }


        private void Click_Timer_Tick(object? sender, EventArgs e)
        {
            Point relativePosition = Mouse.GetPosition(this);
            Point screenPosition = PointToScreen(relativePosition);
            LeftClick(screenPosition);
        }


        private void LeftClick(Point p)
        {
            mouse_event((int)mouseeventFlags.leftDown, (int)p.X, (int)p.Y, 0, 0);
            mouse_event((int)mouseeventFlags.leftUp, (int)p.X, (int)p.Y, 0, 0);
        }

    }
}