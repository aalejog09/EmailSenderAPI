using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MailSenderAPI.Services
{
    public class EncryptionService
    {
        // Clave de 32 bytes (AES-256)
        private readonly string key = "SzQF5j0XeytFLTrt6XmiMCRc9pdw+nFh1aPBR3JUtQ8=";
        // IV de 16 bytes (AES)
        private readonly string iv = "VEl8fRHaw/6ZBr8NYmsV6Q==";

        public string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key); // Usamos la clave generada
                aesAlg.IV = Convert.FromBase64String(iv);   // Usamos el IV generado

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray()); // Devolver texto cifrado en base64
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key); // Usamos la clave generada
                aesAlg.IV = Convert.FromBase64String(iv);   // Usamos el IV generado

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd(); // Devolver texto desencriptado
                        }
                    }
                }
            }
        }
    }
}
