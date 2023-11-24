using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace AutoClickerProject
{
    public partial class MainWindow : Window
    {
        private string defaulttextForeground = "#e1e1e1", disabledtextForeground = "#252523";
        private enum mouseeventFlags
        {
            leftDown = 2,
            leftUp = 4,
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private DispatcherTimer clickTimer = new DispatcherTimer();

        private int randomintervalPercentage, clickInterval, randClickInterval;

        private bool isClicking, isRandom, isLocationSet, hasClickLimit;
        private int currentClickTime;
        public Point setlocationPoint = new Point(404, 404); //for this var, 404,404 means its null
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            stopButton_Click(this, null);   
            HideOrShowPickLocation(false);
            HideOrShowClickRepeat(false);
            ManageClickInterval();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {   
            if (isClicking) return;
            if (clickrepeatTextBox.Text == "0" && hasClickLimit) { stopButton_Click(this, null); return; }
            isClicking = true;
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            clickInterval = Int32.Parse(hourInput.Text) * 3600000 + Int32.Parse(minuteInput.Text) * 60000 + Int32.Parse(secondInput.Text) * 1000 + Int32.Parse(millisecondInput.Text);

            clickTimer = new DispatcherTimer();
            clickTimer.Interval = TimeSpan.FromMilliseconds(clickInterval);
            clickTimer.Tick += Click_Timer_Tick;
            clickTimer.Start();

            if (hideonstartCheckBox.IsChecked == true) WindowState = WindowState.Minimized;
            currentClickTime = 0;
        
        }


        private void stopButton_Click(object sender, RoutedEventArgs? e)
        {
            clickTimer.Stop();
            isClicking = false;
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
            this.DragMove();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SetTimeInput_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            textBox.Text = textBox.Text.TrimStart(new Char[] { '0' });

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "0";
            }
        }
        private void SetTimeInput_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            if (textBox?.Text.Length > 5)
            {
                textBox.Text = textBox.Text.Substring(0, 5);
                textBox.CaretIndex = 5;
            }
        }

        private void picklocationCheckbox_Checked(object sender, RoutedEventArgs e)
        {

            HideOrShowPickLocation(true);
            if (setlocationPoint != new Point(404, 404)) //if isnt empty, since 404, 404 is null for this variable
                {
                    setlocationButton.Content = "Change location";
                } else
                {
                    setlocationButton.Content = "Set location";
                }
        }

        private void picklocationCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            HideOrShowPickLocation(false);
            isLocationSet = false;
        }

        private void setlocationButton_Click(object sender, RoutedEventArgs e)
        {
            PickLocationWindow picklocationWindow = new PickLocationWindow();
            picklocationWindow.Show();
            WindowState = WindowState.Minimized;
        }


        private void clickrepeatCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            HideOrShowClickRepeat(true);
            hasClickLimit = true;

        }

        private void optionsButton_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow();
            optionsWindow.Show();
            optionsWindow.Left = this.Left;
            optionsWindow.Top = this.Top;
            this.Hide();
        }

        private void randomintervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ManageClickInterval();
        }

        private void clickrepeatCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            HideOrShowClickRepeat(false);
            hasClickLimit = false;
        }

        private void Click_Timer_Tick(object? sender, EventArgs e)
        {
            Point relativePosition = Mouse.GetPosition(this);
            Point screenPosition = PointToScreen(relativePosition);
            LeftClick(screenPosition);

            if (isRandom)
            {
                Random r = new Random();
                int currentRandPercentage = (clickInterval * randomintervalPercentage) / 100;
                randClickInterval = r.Next(clickInterval - currentRandPercentage, clickInterval + currentRandPercentage);
                clickTimer.Interval = TimeSpan.FromMilliseconds(randClickInterval);
                Console.WriteLine(randClickInterval.ToString());
            }
        }


        private void LeftClick(Point p)
        {
            currentClickTime++;
            if (hasClickLimit) if (currentClickTime == Int32.Parse(clickrepeatTextBox.Text)) stopButton_Click(this, null);
            if (isLocationSet) SetCursorPos((int)setlocationPoint.X, (int)setlocationPoint.Y);
            mouse_event((int)mouseeventFlags.leftDown, (int)p.X, (int)p.Y, 0, 0);
            mouse_event((int)mouseeventFlags.leftUp, (int)p.X, (int)p.Y, 0, 0);
        }

        public void SetNewClickLocation(Point newPoint)
        {
            setlocationPoint = newPoint;
            setlocationButton.Content = "Change";
            picklocationTextBlock.Text = "X: " + newPoint.X.ToString() + "   Y: " + newPoint.Y.ToString();
            isLocationSet = true;
        }

        private void HideOrShowPickLocation(bool state)
        {
            setlocationButton.IsEnabled = state;
            if (state)
            {
                picklocationTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(defaulttextForeground));
            } else
            {
                picklocationTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(disabledtextForeground));

            }
        }
        private void HideOrShowClickRepeat(bool state)
        {
            clickrepeatTextBox.IsEnabled = state;
            if (state)
            {
                clickrepeatLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(defaulttextForeground));
            }
            else
            {
                clickrepeatLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(disabledtextForeground));
            }
        }

        private void ManageClickInterval()
        {
            randomintervalPercentage = (int)randomintervalSlider.Value * 10;
            randomintervalTextBlock.Text = randomintervalPercentage.ToString() + "%";
            isRandom = randomintervalSlider.Value > 0 ? true : false;
        }
    }
}