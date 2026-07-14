using System;
using System.Security.Cryptography;
using System.Text;

namespace SweetSweeps.Infrastructure
{
    public static class AesCbcEncryption
    {
        public static string Encrypt(string plaintext, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key     = key;
            aes.IV      = iv;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes  = Encoding.UTF8.GetBytes(plaintext);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }

        public static string Decrypt(string ciphertext, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key     = key;
            aes.IV      = iv;

            using var decryptor  = aes.CreateDecryptor();
            var cipherBytes      = Convert.FromBase64String(ciphertext);
            var plainBytes       = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}