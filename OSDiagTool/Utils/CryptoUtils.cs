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
    }
}
