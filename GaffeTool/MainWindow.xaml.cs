using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Text.Json;
using System.Configuration;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace ControlPanel
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

        private async void Window_Initialized(object sender, EventArgs e)
        {
            StatusLabel.Visibility = Visibility.Hidden;
            if (Utility.isBuildAccess)
                GameMenu.Visibility = Visibility.Visible;
            else
                GameMenu.Visibility = Visibility.Collapsed;
            GaffeButton.IsEnabled = false;
            GaffeDropDown.ItemsSource = new List<string>() { "Loading..." };
            GaffeDropDown.SelectedIndex = 0;
            GaffeDropDown.ItemsSource = await Utility.GetGaffeNameList();
            GaffeDropDown.SelectedIndex = 0;
            if (GaffeDropDown.Items.Count < 2)
                GaffeButton.Content = "Load Gaffes";
            else
                GaffeButton.Content = "Set Gaffe";
            GaffeButton.IsEnabled = true;
        }

        private async void GaffeButton_Click(object sender, RoutedEventArgs e)
        {
            GaffeButton.IsEnabled = false;
            //Load Gaffes
            if (GaffeDropDown.Items.Count < 2)
            {
                GaffeDropDown.ItemsSource = new List<string>() { "Loading..." };
                GaffeDropDown.SelectedIndex = 0;
                GaffeDropDown.ItemsSource = await Utility.GetGaffeNameList();
                GaffeDropDown.SelectedIndex = 0;
                if (GaffeDropDown.Items.Count < 2)
                    GaffeButton.Content = "Load Gaffes";
                else
                    GaffeButton.Content = "Set Gaffe";
                GaffeButton.IsEnabled = true;
                return;
            }

            //Set Gaffes
            if (GaffeDropDown.SelectedIndex > 0)
            {
                string gaffeName = GaffeDropDown.SelectedItem.ToString();
                Utility.PostAsync(CreateRequestBody.Gaffe(gaffeName), StatusLabel);
            }
            else
            {
                Utility.DisplayToast(StatusLabel, "Select a gaffe!");
            }
            GaffeButton.IsEnabled = true;
        }

        private void CustomNumbersButton_Click(object sender, RoutedEventArgs e)
        {
            string randomNumbers = CustomNumbersTextBox.Text;
            if (!string.IsNullOrWhiteSpace(randomNumbers))
            {
                List<object> numberList = randomNumbers.Split(',').Select(e => e.Trim()).Select(e => int.TryParse(e, out int o) ? (object)o : (object)e).ToList();
                var x = CreateRequestBody.CustomRandomNumbers(numberList);
                Utility.PostAsync(CreateRequestBody.CustomRandomNumbers(numberList), StatusLabel);
            }
            else
            {
                Utility.DisplayToast(StatusLabel, "Enter random numbers!");
            }
        }

        private void ClearNumbersButton_Click(object sender, RoutedEventArgs e)
        {
            Utility.PostAsync(CreateRequestBody.ClearRandomNumberQueue(), StatusLabel);
            CustomNumbersTextBox.Text = string.Empty;
            GaffeDropDown.SelectedIndex = 0;
        }

        private void StartAppButton_Click(object sender, RoutedEventArgs e)
        {
            Utility.StartApps(StatusLabel);
        }

        private void ClearRAMButton_Click(object sender, RoutedEventArgs e)
        {
            int closedCount = Utility.CloseApps();
            Utility.ClearStorage(StatusLabel);
            if (closedCount > 0)
                Utility.StartApps(StatusLabel);
        }

        private void CloseAppButton_Click(object sender, RoutedEventArgs e)
        {
            Utility.CloseApps();
        }

        private void GaffeHelpTextBlock_Initialized(object sender, EventArgs e)
        {
            GaffeHelpTextBlock.Text = Utility.GetGaffeHelpText();
        }

        private void IDTextBox_Initialized(object sender, EventArgs e)
        {
            IDTextBox.Text = ConfigurationManager.AppSettings.Get("BackendID");
        }

        private void IDUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["BackendID"].Value = IDTextBox.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Window_Initialized(this, null);
        }
    }
}
