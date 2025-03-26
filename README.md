![C# Badge](https://img.shields.io/badge/C%23-9179E4?style=for-the-badge)
![.NET Badge](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff&style=for-the-badge)
![WPF](https://img.shields.io/badge/WPF-0a75b9?style=for-the-badge&logo=m&logoColor=white)

# SecureAppVault

SecureAppVault is a secure application vault that helps you manage and protect your applications with features like two-factor authentication (2FA) and whitelisting.

## Features

- Minimize to system tray
- Two-factor authentication
- Application whitelisting

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Windows OS

### Usage

1. Run the application.
2. Configure two-factor authentication if not already configured.
3. Add applications to the whitelist.

### Object-Oriented Programming (OOP)

SecureAppVault leverages Object-Oriented Programming (OOP) principles to structure the application. Key OOP concepts used include:

- **Encapsulation**: Classes like `TwoFactorAuthService` and `WhitelistService` encapsulate related functionalities and data, providing a clear interface for interaction.
- **Abstraction**: The services abstract the underlying implementation details, allowing other parts of the application to interact with them without needing to understand the internal workings.
- **Inheritance**: Although not explicitly shown in the provided code, inheritance can be used to create a hierarchy of classes that share common functionality.
- **Polymorphism**: Methods like `VerifyCode` in `TwoFactorAuthService` can be overridden in derived classes to provide specific implementations.

### Functional Programming (FP)

While SecureAppVault primarily uses OOP, it also incorporates some Functional Programming (FP) principles:

- **Immutability**: The use of immutable data structures where possible, such as the `List<WhitelistedApp>` returned by `LoadWhitelist`.
- **Pure Functions**: Methods like `GenerateSecretKey` in `TwoFactorAuthService` are pure functions, as they do not cause side effects and always produce the same output for the same input.
- **Higher-Order Functions**: The use of delegates and lambda expressions, such as the event handler for `_notifyIcon.DoubleClick`.

## Code Overview

### MainWindow.xaml.cs

Handles the main window operations, including minimizing to the system tray.

```csharp
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
```

### AddAppWindow.xaml.cs

Handles adding applications to the whitelist.

```csharp
private void AddButton_Click(object sender, RoutedEventArgs e)
{
    var appName = AppNameTextBox.Text;
    var appPath = AppPathTextBox.Text;

    if (!string.IsNullOrEmpty(appName) && !string.IsNullOrEmpty(appPath))
    {
        var app = new WhitelistedApp(appName, appPath);
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
```

### TwoFactorAuthWindow.xaml.cs

Handles two-factor authentication.

```csharp
private void VerifyButton_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var code = CodeTextBox.Text;
        if (_twoFactorAuthService.VerifyCode(code))
        {
            DialogResult = true;
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
```

## Technical Details

### Project Structure

The project is structured as follows:

- `SecureAppVault.csproj`: The project file containing project metadata and dependencies.
- `App.xaml` and `App.xaml.cs`: The application entry point and application-level event handlers.
- `MainWindow.xaml` and `MainWindow.xaml.cs`: The main window of the application.
- `Views/`: Contains additional windows such as `AddAppWindow` and `TwoFactorAuthWindow`.
- `Models/`: Contains data models such as `WhitelistedApp`.
- `Services/`: Contains service classes such as `WhitelistService` and `TwoFactorAuthService`.
- `Resources/`: Contains resource files such as `Icon.ico`.

### Dependency Injection

The project uses dependency injection to manage service instances. For example, `WhitelistService` is injected into `AddAppWindow`:

```csharp
public AddAppWindow(string encryptionKey)
{
    InitializeComponent();
    _whitelistService = new WhitelistService(encryptionKey);
}
```

### Two-Factor Authentication

The `TwoFactorAuthService` class handles two-factor authentication. It verifies the code entered by the user:

```csharp
public bool VerifyCode(string code)
{
    var totp = new Totp(_secretKey);
    return totp.VerifyTotp(code, out _);
}
```

### Whitelisting

The `WhitelistService` class manages the list of whitelisted applications. It provides methods to load and save the whitelist:

```csharp
public List<WhitelistedApp> LoadWhitelist()
{
    if (!File.Exists(_filePath))
    {
        return new List<WhitelistedApp>();
    }

    var encryptedJson = File.ReadAllText(_filePath, Encoding.UTF8);
    var json = _encryptionService.Decrypt(encryptedJson);
    return System.Text.Json.JsonSerializer.Deserialize<List<WhitelistedApp>>(json) ?? new List<WhitelistedApp>();
}

public void SaveWhitelist(List<WhitelistedApp> apps)
{
    var directory = Path.GetDirectoryName(_filePath);
    if (directory != null && !Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    var json = System.Text.Json.JsonSerializer.Serialize(apps);
    var encryptedJson = _encryptionService.Encrypt(json);
    File.WriteAllText(_filePath, encryptedJson, Encoding.UTF8);
}
```

### Error Handling

The application includes error handling to manage exceptions and provide user feedback. For example, in `TwoFactorAuthWindow.xaml.cs`:

```csharp
try
{
    var code = CodeTextBox.Text;
    if (_twoFactorAuthService.VerifyCode(code))
    {
        DialogResult = true;
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
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

---
Made with ❤️ by [Oliwer Pawelski](https://github.com/Guliveer/)   
![Rider Badge](https://img.shields.io/badge/Rider-000?logo=rider&logoColor=fff&style=flat-square)
