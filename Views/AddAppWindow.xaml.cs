using System.Windows;
using SecureAppVault.Models;
using SecureAppVault.Services;
using MessageBox = System.Windows.MessageBox;

namespace SecureAppVault.Views
{
    public partial class AddAppWindow : Window
    {
        private readonly WhitelistService _whitelistService;

        public AddAppWindow(string encryptionKey)
        {
            InitializeComponent();
            _whitelistService = new WhitelistService(encryptionKey);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var appName = AppNameTextBox.Text;
            var appPath = AppPathTextBox.Text;

            if (!string.IsNullOrEmpty(appName) && !string.IsNullOrEmpty(appPath))
            {
                var app = new WhitelistedApp(appName, appPath); // Przekaż wymagane argumenty
                var apps = _whitelistService.LoadWhitelist();
                apps.Add(app);
                _whitelistService.SaveWhitelist(apps);

                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter both application name and path.");
            }
        }
    }
}