﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WinkAtHome
{
    public class Common
    {
        public static bool isLocalHost = (ConfigurationManager.AppSettings["isLocalHost"].ToLower() == "true") ? true : false;
        public static string dbPath = HttpContext.Current.Request.PhysicalApplicationPath + "WinkAtHome.sqlite";
        public static string currentVersion = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string newVersion = string.Empty;
        public static string updateFilePath = null;
        public static string updateNotes = string.Empty;

        public static bool checkForUpdate()
        {
            bool newer = false;

            try
            {
                string url = ConfigurationManager.AppSettings["githubReleaseInfo"];
                JObject jsonResponse = new JObject();

                using (var xhr = new WebClient())
                {
                    xhr.Headers[HttpRequestHeader.ContentType] = "application/json";
                    xhr.Headers[HttpRequestHeader.Accept] = "application/vnd.github.v3+json";
                    xhr.Headers[HttpRequestHeader.UserAgent] = "WinkAtHome";
                    xhr.Credentials = CredentialCache.DefaultCredentials;

                    byte[] result = null;

                    result = xhr.DownloadData(url);

                    if (result != null)
                    {
                        string strTag = string.Empty;

                        string responseString = Encoding.Default.GetString(result);
                        jsonResponse = JObject.Parse(responseString);

                        strTag = jsonResponse["tag_name"].ToString();
                        newVersion = strTag;

                        if (!string.IsNullOrWhiteSpace(strTag))
                        {
                            var vThis = new Version(currentVersion.Replace("v",""));
                            var vGit = new Version(strTag.Replace("v", ""));

                            var vCompare = vGit.CompareTo(vThis);
                            if (vCompare > 0)
                                newer = true;
                        }

                        foreach (var asset in jsonResponse["assets"])
                        {
                            string name = asset["name"].ToString();
                            if (name.ToLower() == "winkathome.exe")
                            {
                                string updatePath = asset["browser_download_url"].ToString();
                                updateFilePath = updatePath;

                            }
                        }

                        updateNotes = "";
                        var releaseName = jsonResponse["name"];
                        if (releaseName != null)
                            updateNotes += releaseName.ToString() + "\r\n\r\n";
                        var releaseBody = jsonResponse["body"];
                        if (releaseBody != null)
                            updateNotes += releaseBody.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return newer;
        }

        public static DateTime FromUnixTime(string unixTime, bool ConvertToLocalTimezone = false)
        {
            try
            {
                Double longTime;
                bool converted = Double.TryParse(unixTime, out longTime);
                DateTime epoch = new DateTime(1970, 1, 1);
                epoch = epoch.AddSeconds(longTime);

                if (ConvertToLocalTimezone)
                {
                    string strUsertimezone = SettingMgmt.getSetting("TimeZone-Adjuster");
                    string strWinktimezone = ConfigurationManager.AppSettings["WinkServerTimeZoneAdjustment"];
                    Int32 usertimezone = 0;
                    Int32 winktimezone = 0;
                    Int32.TryParse(strUsertimezone, out usertimezone);
                    Int32.TryParse(strWinktimezone, out winktimezone);

                    DateTime utcTime = epoch.ToUniversalTime();
                    epoch = utcTime.AddHours(usertimezone-winktimezone);
                    if (DateTime.Now.IsDaylightSavingTime())
                        epoch = epoch.AddHours(1);
                }
                else
                    epoch = epoch.ToLocalTime();

                return epoch;
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;
                throw; //EventLog.WriteEntry("WinkAtHome.Common.FromUnixTime", ex.Message, EventLogEntryType.Error);
            }
        }
        public static DateTime getLocalTime(Int32 timezoneAdjust = 99)
        {
            Int32 timezone = 0;

            if (timezoneAdjust != 99)
                timezone = timezoneAdjust;
            else
            {
                string strtimezone = SettingMgmt.getSetting("TimeZone-Adjuster");
                Int32.TryParse(strtimezone, out timezone);
            }

            DateTime dtnow = DateTime.Now.ToUniversalTime().AddHours(timezone);
            if (DateTime.Now.IsDaylightSavingTime())
                dtnow = dtnow.AddHours(1);

            return dtnow;
        }
        public static DateTime getLocalTime(DateTime uncTime)
        {
            Int32 timezone = 0;
            string strtimezone = SettingMgmt.getSetting("TimeZone-Adjuster");
            Int32.TryParse(strtimezone, out timezone);

            DateTime dtnow = uncTime.ToUniversalTime().AddHours(timezone);
            if (DateTime.Now.IsDaylightSavingTime())
                dtnow = dtnow.AddHours(1);

            return dtnow;
        }       
        public static string Encrypt(string toEncrypt)
        {
            try
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
            catch (Exception ex)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Common.Encrypt", ex.Message, EventLogEntryType.Error);
            }

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
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public static double FromCelsiusToFahrenheit(double c)
        {
            try
            {
            return Math.Round(((9.0 / 5.0) * c) + 32);
            }
            catch (Exception ex)
            {
                return -1;
            }

        }
        public static double FromFahrenheitToCelsius(double f)
        {
            try
            {
                return Math.Round((5.0 / 9.0) * (f - 32), 2);
            }
            catch (Exception ex)
            {
                return -1;
            }

        }

        public static void prepareDatabase()
        {
            try
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
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Settings(UserID VARCHAR NOT NULL, Name VARCHAR NOT NULL ON CONFLICT REPLACE, Value VARCHAR, DefaultValue VARCHAR, IsEncrypted BOOL DEFAULT false, IsRequired BOOL DEFAULT false, PRIMARY KEY (UserID, Name));";
                        command.ExecuteNonQuery();
                    }

                    //PREPARE DEVICES TABLE
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Devices(UserID VARCHAR NOT NULL, DeviceID VARCHAR NOT NULL ON CONFLICT REPLACE, Name VARCHAR, DisplayName VARCHAR, SubscriptionTopic VARCHAR, SubscriptionExpires DATETIME, subscriptionCapable BOOLEAN NOT NULL DEFAULT 0, Position SMALLINT DEFAULT 1001,  Mfg VARCHAR,  Model VARCHAR,  ModelName VARCHAR, PRIMARY KEY (UserID, DeviceID));";
                        command.ExecuteNonQuery();

                        command.CommandText = "PRAGMA table_info(Devices);";
                        SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        DataRow[] foundRows;

                        foundRows = dt.Select("name = 'Mfg'");
                        if (foundRows.Length == 0)
                        {
                            command.CommandText = "ALTER TABLE Devices ADD COLUMN Mfg VARCHAR;";
                            command.ExecuteNonQuery();
                        }

                        foundRows = dt.Select("name = 'Model'");
                        if (foundRows.Length == 0)
                        {
                            command.CommandText = "ALTER TABLE Devices ADD COLUMN Model VARCHAR;";
                            command.ExecuteNonQuery();
                        }

                        foundRows = dt.Select("name = 'ModelName'");
                        if (foundRows.Length == 0)
                        {
                            command.CommandText = "ALTER TABLE Devices ADD COLUMN ModelName VARCHAR;";
                            command.ExecuteNonQuery();
                        }
                    }

                    //PREPARE ROBOT TABLE
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Robots(UserID VARCHAR NOT NULL, RobotID VARCHAR NOT NULL ON CONFLICT REPLACE, Name VARCHAR, DisplayName VARCHAR, SubscriptionTopic VARCHAR, SubscriptionExpires DATETIME, subscriptionCapable BOOLEAN NOT NULL DEFAULT 0, Position SMALLINT DEFAULT 1001, PRIMARY KEY (UserID, RobotID));";
                        command.ExecuteNonQuery();
                    }

                    //PREPARE GROUP TABLE
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Groups(UserID VARCHAR NOT NULL, GroupID VARCHAR NOT NULL ON CONFLICT REPLACE, Name VARCHAR, DisplayName VARCHAR, SubscriptionTopic VARCHAR, SubscriptionExpires DATETIME, subscriptionCapable BOOLEAN NOT NULL DEFAULT 0, Position SMALLINT DEFAULT 1001, PRIMARY KEY (UserID, GroupID));";
                        command.ExecuteNonQuery();
                    }

                    //PREPARE SHORTCUT TABLE
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Shortcuts(UserID VARCHAR NOT NULL, ShortcutID VARCHAR NOT NULL ON CONFLICT REPLACE, Name VARCHAR, DisplayName VARCHAR, SubscriptionTopic VARCHAR, SubscriptionExpires DATETIME, subscriptionCapable BOOLEAN NOT NULL DEFAULT 0, Position SMALLINT DEFAULT 1001, PRIMARY KEY (UserID, ShortcutID));";
                        command.ExecuteNonQuery();
                    }

                    //PREPARE USER TABLE
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Users(UserID VARCHAR PRIMARY KEY NOT NULL, Email VARCHAR NOT NULL ON CONFLICT REPLACE, Last_Login DATETIME);";
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Common.prepareDatabase", ex.Message, EventLogEntryType.Error);
            }

        }
    }
}