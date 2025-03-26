using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SecureAppVault.Models;

namespace SecureAppVault.Services
{
    public class WhitelistService
    {
        private readonly EncryptionService _encryptionService;
        private readonly string _filePath = "Resources/EncryptedWhitelist.json";

        public WhitelistService(string encryptionKey)
        {
            _encryptionService = new EncryptionService(encryptionKey);
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
    }
}