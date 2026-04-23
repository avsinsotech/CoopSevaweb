using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AVSBackend.Helpers
{
    /// <summary>
    /// Provides AES-256 encryption and decryption for sensitive PII data
    /// such as Aadhaar and PAN numbers before database persistence.
    /// Uses a FIXED IV (Deterministic) so that EF Core can use encrypted
    /// values as Primary Keys and for equality comparisons.
    /// </summary>
    public static class EncryptionHelper
    {
        // Set from Program.cs at application startup via appsettings.json
        public static string EncryptionKey { get; set; } = string.Empty;

        // Fixed IV ensures same plaintext always produces same ciphertext (Deterministic).
        // This is required because AadharNumber and PanNumber are used as Primary Keys.
        private static readonly byte[] FixedIV = new byte[]
        {
            0x41, 0x56, 0x53, 0x42, 0x61, 0x6E, 0x6B, 0x32,
            0x30, 0x32, 0x34, 0x4B, 0x59, 0x43, 0x49, 0x56
        };

        /// <summary>
        /// Encrypts a plain text string using AES-256.
        /// Returns Base64-encoded cipher text safe for database storage.
        /// Returns null if input is null or empty.
        /// </summary>
        public static string? Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            if (string.IsNullOrEmpty(EncryptionKey))
                throw new InvalidOperationException("EncryptionHelper: EncryptionKey has not been set. Ensure it is configured in appsettings.json.");

            byte[] keyBytes = GetKeyBytes();

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = FixedIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            // Prefix with "ENC:" so we can detect encrypted values vs old plaintext
            return "ENC:" + Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Decrypts a Base64-encoded cipher text back to plain text.
        /// Contains a safety fallback: if the value is NOT encrypted (old plain text data),
        /// it is returned as-is to prevent crashes on existing records.
        /// Returns null if input is null or empty.
        /// </summary>
        public static string? Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            // Safety fallback: if it is not our encrypted format, return as-is (backward compatibility)
            if (!cipherText.StartsWith("ENC:"))
                return cipherText;

            if (string.IsNullOrEmpty(EncryptionKey))
                throw new InvalidOperationException("EncryptionHelper: EncryptionKey has not been set. Ensure it is configured in appsettings.json.");

            try
            {
                string base64Data = cipherText.Substring(4); // Strip "ENC:" prefix
                byte[] keyBytes = GetKeyBytes();
                byte[] encryptedBytes = Convert.FromBase64String(base64Data);

                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.IV = FixedIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                // Final safety net: if decryption fails for any reason, return the raw value
                return cipherText;
            }
        }

        /// <summary>
        /// Converts the EncryptionKey string from appsettings to a 32-byte AES-256 key.
        /// Pads or truncates to exactly 32 bytes.
        /// </summary>
        private static byte[] GetKeyBytes()
        {
            byte[] keyBytes = new byte[32];
            byte[] rawKey = Encoding.UTF8.GetBytes(EncryptionKey);
            int copyLength = Math.Min(rawKey.Length, 32);
            Array.Copy(rawKey, keyBytes, copyLength);
            return keyBytes;
        }
    }
}
