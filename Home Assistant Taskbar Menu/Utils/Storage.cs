using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Home_Assistant_Taskbar_Menu.Utils
{
    public static class Storage
    {
        public const string CredentialsPath = @"config.dat";
        public const string ViewConfigPath = @"viewConfig.dat";
        private const string PassPhrase = "ThisIsASecurePassword";
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;

        public static Configuration RestoreConfiguration()
        {
            Configuration cfg;
            try
            {
                using (var streamReader = new StreamReader(CredentialsPath))
                {
                    cfg = JsonConvert.DeserializeObject<Configuration>(Decrypt(streamReader.ReadToEnd()));
                }
            }
            catch (Exception)
            {
                cfg = null;
            }

            return cfg;
        }

        public static ViewConfiguration RestoreViewConfiguration()
        {
            ViewConfiguration viewConfiguration;
            try
            {
                using (var streamReader = new StreamReader(ViewConfigPath))
                {
                    viewConfiguration = JsonConvert.DeserializeObject<ViewConfiguration>(streamReader.ReadToEnd());
                }
            }
            catch (Exception)
            {
                viewConfiguration = null;
            }

            return viewConfiguration ?? ViewConfiguration.Default();
        }

        public static void Save(Configuration cfg)
        {
            using (var streamWriter = new StreamWriter(CredentialsPath))
            {
                streamWriter.Write(Encrypt(JsonConvert.SerializeObject(cfg)));
            }
        }

        public static void Save(ViewConfiguration viewConfiguration)
        {
            using (var streamWriter = new StreamWriter(ViewConfigPath))
            {
                streamWriter.Write(JsonConvert.SerializeObject(viewConfiguration, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
        }

        private static string Encrypt(string plainText)
        {
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(PassPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        private static string Decrypt(string cipherText)
        {
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8 * 2)
                .Take(cipherTextBytesWithSaltAndIv.Length - Keysize / 8 * 2).ToArray();

            using (var password = new Rfc2898DeriveBytes(PassPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }

            return randomBytes;
        }
    }
}