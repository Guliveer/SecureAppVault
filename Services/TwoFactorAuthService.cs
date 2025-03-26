using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OtpNet;
using QRCoder;

namespace SecureAppVault.Services
{
    public class TwoFactorAuthService
    {
        private readonly string _configFilePath = "Resources/2FAConfig.json";
        private readonly byte[] _secretKey;

        public TwoFactorAuthService()
        {
            if (File.Exists(_configFilePath))
            {
                _secretKey = Convert.FromBase64String(File.ReadAllText(_configFilePath));
            }
            else
            {
                _secretKey = GenerateSecretKey();
                SaveConfig();
            }
        }

        public bool VerifyCode(string code)
        {
            var totp = new Totp(_secretKey);
            return totp.VerifyTotp(code, out _);
        }

        private byte[] GenerateSecretKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var key = new char[16];

            for (int i = 0; i < key.Length; i++)
            {
                key[i] = chars[random.Next(chars.Length)];
            }

            return Encoding.UTF8.GetBytes(new string(key));
        }

        public string GetConfigCode()
        {
            return Convert.ToBase64String(_secretKey);
        }

        private void SaveConfig()
        {
            var directory = Path.GetDirectoryName(_configFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_configFilePath, Convert.ToBase64String(_secretKey));
        }

        public string GenerateQrCodeUri(string issuer)
        {
            string uri = $"otpauth://totp/{issuer}?secret={Base32Encoding.ToString(_secretKey)}&issuer={issuer}";
            return uri;
        }

        public byte[] GenerateQrCode(string issuer)
        {
            string uri = GenerateQrCodeUri(issuer);
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }
        
        public bool ConfigExists()
        {
            return File.Exists(_configFilePath);
        }
    }
}