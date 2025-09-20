using System.Security.Cryptography;
using System.Text;

namespace Servcies.DataServcies
{
    public class CryptographyServcies
    {
        private string _secret; //encryption secret
        private byte[] _key;

        public void SetSecret(string secret)
        {
            _secret = secret;
            _key = Encoding.UTF8.GetBytes(_secret);
        }

        public string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes;
            byte[] iv;

            // Set up the encryption objects
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                iv = aes.IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt the input plaintext using the AES algorithm
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            // Combine IV and encrypted bytes
            byte[] combinedBytes = new byte[iv.Length + encryptedBytes.Length];
            Array.Copy(iv, 0, combinedBytes, 0, iv.Length);
            Array.Copy(encryptedBytes, 0, combinedBytes, iv.Length, encryptedBytes.Length);

            return Convert.ToBase64String(combinedBytes);
        }

        public string Decrypt(string cipherText)
        {
            byte[] combinedBytes = Convert.FromBase64String(cipherText);
            byte[] iv = new byte[16];
            byte[] cipherBytes = new byte[combinedBytes.Length - iv.Length];
            byte[] decryptedBytes;

            // Extract IV and cipher text
            Array.Copy(combinedBytes, 0, iv, 0, iv.Length);
            Array.Copy(combinedBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

            // Set up the decryption objects
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Decrypt the input ciphertext using the AES algorithm
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

}
