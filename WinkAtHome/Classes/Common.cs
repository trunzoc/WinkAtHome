using System;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace WinkAtHome
{
    public class Common
    {
        public static DateTime FromUnixTime(string unixTime)
        {
            Double longTime;
            bool converted = Double.TryParse(unixTime, out longTime);
            var epoch = new DateTime(1970, 1, 1);
            epoch = epoch.AddSeconds(longTime);
            epoch = epoch.ToLocalTime();
            return epoch;
        }

        public static string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            string key = ConfigurationManager.AppSettings["encyrptionKey"];

            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string cipherString)
        {
            try
            {
                byte[] keyArray;

                byte[] toEncryptArray = Convert.FromBase64String(cipherString);

                string key = ConfigurationManager.AppSettings["encyrptionKey"];

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;

                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                return null;
            }
        }
        
        public static double FromCelsiusToFahrenheit(double c)
        {
            return Math.Round(((9.0 / 5.0) * c) + 32);
        }

        public static double FromFahrenheitToCelsius(double f)
        {
            return Math.Round((5.0 / 9.0) * (f - 32), 2);
        }
    }
}