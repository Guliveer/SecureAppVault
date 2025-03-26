using System.Diagnostics;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace SecureAppVault.Models
{
    public class WhitelistedApp
    {
        public string Name { get; }
        public string Path { get; }
        public bool IsRunningFromVault { get; set; }

        public WhitelistedApp(string name, string path)
        {
            Name = name;
            Path = path;
            IsRunningFromVault = false;
        }

        public void Run()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start application: {ex.Message}");
            }
        }
    }
}