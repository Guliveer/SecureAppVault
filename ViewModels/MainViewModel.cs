using System.Collections.ObjectModel;
using SecureAppVault.Models;
using SecureAppVault.Services;

namespace SecureAppVault.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool _isUnlocked;

        public ObservableCollection<WhitelistedApp> WhitelistedApps { get; set; }
        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                if (_isUnlocked != value)
                {
                    _isUnlocked = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsLocked));
                }
            }
        }
        public bool IsLocked => !IsUnlocked;
        public WhitelistedApp VaultApp { get; }

        public MainViewModel(string encryptionKey)
        {
            var whitelistService = new WhitelistService(encryptionKey);
            WhitelistedApps = new ObservableCollection<WhitelistedApp>(whitelistService.LoadWhitelist());
            IsUnlocked = false;
            VaultApp = new WhitelistedApp("Vault", "vault/path");
        }
    }
}