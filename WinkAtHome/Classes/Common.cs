using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WinkAtHome
{
    public class Common
    {
        public static string dbPath = HttpContext.Current.Request.PhysicalApplicationPath + "WinkAtHome.sqlite";

        public class twofer : Tuple<string, bool>
        {
            public twofer(string value, bool isEncrypted)
                : base(value, isEncrypted)
            {
            }
        }


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

        public static void prepareDatabase()
        {
            //Create DB File If it doesn't already exist
            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
            {
                connection.Open();

                //PREPARE SETTINGS TABLE
                //CREATE TABLE
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Settings(Name VARCHAR PRIMARY KEY NOT NULL ON CONFLICT REPLACE, Value VARCHAR, DefaultValue VARCHAR, IsEncrypted BOOL DEFAULT false);";
                    command.ExecuteNonQuery();
                }

                //INSERT DEFAULT SETTINGS
                Dictionary<string, twofer> basicSettings = new Dictionary<string, twofer>();
                basicSettings.Add("winkUsername", (new twofer("Username", true)));
                basicSettings.Add("winkPassword", (new twofer("Password", true)));
                basicSettings.Add("winkClientID", (new twofer("quirky_wink_android_app", true)));
                basicSettings.Add("winkClientSecret", (new twofer("e749124ad386a5a35c0ab554a4f2c045", true)));
                basicSettings.Add("PubNub-PublishKey", (new twofer(null, true)));
                basicSettings.Add("PubNub-SubscribeKey", (new twofer(null, true)));
                basicSettings.Add("PubNub-SecretKey", (new twofer(null, true)));
                basicSettings.Add("StartPage", (new twofer("Control.aspx", false)));
                basicSettings.Add("Hide-Empty-Robots", (new twofer("false", false)));
                basicSettings.Add("Hide-Empty-Groups", (new twofer("false", false)));
                basicSettings.Add("Robot-Alert-Minutes-Since-Last-Trigger", (new twofer("60", false)));

                foreach (KeyValuePair<string,twofer> pair in basicSettings)
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Settings SET DefaultValue=@defaultvalue, IsEncrypted=@isEncypted WHERE name = @name;";
                        command.Parameters.Add(new SQLiteParameter("@name", pair.Key));
                        command.Parameters.Add(new SQLiteParameter("@defaultvalue", pair.Value.Item1));
                        command.Parameters.Add(new SQLiteParameter("@isEncypted", pair.Value.Item2));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Settings(Name,DefaultValue,IsEncrypted) VALUES (@name,@defaultvalue,@isEncypted)";
                        command.ExecuteNonQuery();
                    }
                }

                //PREPARE DEVICES TABLE
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Devices(DeviceID VARCHAR PRIMARY KEY NOT NULL ON CONFLICT REPLACE, Name VARCHAR, DisplayName VARCHAR, SubscriptionTopic VARCHAR, SubscriptionExpires DATETIME, Position SMALLINT DEFAULT 1001);";
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "PRAGMA table_info(Devices);";
                    SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    DataRow[] foundRows;
                    foundRows = dt.Select("name = 'Name'");

                    if (foundRows.Length == 0)
                    {
                        command.CommandText = "ALTER TABLE Devices ADD COLUMN Name VARCHAR;";
                        command.ExecuteNonQuery();
                    }

                    foundRows = dt.Select("name = 'subscriptionCapable'");

                    if (foundRows.Length == 0)
                    {
                        command.CommandText = "ALTER TABLE Devices ADD COLUMN subscriptionCapable BOOLEAN NOT NULL DEFAULT 0;";
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
                connection.Dispose();
            }

            
            //MOVE SETTINGS.TXT TO DB
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt"))
            {
                string text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt");
                string decrypedFile = Common.Decrypt(text);

                JObject json = JObject.Parse(decrypedFile);

                foreach (var jo in json)
                {
                    if (jo.Key == "Controllable-Display-Order")
                    {
                        List<string> existingList = jo.Value.ToString().Split(',').ToList();

                        foreach (string ID in existingList)
                        {
                            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
                            {
                                connection.Open();

                                using (SQLiteCommand command = new SQLiteCommand(connection))
                                {

                                    command.CommandText = "INSERT INTO Devices (DeviceID, Position) VALUES (@ID,@Position)";
                                    command.Parameters.Add(new SQLiteParameter("@ID", ID));
                                    command.Parameters.Add(new SQLiteParameter("@Position", existingList.IndexOf(ID) + 1));
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    else
                        SettingMgmt.saveSetting(jo.Key, jo.Value.ToString());
                }

                File.Move(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", AppDomain.CurrentDomain.BaseDirectory + "Settings.txt_Converted");
            }
        }
    }
}