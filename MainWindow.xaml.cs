using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SecureAppVault.Models;
using SecureAppVault.Services;
using SecureAppVault.ViewModels;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SecureAppVault
{
    public partial class MainWindow : Window
    {
        private readonly WhitelistService _whitelistService;
        private readonly TwoFactorAuthService _twoFactorAuthService;
        private readonly DispatcherTimer _monitorTimer;
        private NotifyIcon _notifyIcon = null!;

        public MainWindow()
        {
            InitializeComponent();
            _monitorTimer = new DispatcherTimer();
            var encryptionKey = "xbpz6ZVGvrsF3F6BZY9UeNQWg1BKheOO";
            DataContext = new MainViewModel(encryptionKey);
            _whitelistService = new WhitelistService(encryptionKey);
            _twoFactorAuthService = new TwoFactorAuthService();

            if (!_twoFactorAuthService.ConfigExists())
            {
                ShowConfigCode();
            }

            StartMonitoring();
            Closing += OnWindowClosing; // Add event handler for window closing
            StateChanged += OnStateChanged; // Add event handler for state change

            InitializeNotifyIcon();
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/Icon.ico"),
                Visible = true,
                Text = "SecureAppVault"
            };
            _notifyIcon.DoubleClick += (s, e) => RestoreWindow();
        }

        private void RestoreWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            _notifyIcon.Visible = false;
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _notifyIcon.Visible = true;
            }
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the closing event
            Hide();
            _notifyIcon.Visible = true;
        }

        private void StartMonitoring() {
            _monitorTimer.Interval = TimeSpan.FromSeconds(1); // Check every X seconds
            _monitorTimer.Tick += MonitorApplications;
            _monitorTimer.Start();
        }

        private void MonitorApplications(object? sender, EventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            var appNamesToTerminate = new HashSet<string>();

            foreach (var app in viewModel.WhitelistedApps)
            {
                if (!app.IsRunningFromVault)
                {
                    appNamesToTerminate.Add(Path.GetFileNameWithoutExtension(app.Path));
                }
            }

            var runningProcesses = Process.GetProcesses();
            foreach (var process in runningProcesses)
            {
                if (appNamesToTerminate.Contains(process.ProcessName))
                {
                    try
                    {
                        process.Kill();
                        Debug.WriteLine($"Terminated process {process.ProcessName} with ID {process.Id}");
                        MessageBox.Show($"Terminated process {process.ProcessName} with ID {process.Id}\nRun through the vault", "Process Terminated", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to terminate process {process.ProcessName} with ID {process.Id}: {ex.Message}");
                    }
                }
            }
        }

        private void AddAppButton_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                var appName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                var appPath = openFileDialog.FileName;

                var newApp = new WhitelistedApp(appName, appPath);

                var viewModel = (MainViewModel)DataContext;
                viewModel.WhitelistedApps.Add(newApp);
                _whitelistService.SaveWhitelist(viewModel.WhitelistedApps.ToList());
            }
        }

        private void RemoveAppButton_Click(object sender, RoutedEventArgs e) {
            var viewModel = (MainViewModel)DataContext;
            var selectedApp = (WhitelistedApp)AppListBox.SelectedItem;
            if (selectedApp != null) {
                var inputBox = new InputBox("Enter 2FA Code:");
                if (inputBox.ShowDialog() == true) {
                    var code = inputBox.InputText;
                    if (_twoFactorAuthService.VerifyCode(code)) {
                        viewModel.WhitelistedApps.Remove(selectedApp);
                        _whitelistService.SaveWhitelist(viewModel.WhitelistedApps.ToList());
                    } else {
                        MessageBox.Show("Invalid 2FA Code.");
                    }
                }
            }
        }

        private void RunAppButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedApp = (WhitelistedApp)AppListBox.SelectedItem;
            if (selectedApp != null)
            {
                var appPath = selectedApp.Path;
                if (File.Exists(appPath))
                {
                    try
                    {
                        selectedApp.IsRunningFromVault = true;
                        var processStartInfo = new ProcessStartInfo(appPath)
                        {
                            UseShellExecute = true
                        };
                        var process = Process.Start(processStartInfo);
                        if (process != null)
                        {
                            process.EnableRaisingEvents = true;
                            process.Exited += (s, args) => selectedApp.IsRunningFromVault = false;
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show($"Error starting application: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("The specified application file does not exist.");
                }
            }
        }

        private void UnlockVaultButton_Click(object sender, RoutedEventArgs e) {
            var viewModel = (MainViewModel)DataContext;
            if (viewModel.IsUnlocked) {
                viewModel.IsUnlocked = false;
            } else {
                var inputBox = new InputBox("Enter 2FA Code:");
                if (inputBox.ShowDialog() == true) {
                    var code = inputBox.InputText;
                    if (_twoFactorAuthService.VerifyCode(code)) {
                        viewModel.IsUnlocked = true;
                    } else {
                        MessageBox.Show("Invalid 2FA Code.");
                    }
                }
            }
        }

        private void ShowConfigCode()  {
            var qrCodeBytes = _twoFactorAuthService.GenerateQrCode("SecureAppVault");

            var qrCodeImage = new BitmapImage();
            using (var ms = new MemoryStream(qrCodeBytes)) {
                qrCodeImage.BeginInit();
                qrCodeImage.CacheOption = BitmapCacheOption.OnLoad;
                qrCodeImage.StreamSource = ms;
                qrCodeImage.EndInit();
            }

            var qrCodeWindow = new Window {
                Title = "QR Code",
                Content = new System.Windows.Controls.Image { Source = qrCodeImage }, // Use the correct Image class
                Width = 300,
                Height = 300
            };
            qrCodeWindow.ShowDialog();
        }
    }
}