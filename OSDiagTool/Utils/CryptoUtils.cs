using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Linq;

namespace OSDiagTool.Utils
{

    /*
     * Most private.key utilities are based on https://github.com/ardoric/ardo.decrypter.net 
     */
    static class CryptoUtils
    {
        static private RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();

        private static byte[] DecryptBytes(byte[] key, byte[] iv, byte[] ciphertext)
        {
            using (Aes crypto = new AesManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
            {
                crypto.IV = iv;
                crypto.Key = key;
                using (MemoryStream output = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(output, crypto.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(ciphertext, 0, ciphertext.Length);
                    }
                    return output.ToArray();
                }
            }
        }

        public static string Decrypt(string key, string encryptedText)
        {
            if (!encryptedText.StartsWith("$2$"))
                throw new Exception("Can't decrypt the content: " + encryptedText);

            byte[] the_key = Convert.FromBase64String(key);
            byte[] iv = Convert.FromBase64String(encryptedText.Substring(3, 24));
            byte[] ciphertext = Convert.FromBase64String(encryptedText.Substring(3 + 24));

            return Encoding.UTF8.GetString(DecryptBytes(the_key, iv, ciphertext));
        }

        /*public static String Encrypt(string key, String plaintext) {

            using (Aes crypto = getCipher()) {
                byte[] the_key = Convert.FromBase64String(key);
                crypto.Key = the_key;
                crypto.IV = getRandomBytes(crypto.BlockSize / 8);

                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                MemoryStream ms = new MemoryStream();
                ms.Write(crypto.IV, 0, crypto.IV.Length);
                using (CryptoStream cs = new CryptoStream(ms, crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Write))
                    cs.Write(plainBytes, 0, plainBytes.Length);
                ms.Close();
                var cipherBytes = ms.ToArray();

                HMACSHA256 mac = new HMACSHA256();
                mac.Key = the_key;

                var macBytes = mac.ComputeHash(cipherBytes);

                MemoryStream resStream = new MemoryStream();
                resStream.Write(cipherBytes, 0, cipherBytes.Length);
                resStream.Write(macBytes, 0, macBytes.Length);
                return Convert.ToBase64String(resStream.ToArray());

            }
        }*/

        public static string GetPrivateKeyFromFile(string privateKeyFilepath)
        {
            foreach (string line in File.ReadAllLines(privateKeyFilepath))
            {
                string trimmed = line.Trim();
                if (trimmed.Equals(String.Empty) || trimmed.StartsWith("--"))
                    continue;
                return trimmed;
            }
            return "";
        }

        public static Dictionary<string, string> GetEncryptedEntriesFromPlatformConfFile(string filepath)
        {
            using (Stream fileReader = File.OpenRead(filepath))
            {
                Dictionary<string, string> res = new Dictionary<string, string>();
                XDocument xmlDoc = XDocument.Load(fileReader);
                foreach (XElement e in xmlDoc.Descendants().Where(e => e.Attribute("encrypted") != null && e.Attribute("encrypted").Value.ToLower() == "true"))
                {
                    res.Add(e.Name.LocalName, e.Value);
                }
                return res;
            }
        }

        private static Aes getCipher() {
            return new AesManaged() {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }

        private static byte[] getRandomBytes(int count) {
            byte[] res = new byte[count];
            rnd.GetBytes(res);
            return res;
        }
    }
}
