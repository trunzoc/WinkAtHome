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

        public static string getSetting(string KeyName)
        {
            Setting setting = Settings.SingleOrDefault(s => s.key.ToLower().Equals(KeyName.ToLower()));
            if (setting != null)
                return setting.value;
            else
                return null;
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
                        decrypedFile = Common.Decrypt(text);
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
                foreach (Setting setting in Settings)
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
                    Settings.Add(setting);
                }

                string strJSON = "";

                foreach (Setting setting in Settings)
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

        public static string wipeSettings()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt");
            }

            string text = "{\"winkUsername\":\"Username\",\"winkPassword\":\"Password\",\"winkClientID\":\"quirky_wink_android_app\",\"winkClientSecret\":\"e749124ad386a5a35c0ab554a4f2c045\"}";
            string encrypedFile = Common.Encrypt(text);

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.txt", encrypedFile);
            return text;
        }

    }
}