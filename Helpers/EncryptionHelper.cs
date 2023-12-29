using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DeviceInfoHub.Helpers
{
    /// <summary>
    /// Provides methods to encrypt and decrypt strings using AES encryption.
    /// </summary>
    public class EncryptionHelper
    {
        /// <summary>
        /// Encrypts a plain text string using a specified key.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        /// <param name="plainText">The text to be encrypted.</param>
        /// <returns>The encrypted string in Base64 format.</returns>
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypts a cipher text string using a specified key.
        /// </summary>
        /// <param name="key">The decryption key.</param>
        /// <param name="cipherText">The text to be decrypted.</param>
        /// <returns>The decrypted plain text string.</returns>
        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}