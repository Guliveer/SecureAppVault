using System;
using System.Windows;
using System.Windows.Input;
using SecureAppVault.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace SecureAppVault.Views
{
    public partial class TwoFactorAuthWindow : Window
    {
        private readonly TwoFactorAuthService _twoFactorAuthService;

        public TwoFactorAuthWindow()
        {
            InitializeComponent();
            _twoFactorAuthService = new TwoFactorAuthService();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var code = CodeTextBox.Text;
                if (_twoFactorAuthService.VerifyCode(code))
                {
                    DialogResult = true; // User entered the correct code
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid code. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during verification: {ex.Message}");
            }
        }

        private void CodeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Placeholder.Visibility = Visibility.Collapsed; // Hide placeholder
        }

        private void CodeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CodeTextBox.Text))
            {
                Placeholder.Visibility = Visibility.Visible; // Show placeholder
            }
        }

        private void CodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                VerifyButton_Click(sender, e);
            }
        }
    }
}