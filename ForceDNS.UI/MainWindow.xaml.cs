using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using System.Windows.Shapes;
using ForceDNS.Common;

namespace ForceDNS.UI
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WCFClient WCFClient;

        StatusType _currentStatus;
        public StatusType CurrentStatus
        {
            get
            {
                return _currentStatus;
            }
            set
            {

                if (value == StatusType.Loading || value == StatusType.Running)
                {
                    ToogleInput(false);
                }
                else
                {
                    ToogleInput(true);
                }

                _currentStatus = value;
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            DnsList.ItemsSource = Enum.GetValues(typeof(DnsProvider)).Cast<DnsProvider>().ToList();

            DnsList.SelectedIndex = 0;

            InitializeWCFClient();

            InitializeAppStatus();

            this.Title += " V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        private void InitializeWCFClient()
        {
            BasicHttpBinding binding = new BasicHttpBinding();

            EndpointAddress address = new EndpointAddress("http://localhost:8022/FDService");

            WCFClient = new WCFClient(binding, address);
        }

        private void ToogleInput(Boolean enabled)
        {
            ButtonSaveSettings.IsEnabled = enabled;
            RunUntil.IsEnabled = enabled;
            TxtPrimaryDns.IsEnabled = enabled;
            TxtSecondaryDns.IsEnabled = enabled;
            DnsList.IsEnabled = enabled;
            ComboCriticalNo.IsEnabled = enabled;
            ComboCriticalYes.IsEnabled = enabled;
            ViewUninstall.IsEnabled = enabled;
        }

        public enum StatusType
        {
            Loading,
            NotRunning,
            Running
        }

        private async void InitializeAppStatus()
        {

            CurrentStatus = StatusType.Loading;

            this.Dispatcher.Invoke(() =>
            {
                SettingsEllipse.Fill = Brushes.Gray;
                SettingsMessage.Text = "loading...";

                AppStatusEllipse.Fill = Brushes.Gray;
                AppStatusMessage.Text = "loading...";
            });
            SettingsDto settings;
            try
            {
                settings = WCFClient.GetSettings() as SettingsDto;
            }
            catch (Exception ex)
            {
                throw;
            }


            //Display Settings in UI
            this.Dispatcher.Invoke(() =>
            {
                RunUntil.Value = settings.RunUntil;

                if (settings.Unkillable)
                {
                    ComboCriticalYes.IsChecked = true;
                    ComboCriticalNo.IsChecked = false;
                }
                else
                {
                    ComboCriticalNo.IsChecked = true;
                    ComboCriticalYes.IsChecked = false;
                }
            });

            var statusResponse = await BusinessLayer.AppStatusService.CheckStatus(settings);

            if (statusResponse.Interval.HasValue == false)
            {
                CurrentStatus = StatusType.NotRunning;

                this.Dispatcher.Invoke(() =>
                {
                    SettingsEllipse.Fill = Brushes.Green;
                    SettingsMessage.Text = "you can modify your settings";

                    AppStatusEllipse.Fill = Brushes.Green;
                    AppStatusMessage.Text = "ForceDNS is not running";
                });
            }
            else
            {
                CurrentStatus = StatusType.Running;

                this.Dispatcher.Invoke(() =>
                {
                    if (statusResponse.Status == BusinessLayer.AppStatus.UNKNOWN)
                    {
                        SettingsEllipse.Fill = Brushes.Yellow;
                        SettingsMessage.Text = "cannot retrieve current date and time, please reconnect and wait a few seconds";
                    }
                    else if (statusResponse.Status == BusinessLayer.AppStatus.DNSON)
                    {
                        SettingsEllipse.Fill = Brushes.Red;
                        SettingsMessage.Text = "ForceDNS is ON!";
                    }
                    else
                    {
                        SettingsEllipse.Fill = Brushes.Green;
                        SettingsMessage.Text = "ForceDNS is OFF!";
                    }

                    AppStatusEllipse.Fill = Brushes.Red;
                    AppStatusMessage.Text = $"you can't modify your settings until {settings.RunUntil:G}";
                });
            }
        }

        private void ButtonSaveSettings_Click(Object sender, RoutedEventArgs e)
        {
            if (InputSettingsAreValid() == false)
                return;

            var mbResult = MessageBox.Show($"Are you sure you want to force the selected DNS on this computer until {RunUntil.Value.Value:G}?", "Confirm Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (mbResult != MessageBoxResult.Yes)
                return;

            WCFClient.SaveSettings(new SettingsDto()
            {
                RunUntil = RunUntil.Value.Value,
                TimeZoneID = TimeZoneInfo.Local.Id,
                Unkillable = ComboCriticalYes.IsChecked.Value,
                PrimaryDns = TxtPrimaryDns.Address,
                SecondaryDns = TxtSecondaryDns.Address,
                DnsProvider = (DnsProvider)DnsList.SelectedIndex
            });

            InitializeAppStatus();

            WCFClient.SettingsChanged();

            MessageBox.Show($"In order for changes to take effect, please clean all your browser's cache and restart your router", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewUninstall_Selected(Object sender, RoutedEventArgs e)
        {
            if (CurrentStatus != StatusType.NotRunning)
                return;

            //TODO Uninstall 

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/im ForceDNS.Service.exe /f /t",
                    CreateNoWindow = true,
                    Verb = "runas",
                    UseShellExecute = true
                }).WaitForExit();

            }
            catch
            {
                return;
            }

            try
            {

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = "/X{24F0FF75-35AD-4DE5-B67A-E87E327A5A3D}",
                    CreateNoWindow = true,
                    Verb = "runas",
                    UseShellExecute = false
                });
            }
            catch 
            {

            }


            Environment.Exit(0);
        }

        private void ViewInfo_Selected(Object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/sarveshh/ForceDNS");
        }

        private Boolean InputSettingsAreValid()
        {
            if (RunUntil.Value.HasValue == false)
            {
                RunUntil.Focus();
                MessageBox.Show("please select an end date", "Invalid Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (RunUntil.Value.Value < DateTime.Now)
            {
                RunUntil.Focus();
                MessageBox.Show("end date must be greater than today", "Invalid Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if(DnsList.SelectedIndex == 1 && TxtPrimaryDns.Address == "...")
            {
                MessageBox.Show("Add at least a primary DNS", "Invalid Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private void DnsList_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if(DnsList.SelectedIndex == 0)
            {
                TxtPrimaryDns.Address = "208.67.222.123";
                TxtSecondaryDns.Address = "208.67.220.123";
            }
            else
            {
                TxtPrimaryDns.Address = "...";
                TxtSecondaryDns.Address = "...";
            }
        }
        Boolean testing = false;
        private void ButtonTestSettings_Click(Object sender, RoutedEventArgs e)
        {
            if (InputSettingsAreValid() == false)
                return;


            testing = testing == false;

            if (testing)
            {
                ButtonTestSettings.Content = "Revert Settings";

                WCFClient.ApplyDns(TxtPrimaryDns.Address, TxtSecondaryDns.Address);

                MessageBox.Show($"Please clean all your browser's cache, restart your router and then press OK", "Test Settings", MessageBoxButton.OK, MessageBoxImage.Information);

                //if opendns
                if (DnsList.SelectedIndex == 0)
                {
                    System.Diagnostics.Process.Start("https://welcome.opendns.com");

                }
                else
                {
                    MessageBox.Show($"You can now test your custom dns settings", "Test Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                ButtonTestSettings.Content = "Test Settings";

                WCFClient.RemoveDns();

                MessageBox.Show($"Please clean all your browser's cache, restart your router and then press OK", "Test Settings", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }
    }
}
