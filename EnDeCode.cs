using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LujuPet
{
    public partial class EnDeCode : Component
    {
        public EnDeCode()
        {
            InitializeComponent();
        }

        public EnDeCode(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public static bool IfFolderExist(string path)
        {
            return Directory.Exists(path);
        }

        public static List<string> CheckFileAndGetString(string fileName)
        {
            // 檢查檔案是否存在
            if (!File.Exists(fileName))
            {
                Console.WriteLine("檔案不存在: " + fileName);
                return null; // 不存在就回傳 null
            }

            // 逐行讀取檔案並回傳 List<string>
            List<string> lines = new List<string>(File.ReadAllLines(fileName));
            if (lines.Count > 0)
            {
                Console.WriteLine("檔案: " + fileName + "共" + lines.Count + "行");
            }
            return lines;
        }

        public static string GetNowString(bool withTime)
        {
            string nowString = GetBasicString();

            if (withTime)
            {
                nowString += DateTime.Now.ToString("yyMMdd_HHmm");
            }
            else
            {
                nowString += DateTime.Now.ToString("yyMMdd");
            }

                Console.WriteLine(nowString);
            return nowString;
        }

        public static string GetBasicString()
        {
            string basicString = "Luro";
            return basicString;
        }

        public static string EncryptString(string plainText, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
                aes.IV = new byte[16]; // 固定 IV，實務上建議隨機產生並存儲

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(plainBytes, 0, plainBytes.Length);
                    cs.FlushFinalBlock();

                    // 使用 Base64Url 編碼，避免非法字元
                    string base64 = Convert.ToBase64String(ms.ToArray());
                    return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
                }
            }
        }

        public static string DecryptString(string cipherText, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
                aes.IV = new byte[16]; // 與加密時相同

                // 還原成標準 Base64
                string base64 = cipherText.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static bool IsWinner(int winningRate, int randomSeed, string inputString)
        {
            if (winningRate <= 0) return false;
            if (winningRate >= 100) return true;

            // 將 inputString 與 randomSeed 結合，產生固定雜湊
            string combined = inputString + randomSeed.ToString();

            // 使用 SHA256 產生雜湊
            byte[] hashBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(combined));

            // 取前 4 bytes 轉成整數，作為隨機值
            int value = BitConverter.ToInt32(hashBytes, 0);
            if (value < 0) value = -value; // 確保正數

            // 對 100 取餘數，判斷是否中獎
            int result = value % 100;

            return result < winningRate;
        }
    }
}
