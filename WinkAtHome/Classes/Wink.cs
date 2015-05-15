using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using WinkAtHome;

public class Wink
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SimplePropertyAttribute : Attribute
    {
    }

#region Public Functions
    public static Wink myWink
    {
        get { return HttpContext.Current.Session["_wink"] == null ? createWink() : (Wink)HttpContext.Current.Session["_wink"]; }
        set { HttpContext.Current.Session["_wink"] = value; }
    }

    private static Wink createWink()
    {
        if (HttpContext.Current.Session["_wink"] == null)
            HttpContext.Current.Session["_wink"] = new Wink();
        
        return (Wink)HttpContext.Current.Session["_wink"];
    }
    public string getSubscriptionTopics()
    {
        string subTopics = string.Empty;
        foreach (Device device in Devices)
        {
            if (!string.IsNullOrWhiteSpace(device.subscriptionTopic))
                subTopics += "," + device.subscriptionTopic;
        }
        foreach (Robot robot in Robots)
        {
            if (!string.IsNullOrWhiteSpace(robot.subscriptionTopic))
                subTopics += "," + robot.subscriptionTopic;
        }
        foreach (Shortcut shortcut in Shortcuts)
        {
            if (!string.IsNullOrWhiteSpace(shortcut.subscriptionTopic))
                subTopics += "," + shortcut.subscriptionTopic;
        }
        foreach (Group group in Groups)
        {
            if (!string.IsNullOrWhiteSpace(group.subscriptionTopic))
                subTopics += "," + group.subscriptionTopic;
        }
        if (subTopics.Length > 0)
            return subTopics.Substring(1);

        return null;
    }
    public bool validateWinkCredentialsByUsername(string username, string password)
    {
        try
        {
            if (HttpContext.Current.Session["_wink"] == null)
                createWink(); 
            
            string token = winkGetTokenByUsername(username, password, true, ConfigurationManager.AppSettings["APIClientID"], ConfigurationManager.AppSettings["APIClientSecret"]);
            if (token != null)
            {
                winkUser.password = Common.Encrypt(password);

                return true;
            }
        }
        catch (Exception ex)
        {
        }

        return false;
    }
    public bool validateWinkCredentialsByAuthCode(string AuthCode)
    {
        try
        {
            if (HttpContext.Current.Session["_wink"] == null)
                createWink();

            string token = winkGetTokenByAuthToken(AuthCode, true, ConfigurationManager.AppSettings["APIClientID"], ConfigurationManager.AppSettings["APIClientSecret"]);
            if (token != null)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
        }

        return false;
    }
    public bool validateWinkCredentialsByToken(string Token)
    {
        try
        {
            if (HttpContext.Current.Session["_wink"] == null)
                createWink();

            HttpContext.Current.Session["_winkToken"] = Token;

            User user = winkGetUser(true);

            if (user != null)
                return true;
        }
        catch (Exception ex)
        {
        }

        return false;
    }
    public void clearWink()
    {
        myWink= new Wink();
    }
    public void reloadWink(bool clearFirst = true)
    {
        winkGetDevices(null,true);
        winkGetShortcuts(true);
        winkGetGroups(true);
        winkGetRobots(null,true);
    }
    public static Dictionary<string, string>[] winkGetServerStatus()
    {
        try
        {
            Dictionary<string, string> dictStatus = new Dictionary<string, string>();
            Dictionary<string, string> dictIncidents = new Dictionary<string, string>();
            Dictionary<string, string> dictInfo = new Dictionary<string, string>();

            JObject jsonResponse = myWink.winkCallAPI(ConfigurationManager.AppSettings["winkStatusURL"], "", "", false);

            foreach (var data in jsonResponse["components"])
            {
                dictStatus.Add(data["name"].ToString(), data["status"].ToString());
            }

            foreach (var data in jsonResponse["incidents"])
            {
                dictIncidents.Add(data["name"].ToString(), data["status"].ToString());
            }

            string strLastUpdate = jsonResponse["page"]["updated_at"].ToString();
            dictInfo.Add("LastUpdated", strLastUpdate);

            return new Dictionary<string, string>[] { dictStatus, dictIncidents, dictInfo };
        }
        catch
        {
            return null;
        }
    }
#endregion

#region token
    private string WinkToken
    {
        get { return HttpContext.Current.Session["_winkToken"] == null ? winkGetTokenByUsername() : (string)HttpContext.Current.Session["_winkToken"]; }
        set { HttpContext.Current.Session["_winkToken"] = value; }
    }

    private string winkGetTokenByUsername(string Username = null, string Password = null, bool forceRefresh = false,  string forceClientID = null, string forceClientSecret = null)
    {
        try
        {
            string token = string.Empty;

            if (forceRefresh || HttpContext.Current == null || (HttpContext.Current != null && HttpContext.Current.Session["_winkToken"] == null))
            {
                string winkUsername = Username == null ? winkUser.email : Username;
                string winkPassword = Password == null ? Common.Decrypt(winkUser.password) : Password;
                string winkClientID = forceClientID == null ? ConfigurationManager.AppSettings["APIClientID"] : forceClientID;
                string winkClientSecret = forceClientSecret == null ? ConfigurationManager.AppSettings["APIClientSecret"] : forceClientSecret;

                string oAuthURL = ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkOAuthURL"];
                string sendstring = "{\"client_id\":\"" + winkClientID + "\",\"client_secret\":\"" + winkClientSecret + "\",\"username\":\"" + winkUsername + "\",\"password\":\"" + winkPassword + "\",\"grant_type\":\"password\"}";

                JObject jsonResponse = winkCallAPI(oAuthURL, "POST", sendstring, false);

                token = jsonResponse["access_token"].ToString();

                HttpContext.Current.Session["_winkToken"] = token;

                winkGetUser(true);
            }
            else
                token = HttpContext.Current.Session["_winkToken"].ToString();


            return token;
        }
        catch
        {
        }

        return null;
    }
    private string winkGetTokenByAuthToken(string AuthToken, bool forceRefresh = false, string forceClientID = null, string forceClientSecret = null)
    {
        try
        {
            string token = string.Empty;

            if (forceRefresh || HttpContext.Current == null || (HttpContext.Current != null && HttpContext.Current.Session["_winkToken"] == null))
            {
                string winkClientID = forceClientID == null ? ConfigurationManager.AppSettings["APIClientID"] : forceClientID;
                string winkClientSecret = forceClientSecret == null ? ConfigurationManager.AppSettings["APIClientSecret"] : forceClientSecret;

                string oAuthURL = ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkOAuthURL"];
                string sendstring = "{\"client_id\":\"" + winkClientID + "\",\"client_secret\":\"" + winkClientSecret + "\",\"grant_type\":\"authorization_code\",\"code\":\"" + AuthToken + "\"}";

                JObject jsonResponse = winkCallAPI(oAuthURL, "POST", sendstring, false);

                token = jsonResponse["access_token"].ToString();

                HttpContext.Current.Session["_winkToken"] = token;

                winkGetUser(true);
            }
            else
                token = HttpContext.Current.Session["_winkToken"].ToString();


            return token;
        }
        catch
        {
        }

        return null;
    }
#endregion

#region user
    public class User
    {
        [SimpleProperty]
        public string userID { get; set; }
        [SimpleProperty]
        public string first_name { get; set; }
        [SimpleProperty]
        public string last_name { get; set; }
        [SimpleProperty]
        public string email { get; set; }
        [SimpleProperty]
        public string password { get; set; }
    }
    public User winkUser
    {
        get { return winkGetUser(); }
        set { HttpContext.Current.Session["_winkUser"] = value; }
    }

    private User winkGetUser(bool forceRefresh = false)
    {
        try
        {
            User user = new User();

            if (forceRefresh || HttpContext.Current.Session["_winkUser"] == null)
            {
                string URL = ConfigurationManager.AppSettings["winkRootURL"] + "users/me";

                JObject jsonResponse = winkCallAPI(URL);

                if (jsonResponse != null)
                {
                    user.userID = jsonResponse["data"]["user_id"].ToString();
                    user.first_name = jsonResponse["data"]["first_name"].ToString();
                    user.last_name = jsonResponse["data"]["last_name"].ToString();
                    user.email = jsonResponse["data"]["email"].ToString();

                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                    {
                        connection.Open();

                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = "INSERT OR REPLACE INTO Users (UserID, Email, Last_Login) VALUES (@UserID,@Email,@Last_Login);";
                            command.Parameters.Add(new SQLiteParameter("@UserID", user.userID));
                            command.Parameters.Add(new SQLiteParameter("@Email", user.email));
                            command.Parameters.Add(new SQLiteParameter("@Last_Login", DateTime.Now));
                            command.ExecuteNonQuery();

                            //LEGACY CORRECTIONS
                            command.CommandText = "UPDATE Devices SET UserID=@UserID WHERE UserID='single'";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE Groups SET UserID=@UserID WHERE UserID='single'";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE Robots SET UserID=@UserID WHERE UserID='single'";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE Shortcuts SET UserID=@UserID WHERE UserID='single'";
                            command.ExecuteNonQuery();
                        }
                    }

                    HttpContext.Current.Session["_winkUser"] = user;
                }
                else
                    return null;
            }
            else
                user = (User)HttpContext.Current.Session["_winkUser"];

            return user;
        }
        catch
        {
            return null;
        }
    }
#endregion

