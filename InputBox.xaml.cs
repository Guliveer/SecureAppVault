using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SecureAppVault
{
    public partial class InputBox : Window
    {
        public string InputText { get; private set; } = string.Empty; // Initialize to an empty string

        public InputBox(string prompt)
        {
            InitializeComponent();
            PromptTextBlock.Text = prompt;
            WindowStartupLocation = WindowStartupLocation.CenterScreen; // Center the window on the screen
            Loaded += (sender, e) => InputTextBox.Focus(); // Set focus to the text box when the window is loaded
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputText = InputTextBox.Text;
                DialogResult = true;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            DialogResult = true;
        }
    }
}