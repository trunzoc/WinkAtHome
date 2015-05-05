using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;

namespace WinkAtHome
{
    public class SettingMgmt
    {
        private static string dbPath = Common.dbPath;

        public class stringbool : Tuple<string, bool>
        {
            public stringbool(string value, bool isEncrypted)
                : base(value, isEncrypted)
            {
            }
        }

        public class Setting
        {
            public string key;
            public string value;
            public bool isEncrypted = false;
            public bool isRequired = false;
        }
        public static List<Setting> Settings
        {
            get
            {
                return loadSettings();
            }
            set
            {
                _settings = value;
            }
        }
        private static List<Setting> _settings;

        public static string getSetting(string KeyName)
        {
            try
            {
                Setting setting = Settings.SingleOrDefault(s => s.key.ToLower().Equals(KeyName.ToLower()));
                if (setting != null)
                    return setting.value;
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
                throw ex; //EventLog.WriteEntry("WinkAtHome.SettingsMgmt.getSetting", ex.Message, EventLogEntryType.Error);
            }
        }

        public static List<Setting> loadSettings(bool forceReset = false)
        {
            try
            {
                if (_settings == null || forceReset)
                {
                    List<Setting> settings = new List<Setting>();

                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
                    {
                        connection.Open();

                        //LEGACY CORRECTIONS
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = "DELETE FROM Settings WHERE Name='winkPassword' or Name='winkUsername'";
                            command.ExecuteNonQuery();

                            if (Wink.winkUser != null)
                            {
                                command.CommandText = "UPDATE Settings SET UserID=@UserID WHERE UserID='single'";
                                command.Parameters.Add(new SQLiteParameter("@UserID", Wink.winkUser.userID));
                                command.ExecuteNonQuery();
                            }
                        }

                        //INSERT DEFAULT SETTINGS
                        Dictionary<string, stringbool> basicSettings = new Dictionary<string, stringbool>();
                        basicSettings.Add("winkClientID", (new stringbool("quirky_wink_android_app", true)));
                        basicSettings.Add("winkClientSecret", (new stringbool("e749124ad386a5a35c0ab554a4f2c045", true)));
                        basicSettings.Add("PubNub-PublishKey", (new stringbool(null, true)));
                        basicSettings.Add("PubNub-SubscribeKey", (new stringbool(null, true)));
                        basicSettings.Add("PubNub-SecretKey", (new stringbool(null, true)));
                        basicSettings.Add("StartPage", (new stringbool("Control.aspx", false)));
                        basicSettings.Add("Hide-Empty-Robots", (new stringbool("false", false)));
                        basicSettings.Add("Hide-Empty-Groups", (new stringbool("false", false)));
                        basicSettings.Add("Robot-Alert-Minutes-Since-Last-Trigger", (new stringbool("60", false)));

                        foreach (KeyValuePair<string, stringbool> pair in basicSettings)
                        {
                            using (SQLiteCommand defaultcommand = new SQLiteCommand(connection))
                            {
                                defaultcommand.CommandText = "UPDATE Settings SET DefaultValue=@defaultvalue, IsEncrypted=@isEncypted, IsRequired=@isRequired WHERE name = @name;";
                                defaultcommand.Parameters.Add(new SQLiteParameter("@name", pair.Key));
                                defaultcommand.Parameters.Add(new SQLiteParameter("@defaultvalue", pair.Value.Item1));
                                defaultcommand.Parameters.Add(new SQLiteParameter("@isEncypted", pair.Value.Item2));
                                defaultcommand.Parameters.Add(new SQLiteParameter("@isRequired", true));
                                defaultcommand.ExecuteNonQuery();

                                defaultcommand.CommandText = "SELECT DISTINCT UserID FROM Users;";
                                SQLiteDataAdapter da = new SQLiteDataAdapter(defaultcommand);
                                DataTable dtTable = new DataTable();
                                da.Fill(dtTable);

                                foreach (DataRow row in dtTable.Rows)
                                {
                                    defaultcommand.CommandText = "INSERT OR IGNORE INTO Settings(UserID,Name,DefaultValue,IsEncrypted,IsRequired) VALUES ('" + row[0] + "',@name,@defaultvalue,@isEncypted,@isRequired)";
                                    defaultcommand.ExecuteNonQuery();
                                }
                            }
                        }

                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            //PROCESS SETTINGS
                            command.CommandText = "select * from Settings WHERE UserID = @UserID";
                            command.Parameters.Add(new SQLiteParameter("@UserID", Wink.winkUser.userID));

                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                Setting setting = new Setting();
                                setting.key = reader["name"].ToString();
                                string isEncrypted = reader["IsEncrypted"].ToString();
                                string isRequired = reader["isRequired"].ToString();

                                if (reader.IsDBNull(reader.GetOrdinal("value")) || string.IsNullOrWhiteSpace(reader["value"].ToString()))
                                    setting.value = reader["defaultvalue"].ToString();
                                else
                                {
                                    if (Convert.ToBoolean(isEncrypted))
                                        setting.value = Common.Decrypt(reader["value"].ToString());
                                    else
                                        setting.value = reader["value"].ToString();
                                }

                                if (Convert.ToBoolean(isEncrypted))
                                    setting.isEncrypted = true;

                                if (Convert.ToBoolean(isRequired))
                                    setting.isRequired = true;

                                settings.Add(setting);
                            }
                        }
                    }

                    _settings = settings;
                }
                return _settings;
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.SettingsMgmt.loadSettings", ex.Message, EventLogEntryType.Error);
            }
        }

        public static void saveSetting(string key, string value)
        {
            try
            {
                Setting setting = Settings.SingleOrDefault(s => s.key == key);
                string newValue = (setting != null && setting.isEncrypted) ? Common.Encrypt(value) : value;
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
                {

                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Settings SET Value=@value WHERE UserID = @UserID AND name = @name;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", Wink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@name", key));
                        command.Parameters.Add(new SQLiteParameter("@value", newValue));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Settings(UserID, Name,Value) VALUES (@UserID, @name,@value)";
                        command.ExecuteNonQuery();

                        command.CommandText = "DELETE FROM Settings WHERE UserID = @UserID AND IFNULL(Value, '') = '' and IFNULL(DefaultValue, '') = '' and IsEncrypted = 'false'";
                        command.ExecuteNonQuery();
                    }
                }

                loadSettings(true);
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.SettingsMgmt.saveSetting", ex.Message, EventLogEntryType.Error);
                throw ex;
            }
        }
        
        public static void saveManualEdit(string json)
        {
            try
            {
                JObject jsonTest = JObject.Parse(json);
                foreach (var jo in jsonTest)
                {
                    saveSetting(jo.Key, jo.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.SettingsMgmt.saveSetting", ex.Message, EventLogEntryType.Error);
                throw ex;
            }
        }

        public static void wipeSettings()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "delete from Settings where UserID = @UserID AND defaultvalue is null";
                        command.Parameters.Add(new SQLiteParameter("@UserID", Wink.winkUser.userID));
                        command.ExecuteNonQuery();

                        command.CommandText = "update Settings set value = null WHERE UserID = @UserID";
                        command.ExecuteNonQuery();

                    }
                }

                loadSettings(true);
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.SettingsMgmt.wipeSettings", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}