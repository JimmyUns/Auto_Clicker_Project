using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace AutoClickerProject
{
    public partial class MainWindow : Window
    {
        //to execute left click input
        private enum mouseeventFlags
        {
            leftDown = 2,
            leftUp = 4,
            rightDown = 8,
            RightUp = 16,
        }
        //the timer that handles clicking
        private DispatcherTimer clickTimer = new DispatcherTimer();
        //clickInterval is the original clicking interval
        private int randomintervalPercentage, clickInterval, randClickInterval;
        //enablers for different options
        private bool isClicking, isRandom, isLocationSet, hasClickLimit;
        //the amount of clicks that has been clicked since start
        private int currentClickTime;
        //for this var, 404,404 means its null
        public Point setlocationPoint = new Point(404, 404);

        //color values for texts
        private string defaulttextForeground = "#e1e1e1", disabledtextForeground = "#121212";

        #region Dll Import Region
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        #endregion

        #region Window Open and Close Region
        public MainWindow()
        {
            InitializeComponent();
            LoadValues();
            Topmost = true;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            stopButton_Click(this, null);
            HideOrShowPickLocation(picklocationCheckbox.IsChecked == true);
            HideOrShowClickRepeat(clickrepeatCheckbox.IsChecked == true);
            ManageClickInterval(); //Sets the random interval slider to its proper % value
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveValues();
        }

        #endregion

        #region Event Argumented Region
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void startButton_Click(object sender, RoutedEventArgs? e)
        {
            if (isClicking) return;
            if (clickrepeatTextBox.Text == "0" && hasClickLimit) {stopButton_Click(this, null); return;}

            if(clickmodeTabControl.SelectedIndex == 0)
            {
                clickInterval = Int32.Parse(hourInput.Text) * 3600000 + Int32.Parse(minuteInput.Text) * 60000 + Int32.Parse(secondInput.Text) * 1000 + Int32.Parse(millisecondInput.Text);
                if (clickInterval <= 0) return;
            } else if (clickmodeTabControl.SelectedIndex == 0)
            {
                clickInterval = Int32.Parse(cpsInput.Text) * 1000;
                if (clickInterval <= 0) return;
            }
            isClicking = true;
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;


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
            if (e.LeftButton == MouseButtonState.Pressed)
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

            if (textBox?.Text.Length > 4)
            {
                textBox.Text = textBox.Text.Substring(0, 4);
                textBox.CaretIndex = 4;
            }
        }

        private void picklocationCheckbox_Checked(object sender, RoutedEventArgs e)
        {

            HideOrShowPickLocation(true);
            if (setlocationPoint != new Point(404, 404)) //if isnt empty, since 404, 404 is null for this variable
            {
                setlocationButton.Content = "Change location";
            }
            else
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

        private void randomintervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ManageClickInterval();
        }

        private void clickrepeatCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            HideOrShowClickRepeat(false);
            hasClickLimit = false;
        }

        private void hidefromtaskbarButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = false;
        }
        private void hidefromtaskbarButton_UnChecked(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = true;
        }

        private void changeshortcutButton_Click(object sender, RoutedEventArgs e)
        {
            shortcutLabel.Content = "Press key";
        }

        private void saveButton_Click(object sender, RoutedEventArgs? e)
        {
            save_on_exit = !save_on_exit;
            if (save_on_exit)
            {
                saveButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008000"));
            }
            else
            {
                saveButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
            }
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

        #endregion

        #region Custom Functions Region
        private void LeftClick(Point p)
        {
            currentClickTime++;
            if (hasClickLimit) if (currentClickTime == Int32.Parse(clickrepeatTextBox.Text)) stopButton_Click(this, null);
            if (isLocationSet) SetCursorPos((int)setlocationPoint.X, (int)setlocationPoint.Y);
            if (leftclickRadio.IsChecked == true)
            {
                mouse_event((int)mouseeventFlags.leftDown, (int)p.X, (int)p.Y, 0, 0);
                mouse_event((int)mouseeventFlags.leftUp, (int)p.X, (int)p.Y, 0, 0);
            } else if (rightclickRadio.IsChecked == true)
            {
                mouse_event((int)mouseeventFlags.rightDown, (int)p.X, (int)p.Y, 0, 0);
                mouse_event((int)mouseeventFlags.RightUp, (int)p.X, (int)p.Y, 0, 0);
            }
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
            }
            else
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
                clickrepeatTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(defaulttextForeground));

            }
            else
            {
                clickrepeatLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(disabledtextForeground));
                clickrepeatTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(disabledtextForeground));

            }
        }

        private void ManageClickInterval()
        {
            randomintervalPercentage = (int)randomintervalSlider.Value * 10;
            randomintervalTextBlock.Text = randomintervalPercentage.ToString() + "%";
            isRandom = randomintervalSlider.Value > 0 ? true : false;
        }

        #endregion

        #region Save and Load Region
        private bool save_on_exit = false;

        private void SaveValues()
        {
             string roamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
             string folderDir = Path.Combine(roamingDir, "ModAutoClicker");
             string fileDir = Path.Combine(folderDir, "config.txt");

            if (!Directory.Exists(folderDir))
            {
                Directory.CreateDirectory(folderDir);
            }
           
            string savefileText = "save_on_exit=" + save_on_exit
                + "\nclick_mode_tab_control=" + clickmodeTabControl.SelectedIndex
                + "\nhour_input=" + hourInput.Text
                + "\nminute_input=" + minuteInput.Text
                + "\nsecond_input=" + secondInput.Text
                + "\nmillisecond_input=" + millisecondInput.Text
                + "\ncps=" + cpsInput.Text

                + "\npick_location_cb=" + picklocationCheckbox.IsChecked
                + "\npick_location_point=" + setlocationPoint

                + "\nclick_repeat_cb=" + clickrepeatCheckbox.IsChecked
                + "\nclick_repeat_input=" + clickrepeatTextBox.Text

                + "\nrandom_interval_slider=" + (int)randomintervalSlider.Value

                + "\nhide_from_taskbar_cb=" + hidefromtaskbarCheckBox.IsChecked
                + "\nhide_on_start=" + hideonstartCheckBox.IsChecked
                +"\nleft_click_radio=" + leftclickRadio.IsChecked
                + "\nright_click_radio=" + rightclickRadio.IsChecked;

            File.WriteAllText(fileDir, savefileText);
        }

        private void LoadValues()
        {
            string roamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderDir = Path.Combine(roamingDir, "ModAutoClicker");
            string fileDir = Path.Combine(folderDir, "config.txt");

            if (File.Exists(fileDir))
            {
                string configFileContent = File.ReadAllText(fileDir);

                string[] lines = configFileContent.Split('\n');

                // Check if save_on_exit is false and return without loading other settings
                if (lines.Length > 0 && lines[0].Trim().Equals("save_on_exit=false", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                } else
                {
                    saveButton_Click(this, null);
                }

                // Mapping of setting keys to corresponding assignment actions
                var settingActions = new Dictionary<string, Action<string>>
        {
            { "click_mode_tab_control", value => clickmodeTabControl.SelectedIndex = Convert.ToInt32(value) },
            { "hour_input", value => hourInput.Text = value },
            { "minute_input", value => minuteInput.Text = value },
            { "second_input", value => secondInput.Text = value },
            { "millisecond_input", value => millisecondInput.Text = value },
            { "cps", value => cpsInput.Text = value },
            { "pick_location_cb", value => picklocationCheckbox.IsChecked = Convert.ToBoolean(value) },
            { "pick_location_point", value =>
                {
                    string[] pointValues = value.Split(',');
                    if (pointValues.Length == 2)
                    {
                        double x = Convert.ToDouble(pointValues[0]);
                        double y = Convert.ToDouble(pointValues[1]);
                        setlocationPoint = new Point(x, y);
                    }
                }
            },
            { "click_repeat_cb", value => clickrepeatCheckbox.IsChecked = Convert.ToBoolean(value) },
            { "click_repeat_input", value => clickrepeatTextBox.Text = value },
            { "random_interval_slider", value => randomintervalSlider.Value = Convert.ToDouble(value) },
            { "hide_from_taskbar_cb", value => hidefromtaskbarCheckBox.IsChecked = Convert.ToBoolean(value) },
            { "npick_location_cb", value => hideonstartCheckBox.IsChecked = Convert.ToBoolean(value) },
            { "left_click_radio", value => leftclickRadio.IsChecked = Convert.ToBoolean(value) },
            { "right_click_radio", value => rightclickRadio.IsChecked = Convert.ToBoolean(value) },
        };

                for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the "save_on_exit" line
                {
                    // Split each line into key and value
                    string[] keyValue = lines[i].Split('=');

                    if (keyValue.Length == 2 && settingActions.TryGetValue(keyValue[0].Trim(), out var action))
                    {
                        action(keyValue[1].Trim());
                    }
                }
            }
            else
            {
                // Handle the case when the file doesn't exist
                Console.WriteLine("Config file not found.");
            }
        }


        #endregion

    }
}



