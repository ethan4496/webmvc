using System.Security.Cryptography;
using System.Text;
using WebMVC.Entities;

namespace WebMVC.Ultilities
{
    public static class AesEncryptionHelper
    {
        private static readonly string Key = "yWQa2MufmVQ^2BqT";
        private static readonly string IV = "GV@d@Sd!A@#mV2$C";
        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var base64 = Convert.ToBase64String(ms.ToArray());
            return Base64UrlEncode(base64);
        }

        private static string Base64UrlEncode(string base64)
        {
            return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public static Account DecryptCurrentAccount(string token)
        {
            try
            {
                string[] decryptValues = Decrypt(token).Split('|');
                return new Account
                {
                    Id = int.Parse(decryptValues[0]),
                    Username = decryptValues[1],
                    RoleId = int.Parse(decryptValues[2]),
                };
            }
            catch
            {
                throw new Exception("Không có quyền truy cập");
            }

        }

        private static string Decrypt(string encodedText)
        {
            if (encodedText == null) { return ""; }
            string base64 = Base64UrlDecode(encodedText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            var buffer = Convert.FromBase64String(base64);
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        private static string Base64UrlDecode(string encoded)
        {
            string base64 = encoded.Replace("-", "+").Replace("_", "/");
            int padding = 4 - (base64.Length % 4);
            if (padding < 4) base64 += new string('=', padding);
            return base64;
        }
    }
}
