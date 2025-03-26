using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecureAppVault.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(string key)
        {
            // Ensure the key is 32 bytes for AES-256
            _key = new byte[32];
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Copy(keyBytes, _key, Math.Min(keyBytes.Length, _key.Length));
        }

        public string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                aes.Padding = PaddingMode.PKCS7; // Upewniamy się, że padding jest poprawny

                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Zapisujemy IV na początku

                    using (var encryptor = aes.CreateEncryptor())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8, 1024, true)) // Pozostawiamy strumień otwarty
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    var iv = new byte[16];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);

                    var cipher = new byte[fullCipher.Length - iv.Length];
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                    aes.Key = _key;
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7; // Dopilnujmy, by tryb paddingu był taki sam jak przy szyfrowaniu

                    using (var ms = new MemoryStream(cipher))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs, Encoding.UTF8)) // Upewniamy się, że używamy poprawnego kodowania
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (FormatException)
            {
                throw new CryptographicException("Invalid Base64 format. Ensure the input is correctly encoded.");
            }
            catch (CryptographicException e)
            {
                throw new CryptographicException("Decryption failed. Possible causes: incorrect key, modified cipher text, or corrupted data.", e);
            }
        }
    }
}