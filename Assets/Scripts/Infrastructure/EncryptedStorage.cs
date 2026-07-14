using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SweetSweeps.Infrastructure
{
    public static class EncryptedStorage
    {
        private static readonly byte[] Key = DeriveKey(
            "m!&wDZ!G2wQ983S@yB0@^gjq",
            "W&Xb%He9zrb@8b0H&2");

        private static readonly byte[] IV = new byte[]
        {
            0xa3, 0xf1, 0xc2, 0xe4,
            0xb5, 0xd6, 0xf7, 0xa8,
            0xb9, 0xc0, 0xd1, 0xe2,
            0xf3, 0xa4, 0xb5, 0xc6
        };

        public static void Set(string key, string value)
        {
            try
            {
                string encrypted = AesCbcEncryption.Encrypt(value, Key, IV);
                PlayerPrefs.SetString(key, encrypted);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EncryptedStorage] Set error key={key}: {e.Message}");
            }
        }

        public static string Get(string key)
        {
            try
            {
                string encrypted = PlayerPrefs.GetString(key, string.Empty);

                if (string.IsNullOrEmpty(encrypted))
                    return string.Empty;

                return AesCbcEncryption.Decrypt(encrypted, Key, IV);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EncryptedStorage] Get error key={key}: {e.Message}");
                return string.Empty;
            }
        }

        public static void Remove(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public static bool Has(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        private static byte[] DeriveKey(string secret, string salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                secret,
                Encoding.UTF8.GetBytes(salt),
                iterations: 10000,
                HashAlgorithmName.SHA256);

            return pbkdf2.GetBytes(32);
        }
    }
}