using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;

namespace WinkAtHome
{
    public class SettingMgmt
    {
        private static string basicSettings = "{\"winkUsername\":\"Username\",\"winkPassword\":\"Password\",\"winkClientID\":\"quirky_wink_android_app\",\"winkClientSecret\":\"e749124ad386a5a35c0ab554a4f2c045\",\"StartPage\":\"Control.aspx\",\"Hide-Empty-Robots\":\"false\",\"Hide-Empty-Groups\":\"false\"}";
        public class Setting
        {
            public string key;
            public string value;
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

        public static string getSetting(string KeyName, bool requiredSetting = false)
        {
            Setting setting = Settings.SingleOrDefault(s => s.key.ToLower().Equals(KeyName.ToLower()));
            if (setting == null || (requiredSetting && string.IsNullOrWhiteSpace(setting.value)))
            {
                if (requiredSetting)
                {
                    string value = addMissingRequiredSetting(KeyName);
                    return value;
                }
                else
                    return null;
            }
            else
                return setting.value;

        }

        public static List<Setting> loadSettings(bool forceReset = false)
        {
            try
            {
                if (_settings == null || forceReset)
                {
                    string text = string.Empty;
                    string decrypedFile = string.Empty;
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt"))
                    {
                        text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt");
                        try
                        {
                            decrypedFile = Common.Decrypt(text);
                            if (decrypedFile == null)
                                decrypedFile = wipeSettings();
                        }
                        catch
                        {
                            decrypedFile = wipeSettings();
                        }
                    }
                    else
                    {
                        decrypedFile = wipeSettings();
                    }

                    JObject json = JObject.Parse(decrypedFile);

                    List<Setting> settings = new List<Setting>();

                    foreach (var jo in json)
                    {
                        Setting setting = new Setting();
                        setting.key = jo.Key;
                        setting.value = jo.Value.ToString();

                        settings.Add(setting);
                    }
                    _settings = settings;
                }
                return _settings;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void saveSetting(string key, string value)
        {
            try
            {
                bool keyfound = false;
                foreach (Setting setting in _settings)
                {
                    if (setting.key.ToLower() == key.ToLower())
                    {
                        setting.value = value;
                        keyfound = true;
                        break;
                    }
                }

                if (!keyfound)
                {
                    Setting setting = new Setting();
                    setting.key = key;
                    setting.value = value;
                    _settings.Add(setting);
                }

                string strJSON = "";

                foreach (Setting setting in _settings)
                {
                    strJSON += ",\"" + setting.key + "\":\"" + setting.value + "\"" ;
                }
                strJSON = "{" + strJSON.Substring(1) + "}";

                string encrypedFile = Common.Encrypt(strJSON);

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", encrypedFile);

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public static void saveManualEdit(string json)
        {
            try
            {
                JObject jsonTest = JObject.Parse(json);
                string encrypedFile = Common.Encrypt(json);

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", encrypedFile);
                loadSettings(true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static string addMissingRequiredSetting(string keyName)
        {
            JObject settings = JObject.Parse(basicSettings);
            var setting = settings[keyName];
            if (setting != null)
            {
                saveSetting(keyName, setting.ToString());
                return setting.ToString();
            }
            else
                return null;
        }

        public static string wipeSettings()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt"))
            {
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", AppDomain.CurrentDomain.BaseDirectory + "Settings_Backup_" + DateTime.Now.Ticks + ".txt");
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt");
            }

            string encrypedFile = Common.Encrypt(basicSettings);

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", encrypedFile);

            _settings = null;
            
            return basicSettings;


            //string text = "{ \"Required_Settings\": [ { \"setting_name\": \"winkUsername\", \"setting_value\": \"Username\", \"encrypted\": \"true\", }, { \"setting_name\": \"winkPassowrd\", \"setting_value\": \"Password\", \"encrypted\": \"true\", }, { \"setting_name\": \"winkClientID\", \"setting_value\": \"quirky_wink_android_app\", \"encrypted\": \"true\", }, { \"setting_name\": \"winkClientSecret\", \"setting_value\": \"e749124ad386a5a35c0ab554a4f2c045\", \"encrypted\": \"true\", }, { \"setting_name\": \"StartPage\", \"setting_value\": \"Control.aspx\", \"encrypted\": \"false\", }, { \"setting_name\": \"Hide-Empty-Robots\", \"setting_value\": \"false\", \"encrypted\": \"false\", }, { \"setting_name\": \"Hide-Empty-Groups\", \"setting_value\": \"false\", \"encrypted\": \"false\", } ], \"Optional_Settings\": [ ]}";
            //JObject settings = JObject.Parse(text);
            //foreach (var setting in settings["Required_Settings"])
            //{
            //    if (setting["encrypted"].ToString() == "true")
            //    {
            //        setting["setting_value"] = Common.Encrypt(setting["setting_value"].ToString());
            //    }
            //}

            //File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", settings.ToString());
            //return text;
        }
    }
}