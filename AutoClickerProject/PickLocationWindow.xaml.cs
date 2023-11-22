using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace AutoClickerProject
{
    public partial class PickLocationWindow : Window
    {
        public PickLocationWindow()
        {
            InitializeComponent();
            this.Topmost = true;

            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseMove += MainWindow_MouseMove;
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point windowPoint = e.GetPosition(this);
            var mainWindow = Application.Current.MainWindow;
            Point relativePosition = new Point(windowPoint.X, windowPoint.Y);
            Point screenPosition = PointToScreen(relativePosition);
            (mainWindow as MainWindow).WindowState = WindowState.Normal;
            (mainWindow as MainWindow).SetNewClickLocation(screenPosition);
            this.Close();
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point cursorPosition = e.GetPosition(this);
            picklocationPanel.Margin = new Thickness(cursorPosition.X, cursorPosition.Y, 0, 0);

        }
    }
}