#region Device
    public class Device
    {
        [SimpleProperty]
        public string name { get; set; }
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string displayName { get; set; }
        [SimpleProperty]
        public string type { get; set; }
        [SimpleProperty]
        public string menu_type { get; set; }
        [SimpleProperty]
        public string sensor_type { get; set; }
        [SimpleProperty]
        public bool issensor { get; set; }
        [SimpleProperty]
        public string sensortripped { get; set; }
        [SimpleProperty]
        public bool iscontrollable { get; set; }
        [SimpleProperty]
        public string manufacturer { get; set; }
        [SimpleProperty]
        public string model { get; set; }
        [SimpleProperty]
        public string radio_type { get; set; }
        [SimpleProperty]
        public bool isvariable { get; set; }
        [SimpleProperty]
        public bool update_needed { get; set; }
        [SimpleProperty]
        public string hub_id { get; set; }
        [SimpleProperty]
        public string hub_name { get; set; }

        public string json;
        public bool subscriptionCapable = true;
        public string subscriptionTopic;
        public DateTime subscriptionExpires;
        public int position = 1001;
        public List<string> desired_states = new List<string>();
        public List<DeviceStatus> sensor_states = new List<DeviceStatus>();
        public List<DeviceStatus> status = new List<DeviceStatus>();
        public class DeviceStatus
        {
            public string id;
            public string name;
            public string current_status;
            public DateTime? last_updated;

        }

        public static Device getDeviceByID(string deviceID)
        {
            Device device = myWink._devices.SingleOrDefault(Device => Device.id.Equals(deviceID));
            return device;
        }
        
        public static Device getDeviceByName(string deviceName)
        {
            Device device = myWink._devices.SingleOrDefault(Device => Device.name.ToLower().Equals(deviceName.ToLower()));
            return device;
        }
                
        public static List<Device> getDevicesByHubID(string hubID)
        {
            List<Device> devices = myWink._devices.Where(device => device.hub_id == hubID).ToList();

            return devices;
        }
       
        public static List<string> getDeviceTypes(bool forMenu = false)
        {
            List<string> types = null;
            if (forMenu)
                types = myWink.Devices.Select(t => t.menu_type).Distinct().ToList();
            else
                types = myWink.Devices.Select(t => t.type).Distinct().ToList();
            
            if (types != null)
                types.Sort();

            return types;
        }
        
        public static void updateDevice(JObject json)
        {
            myWink.winkGetDevices(json);
        }
        
        public static JObject getDeviceJSON()
        {
            JObject json = myWink.winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetAllDevicesURL"]);
            if (json != null)
                return json;

            return null;
        }

        public static string setDeviceDisplayName(string DeviceID, string DisplayName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Devices SET displayname=@displayname WHERE UserID = @UserID AND DeviceID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", DeviceID));
                        command.Parameters.Add(new SQLiteParameter("@displayname", DisplayName));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Devices (UserID,DeviceID, displayname) VALUES (@UserID,@ID, @displayname);";
                        command.ExecuteNonQuery();
                    }
                }

                Device device = myWink._devices.SingleOrDefault(d => d.id == DeviceID);
                device.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception ex)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setDeviceDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public static int setDevicePosition(string DeviceID, int Position)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Devices SET position=@Position WHERE UserID = @UserID AND DeviceID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", DeviceID));
                        command.Parameters.Add(new SQLiteParameter("@Position", Position));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Devices (UserID,DeviceID, position) VALUES (@UserID, @ID, @Position);";
                        command.ExecuteNonQuery();
                    }
                }

                return Position;
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setDevicePosition", ex.Message, EventLogEntryType.Error);
                return -1;
            }
        }

        internal static Device getDesired_States(Device device, JObject states)
        {
            if (states != null)
            {
                foreach (var state in states)
                {
                    device.desired_states.Add(state.Key);
                }

                if (device.desired_states.Count > 0)
                {
                    device.iscontrollable = true;
                }
                else if (device.desired_states.Count == 0)
                {
                    device.issensor = true;
                }

                if (states.ToString().Contains("brightness") || states.ToString().Contains("position"))
                {
                    device.isvariable = true;
                }
            }

            return device;
        }
        internal static Device getLast_Readings(Device device, JObject readings)
        {
            List<Device.DeviceStatus> states = new List<Device.DeviceStatus>();
            device.status = new List<Device.DeviceStatus>();

            if (readings != null)
            {
                foreach (var reading in readings)
                {
                    if (!reading.Key.Contains("_updated_at"))
                    {
                        Device.DeviceStatus deviceStatus = new Device.DeviceStatus();
                        deviceStatus.id = device.id;
                        deviceStatus.name = reading.Key;
                        deviceStatus.current_status = reading.Value.ToString();

                        if (readings[reading.Key + "_updated_at"] != null)
                        {
                            string lastupdated = readings[reading.Key + "_updated_at"].ToString();
                            deviceStatus.last_updated = Common.FromUnixTime(lastupdated);
                        }

                        device.status.Add(deviceStatus);
                    }
                }
            }

            return device;
        }

        internal static void sendDeviceCommand(string deviceID, string command)
        {
            try
            {
                Device device = Device.getDeviceByID(deviceID);
                if (device != null)
                {
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + device.type + "/" + device.id;
                    myWink.winkCallAPI(url, "PUT", command);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
    public List<Device> Devices
    {
        get
        {
            return winkGetDevices();
        }
        set
        {
            _devices = value;
        }
    }
    private List<Device> _devices;
    public bool hasDeviceChanges;

    private List<Device> winkGetDevices(JObject jsonObject = null, bool forceRefresh = false)
    {
        try
        {
           
            bool firstRun = false;
            if (_devices == null || jsonObject != null || forceRefresh)
            {
                List<Device> Devices = new List<Device>();
                JObject json = null;

                if (_devices == null || forceRefresh)
                {
                    firstRun = true;
                    json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetAllDevicesURL"]);
                }
                else if (jsonObject != null)
                {
                    json = jsonObject;
                    Devices = _devices;
                }

                if (json != null)
                {
                    foreach (JObject data in json["data"])
                    {
                        IEnumerable<string> ikeys = data.Properties().Select(p => p.Name);
                        if (ikeys != null)
                        {
                            List<string> keys = ikeys.ToList();

                            string desired_states = string.Empty;
                            string last_readings = string.Empty;

                            string typeName = keys[0];

                            Device device = new Device();
                            device.json = data.ToString();
                            device.name = data["name"] != null ? data["name"].ToString() : "error: name";
                            if (jsonObject != null)
                            {
                                device = Device.getDeviceByName(device.name);
                                Devices.Remove(device);
                            }
                            else
                            {
                                device.iscontrollable = false;

                                device.id = data[typeName] != null ? data[typeName].ToString() : "error: typeName";
                                device.displayName = device.name;
                                device.type = data[typeName] != null ? typeName.Replace("_id", "s").Replace("switchs", "switches") : "error: type";
                                device.menu_type = device.type;

                                if (keys.Contains("device_manufacturer"))
                                {
                                    device.manufacturer = data["device_manufacturer"].ToString();
                                }

                                if (keys.Contains("radio_type"))
                                {
                                    device.radio_type = data["radio_type"].ToString();
                                }

                                if (keys.Contains("model_name"))
                                {
                                    device.model = data["model_name"].ToString();
                                }

                                if (keys.Contains("update_needed"))
                                {
                                    if (!string.IsNullOrWhiteSpace(data["update_needed"].ToString()))
                                    {
                                        string update = data["update_needed"].ToString();
                                        device.update_needed = Convert.ToBoolean(update);
                                    }
                                }
                                
                                if (keys.Contains("hub_id"))
                                {
                                    device.hub_id = data["hub_id"].ToString();
                                }
                            }

                            if (keys.Contains("desired_state"))
                            {
                                JObject states = (JObject)data["desired_state"];
                                desired_states = states.ToString();

                                Device.getDesired_States(device, states);
                            }
                            else
                            {
                                device.issensor = true;
                            }


                            if (keys.Contains("last_reading"))
                            {
                                JObject readings = (JObject)data["last_reading"];
                                last_readings = readings.ToString();

                                Device.getLast_Readings(device, readings);
                            }


                            //FIX THE BROKEN API TEMPORARY FIX
                            if (device.type == "light_bulbs" || device.type == "binary_switches")
                            {
                                device.issensor = false;
                                device.iscontrollable = true;
                                device.desired_states.Add("powered");

                                if (device.type == "light_bulbs")
                                {
                                    device.isvariable = true;
                                    device.desired_states.Add("brightness");
                                }
                            }
                            //END BROKEN API FIX

                            #region NON-SENSOR DEVICE-SPECIFIC CONFIGURATIONS
                            //DEVICE EXCEPTIONS

                            //Power Pivot Genius
                            if (keys.Contains("powerstrip_id"))
                            {
                                device.id = data["powerstrip_id"].ToString();
                                device.type = "powerstrips";
                                device.menu_type = device.type;
                                device.issensor = false;
                                foreach (Device.DeviceStatus status in device.status)
                                    status.id = device.id;

                                foreach (var outlet in data["outlets"])
                                {
                                    Device outletdevice = new Device();
                                    outletdevice.json = outlet.ToString();
                                    outletdevice.id = outlet["outlet_id"].ToString();
                                    outletdevice.name = device.name + " - " + outlet["name"].ToString();
                                    outletdevice.displayName = outletdevice.name;
                                    outletdevice.type = "outlets";
                                    outletdevice.menu_type = "powerstrips";
                                    outletdevice.manufacturer = device.manufacturer;
                                    outletdevice.radio_type = device.radio_type;
                                    outletdevice.model = device.model;

                                    JObject states = (JObject)outlet["desired_state"];
                                    Device.getDesired_States(outletdevice, states);

                                    JObject readings = (JObject)outlet["last_reading"];
                                    Device.getLast_Readings(outletdevice, readings);

                                    Device.DeviceStatus connstatus = device.status.Single(s => s.name == "connection");
                                    outletdevice.status.Add(connstatus);

                                    Devices.Add(outletdevice);
                                }
                            }

                            //Relay
                            if (device.type == "buttons" || device.type == "gangs")
                            {
                                device.issensor = false;
                                device.menu_type = "unknown_devices";
                            }

                            //Relay Switches
                            if (device.type == "binary_switches" && device.radio_type == "project_one")
                            {
                                Device.DeviceStatus status = device.status.SingleOrDefault(s => s.name == "powering_mode");
                                if (status.current_status=="none")
                                {
                                    device.iscontrollable = false;
                                }
                            }

                            //remotes
                            if (device.type == "remotes")
                            {
                                device.issensor = false;
                            }

                            //garage door openers
                            if (device.type == "garage_doors")
                            {
                                device.isvariable = false;
                            }

                            //lock PINs
                            if (keys.Contains("key_id") && keys.Contains("parent_object_type") && data["parent_object_type"].ToString().ToLower() == "lock")
                            {
                                device.id = data["key_id"] != null ? data["key_id"].ToString() : "error: key_id";
                                device.name = data["name"] != null ? data["name"].ToString() : "error: name";
                                device.type = "lock_pins";
                                device.issensor = false;
                                device.menu_type = "locks";
                                foreach (Device.DeviceStatus status in device.status)
                                    status.id = device.id;
                            }
                            #endregion

                            #region SENSOR DEVICE CONFIGURATIONS
                            List<string> capabilities = new List<string>();

                            //Sensor Pods
                            if (keys.Contains("sensor_pod_id"))
                            {
                                device.id = data["sensor_pod_id"].ToString();
                                device.type = "sensor_pods";
                                device.menu_type = device.type;
                                foreach (Device.DeviceStatus status in device.status)
                                    status.id = device.id;
                            }

                            //hubs
                            if (device.type == "hubs")
                            {
                                device.issensor = true;
                                device.iscontrollable = false;

                                if (device.model.ToLower() == "wink relay")
                                    device.type = "wink_relays";
                            }

                            //refuel
                            if (device.type == "propane_tanks")
                            {
                                if (keys.Contains("tank_changed_at"))
                                {
                                    Device.DeviceStatus tankstatus = new Device.DeviceStatus();
                                    tankstatus.id = device.id;
                                    tankstatus.name = "tank_changed_at";
                                    tankstatus.current_status = Common.FromUnixTime(data["tank_changed_at"].ToString()).ToString();
                                    tankstatus.last_updated = Common.FromUnixTime(data["tank_changed_at"].ToString());
                                    device.status.Add(tankstatus);
                                }

                                Wink.Device.DeviceStatus stat = device.status.SingleOrDefault(p => p.name == "remaining");
                                if (stat != null)
                                {
                                    Double converted = Convert.ToDouble(stat.current_status) * 100;
                                    string degree = converted.ToString();

                                    string imgDegree = "100";
                                    if (converted <= 10)
                                        imgDegree = "0";
                                    else if (converted <= 30)
                                        imgDegree = "25";
                                    else if (converted <= 60)
                                        imgDegree = "50";
                                    else if (converted <= 90)
                                        imgDegree = "75";
                                    else
                                        imgDegree = "100";

                                    device.sensortripped = imgDegree;
                                }

                            }

                            //POS
                            if (device.model != null && device.model.ToLower() == "egg minder")
                            {
                                device.id = data["eggtray_id"] != null ? data["eggtray_id"].ToString() : "error: key_id";
                                device.type = "eggtray";
                                device.menu_type = "eggtray";

                                capabilities.Add("inventory");
                                capabilities.Add("age");
                            }

                            if (device.type == "smoke_detectors")
                            {
                                device.sensortripped = null;
                                    
                                string strStatuses = string.Empty;

                                Wink.Device.DeviceStatus costat = device.status.SingleOrDefault(p => p.name == "co_detected");
                                if (costat != null)
                                    strStatuses += costat.current_status;

                                Wink.Device.DeviceStatus smstat = device.status.SingleOrDefault(p => p.name == "smoke_detected");
                                if (smstat != null)
                                    strStatuses += smstat.current_status;

                                Wink.Device.DeviceStatus teststat = device.status.SingleOrDefault(p => p.name == "test_activated");
                                if (teststat != null && teststat.current_status.ToLower() == "true")
                                    strStatuses += "test";

                                if (strStatuses.ToLower().Contains("true"))
                                    device.sensortripped = "true";
                                else if (strStatuses.ToLower().Contains("test"))
                                    device.sensortripped = "test";

                                device.sensor_type = device.type;

                                capabilities.Add("co_detected");
                                capabilities.Add("smoke_detected");
                            }                            
                            
                            if (device.issensor)
                            {
                                string model = device.model.ToLower();
                                List<Wink.Device.DeviceStatus> OpenClose = device.status.Where(s => s.name == "opened").ToList();

                                if (OpenClose.Count > 0)
                                {
                                    Wink.Device.DeviceStatus stat = device.status.Single(p => p.name == "opened");
                                    device.sensortripped = stat.current_status.ToLower();

                                    string type = "dooropen";
                                    string lowername = device.name.ToLower();
                                    if (lowername.Contains("window"))
                                        type = "windowopen";
                                    else if (lowername.Contains("patio") || lowername.Contains("deck"))
                                        type = "deckopen";
                                    else if (lowername.Contains("cabinet"))
                                        type = "cabinetopen";

                                    device.sensor_type = type;
                                }
                                else if (model == "spotter")
                                {
                                    device.sensor_type = "spotter";
                                }
                                else if (model.Contains("pir") || model.Contains("infrared") || model.Contains("motion"))
                                {
                                    device.sensor_type = "motion_sensor";
                                }
                                
                                if (string.IsNullOrWhiteSpace(device.sensor_type))
                                {
                                    device.sensor_type = device.type;
                                }

                                //CAPABILITIES
                                if (device.menu_type != "hubs")
                                {
                                    if (keys.Contains("capabilities"))
                                    {
                                        var capabilityList = data["capabilities"];
                                        if (capabilityList != null)
                                        {
                                            var sensor_types = capabilityList["sensor_types"];
                                            if (sensor_types != null)
                                            {
                                                foreach (var type in sensor_types)
                                                {
                                                    string sensorname = type["field"].ToString();
                                                    //if (sensorname != "battery" && sensorname != "external_power")
                                                        capabilities.Add(sensorname);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (capabilities.Count > 0)
                                {
                                    foreach (string capability in capabilities)
                                    {
                                        Device.DeviceStatus status = device.status.SingleOrDefault(s => s.name == capability);
                                        if (status != null)
                                        {
                                            string strValue = status.current_status;
                                            Int32 intValue = 0;
                                            Int32.TryParse(strValue, out intValue);

                                            if (status.name == "temperature")
                                            {
                                                double temp = 0;
                                                double.TryParse(strValue ,out temp);
                                                temp = Common.FromCelsiusToFahrenheit(temp);
                                                status.current_status = Convert.ToString(temp);
                                            }

                                            if (intValue > 1000000000)
                                                status.current_status = Common.FromUnixTime(strValue).ToString();

                                            device.sensor_states.Add(status);
                                        }
                                    }
                                }
                            }
                            #endregion

                            Devices.Add(device);
                            
                            //UPDATE DEVICE DB
                            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                            {
                                connection.Open();

                                using (SQLiteCommand command = new SQLiteCommand(connection))
                                {
                                    command.CommandText = "UPDATE Devices SET name=@name WHERE UserID=@UserID AND DeviceID = @ID AND Name<>@name;";
                                    command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                    command.Parameters.Add(new SQLiteParameter("@ID", device.id));
                                    command.Parameters.Add(new SQLiteParameter("@name", device.name));
                                    command.ExecuteNonQuery();

                                    command.CommandText = "INSERT OR IGNORE INTO Devices (UserID, DeviceID, name) VALUES (@UserID, @ID, @name);";
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                _devices = Devices.OrderBy(c => !c.iscontrollable).ThenBy(c => c.name).ToList();
            }

            #region RETRIEVE DATABASE VALUES & POST-LOAD PROCESSING
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    DataTable dt = new DataTable();
                    command.CommandText = "SELECT * FROM Devices WHERE UserID=@UserID";
                    command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                    SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                    da.Fill(dt);

                    foreach (Device device in _devices)
                    {
                        //VALUE FROM DATABASE
                        DataRow[] rowArray = dt.Select("DeviceID = '" + device.id + "'");
                        if (rowArray.Length > 0)
                        {
                            DataRow row = rowArray[0];

                            device.position = Convert.ToInt32(row["Position"].ToString());

                            device.subscriptionCapable = Convert.ToBoolean(row["subscriptionCapable"].ToString());

                            device.subscriptionTopic = row["SubscriptionTopic"].ToString();

                            DateTime expires = new DateTime();
                            string date = row["SubscriptionExpires"].ToString();
                            DateTime.TryParse(date, out expires);
                            device.subscriptionExpires = Convert.ToDateTime(expires);

                            if (!string.IsNullOrWhiteSpace(row["DisplayName"].ToString()))
                                device.displayName = row["DisplayName"].ToString();
                        }
                    }
                }
            }
            #endregion


            #region PUBNUB SUBSCRIPTIONS
            if (PubNub.myPubNub.hasPubNub)
            {
                foreach (Device device in _devices)
                {
                    if ((string.IsNullOrWhiteSpace(device.subscriptionTopic) || DateTime.Now > device.subscriptionExpires) && (device.subscriptionCapable || firstRun))
                    {
                        string URL = ConfigurationManager.AppSettings["winkRootURL"] + device.type + "/" + device.id + "/subscriptions";
                        string sendCommand = "{\"publisher_key\":\"" + PubNub.myPubNub.publishKey + "\",\"subscriber_key\":\"" + PubNub.myPubNub.subscriberKey + "\"}";
                        JObject subJSON = winkCallAPI(URL, "POST", sendCommand);
                        if (subJSON != null)
                        {
                            string subCapable = string.Empty;

                            if (subJSON.ToString().Contains("404") && !firstRun)
                            {
                                subCapable = "0";
                                device.subscriptionCapable = false;
                            }
                            else if (subJSON["data"] != null)
                            {
                                subCapable = "1";
                                device.subscriptionCapable = true;

                                if (subJSON["data"]["topic"] != null)
                                    device.subscriptionTopic = subJSON["data"]["topic"].ToString();

                                if (subJSON["data"]["expires_at"] != null)
                                    device.subscriptionExpires = Common.FromUnixTime(subJSON["data"]["expires_at"].ToString());
                            }

                            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                            {
                                connection.Open();
                                using (SQLiteCommand command = new SQLiteCommand(connection))
                                {
                                    command.CommandText = "UPDATE Devices SET subscriptionTopic=@subscriptionTopic,subscriptionExpires=@subscriptionExpires,subscriptionCapable=@subscriptionCapable WHERE UserID=@UserID AND DeviceID = @ID;";
                                    command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                    command.Parameters.Add(new SQLiteParameter("@ID", device.id));
                                    command.Parameters.Add(new SQLiteParameter("@subscriptionTopic", device.subscriptionTopic));
                                    command.Parameters.Add(new SQLiteParameter("@subscriptionExpires", device.subscriptionExpires));
                                    command.Parameters.Add(new SQLiteParameter("@subscriptionCapable", subCapable));
                                    command.ExecuteNonQuery();

                                    command.CommandText = "INSERT OR IGNORE INTO Devices(UserID,DeviceID,subscriptionTopic,subscriptionExpires,subscriptionCapable) VALUES (@UserID, @ID,@subscriptionTopic,@subscriptionExpires,@subscriptionCapable)";
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region FINAL PROCESSING
            foreach (Device device in _devices)
            {
                Device hubDevice = _devices.SingleOrDefault(h => h.id == device.hub_id);
                if (hubDevice != null)
                    device.hub_name = hubDevice.displayName;
            }
            #endregion

            return _devices;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
#endregion

#region Shortcut
    public class Shortcut
    {
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string name { get; set; }
        [SimpleProperty]
        public string displayName { get; set; }

        public string json;
        public bool subscriptionCapable = true;
        public string subscriptionTopic;
        public DateTime subscriptionExpires;
        public int position = 1001;

        public List<ShortcutMember> members = new List<ShortcutMember>();
        public class ShortcutMember
        {
            public string id;
            public string type;
            public Dictionary<string, string> actions = new Dictionary<string, string>();
        }

        public static Shortcut getShortcutByID(string ShortcutID)
        {
            Shortcut shortcut = myWink.Shortcuts.SingleOrDefault(s => s.id.Equals(ShortcutID));
            return shortcut;
        }

        public static Shortcut getShortcutByName(string shortcutName)
        {
            Shortcut shortcut = myWink.Shortcuts.SingleOrDefault(s => s.name.ToLower().Equals(shortcutName.ToLower()));
            return shortcut;
        }

        public static string setShortcutDisplayName(string ShortcutID, string DisplayName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Shortcuts SET displayname=@displayname WHERE UserID = @UserID AND ShortcutID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", ShortcutID));
                        command.Parameters.Add(new SQLiteParameter("@displayname", DisplayName));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Shortcuts (UserID,ShortcutID, displayname) VALUES (@UserID, @ID, @displayname);";
                        command.ExecuteNonQuery();
                    }
                }
                Shortcut shortcut = myWink._shortcuts.SingleOrDefault(d => d.id == ShortcutID);
                shortcut.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception ex)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setShortcutDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public static int setShortcutPosition(string ShortcutID, int Position)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Shortcuts SET position=@Position WHERE UserID = @UserID AND ShortcutID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", ShortcutID));
                        command.Parameters.Add(new SQLiteParameter("@Position", Position));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Shortcuts (UserID,ShortcutID, position) VALUES (@UserID, @ID, @Position);";
                        command.ExecuteNonQuery();
                    }
                }

                return Position;
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setShortcutPosition", ex.Message, EventLogEntryType.Error);
                return -1;
            }
        }
        internal static void activateShortcut(string shortcutID)
        {
            try
            {
                Shortcut shortcut = Shortcut.getShortcutByID(shortcutID);

                if (shortcut != null)
                {
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + "scenes/" + shortcut.id + "/activate/";
                    myWink.winkCallAPI(url, "POST");

                    List<Shortcut.ShortcutMember> members = shortcut.members;
                    foreach (Shortcut.ShortcutMember member in members)
                    {
                        List<Device> devices = new List<Device>();

                        if (member.type == "group")
                        {
                            Group group = Group.getGroupByID(member.id);
                            if (group != null)
                            {
                                foreach (KeyValuePair<string, string> entry in member.actions)
                                {
                                    Group.GroupStatus status = group.status.SingleOrDefault(n => n.name == entry.Key);
                                    if (status != null)
                                        status.current_status = entry.Value;
                                }

                                foreach (Group.GroupMember groupmember in group.members)
                                {
                                    Device device = Device.getDeviceByID(groupmember.id);
                                    if (device != null)
                                        devices.Add(device);
                                }
                            }
                        }
                        else
                        {
                            Device device = Device.getDeviceByID(member.id);
                            if (device != null)
                                devices.Add(device);
                        }

                        foreach (Device device in devices)
                        {
                            foreach (KeyValuePair<string, string> entry in member.actions)
                            {
                                Device.DeviceStatus status = device.status.SingleOrDefault(p => p.name == entry.Key);
                                if (status != null)
                                    status.current_status = entry.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public List<Shortcut> Shortcuts
    {
        get
        {
            return winkGetShortcuts();
        }
        set
        {
            _shortcuts = value;
        }
    }
    private List<Shortcut> _shortcuts;
    public bool hasShortcutChanges;

    private List<Shortcut> winkGetShortcuts(bool forceRefresh = false)
    {
        try
        {
            bool firstRun = false;
            if (myWink._shortcuts == null || forceRefresh)
            {
                firstRun = true;

                JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetShortcutsURL"]);
                List<Shortcut> Shortcuts = new List<Shortcut>();

                foreach (JObject data in json["data"])
                {
                    IList<string> keys = data.Properties().Select(p => p.Name).ToList();
                    string typeName = keys[0];

                    Shortcut shortcut = new Shortcut();

                    shortcut.id = data[typeName].ToString();
                    shortcut.name = data["name"].ToString();
                    shortcut.displayName = shortcut.name;
                    shortcut.json = data.ToString();

                    if (keys.Contains("members"))
                    {
                        var members = data["members"];
                        foreach (var member in members)
                        {
                            Shortcut.ShortcutMember newmember = new Shortcut.ShortcutMember();
                            newmember.id = member["object_id"].ToString();
                            newmember.type = member["object_type"].ToString();

                            var states = member["desired_state"];
                            foreach (JProperty state in states)
                            {
                                newmember.actions.Add(state.Name.ToString(), state.Value.ToString());
                            }
                            shortcut.members.Add(newmember);
                        }
                    }

                    //UPDATE DEVICE DB
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                    {
                        connection.Open();

                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = "UPDATE Shortcuts SET Name=@name WHERE UserID = @UserID AND ShortcutID = @ID and Name<>@name;";
                            command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                            command.Parameters.Add(new SQLiteParameter("@ID", shortcut.id));
                            command.Parameters.Add(new SQLiteParameter("@name", shortcut.name));
                            command.ExecuteNonQuery();

                            command.CommandText = "INSERT OR IGNORE INTO Shortcuts (UserID,ShortcutID, name) VALUES (@UserID,@ID, @name);";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE Shortcuts SET DisplayName=@displayname WHERE UserID = @UserID AND ShortcutID = @ID and @name <> @displayname and IFNULL(DisplayName, '') = '';";
                            command.Parameters.Add(new SQLiteParameter("@displayname", shortcut.displayName));
                            command.ExecuteNonQuery();
                        }
                    }

                    Shortcuts.Add(shortcut);
                }

                myWink._shortcuts = Shortcuts.OrderBy(c => c.name).ToList();
            }

            #region RETRIEVE DATABASE VALUES
            foreach (Shortcut shortcut in myWink._shortcuts)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "select * from Shortcuts where UserID = @UserID AND ShortcutID = @ID";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", shortcut.id));
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            shortcut.position = Convert.ToInt32(reader["Position"].ToString());

                            if (!string.IsNullOrWhiteSpace(reader["DisplayName"].ToString()))
                                shortcut.displayName = reader["DisplayName"].ToString();

                            string subCapable = reader["subscriptionCapable"].ToString();
                            shortcut.subscriptionCapable = Convert.ToBoolean(subCapable);
                            shortcut.subscriptionTopic = reader["SubscriptionTopic"].ToString();

                            DateTime expires = new DateTime();
                            string date = reader["SubscriptionExpires"].ToString();
                            DateTime.TryParse(date, out expires);
                            shortcut.subscriptionExpires = Convert.ToDateTime(expires);

                        }
                    }
                }
            }
            #endregion

            #region PUBNUB SUBSCRIPTIONS

            //SHORTCUTS CAN'T HAVE SUBS AT THE MOMENT
            if (PubNub.myPubNub.hasPubNub)
            {
                foreach (Shortcut shortcut in myWink._shortcuts)
                {
                    if (string.IsNullOrWhiteSpace(shortcut.subscriptionTopic) || DateTime.Now > shortcut.subscriptionExpires)
                    {
                        if (shortcut.subscriptionCapable || firstRun)
                        {
                            string URL = ConfigurationManager.AppSettings["winkRootURL"] + "/scenes/" + shortcut.id + "/subscriptions";
                            string sendCommand = "{\"publisher_key\":\"" + PubNub.myPubNub.publishKey + "\",\"subscriber_key\":\"" + PubNub.myPubNub.subscriberKey + "\"}";
                            JObject subJSON = winkCallAPI(URL, "POST", sendCommand);
                            if (subJSON != null)
                            {
                                if (subJSON.ToString().Contains("404") && !firstRun)
                                {
                                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                    {

                                        connection.Open();
                                        using (SQLiteCommand command = new SQLiteCommand(connection))
                                        {
                                            command.CommandText = "UPDATE Shortcuts SET subscriptionCapable='0' WHERE UserID = @UserID AND ShortcutID = @ID;";
                                            command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                            command.Parameters.Add(new SQLiteParameter("@ID", shortcut.id));
                                            command.ExecuteNonQuery();

                                            command.CommandText = "INSERT OR IGNORE INTO Shortcuts (UserID,ShortcutID,subscriptionCapable) VALUES (@UserID, @ID,'0')";
                                            command.ExecuteNonQuery();

                                        }
                                    }
                                }
                                else
                                {
                                    if (subJSON["data"] != null)
                                    {
                                        if (subJSON["data"]["topic"] != null)
                                            shortcut.subscriptionTopic = subJSON["data"]["topic"].ToString();

                                        if (subJSON["data"]["expires_at"] != null)
                                            shortcut.subscriptionExpires = Common.FromUnixTime(subJSON["data"]["expires_at"].ToString());

                                        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                        {

                                            connection.Open();
                                            using (SQLiteCommand command = new SQLiteCommand(connection))
                                            {
                                                command.CommandText = "UPDATE Shortcuts SET subscriptionTopic=@subscriptionTopic,subscriptionExpires=@subscriptionExpires,subscriptionCapable='1' WHERE UserID = @UserID AND ShortcutID = @ID;";
                                                command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                                command.Parameters.Add(new SQLiteParameter("@subscriptionTopic", shortcut.subscriptionTopic));
                                                command.Parameters.Add(new SQLiteParameter("@subscriptionExpires", shortcut.subscriptionExpires));
                                                command.Parameters.Add(new SQLiteParameter("@ID", shortcut.id));
                                                command.ExecuteNonQuery();

                                                command.CommandText = "INSERT OR IGNORE INTO Shortcuts(UserID,ShortcutID,subscriptionTopic,subscriptionExpires,subscriptionCapable) VALUES (@UserID,@ID,@subscriptionTopic,@subscriptionExpires,'1')";
                                                command.ExecuteNonQuery();

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return myWink._shortcuts;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
#endregion
    
#region Group 
    public class Group
    {
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string name { get; set; }
        [SimpleProperty]
        public bool isempty { get; set; }
        [SimpleProperty]
        public string displayName { get; set; }

        public string json;
        public bool subscriptionCapable = true;
        public string subscriptionTopic;
        public DateTime subscriptionExpires;
        public int position = 1001;

        public List<GroupMember> members = new List<GroupMember>();
        public List<GroupStatus> status = new List<GroupStatus>();
        public class GroupMember
        {
            public string id;
            public string type;
            public List<string> actions = new List<string>();
        }
        public class GroupStatus
        {
            public string id;
            public string name;
            public string current_status;
            public DateTime? last_updated;
            public static void clearGroupStatus(string groupID)
            {
                Group group = myWink.Groups.SingleOrDefault(g => g.id.Equals(groupID));
                group.status = null;
                group.status = new List<GroupStatus>();
            }
        }

        public static Group getGroupByID(string GroupID)
        {
            Group group = myWink.Groups.SingleOrDefault(s => s.id.Equals(GroupID));
            return group;
        }
        public static Group getGroupByName(string GroupName)
        {
            Group group = myWink.Groups.SingleOrDefault(s => s.name.ToLower().Equals(GroupName.ToLower()));
            return group;
        }

        public static string setGroupDisplayName(string GroupID, string DisplayName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Groups SET displayname=@displayname WHERE UserID = @UserID AND GroupID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", GroupID));
                        command.Parameters.Add(new SQLiteParameter("@displayname", DisplayName));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Groups (UserID,GroupID, displayname) VALUES (@UserID,@ID, @displayname);";
                        command.ExecuteNonQuery();
                    }
                }
                Group group = myWink._groups.SingleOrDefault(d => d.id == GroupID);
                group.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception ex)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setGroupDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public static int setGroupPosition(string GroupID, int Position)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Groups SET position=@Position WHERE UserID = @UserID AND GroupID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", GroupID));
                        command.Parameters.Add(new SQLiteParameter("@Position", Position));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Groups (UserID,GroupID, position) VALUES (@UserID,@ID, @Position);";
                        command.ExecuteNonQuery();
                    }
                }

                return Position;
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setGroupPosition", ex.Message, EventLogEntryType.Error);
                return -1;
            }
        }
        
        internal static void sendGroupCommand(string groupID, string command)
        {
            try
            {
                Group group = Group.getGroupByID(groupID);
                if (group != null)
                {
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + "groups/" + groupID + "/activate/";
                    myWink.winkCallAPI(url, "POST", command);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
 
    public List<Group> Groups
    {
        get
        {
            return winkGetGroups();
        }
        set
        {
            _groups = value;
        }
    }
    private List<Group> _groups;
    public bool hasGroupChanges;

    private List<Group> winkGetGroups(bool forceRefresh = false)
    {
        try
        {
            bool firstRun = false;
            if (_groups == null || forceRefresh)
            {
                firstRun = true;

                JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetGroupsURL"]);
                List<Group> Groups = new List<Group>();

                foreach (JObject data in json["data"])
                {
                    IList<string> keys = data.Properties().Select(p => p.Name).ToList();
                    string typeName = keys[0];

                    Group group = new Group();

                    group.id = data[typeName].ToString();
                    group.name = data["name"].ToString();
                    group.displayName = group.name;
                    group.json = data.ToString();

                    if (keys.Contains("members"))
                    {
                        var members = data["members"];
                        foreach (var member in members)
                        {
                            Group.GroupMember newmember = new Group.GroupMember();
                            newmember.id = member["object_id"].ToString();
                            newmember.type = member["object_type"].ToString();

                            var states = member["desired_state"];
                            foreach (JProperty state in states)
                            {
                                if (!state.Name.Contains("_updated_at"))
                                    newmember.actions.Add(state.Name.ToString());
                            }
                            group.members.Add(newmember);
                        }
                    }

                    if (keys.Contains("reading_aggregation"))
                    {
                        JToken readings = data["reading_aggregation"];
                        foreach (JProperty reading in readings)
                        {
                            Group.GroupStatus newreading = new Group.GroupStatus();
                            newreading.id = group.id;
                            newreading.name = reading.Name;
                            newreading.last_updated = Common.FromUnixTime(reading.Value["updated_at"].ToString());

                            if (reading.Value["true_count"] != null)
                                newreading.current_status = reading.Value["true_count"].ToString();
                            else if (reading.Value["average"] != null)
                                newreading.current_status = reading.Value["average"].ToString();

                            group.status.Add(newreading);
                        }
                    }

                    if (group.members.Count == 0)
                        group.isempty = true;
                    
                    
                    //UPDATE DEVICE DB
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                    {
                        connection.Open();

                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = "UPDATE Groups SET Name=@name WHERE UserID = @UserID AND GroupID = @ID and Name<>@name;";
                            command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                            command.Parameters.Add(new SQLiteParameter("@ID", group.id));
                            command.Parameters.Add(new SQLiteParameter("@name", group.name));
                            command.ExecuteNonQuery();

                            command.CommandText = "INSERT OR IGNORE INTO Groups (UserID,GroupID, name) VALUES (@UserID, @ID, @name);";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE Groups SET DisplayName=@displayname WHERE UserID = @UserID AND GroupID = @ID and @name <> @displayname and IFNULL(DisplayName, '') = '';";
                            command.Parameters.Add(new SQLiteParameter("@displayname", group.displayName));
                            command.ExecuteNonQuery();
                        }
                    }

                    Groups.Add(group);
                }

                _groups = Groups.OrderBy(c => c.name).ToList();
            }

            #region RETRIEVE DATABASE VALUES
            foreach (Group group in _groups)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "select * from Groups where UserID = @UserID AND GroupID = @ID";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", group.id));
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            group.position = Convert.ToInt32(reader["Position"].ToString());

                            if (!string.IsNullOrWhiteSpace(reader["DisplayName"].ToString()))
                                group.displayName = reader["DisplayName"].ToString();

                            string subCapable = reader["subscriptionCapable"].ToString();
                            group.subscriptionCapable = Convert.ToBoolean(subCapable);
                            group.subscriptionTopic = reader["SubscriptionTopic"].ToString();

                            DateTime expires = new DateTime();
                            string date = reader["SubscriptionExpires"].ToString();
                            DateTime.TryParse(date, out expires);
                            group.subscriptionExpires = Convert.ToDateTime(expires);

                        }
                    }
                }
            }
            #endregion

            #region PUBNUB SUBSCRIPTIONS

            //GROUPS CAN'T HAVE SUBS AT THE MOMENT

            if (PubNub.myPubNub.hasPubNub)
            {
                foreach (Group group in _groups)
                {
                    if (string.IsNullOrWhiteSpace(group.subscriptionTopic) || DateTime.Now > group.subscriptionExpires)
                    {
                        if (group.subscriptionCapable || firstRun)
                        {
                            string URL = ConfigurationManager.AppSettings["winkRootURL"] + "/groups/" + group.id + "/subscriptions";
                            string sendCommand = "{\"publisher_key\":\"" + PubNub.myPubNub.publishKey + "\",\"subscriber_key\":\"" + PubNub.myPubNub.subscriberKey + "\"}";
                            JObject subJSON = winkCallAPI(URL, "POST", sendCommand);
                            if (subJSON != null)
                            {
                                if (subJSON.ToString().Contains("404") && !firstRun)
                                {
                                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                    {

                                        connection.Open();
                                        using (SQLiteCommand command = new SQLiteCommand(connection))
                                        {
                                            command.CommandText = "UPDATE Groups SET subscriptionCapable='0' WHERE UserID = @UserID AND GroupID = @ID;";
                                            command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                            command.Parameters.Add(new SQLiteParameter("@ID", group.id));
                                            command.ExecuteNonQuery();

                                            command.CommandText = "INSERT OR IGNORE INTO Groups(UserID, GroupID,subscriptionCapable) VALUES (@UserID,@ID,'0')";
                                            command.ExecuteNonQuery();

                                        }
                                    }
                                }
                                else
                                {
                                    if (subJSON["data"] != null)
                                    {
                                        if (subJSON["data"]["topic"] != null)
                                            group.subscriptionTopic = subJSON["data"]["topic"].ToString();

                                        if (subJSON["data"]["expires_at"] != null)
                                            group.subscriptionExpires = Common.FromUnixTime(subJSON["data"]["expires_at"].ToString());

                                        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                        {

                                            connection.Open();
                                            using (SQLiteCommand command = new SQLiteCommand(connection))
                                            {
                                                command.CommandText = "UPDATE Groups SET subscriptionTopic=@subscriptionTopic,subscriptionExpires=@subscriptionExpires,subscriptionCapable='1' WHERE UserID = @UserID AND GroupID = @ID;";
                                                command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                                command.Parameters.Add(new SQLiteParameter("@subscriptionTopic", group.subscriptionTopic));
                                                command.Parameters.Add(new SQLiteParameter("@subscriptionExpires", group.subscriptionExpires));
                                                command.Parameters.Add(new SQLiteParameter("@ID", group.id));
                                                command.ExecuteNonQuery();

                                                command.CommandText = "INSERT OR IGNORE INTO Groups(UserID,GroupID,subscriptionTopic,subscriptionExpires,subscriptionCapable) VALUES (@UserID,@ID,@subscriptionTopic,@subscriptionExpires,'1')";
                                                command.ExecuteNonQuery();

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return _groups;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
#endregion

#region Robot
    public class Robot
    {
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string name { get; set; }
        [SimpleProperty]
        public string enabled { get; set; }
        [SimpleProperty]
        public bool isschedule { get; set; }
        [SimpleProperty]
        public bool isempty { get; set; }
        [SimpleProperty]
        public string last_fired { get; set; }
        [SimpleProperty]
        public string next_run { get; set; }
        [SimpleProperty]
        public string displayName { get; set; }

        public string json;
        public bool subscriptionCapable = true;
        public string subscriptionTopic;
        public DateTime subscriptionExpires;
        public int position = 1001;

        public List<string> members = new List<string>();
        public static Robot getRobotByID(string RobotID)
        {
            Robot robot = myWink.Robots.SingleOrDefault(s => s.id.Equals(RobotID));
            return robot;
        }
        public static Robot getRobotByName(string robotName)
        {
            Robot robot = myWink.Robots.SingleOrDefault(s => s.name.ToLower().Equals(robotName.ToLower()));
            return robot;
        }
        internal static void changeRobotState(string robotID, bool newEnabledState)
        {
            try
            {
                Robot robot = Robot.getRobotByID(robotID);

                if (robot != null)
                {
                    string newstate = newEnabledState.ToString().ToLower();
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + "robots/" + robot.id;
                    string sendcommand = "{\"enabled\":" + newstate + "}";

                    myWink.winkCallAPI(url, "PUT", sendcommand);

                    robot.enabled = newstate;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static JObject getRobotJSON()
        {
            JObject json = myWink.winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetRobotsURL"]);
            if (json != null)
                return json;

            return null;
        }
        public static string setRobotDisplayName(string RobotID, string DisplayName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Robots SET displayname=@displayname WHERE UserID = @UserID AND RobotID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", RobotID));
                        command.Parameters.Add(new SQLiteParameter("@displayname", DisplayName));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Robots (UserID,RobotID, displayname) VALUES (@UserID,@ID, @displayname);";
                        command.ExecuteNonQuery();
                    }
                }
                Robot robot = myWink._robots.SingleOrDefault(d => d.id == RobotID);
                robot.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception ex)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setRobotDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public static int setRobotPosition(string RobotID, int Position)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "UPDATE Robots SET position=@Position WHERE UserID = @UserID AND RobotID = @ID;";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", RobotID));
                        command.Parameters.Add(new SQLiteParameter("@Position", Position));
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT OR IGNORE INTO Robots (UserID,RobotID, position) VALUES (@UserID,@ID, @Position);";
                        command.ExecuteNonQuery();
                    }
                }

                return Position;
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setRobotPosition", ex.Message, EventLogEntryType.Error);
                return -1;
            }
        }
    }

    public List<Robot> Robots
    {
        get
        {
            return winkGetRobots();
        }
        set
        {
            _robots = value;
        }
    }
    private List<Robot> _robots;
    public bool hasRobotChanges;

    private List<Robot> winkGetRobots(JObject jsonObject = null, bool forceRefresh = false)
    {
        try
        {
            bool firstRun = false;
            if (_robots == null || forceRefresh)
            {
                List<Robot> Robots = new List<Robot>();
                JObject json = null;

                if (_robots == null || forceRefresh)
                {
                    firstRun = true;
                    json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetRobotsURL"]);
                }
                else if (jsonObject != null)
                {
                    json = jsonObject;
                    Robots = _robots;
                }

                if (json != null)
                {
                    foreach (JObject data in json["data"])
                    {
                        Robot robot = new Robot();
                        bool hasData = false;

                        robot.id = data["robot_id"].ToString();
                        robot.name = data["name"].ToString();
                        robot.displayName = robot.name;
                        robot.enabled = data["enabled"].ToString();
                        robot.json = data.ToString();
                        robot.isschedule = (data["automation_mode"].ToString() == "schedule");

                        DateTime cutoff = Convert.ToDateTime("1/1/2012");
                        DateTime lastfired = Common.FromUnixTime(data["last_fired"].ToString());;
                        robot.last_fired = (lastfired < cutoff ? "Never" : lastfired.ToString());
                        robot.next_run = "Schedules Only";

                        if (data["causes"] != null)
                        {
                            var causes = data["causes"];
                            if (causes.HasValues)
                            {
                                if (data["causes"][0] != null && data["causes"][0]["next_at"] != null)
                                {
                                    if (robot.isschedule)
                                    {
                                        DateTime nextrun = Common.FromUnixTime(data["causes"][0]["next_at"].ToString());
                                        robot.next_run = (nextrun < cutoff ? "Not Scheduled" : nextrun.ToString());
                                    }
                                }

                                //RENAME "MY ROBOT" WHEN IS SENSOR BATTERY WARNING
                                if (robot.name.ToLower() == "my robot")
                                {
                                    Device device = myWink._devices.SingleOrDefault(d => d.id == data["causes"][0]["observed_object_id"].ToString());

                                    if (device != null)
                                    {
                                        string strDeviceIden = device != null ? device.name : "device ID " + data["causes"][0]["observed_object_id"].ToString();

                                        robot.displayName = robot.name + " (" + strDeviceIden + " " + data["causes"][0]["observed_field"].ToString() + " warning)";
                                    }
                                }

                            }
                        }

                        foreach (var effect in data["effects"])
                        {
                            var scene = effect["scene"];
                            if (!string.IsNullOrWhiteSpace(scene.ToString()))
                            {
                                var members = scene["members"];
                                if (!string.IsNullOrWhiteSpace(members.ToString()))
                                {
                                    foreach (var member in members)
                                    {
                                        robot.members.Add(member["object_id"].ToString());
                                    }
                                }
                            }
                        }

                        if (robot.members.Count > 0)
                            hasData = true;
                        else
                        {
                            foreach (var cause in data["causes"])
                            {
                                if (!string.IsNullOrWhiteSpace(cause["observed_object_id"].ToString()))
                                {
                                    hasData = true;
                                    break;
                                }
                            }
                        }

                        if (!hasData)
                            robot.isempty = true;

                        //UPDATE DEVICE DB
                        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                        {
                            connection.Open();

                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                command.CommandText = "UPDATE Robots SET Name=@name WHERE UserID = @UserID AND RobotID = @ID and Name<>@name;";
                                command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                command.Parameters.Add(new SQLiteParameter("@ID", robot.id));
                                command.Parameters.Add(new SQLiteParameter("@name", robot.name));
                                command.ExecuteNonQuery();

                                command.CommandText = "INSERT OR IGNORE INTO Robots (UserID,RobotID, name) VALUES (@UserID,@ID, @name);";
                                command.ExecuteNonQuery();

                                command.CommandText = "UPDATE Robots SET DisplayName=@displayname WHERE UserID = @UserID AND RobotID = @ID and @name <> @displayname and IFNULL(DisplayName, '') = '';";
                                command.Parameters.Add(new SQLiteParameter("@displayname", robot.displayName));
                                command.ExecuteNonQuery();
                            }
                        }

                        Robots.Add(robot);
                    }
                }
                _robots = Robots.OrderBy(c => c.name).ToList();
            }

            #region RETRIEVE DATABASE VALUES
            foreach (Robot robot in _robots)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "select * from Robots where UserID = @UserID AND RobotID = @ID";
                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                        command.Parameters.Add(new SQLiteParameter("@ID", robot.id));
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            robot.position = Convert.ToInt32(reader["Position"].ToString());

                            if (!string.IsNullOrWhiteSpace(reader["DisplayName"].ToString()))
                                robot.displayName = reader["DisplayName"].ToString();

                            string subCapable = reader["subscriptionCapable"].ToString();
                            robot.subscriptionCapable = Convert.ToBoolean(subCapable);
                            robot.subscriptionTopic = reader["SubscriptionTopic"].ToString();

                            DateTime expires = new DateTime();
                            string date = reader["SubscriptionExpires"].ToString();
                            DateTime.TryParse(date, out expires);
                            robot.subscriptionExpires = Convert.ToDateTime(expires);

                        }
                    }
                }
            }
            #endregion

            #region PUBNUB SUBSCRIPTIONS
            //ROBOTS CAN'T HAVE SUBS AT THE MOMENT

            if (PubNub.myPubNub.hasPubNub)
            {
                foreach (Robot robot in _robots)
                {
                    if (string.IsNullOrWhiteSpace(robot.subscriptionTopic) || DateTime.Now > robot.subscriptionExpires)
                    {
                        if (robot.subscriptionCapable || firstRun)
                        {
                            string URL = ConfigurationManager.AppSettings["winkRootURL"] + "/robots/" + robot.id + "/subscriptions";
                            string sendCommand = "{\"publisher_key\":\"" + PubNub.myPubNub.publishKey + "\",\"subscriber_key\":\"" + PubNub.myPubNub.subscriberKey + "\"}";
                            JObject subJSON = winkCallAPI(URL, "POST", sendCommand);
                            if (subJSON != null)
                            {
                                if (subJSON.ToString().Contains("404") && !firstRun)
                                {
                                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                    {

                                        connection.Open();
                                        using (SQLiteCommand command = new SQLiteCommand(connection))
                                        {
                                            command.CommandText = "UPDATE Robots SET subscriptionCapable='0' WHERE UserID = @UserID AND RobotID = @ID;";
                                            command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                            command.Parameters.Add(new SQLiteParameter("@ID", robot.id));
                                            command.ExecuteNonQuery();

                                            command.CommandText = "INSERT OR IGNORE INTO Robots(UserID,RobotID,subscriptionCapable) VALUES (@UserID,@ID,'0')";
                                            command.ExecuteNonQuery();

                                        }
                                    }
                                }
                                else
                                {
                                    if (subJSON["data"] != null)
                                    {
                                        if (subJSON["data"]["topic"] != null)
                                            robot.subscriptionTopic = subJSON["data"]["topic"].ToString();

                                        if (subJSON["data"]["expires_at"] != null)
                                            robot.subscriptionExpires = Common.FromUnixTime(subJSON["data"]["expires_at"].ToString());

                                        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                        {

                                            connection.Open();
                                            using (SQLiteCommand command = new SQLiteCommand(connection))
                                            {
                                                command.CommandText = "UPDATE Robots SET subscriptionTopic=@subscriptionTopic,subscriptionExpires=@subscriptionExpires,subscriptionCapable='1' WHERE UserID = @UserID AND RobotID = @ID;";
                                                command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                                command.Parameters.Add(new SQLiteParameter("@subscriptionTopic", robot.subscriptionTopic));
                                                command.Parameters.Add(new SQLiteParameter("@subscriptionExpires", robot.subscriptionExpires));
                                                command.Parameters.Add(new SQLiteParameter("@ID", robot.id));
                                                command.ExecuteNonQuery();

                                                command.CommandText = "INSERT OR IGNORE INTO Robots(UserID,RobotID,subscriptionTopic,subscriptionExpires,subscriptionCapable) VALUES (@UserID,@ID,@subscriptionTopic,@subscriptionExpires,'1')";
                                                command.ExecuteNonQuery();

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return _robots;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
#endregion

    private JObject winkCallAPI(string url, string method = "", string sendcommand = "", bool requiresToken = true)
    {
        try
        {
            String responseString = string.Empty;
            JObject jsonResponse = new JObject();

            using (var xhr = new WebClient())
            {
                xhr.Headers[HttpRequestHeader.ContentType] = "application/json";

                if (requiresToken)
                    xhr.Headers.Add("Authorization", "Bearer " + myWink.WinkToken);

                byte[] result = null;

                if (String.IsNullOrWhiteSpace(sendcommand) && method.ToLower() != "post")
                {
                    result = xhr.DownloadData(url);
                }
                else
                {
                    byte[] data = Encoding.Default.GetBytes(sendcommand);
                    result = xhr.UploadData(url, method, data);
                }

                responseString = Encoding.Default.GetString(result);
            }

            if (responseString != null)
            {
                #if DEBUG
                if (url == "https://winkapi.quirky.com/users/me/wink_devices")
                {
                    if (myWink.winkUser.email.ToLower().Contains("trunzo"))
                    {
                        //Add Garage Door
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"garage_door_id\": \"8552\",\"name\": \"zTest Chamberlain\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1420250978,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"af309d2e12b86bd1e5e63123db745dad703e46fb|garage_door-8552|user-123172\"}},\"user_ids\": [\"123172\"],\"triggers\": [],\"desired_state\": {\"position\": 1.0},\"manufacturer_device_model\": \"chamberlain_vgdo\",\"manufacturer_device_id\": \"1180839\",\"device_manufacturer\": \"chamberlain\",\"model_name\": \"MyQ Garage Door Controller\",\"upc_id\": \"26\",\"linked_service_id\": \"59900\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428535030.025161,\"position\": 1.0,\"position_updated_at\": 1428535020.76,\"position_opened\": \"N/A\",\"position_opened_updated_at\": 1428534916.709,\"battery\": 1.0,\"battery_updated_at\": 1428534350.3417819,\"fault\": false,\"fault_updated_at\": 1428534350.3417749,\"control_enabled\": true,\"control_enabled_updated_at\": 1428534350.3417563,\"desired_position\": 0.0,\"desired_position_updated_at\": 1428535030.0404377},\"lat_lng\": [33.162135,-97.090945],\"location\": \"\",\"order\": 0},");

                        //Add Honeywell Thermostat
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"27239\",\"name\": \"zTest Honeywell\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419909349,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"f5cb03e4101d1668ff7933a703a864b4984fce5a|thermostat-27239|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"mode\": \"heat_only\",\"powered\": true,\"min_set_point\": 20.0,\"max_set_point\": 22.777777777777779},\"manufacturer_device_model\": \"MANHATTAN\",\"manufacturer_device_id\": \"798165\",\"device_manufacturer\": \"honeywell\",\"model_name\": \"Honeywell Wi-Fi Smart Thermostat\",\"upc_id\": \"151\",\"hub_id\": null,\"local_id\": \"00D02D49A90A\",\"radio_type\": null,\"linked_service_id\": \"57563\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428536316.8312173,\"mode\": \"heat_only\",\"mode_updated_at\": 1428536316.8312581,\"powered\": true,\"powered_updated_at\": 1428536316.831265,\"min_set_point\": 20.0,\"min_set_point_updated_at\": 1428536316.8312485,\"max_set_point\": 22.777777777777779,\"max_set_point_updated_at\": 1428536316.8312287,\"temperature\": 22.777777777777779,\"temperature_updated_at\": 1428536316.8312783,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.6666666666666667,\"deadband_updated_at\": 1428536316.831311,\"min_min_set_point\": 4.4444444444444446,\"min_min_set_point_updated_at\": 1428536316.8313046,\"max_min_set_point\": 29.444444444444443,\"max_min_set_point_updated_at\": 1428536316.8312914,\"min_max_set_point\": 16.666666666666668,\"min_max_set_point_updated_at\": 1428536316.8312984,\"max_max_set_point\": 37.222222222222221,\"max_max_set_point_updated_at\": 1428536316.831285,\"modes_allowed\": [\"auto\",\"cool_only\",\"heat_only\"],\"modes_allowed_updated_at\": 1428536316.8313177,\"units\": \"f\",\"units_updated_at\": 1428536316.8312719,\"desired_mode\": \"heat_only\",\"desired_mode_updated_at\": 1428365474.3775809,\"desired_powered\": true,\"desired_powered_updated_at\": 1424823532.9645114,\"desired_min_set_point\": 20.0,\"desired_min_set_point_updated_at\": 1428509375.1094887,\"desired_max_set_point\": 22.777777777777779,\"desired_max_set_point_updated_at\": 1428509375.109503},\"lat_lng\": [33.162074,-97.090928],\"location\": \"\",\"smart_schedule_enabled\": false},");

                        //Add Nest Thermostat
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"49534\",\"name\": \"zTest Nest\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1427241058,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"0e67d43624e47b3633273f1236b7cde2c1823ac7|thermostat-49410|user-81926\"}},\"user_ids\": [\"81926\"],\"triggers\": [],\"desired_state\": {\"mode\": \"cool_only\",\"powered\": true,\"min_set_point\": 21.0,\"max_set_point\": 22.0,\"users_away\": false,\"fan_timer_active\": false},\"manufacturer_device_model\": \"nest\",\"manufacturer_device_id\": \"pHNukJTND3MHRBT9zks77kxx11Qobba_\",\"device_manufacturer\": \"nest\",\"model_name\": \"Learning Thermostat\",\"upc_id\": \"168\",\"hub_id\": null,\"local_id\": null,\"radio_type\": null,\"linked_service_id\": \"92972\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428516397.2914052,\"mode\": \"cool_only\",\"mode_updated_at\": 1428516397.2914376,\"powered\": true,\"powered_updated_at\": 1428516397.2914565,\"min_set_point\": 21.0,\"min_set_point_updated_at\": 1428461324.5980272,\"max_set_point\": 22.0,\"max_set_point_updated_at\": 1428516397.2914746,\"users_away\": false,\"users_away_updated_at\": 1428516397.2914917,\"fan_timer_active\": false,\"fan_timer_active_updated_at\": 1428516397.2914684,\"temperature\": 22.0,\"temperature_updated_at\": 1428516397.2914257,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.5,\"deadband_updated_at\": 1428516397.2914317,\"min_min_set_point\": null,\"min_min_set_point_updated_at\": null,\"max_min_set_point\": null,\"max_min_set_point_updated_at\": null,\"min_max_set_point\": null,\"min_max_set_point_updated_at\": null,\"max_max_set_point\": null,\"max_max_set_point_updated_at\": null,\"modes_allowed\": [\"auto\",\"heat_only\",\"cool_only\"],\"modes_allowed_updated_at\": 1428516397.2914805,\"units\": \"f\",\"units_updated_at\": 1428516397.2914197,\"eco_target\": false,\"eco_target_updated_at\": 1428516397.2914433,\"manufacturer_structure_id\": \"kdCrRKp3UahHp8xWEoJBRYX9xnQWDsoU1sb5ej9Mp5Zb41WEIOKJtg\",\"manufacturer_structure_id_updated_at\": 1428516397.2914503,\"has_fan\": true,\"has_fan_updated_at\": 1428516397.2914622,\"fan_duration\": 0,\"fan_duration_updated_at\": 1428516397.2914863,\"last_error\": null,\"last_error_updated_at\": 1427241058.6980464,\"desired_mode\": \"cool_only\",\"desired_mode_updated_at\": 1427593066.90498,\"desired_powered\": true,\"desired_powered_updated_at\": 1428462838.6427567,\"desired_min_set_point\": 21.0,\"desired_min_set_point_updated_at\": 1428427791.3297703,\"desired_max_set_point\": 22.0,\"desired_max_set_point_updated_at\": 1428497187.9092989,\"desired_users_away\": false,\"desired_users_away_updated_at\": 1428440888.9921448,\"desired_fan_timer_active\": false,\"desired_fan_timer_active_updated_at\": 1427241058.6981435},\"lat_lng\": [null,null],\"location\": \"\",\"smart_schedule_enabled\": false},");

                        //Add Refuel
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"propane_tank_id\": \"6521\",\"name\": \"zTest Refuel\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419569612,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"5055752531a8aac104827ec4ba2a3366038ee15a|propane_tank-6521|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"device_manufacturer\": \"quirky_ge\",\"model_name\": \"Refuel\",\"upc_id\": \"17\",\"last_reading\": {\"connection\": true,\"battery\": 0.52,\"remaining\": 0.5},\"lat_lng\": [33.162101,-97.090547],\"location\": \"76210\",\"mac_address\": \"0c2a6907025a\",\"serial\": \"ACAB00033589\",\"tare\": 18.0,\"tank_changed_at\": 1421352479},");

                        //Add Nest Protect
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"smoke_detector_id\": \"10076\", \"name\": \"zTest Nest Protect\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1419086573, \"hidden_at\": null, \"capabilities\": {}, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"5882005d3a98dbbd335c2cf778a6734557cd1f2f|smoke_detector-10076|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"manufacturer_device_model\": \"nest\", \"manufacturer_device_id\": \"VpXN4GQ7MUD5QqV8vgvQOExx11Qobba_\", \"device_manufacturer\": \"nest\", \"model_name\": \"Smoke + Carbon Monoxide Detector\", \"upc_id\": \"170\", \"hub_id\": null, \"local_id\": null, \"radio_type\": null, \"linked_service_id\": \"50847\", \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1428865904.1217248, \"battery\": 1.0, \"battery_updated_at\": 1428865904.1217616, \"co_detected\": false, \"co_detected_updated_at\": 1428865904.1217353, \"smoke_detected\": false, \"smoke_detected_updated_at\": 1428865904.1217477, \"test_activated\": null, \"test_activated_updated_at\": 1428855697.3881633, \"smoke_severity\": 0.0, \"smoke_severity_updated_at\": 1428865904.1217549, \"co_severity\": 0.0, \"co_severity_updated_at\": 1428865904.1217415 }, \"lat_lng\": [ null, null ], \"location\": \"\" },");

                        //Add Relay & Related
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"hub_id\": \"132595\", \"name\": \"zTest Wink Relay\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1428545678, \"hidden_at\": null, \"capabilities\": { \"oauth2_clients\": [ \"wink_project_one\" ] }, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"d9be1fe3abeb9fc46bec54a7cb62719a5a576c86|hub-132595|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"triggers\": [], \"desired_state\": { \"pairing_mode\": null }, \"manufacturer_device_model\": \"wink_project_one\", \"manufacturer_device_id\": null, \"device_manufacturer\": \"wink\", \"model_name\": \"Wink Relay\", \"upc_id\": \"186\", \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1428865074.9676549, \"agent_session_id\": \"9c99e3314c3a39f2eb9830a78d10684c\", \"agent_session_id_updated_at\": 1428855693.8299525, \"remote_pairable\": null, \"remote_pairable_updated_at\": null, \"updating_firmware\": false, \"updating_firmware_updated_at\": 1428855688.0774271, \"app_rootfs_version\": \"1.0.221\", \"app_rootfs_version_updated_at\": 1428855697.3881633, \"firmware_version\": \"1.0.221\", \"firmware_version_updated_at\": 1428855697.3881423, \"update_needed\": false, \"update_needed_updated_at\": 1428855697.3881698, \"mac_address\": \"B4:79:A7:0F:F7:DF\", \"mac_address_updated_at\": 1428855697.3881495, \"ip_address\": \"192.168.1.187\", \"ip_address_updated_at\": 1428855697.3881567, \"hub_version\": \"user\", \"hub_version_updated_at\": 1428855697.3881316, \"pairing_mode\": null, \"pairing_mode_updated_at\": 1428545678.0519505, \"desired_pairing_mode\": null, \"desired_pairing_mode_updated_at\": 1428545678.0519564 }, \"lat_lng\": [ null, null ], \"location\": \"\", \"configuration\": null, \"update_needed\": true, \"uuid\": \"9bb6a0d5-30f2-4bf7-8b37-d667c30e05bc\" },");
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"binary_switch_id\": \"46401\", \"name\": \"zTest Relay Switch A\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1428545701, \"hidden_at\": null, \"capabilities\": { \"configuration\": null }, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"bb1cb7a5d146cbc2cf09dca3655f684b2e24be89|binary_switch-46401|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"triggers\": [], \"desired_state\": { \"powered\": false, \"powering_mode\": \"dumb\" }, \"manufacturer_device_model\": null, \"manufacturer_device_id\": null, \"device_manufacturer\": null, \"model_name\": null, \"upc_id\": null, \"gang_id\": \"6776\", \"hub_id\": \"132595\", \"local_id\": \"1\", \"radio_type\": \"project_one\", \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1428855698.9869163, \"powered\": false, \"powered_updated_at\": 1428855698.9869256, \"powering_mode\": \"dumb\", \"powering_mode_updated_at\": 1428545815.5253267, \"consumption\": null, \"consumption_updated_at\": null, \"cost\": null, \"cost_updated_at\": null, \"budget_percentage\": null, \"budget_percentage_updated_at\": null, \"budget_velocity\": null, \"budget_velocity_updated_at\": null, \"summation_delivered\": null, \"summation_delivered_updated_at\": null, \"sum_delivered_multiplier\": null, \"sum_delivered_multiplier_updated_at\": null, \"sum_delivered_divisor\": null, \"sum_delivered_divisor_updated_at\": null, \"sum_delivered_formatting\": null, \"sum_delivered_formatting_updated_at\": null, \"sum_unit_of_measure\": null, \"sum_unit_of_measure_updated_at\": null, \"desired_powered\": false, \"desired_powered_updated_at\": 1428546752.8178129, \"desired_powering_mode\": \"dumb\", \"desired_powering_mode_updated_at\": 1428545815.5253191 }, \"current_budget\": null, \"lat_lng\": [ 0.0, 0.0 ], \"location\": \"\", \"order\": 0 },");
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"binary_switch_id\": \"30320\", \"name\": \"zTest Relay Switch B\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1422813943, \"hidden_at\": null, \"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"8afe2bdfa540e4459b510bff44db636388afea88|binary_switch-30320|user-186645\"}},\"user_ids\": [\"186645\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"powering_mode\": \"none\"},\"manufacturer_device_model\": null,\"manufacturer_device_id\": null,\"device_manufacturer\": null,\"model_name\": null,\"upc_id\": null,\"gang_id\": \"2997\",\"hub_id\": \"98045\",\"local_id\": \"2\",\"radio_type\": \"project_one\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1429016828.833034,\"powered\": false,\"powered_updated_at\": 1429016828.8330438,\"powering_mode\": \"none\",\"powering_mode_updated_at\": 1422824216.9928842,\"consumption\": null,\"consumption_updated_at\": null,\"cost\": null,\"cost_updated_at\": null,\"budget_percentage\": null,\"budget_percentage_updated_at\": null,\"budget_velocity\": null,\"budget_velocity_updated_at\": null,\"summation_delivered\": null,\"summation_delivered_updated_at\": null,\"sum_delivered_multiplier\": null,\"sum_delivered_multiplier_updated_at\": null,\"sum_delivered_divisor\": null,\"sum_delivered_divisor_updated_at\": null,\"sum_delivered_formatting\": null,\"sum_delivered_formatting_updated_at\": null,\"sum_unit_of_measure\": null,\"sum_unit_of_measure_updated_at\": null,\"desired_powered\": false,\"desired_powered_updated_at\": 1422824138.418901,\"desired_powering_mode\": \"none\",\"desired_powering_mode_updated_at\": 1422824216.9928727},\"current_budget\": null,\"lat_lng\": [0.0,0.0],\"location\": \"\",\"order\": 0},");
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"button_id\": \"13333\", \"name\": \"zTest Smart Button 1\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1428545701, \"hidden_at\": null, \"capabilities\": {}, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"2894740376a764e392d566474aac56f634c3731f|button-13333|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"manufacturer_device_model\": null, \"manufacturer_device_id\": null, \"device_manufacturer\": null, \"model_name\": null, \"upc_id\": null, \"gang_id\": \"6776\", \"hub_id\": \"132595\", \"local_id\": \"4\", \"radio_type\": \"project_one\", \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1428855697.7729235, \"pressed\": false, \"pressed_updated_at\": 1428855697.7729335, \"long_pressed\": null, \"long_pressed_updated_at\": null }, \"lat_lng\": [ null, null ], \"location\": \"\" },");
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"button_id\": \"13334\", \"name\": \"zTest Smart Button 2\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1428545701, \"hidden_at\": null, \"capabilities\": {}, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"51c9e90c1b352231a0ce9bdbfa1295c052af91f2|button-13334|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"manufacturer_device_model\": null, \"manufacturer_device_id\": null, \"device_manufacturer\": null, \"model_name\": null, \"upc_id\": null, \"gang_id\": \"6776\", \"hub_id\": \"132595\", \"local_id\": \"5\", \"radio_type\": \"project_one\", \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1428855697.9626672, \"pressed\": false, \"pressed_updated_at\": 1428855697.962677, \"long_pressed\": null, \"long_pressed_updated_at\": null }, \"lat_lng\": [ null, null ], \"location\": \"\" },");
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"gang_id\": \"6776\", \"name\": \"zTest Gang\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1428545678, \"hidden_at\": null, \"capabilities\": {}, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"1d8877fe2c53439330ef7e72548c6fa38e111420|gang-6776|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"desired_state\": {}, \"manufacturer_device_model\": \"wink_project_one\", \"manufacturer_device_id\": null, \"device_manufacturer\": null, \"model_name\": null, \"upc_id\": null, \"hub_id\": \"132595\", \"local_id\": null, \"radio_type\": null, \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1428545678.122422 }, \"lat_lng\": [ null, null ], \"location\": \"\" },");

                        //Spotter
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"last_event\":{\"brightness_occurred_at\":1428961404.2836313,\"loudness_occurred_at\":1427547964.9292188,\"vibration_occurred_at\":1428879300.9600453},\"sensor_threshold_events\":[],\"sensor_pod_id\":\"40794\",\"name\":\"zTest Spotter\",\"locale\":\"en_us\",\"units\":{\"temperature\":\"f\"},\"created_at\":1422657639,\"hidden_at\":null,\"capabilities\":{\"sensor_types\":[{\"type\":\"percentage\",\"field\":\"battery\"},{\"type\":\"percentage\",\"field\":\"brightness\"},{\"type\":\"boolean\",\"field\":\"external_power\"},{\"type\":\"integer_percentage\",\"field\":\"humidity\"},{\"type\":\"percentage\",\"field\":\"loudness\"},{\"type\":\"float\",\"field\":\"temperature\"},{\"type\":\"boolean\",\"field\":\"vibration\"}]},\"subscription\":{\"pubnub\":{\"subscribe_key\":\"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\":\"959bda140ade2f77ded3fd968bfac2242489175e|sensor_pod-40794|user-186645\"}},\"user_ids\":[\"186645\"],\"triggers\":[],\"desired_state\":{},\"manufacturer_device_model\":\"quirky_ge_spotter\",\"manufacturer_device_id\":null,\"device_manufacturer\":\"quirky_ge\",\"model_name\":\"Spotter\",\"upc_id\":\"25\",\"gang_id\":null,\"hub_id\":null,\"local_id\":null,\"radio_type\":null,\"last_reading\":{\"connection\":true,\"connection_updated_at\":1428969581.0099306,\"agent_session_id\":null,\"agent_session_id_updated_at\":1425384214.8524168,\"battery\":0.98,\"battery_updated_at\":1428969581.0099251,\"brightness\":1.0,\"brightness_updated_at\":1428969581.0098875,\"external_power\":true,\"external_power_updated_at\":1428969581.0098965,\"humidity\":35,\"humidity_updated_at\":1428969581.0099025,\"loudness\":0.0,\"loudness_updated_at\":1428969581.0099192,\"temperature\":19.0,\"temperature_updated_at\":1428969581.0099139,\"vibration\":false,\"vibration_updated_at\":1428969581.0099082,\"brightness_true\":\"N/A\",\"brightness_true_updated_at\":1428961404.2836313,\"loudness_true\":\"N/A\",\"loudness_true_updated_at\":1427547964.9292188,\"vibration_true\":\"N/A\",\"vibration_true_updated_at\":1428879300.9600453},\"lat_lng\":[0.0,0.0],\"location\":\"\",\"mac_address\":\"0c2a690656b3\",\"serial\":\"ABAB00029469\",\"uuid\":\"005f5493-2ab6-46fa-ab7d-f2932c37dd4a\"},");

                        //Rachio Iro
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"sprinkler_id\": \"1483\",\"name\": \"Sprinkler\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1429295028,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"ecbd8151da8524635b2dfd777c4f55f33a042285|sprinkler-1483|user-186645\"}},\"user_ids\": [\"186645\"],\"desired_state\": {\"master_valve\": false,\"rain_sensor\": false,\"schedule_enabled\": false,\"run_zone_indices\": [],\"run_zone_durations\": []},\"manufacturer_device_model\": \"rachio_iro\",\"manufacturer_device_id\": \"b26d4e70-f4df-481a-9149-c9bac4c3a09e\",\"device_manufacturer\": \"rachio\",\"model_name\": \"Iro\",\"upc_id\": \"152\",\"linked_service_id\": \"100792\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1429477160.725,\"master_valve\": false,\"master_valve_updated_at\": 1429477160.725,\"rain_sensor\": false,\"rain_sensor_updated_at\": 1429477160.725,\"schedule_enabled\": false,\"schedule_enabled_updated_at\": 1429477160.725,\"run_zone_indices\": [],\"run_zone_indices_updated_at\": 1429361044.5683227,\"run_zone_durations\": [],\"run_zone_durations_updated_at\": 1429361044.5683227,\"desired_master_valve\": false,\"desired_master_valve_updated_at\": 1429361044.6298194,\"desired_rain_sensor\": false,\"desired_rain_sensor_updated_at\": 1429361042.534966,\"desired_schedule_enabled\": false,\"desired_schedule_enabled_updated_at\": 1429361044.6298397,\"desired_run_zone_indices\": [],\"desired_run_zone_indices_updated_at\": 1429361042.5349822,\"desired_run_zone_durations\": [],\"desired_run_zone_durations_updated_at\": 1429361042.5349896},\"lat_lng\": [41.38983,-81.42602],\"location\": \"\",\"zones\": [{\"name\": \"Top Driveway\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6298468,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6298535,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6298602,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6298668,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.629873,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6298814},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 0,\"zone_id\": \"12501\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Driveway Bottom\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6298881,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6298945,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6299007,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6299071,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6299136,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6299202},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 1,\"zone_id\": \"12502\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Tree Lawn\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6299269,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6299338,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6299408,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6299474,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6299543,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.629961},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 2,\"zone_id\": \"12503\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Back Right\",\"desired_state\": {\"enabled\": false,\"enabled_updated_at\": 1429361044.6299675,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6299734,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6299799,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6299863,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6299949,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6300077},\"last_reading\": {\"enabled\": false,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 3,\"zone_id\": \"12504\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Mailbox\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6300216,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6300349,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6300464,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.630054,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6300609,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6300678},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 4,\"zone_id\": \"12505\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Back Center\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6300743,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6300812,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6300881,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6300948,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6301012,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6301079},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 5,\"zone_id\": \"12506\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Back Left\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6301141,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6301203,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6301262,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6301327,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6301389,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6301456},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 6,\"zone_id\": \"12507\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Zone 8\",\"desired_state\": {\"enabled\": false,\"enabled_updated_at\": 1429361044.630152,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6301584,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6301646,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6301739,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6301811,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6301878},\"last_reading\": {\"enabled\": false,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 7,\"zone_id\": \"12508\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"}]},");

                        //Ascend
                        //responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"garage_door_id\": \"16896\", \"name\": \"zTest Ascend Garage Door\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1430344326, \"hidden_at\": null, \"capabilities\": {}, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"9fe350e72a807190df334cd839a00992f59446b2|garage_door-16896|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"triggers\": [], \"desired_state\": { \"position\": 0.0, \"laser\": false, \"calibration_enabled\": false }, \"manufacturer_device_model\": \"quirky_ge_ascend\", \"manufacturer_device_id\": null, \"device_manufacturer\": \"quirky_ge\", \"model_name\": \"Ascend\", \"upc_id\": \"182\", \"linked_service_id\": null, \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1430362778.3658881, \"position\": 0.0, \"position_updated_at\": 1430362778.3658676, \"position_opened\": \"N/A\", \"position_opened_updated_at\": 1430351386.88027, \"battery\": null, \"battery_updated_at\": null, \"fault\": false, \"fault_updated_at\": 1430362778.3658946, \"control_enabled\": null, \"control_enabled_updated_at\": null, \"laser\": false, \"laser_updated_at\": 1430362778.3658564, \"buzzer\": null, \"buzzer_updated_at\": null, \"led\": null, \"led_updated_at\": null, \"moving\": null, \"moving_updated_at\": null, \"calibrated\": true, \"calibrated_updated_at\": 1430362778.3658812, \"calibration_enabled\": false, \"calibration_enabled_updated_at\": 1430362778.3658745, \"last_error\": null, \"last_error_updated_at\": 1430362778.3659008, \"desired_position\": 0.0, \"desired_position_updated_at\": 1430362531.397609, \"desired_laser\": false, \"desired_laser_updated_at\": 1430345748.1740961, \"desired_calibration_enabled\": false, \"desired_calibration_enabled_updated_at\": 1430345759.9318135 }, \"lat_lng\": [ 41.675863, -81.287492 ], \"location\": \"44060\", \"mac_address\": null, \"serial\": \"20000c2a69088729\", \"order\": null},");

                        //T
                        //responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : 1431612949.9196968 }, \"sensor_pod_id\" : \"18773\", \"name\" : \"Second floor sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1409145807, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"battery\", \"type\" : \"percentage\" }, { \"field\" : \"brightness\", \"type\" : \"percentage\" }, { \"field\" : \"external_power\", \"type\" : \"boolean\" }, { \"field\" : \"humidity\", \"type\" : \"integer_percentage\" }, { \"field\" : \"loudness\", \"type\" : \"percentage\" }, { \"field\" : \"temperature\", \"type\" : \"float\" }, { \"field\" : \"vibration\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"quirky_ge_spotter\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"quirky_ge\", \"model_name\" : \"Spotter\", \"upc_id\" : \"25\", \"gang_id\" : null, \"hub_id\" : null, \"local_id\" : null, \"radio_type\" : null, \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431627611.3811483, \"agent_session_id\" : null, \"agent_session_id_updated_at\" : 1425373154.4904332, \"battery\" : 0.45, \"battery_updated_at\" : 1431627611.3811412, \"brightness\" : null, \"brightness_updated_at\" : null, \"external_power\" : false, \"external_power_updated_at\" : 1431627611.3811133, \"humidity\" : 42, \"humidity_updated_at\" : 1431627611.3811243, \"loudness\" : null, \"loudness_updated_at\" : null, \"temperature\" : 20.0, \"temperature_updated_at\" : 1431627611.3811338, \"vibration\" : true, \"vibration_updated_at\" : 1431612949.9196968, \"brightness_true\" : null, \"brightness_true_updated_at\" : null, \"loudness_true\" : null, \"loudness_true_updated_at\" : null, \"vibration_true\" : \"N/A\", \"vibration_true_updated_at\" : 1431612949.9196968, \"battery_changed_at\" : 1431609224.1658309, \"humidity_changed_at\" : 1431627611.3811243, \"temperature_changed_at\" : 1431620227.7601471, \"vibration_changed_at\" : 1431612949.9196968 }, \"lat_lng\" : [ 39.02421, -77.039003 ], \"location\" : \"\", \"mac_address\" : \"0c2a69061620\", \"serial\" : \"ABAB00019218\", \"uuid\" : \"1dc05ffd-7f1f-4617-bd1b-6554bf64e4ca\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"53479\", \"name\" : \"Bedroom Sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1427985576, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"19\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431625783.7867568, \"agent_session_id\" : \"FALSE\", \"agent_session_id_updated_at\" : 1431625783.7867672, \"motion\" : false, \"motion_updated_at\" : 1431625783.7867749, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431625783.7867839, \"agent_session_id_changed_at\" : 1431625783.7867672, \"motion_changed_at\" : 1431625783.7867749 }, \"lat_lng\" : [ 39.024424, -77.038657 ], \"location\" : \"\", \"uuid\" : \"e9eaeaf1-2ce4-46d4-adb6-6f3f560a0594\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"53481\", \"name\" : \"Bar sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1427985745, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"21\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431626950.371877, \"agent_session_id\" : \"FALSE\", \"agent_session_id_updated_at\" : 1431626950.3718965, \"motion\" : false, \"motion_updated_at\" : 1431626950.3719103, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431626950.3719225, \"agent_session_id_changed_at\" : 1431626950.3718965, \"motion_changed_at\" : 1431626950.3719103 }, \"lat_lng\" : [ 39.024424, -77.038657 ], \"location\" : \"\", \"uuid\" : \"6e4fb2d0-0c36-4a20-8da4-dd2250e55d6a\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"53540\", \"name\" : \"Kitchen sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1428005781, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"28\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431625600.9602163, \"agent_session_id\" : \"TRUE\", \"agent_session_id_updated_at\" : 1431625600.960227, \"motion\" : true, \"motion_updated_at\" : 1431625600.9602346, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431625600.9602346, \"agent_session_id_changed_at\" : 1431625600.960227, \"motion_changed_at\" : 1431625600.9602346 }, \"lat_lng\" : [ 39.024199, -77.03895 ], \"location\" : \"\", \"uuid\" : \"efdf0aff-b5a1-49d6-af25-39236b0b87ff\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"57624\", \"name\" : \"Kitchen Sensor 2\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1429494720, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"32\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431627961.7463522, \"agent_session_id\" : \"FALSE\", \"agent_session_id_updated_at\" : 1431627961.7463634, \"motion\" : false, \"motion_updated_at\" : 1431627961.7463706, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431627961.7463789, \"agent_session_id_changed_at\" : 1431627961.7463634, \"motion_changed_at\" : 1431627961.7463706 }, \"lat_lng\" : [ 39.024238, -77.038992 ], \"location\" : \"\", \"uuid\" : \"1ce10ed1-a1a9-4dd2-b107-ed3c6c17db81\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"57687\", \"name\" : \"Office Sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1429550945, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"33\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431627423.0703807, \"agent_session_id\" : \"TRUE\", \"agent_session_id_updated_at\" : 1431627423.0703919, \"motion\" : true, \"motion_updated_at\" : 1431627423.0703995, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431627423.0703995, \"agent_session_id_changed_at\" : 1431627423.0703919, \"motion_changed_at\" : 1431627423.0703995 }, \"lat_lng\" : [ 39.024238, -77.038967 ], \"location\" : \"\", \"uuid\" : \"1edb03c9-7b8d-4af7-b2d7-da6a310180ec\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"60402\", \"name\" : \"Basement Door\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1430514447, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"opened\", \"type\" : \"boolean\" }, { \"field\" : \"battery\", \"type\" : \"percentage\" }, { \"field\" : \"tamper_detected\", \"type\" : \"boolean\" } ], \"polling_interval\" : 4200, \"home_security_device\" : true, \"offline_notification\" : true }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"quirky_ge_tripper\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"quirky_ge\", \"model_name\" : \"Tripper\", \"upc_id\" : \"184\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"34\", \"radio_type\" : \"zigbee\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431626601.5417626, \"agent_session_id\" : null, \"agent_session_id_updated_at\" : 1430514451.8775995, \"firmware_version\" : \"1.8b00 / 5.1b21\", \"firmware_version_updated_at\" : 1431626601.5417826, \"firmware_date_code\" : \"20150120\", \"firmware_date_code_updated_at\" : 1431626601.5417743, \"opened\" : false, \"opened_updated_at\" : 1431626601.541836, \"battery\" : 1.0, \"battery_updated_at\" : 1431626601.5418434, \"tamper_detected\" : false, \"tamper_detected_updated_at\" : 1431626601.5418506, \"tamper_detected_true\" : null, \"tamper_detected_true_updated_at\" : null, \"battery_voltage\" : 29, \"battery_voltage_updated_at\" : 1431626601.54179, \"battery_alarm_mask\" : 15, \"battery_alarm_mask_updated_at\" : 1431626601.5417976, \"battery_voltage_min_threshold\" : 24, \"battery_voltage_min_threshold_updated_at\" : 1431626601.5418057, \"battery_voltage_threshold_1\" : 24, \"battery_voltage_threshold_1_updated_at\" : 1431626601.5418136, \"battery_voltage_threshold_2\" : 24, \"battery_voltage_threshold_2_updated_at\" : 1431626601.5418212, \"battery_voltage_threshold_3\" : 25, \"battery_voltage_threshold_3_updated_at\" : 1431626601.5418289, \"opened_changed_at\" : 1431626601.541836, \"battery_voltage_changed_at\" : 1431549393.1340518 }, \"lat_lng\" : [ 39.024242, -77.038966 ], \"location\" : \"\", \"uuid\" : \"52d5f095-6fc1-4256-8f4e-ce3c1d8b9215\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"60965\", \"name\" : \"Front door\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1430666191, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"opened\", \"type\" : \"boolean\" }, { \"field\" : \"battery\", \"type\" : \"percentage\" }, { \"field\" : \"tamper_detected\", \"type\" : \"boolean\" } ], \"polling_interval\" : 4200, \"home_security_device\" : true, \"offline_notification\" : true }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"quirky_ge_tripper\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"quirky_ge\", \"model_name\" : \"Tripper\", \"upc_id\" : \"184\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"35\", \"radio_type\" : \"zigbee\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431619864.8199573, \"agent_session_id\" : null, \"agent_session_id_updated_at\" : 1430666193.5535953, \"firmware_version\" : \"1.8b00 / 5.1b21\", \"firmware_version_updated_at\" : 1431619864.8199759, \"firmware_date_code\" : \"20150120\", \"firmware_date_code_updated_at\" : 1431619864.8199678, \"opened\" : false, \"opened_updated_at\" : 1431619864.8200295, \"battery\" : 1.0, \"battery_updated_at\" : 1431619864.8200362, \"tamper_detected\" : false, \"tamper_detected_updated_at\" : 1431619864.8200431, \"tamper_detected_true\" : \"N/A\", \"tamper_detected_true_updated_at\" : 1430666193.5537961, \"battery_voltage\" : 28, \"battery_voltage_updated_at\" : 1431619864.819983, \"battery_alarm_mask\" : 15, \"battery_alarm_mask_updated_at\" : 1431619864.8199911, \"battery_voltage_min_threshold\" : 24, \"battery_voltage_min_threshold_updated_at\" : 1431619864.819999, \"battery_voltage_threshold_1\" : 24, \"battery_voltage_threshold_1_updated_at\" : 1431619864.8200069, \"battery_voltage_threshold_2\" : 24, \"battery_voltage_threshold_2_updated_at\" : 1431619864.820015, \"battery_voltage_threshold_3\" : 25, \"battery_voltage_threshold_3_updated_at\" : 1431619864.8200226, \"opened_changed_at\" : 1431569313.1120057, \"battery_voltage_changed_at\" : 1431535108.6598671 }, \"lat_lng\" : [ 39.024228, -77.038927 ], \"location\" : \"\", \"uuid\" : \"d396485b-4125-4e0b-8891-c847618b0d0c\" },");
                    }
                }
                #endif                
                jsonResponse = JObject.Parse(responseString);
                if (jsonResponse != null)
                {
                    return jsonResponse;
                }
            }
        }
        catch (Exception ex)
        {
            string em = ex.Message;
            if (em.Contains("404"))
                return JObject.Parse("{\"error\":404}");
        }
        return null;
    }
}
