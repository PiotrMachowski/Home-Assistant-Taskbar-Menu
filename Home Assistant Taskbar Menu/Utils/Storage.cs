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
        public static string BrowserCachePath => $"{_basePath}\\browserCache";
        private static string CredentialsPathOld => $"{_basePath}\\config.dat";
        private static string ViewConfigPathOld => $"{_basePath}\\viewConfig.dat";
        private static string CredentialsPath => $"{_basePath}\\config_credentials.dat";
        private static string ViewConfigPath => $"{_basePath}\\config_view.dat";
        private static string BrowserConfigPath => $"{_basePath}\\config_position.dat";
        private static string LogPath => $"{_basePath}\\log.txt";

        private const string PassPhrase = "ThisIsASecurePassword";
        private const int KeySize = 256;
        private const int DerivationIterations = 1000;
        private static string _basePath = "";

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
                streamWriter.Write(JsonConvert.SerializeObject(viewConfiguration, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));
            }
        }

        public static void SavePosition((double x, double y, double width, double height) position)
        {
            using (var streamWriter = new StreamWriter(BrowserConfigPath))
            {
                streamWriter.WriteLine(position.x);
                streamWriter.WriteLine(position.y);
                streamWriter.WriteLine(position.width);
                streamWriter.WriteLine(position.height);
            }
        }

        public static (double x, double y, double width, double height)? RestorePosition()
        {
            (double, double, double, double)? position;
            try
            {
                using (var streamReader = new StreamReader(BrowserConfigPath))
                {
                    double x = Convert.ToDouble(streamReader.ReadLine());
                    double y = Convert.ToDouble(streamReader.ReadLine());
                    double width = Convert.ToDouble(streamReader.ReadLine());
                    double height = Convert.ToDouble(streamReader.ReadLine());
                    position = (x, y, width, height);
                }
            }
            catch (Exception)
            {
                position = null;
            }

            return position;
        }

        public static void InitConfigDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory().Split('\\').ToList().Last();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _basePath = $"{appData}\\Home Assistant Taskbar Menu\\{currentDir}";
            Directory.CreateDirectory(_basePath);
            if (!IsConsoleAvailable())
            {
                FileStream fileStream = new FileStream(LogPath, FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream) {AutoFlush = true};
                Console.SetOut(streamWriter);
                Console.SetError(streamWriter);
            }
            ConsoleWriter.WriteLine($"Config directory: {_basePath}", ConsoleColor.DarkYellow);
            MoveFile(CredentialsPathOld, CredentialsPath);
            MoveFile(ViewConfigPathOld, ViewConfigPath);
        }

        private static void MoveFile(string source, string destination)
        {
            try
            {
                File.Move(source, destination);
            }
            catch (Exception)
            {
                // ignored
            }
        }


        private static string Encrypt(string plainText)
        {
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(PassPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(KeySize / 8);
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
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8 * 2)
                .Take(cipherTextBytesWithSaltAndIv.Length - KeySize / 8 * 2).ToArray();

            using (var password = new Rfc2898DeriveBytes(PassPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(KeySize / 8);
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

        private static bool IsConsoleAvailable()
        {
            bool consolePresent = true;
            try
            {
                int _ = Console.WindowHeight;
            }
            catch
            {
                consolePresent = false;
            }

            return consolePresent;
        }
    }
}