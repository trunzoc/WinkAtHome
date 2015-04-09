using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using WinkAtHome;

public class Wink
{
    #region token
    private static string WinkToken
    {
        get
        {
            return winkGetToken();
        }
        set
        {
            _winkToken = value;
        }
    }
    private static string _winkToken;
    
    public static string winkGetToken()
    {
        try
        {
            if (_winkToken == null)
            {
                string winkUsername = SettingMgmt.getSetting("winkUsername");
                string winkPassword = SettingMgmt.getSetting("winkPassword");
                string winkClientID = SettingMgmt.getSetting("winkClientID");
                string winkClientSecret = SettingMgmt.getSetting("winkClientSecret");

                string oAuthURL = ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkOAuthURL"];
                string sendstring = "{\"client_id\":\"" + winkClientID + "\",\"client_secret\":\"" + winkClientSecret + "\",\"username\":\"" + winkUsername + "\",\"password\":\"" + winkPassword + "\",\"grant_type\":\"password\"}";

                JObject jsonResponse = winkCallAPI(oAuthURL, "POST", sendstring, false);

                _winkToken = jsonResponse["access_token"].ToString();
            }

            return _winkToken;
        }
        catch (Exception e)
        {
            HttpContext.Current.Response.Redirect("~/Settings.aspx");
            return null;
        }
    }
    #endregion

    #region Device
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SimplePropertyAttribute : Attribute
    {
    }
    public class Device
    {
        [SimpleProperty]
        public string name { get; set; }
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string type { get; set; }
        public List<string> desired_states = new List<string>();
        public List<DeviceStatus> status = new List<DeviceStatus>();
        [SimpleProperty]
        public bool controllable { get; set; }
        [SimpleProperty]
        public bool sensor { get; set; }
        [SimpleProperty]
        public string json { get; set; }
        [SimpleProperty]
        public string units { get; set; }
        [SimpleProperty]
        public string manufacturer { get; set; }
        [SimpleProperty]
        public string model { get; set; }
        [SimpleProperty]
        public string radio_type { get; set; }

        public static Device getDeviceByID(string deviceID)
        {
            Device device = Devices.SingleOrDefault(Device => Device.id.Equals(deviceID));
            return device;
        }
        public static Device getDeviceByName(string deviceName)
        {
            Device device = Devices.SingleOrDefault(Device => Device.name.ToLower().Equals(deviceName.ToLower()));
            return device;
        }
        public static List<Device> getDevicesByType(List<string> deviceTypes)
        {
            List<Device> devices = new List<Device>();

            foreach (string deviceType in deviceTypes)
            {
                List<Device> lista = Devices.Where(device => device.type.ToLower().Equals(deviceType.ToLower())).ToList();
                devices = devices.Union(lista).ToList();
            }

            return devices;
        }
        public static List<string> getDeviceTypes()
        {
            List<string> types = Devices.Select(t => t.type).Distinct().ToList();
            if (types != null)
                types.Sort();

            return types;
        }
    }
    public class DeviceStatus
    {
        public string id;
        public string name;
        public string current_status;
        public DateTime? last_updated;

        public static void clearDeviceStatus(string deviceID)
        {
            Device device = Devices.SingleOrDefault(Device => Device.id.Equals(deviceID));
            device.status = null;
            device.status = new List<DeviceStatus>();
        }
    }
    public static List<Device> Devices
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
    private static List<Device> _devices;
    
    private static List<Device> winkGetDevices()
    {
        try
        {
            if (_devices == null)
            {
                JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetAllDevicesURL"]);

                List<Device> Devices = new List<Device>();

                if (json != null)
                {
                    foreach (JObject data in json["data"])
                    {
                        IEnumerable<string> ikeys = data.Properties().Select(p => p.Name);
                        if (ikeys != null)
                        {
                            List<string> keys = ikeys.ToList();

                            string typeName = keys[0];

                            Device device = new Device();
                            device.json = data.ToString();
                            device.controllable = false;

                            if (keys.Contains("key_id") && keys.Contains("parent_object_type") && data["parent_object_type"].ToString().ToLower() == "lock")
                            {
                                device.id = data["key_id"] != null ? data["key_id"].ToString() : "error: key_id";
                                device.name = data["name"] != null ? data["name"].ToString() : "error: name";
                                device.type = "lock_pins";
                            }
                            else
                            {
                                device.id = data[typeName] != null ? data[typeName].ToString() : "error: typeName";
                                device.name = data["name"] != null ? data["name"].ToString() : "error: name";
                                device.type = data[typeName] != null ? typeName.Replace("_id", "s").Replace("switchs", "switches") : "error: type";

                                if (keys.Contains("desired_state"))
                                {
                                    JObject states = (JObject)data["desired_state"];

                                    if (states != null)
                                    {
                                        foreach (var state in states)
                                        {
                                            device.desired_states.Add(state.Key);
                                        }

                                        if (!(device.desired_states.Count == 0 || device.type == "hubs" || device.type == "unknown_devices"))
                                        {
                                            device.controllable = true;
                                        }
                                        else if (device.desired_states.Count == 0)
                                        {
                                            device.sensor = true;
                                        }
                                    }
                                }
                            }

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

                            if (keys.Contains("last_reading"))
                            {
                                JObject readings = (JObject)data["last_reading"];
                                if (readings != null)
                                {
                                    foreach (var reading in readings)
                                    {
                                        if (!reading.Key.Contains("_updated_at"))
                                        {
                                            DeviceStatus deviceStatus = new DeviceStatus();
                                            deviceStatus.id = device.id;
                                            deviceStatus.name = reading.Key;
                                            deviceStatus.current_status = reading.Value.ToString();

                                            if (readings[reading.Key + "_updated_at"] != null)
                                            {
                                                string lastupdated = readings[reading.Key + "_updated_at"].ToString();
                                                deviceStatus.last_updated = Common.FromUnixTime(lastupdated);
                                            }

                                            if (reading.Key.Contains("units"))
                                            {
                                                device.units = readings["units"].ToString();
                                            }

                                            device.status.Add(deviceStatus);
                                        }
                                    }
                                }
                            }

                            if (keys.Contains("tank_changed_at"))
                            {
                                DeviceStatus tankstatus = new DeviceStatus();
                                tankstatus.id = device.id;
                                tankstatus.name = "tank_changed_at";
                                tankstatus.current_status = Common.FromUnixTime(data["tank_changed_at"].ToString()).ToString();
                                tankstatus.last_updated = Common.FromUnixTime(data["tank_changed_at"].ToString());
                            }

                            Devices.Add(device);
                        }
                    }
                }
                _devices = Devices.OrderBy(c => !c.controllable).ThenBy(c => c.name).ToList();
            }
            return _devices;
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    internal static void sendDeviceCommand(string deviceID, string command)
    {
        try
        {
            Device device = Device.getDeviceByID(deviceID);
            if (device != null)
            {
                string url = ConfigurationManager.AppSettings["winkRootURL"] + device.type + "/" + device.id;
                winkCallAPI(url, "PUT", command);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    internal static void getDeviceStatus(string deviceID)
    {
        try
        {
            Device device = Device.getDeviceByID(deviceID);
            if (device != null)
            {
                string url = ConfigurationManager.AppSettings["winkRootURL"] + device.type + "/" + device.id;
                JObject json = winkCallAPI(url);

                JToken data = json["data"]["last_reading"];
                JObject readings = JObject.Parse(data.ToString());
                if (readings != null)
                {
                    DeviceStatus.clearDeviceStatus(device.id);
                    foreach (var reading in readings)
                    {
                        if (!reading.Key.Contains("_updated_at"))
                        {
                            DeviceStatus deviceStatus = new DeviceStatus();
                            deviceStatus.id = device.id;
                            deviceStatus.name = reading.Key;
                            deviceStatus.current_status = reading.Value.ToString();

                            string lastupdated = readings[reading.Key + "_updated_at"].ToString();
                            deviceStatus.last_updated = Common.FromUnixTime(lastupdated);
                            device.status.Add(deviceStatus);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    public static JObject getDeviceJSON()
    {
        JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetAllDevicesURL"]);
        if (json != null)
            return json;

        return null;
    }
    #endregion

    #region Shortcut
    public class Shortcut
    {
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string name { get; set; }
        public List<ShortcutMember> members = new List<ShortcutMember>();
        [SimpleProperty]
        public string json { get; set; }

        public static Shortcut getShortcutByID(string ShortcutID)
        {
            Shortcut shortcut = Shortcuts.SingleOrDefault(s => s.id.Equals(ShortcutID));
            return shortcut;
        }
        public static Shortcut getShortcutByName(string shortcutName)
        {
            Shortcut shortcut = Shortcuts.SingleOrDefault(s => s.name.ToLower().Equals(shortcutName.ToLower()));
            return shortcut;
        }
    }
    public class ShortcutMember
    {
        public string id;
        public string type;
        public Dictionary<string, string> actions = new Dictionary<string, string>();
    }

    public static List<Shortcut> Shortcuts
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
    private static List<Shortcut> _shortcuts;
    private static List<Shortcut> winkGetShortcuts()
    {
        try
        {
            if (_shortcuts == null)
            {
                JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetShortcutsURL"]);
                List<Shortcut> Shortcuts = new List<Shortcut>();

                foreach (JObject data in json["data"])
                {
                    IList<string> keys = data.Properties().Select(p => p.Name).ToList();
                    string typeName = keys[0];

                    Shortcut shortcut = new Shortcut();

                    shortcut.id = data[typeName].ToString();
                    shortcut.name = data["name"].ToString();
                    shortcut.json = data.ToString();

                    if (keys.Contains("members"))
                    {
                        var members = data["members"];
                        foreach (var member in members)
                        {
                            ShortcutMember newmember = new ShortcutMember();
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

                    Shortcuts.Add(shortcut);
                }

                _shortcuts = Shortcuts.OrderBy(c => c.name).ToList();
            }
            return _shortcuts;
        }
        catch (Exception e)
        {
            throw e;
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
                winkCallAPI(url, "POST");

                List<ShortcutMember> members = shortcut.members;
                foreach (ShortcutMember member in members)
                {
                    List<Device> devices = new List<Device>();

                    if (member.type == "group")
                    {
                        Group groups = Group.getGroupByID(member.id);
                        foreach (KeyValuePair<string, string> entry in member.actions)
                        {
                            GroupStatus status = groups.status.SingleOrDefault(n => n.name == entry.Key);
                            if (status != null)
                                status.current_status = entry.Value;
                        }

                        foreach(GroupMember groupmember in groups.members)
                        {
                            devices.Add(Device.getDeviceByID(groupmember.id));
                        }
                    }
                    else
                    {
                        devices.Add(Device.getDeviceByID(member.id));
                    }

                    foreach (Device device in devices)
                    {
                        foreach (KeyValuePair<string, string> entry in member.actions)
                        {
                            DeviceStatus status = device.status.SingleOrDefault(p => p.name == entry.Key);
                            if (status != null)
                                status.current_status = entry.Value;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion
    
    #region Group 
    public class Group
    {
        public string id;
        public string name;
        public List<GroupMember> members = new List<GroupMember>();
        public List<GroupStatus> status = new List<GroupStatus>();

        public static Group getGroupByID(string GroupID)
        {
            Group group = Groups.SingleOrDefault(s => s.id.Equals(GroupID));
            return group;
        }
        public static Group getGroupByName(string GroupName)
        {
            Group group = Groups.SingleOrDefault(s => s.name.ToLower().Equals(GroupName.ToLower()));
            return group;
        }
    }
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
            Group group = Groups.SingleOrDefault(g => g.id.Equals(groupID));
            group.status = null;
            group.status = new List<GroupStatus>();
        }
    }

    public static List<Group> Groups
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
    private static List<Group> _groups;

    private static List<Group> winkGetGroups()
    {
        try
        {
            if (_groups == null)
            {
                JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetGroupsURL"]);
                List<Group> Groups = new List<Group>();

                foreach (JObject data in json["data"])
                {
                    IList<string> keys = data.Properties().Select(p => p.Name).ToList();
                    string typeName = keys[0];

                    Group group = new Group();

                    group.id = data[typeName].ToString();
                    group.name = data["name"].ToString();

                    if (keys.Contains("members"))
                    {
                        var members = data["members"];
                        foreach (var member in members)
                        {
                            GroupMember newmember = new GroupMember();
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
                            GroupStatus newreading = new GroupStatus();
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

                    Groups.Add(group);
                }

                _groups = Groups.OrderBy(c => c.name).ToList();
            }
            return _groups;
        }
        catch (Exception e)
        {
            throw e;
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
                winkCallAPI(url, "POST", command);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    internal static void getGroupStatus(string groupID)
    {
        try
        {
            Group group = Group.getGroupByID(groupID);
            if (group != null)
            {
                string url = ConfigurationManager.AppSettings["winkRootURL"] + "/groups/" + group.id;
                JObject json = winkCallAPI(url);

                JToken readings = json["reading_aggregation"];
                foreach (JProperty reading in readings)
                {
                    GroupStatus newreading = new GroupStatus();
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
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    #endregion

    #region Robot
    public class Robot
    {
        public string id;
        public string name;
        public string enabled;
        public DateTime last_fired;
        public static Robot getRobotByID(string RobotID)
        {
            Robot robot = Robots.SingleOrDefault(s => s.id.Equals(RobotID));
            return robot;
        }
        public static Robot getRobotByName(string robotName)
        {
            Robot robot = Robots.SingleOrDefault(s => s.name.ToLower().Equals(robotName.ToLower()));
            return robot;
        }
    }
    public static List<Robot> Robots
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
    private static List<Robot> _robots;

    private static List<Robot> winkGetRobots()
    {
        try
        {
            if (_robots == null)
            {
                JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetRobotsURL"]);
                List<Robot> Robots = new List<Robot>();

                foreach (JObject data in json["data"])
                {
                    Robot robot = new Robot();

                    robot.id = data["robot_id"].ToString();
                    robot.name = data["name"].ToString();
                    robot.enabled = data["enabled"].ToString();
                    robot.last_fired = Common.FromUnixTime(data["last_fired"].ToString());

                    Robots.Add(robot);
                }

                _robots = Robots.OrderBy(c => c.name).ToList();
            }
            return _robots;
        }
        catch (Exception e)
        {
            throw e;
        }
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

                winkCallAPI(url, "PUT", sendcommand);

                robot.enabled = newstate;
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    public static void clearWink()
    {
        _winkToken = null;
        _devices = null;
        _shortcuts = null;
        _groups = null;
        _robots = null;
    }
    public static void reloadWink()
    {
        clearWink();
        winkGetDevices();
        winkGetShortcuts();
        winkGetGroups();
        winkGetRobots();
    }
    public static Dictionary<string, string>[] winkGetServerStatus()
    {
        try
        {
            Dictionary<string, string> dictStatus = new Dictionary<string, string>();
            Dictionary<string, string> dictIncidents = new Dictionary<string, string>();
            Dictionary<string, string> dictInfo = new Dictionary<string, string>();

            JObject jsonResponse = winkCallAPI(ConfigurationManager.AppSettings["winkStatusURL"],"","",false);

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
        catch (Exception e)
        {
            return null;
        }
    }
    private static JObject winkCallAPI(string url, string method = "", string sendcommand = "", bool requiresToken = true)
    {
        try
        {
            String responseString = string.Empty;
            JObject jsonResponse = new JObject();

            using (var xhr = new WebClient())
            {
                xhr.Headers[HttpRequestHeader.ContentType] = "application/json";

                if (requiresToken)
                    xhr.Headers.Add("Authorization", "Bearer " + WinkToken);

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
                    //Add Garage Door
                    responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"garage_door_id\": \"8552\",\"name\": \"Test Chamberlain\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1420250978,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"af309d2e12b86bd1e5e63123db745dad703e46fb|garage_door-8552|user-123172\"}},\"user_ids\": [\"123172\"],\"triggers\": [],\"desired_state\": {\"position\": 1.0},\"manufacturer_device_model\": \"chamberlain_vgdo\",\"manufacturer_device_id\": \"1180839\",\"device_manufacturer\": \"chamberlain\",\"model_name\": \"MyQ Garage Door Controller\",\"upc_id\": \"26\",\"linked_service_id\": \"59900\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428535030.025161,\"position\": 1.0,\"position_updated_at\": 1428535020.76,\"position_opened\": \"N/A\",\"position_opened_updated_at\": 1428534916.709,\"battery\": 1.0,\"battery_updated_at\": 1428534350.3417819,\"fault\": false,\"fault_updated_at\": 1428534350.3417749,\"control_enabled\": true,\"control_enabled_updated_at\": 1428534350.3417563,\"desired_position\": 0.0,\"desired_position_updated_at\": 1428535030.0404377},\"lat_lng\": [33.162135,-97.090945],\"location\": \"\",\"order\": 0},");

                    //Add Honeywell Thermostat
                    responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"27239\",\"name\": \"Test Honeywell\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419909349,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"f5cb03e4101d1668ff7933a703a864b4984fce5a|thermostat-27239|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"mode\": \"heat_only\",\"powered\": true,\"min_set_point\": 20.0,\"max_set_point\": 22.777777777777779},\"manufacturer_device_model\": \"MANHATTAN\",\"manufacturer_device_id\": \"798165\",\"device_manufacturer\": \"honeywell\",\"model_name\": \"Honeywell Wi-Fi Smart Thermostat\",\"upc_id\": \"151\",\"hub_id\": null,\"local_id\": \"00D02D49A90A\",\"radio_type\": null,\"linked_service_id\": \"57563\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428536316.8312173,\"mode\": \"heat_only\",\"mode_updated_at\": 1428536316.8312581,\"powered\": true,\"powered_updated_at\": 1428536316.831265,\"min_set_point\": 20.0,\"min_set_point_updated_at\": 1428536316.8312485,\"max_set_point\": 22.777777777777779,\"max_set_point_updated_at\": 1428536316.8312287,\"temperature\": 22.777777777777779,\"temperature_updated_at\": 1428536316.8312783,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.6666666666666667,\"deadband_updated_at\": 1428536316.831311,\"min_min_set_point\": 4.4444444444444446,\"min_min_set_point_updated_at\": 1428536316.8313046,\"max_min_set_point\": 29.444444444444443,\"max_min_set_point_updated_at\": 1428536316.8312914,\"min_max_set_point\": 16.666666666666668,\"min_max_set_point_updated_at\": 1428536316.8312984,\"max_max_set_point\": 37.222222222222221,\"max_max_set_point_updated_at\": 1428536316.831285,\"modes_allowed\": [\"auto\",\"cool_only\",\"heat_only\"],\"modes_allowed_updated_at\": 1428536316.8313177,\"units\": \"f\",\"units_updated_at\": 1428536316.8312719,\"desired_mode\": \"heat_only\",\"desired_mode_updated_at\": 1428365474.3775809,\"desired_powered\": true,\"desired_powered_updated_at\": 1424823532.9645114,\"desired_min_set_point\": 20.0,\"desired_min_set_point_updated_at\": 1428509375.1094887,\"desired_max_set_point\": 22.777777777777779,\"desired_max_set_point_updated_at\": 1428509375.109503},\"lat_lng\": [33.162074,-97.090928],\"location\": \"\",\"smart_schedule_enabled\": false},");

                    //Add Nest Thermostat
                    responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"49534\",\"name\": \"Test Nest\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1427241058,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"0e67d43624e47b3633273f1236b7cde2c1823ac7|thermostat-49410|user-81926\"}},\"user_ids\": [\"81926\"],\"triggers\": [],\"desired_state\": {\"mode\": \"cool_only\",\"powered\": true,\"min_set_point\": 21.0,\"max_set_point\": 22.0,\"users_away\": false,\"fan_timer_active\": false},\"manufacturer_device_model\": \"nest\",\"manufacturer_device_id\": \"pHNukJTND3MHRBT9zks77kxx11Qobba_\",\"device_manufacturer\": \"nest\",\"model_name\": \"Learning Thermostat\",\"upc_id\": \"168\",\"hub_id\": null,\"local_id\": null,\"radio_type\": null,\"linked_service_id\": \"92972\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428516397.2914052,\"mode\": \"cool_only\",\"mode_updated_at\": 1428516397.2914376,\"powered\": true,\"powered_updated_at\": 1428516397.2914565,\"min_set_point\": 21.0,\"min_set_point_updated_at\": 1428461324.5980272,\"max_set_point\": 22.0,\"max_set_point_updated_at\": 1428516397.2914746,\"users_away\": false,\"users_away_updated_at\": 1428516397.2914917,\"fan_timer_active\": false,\"fan_timer_active_updated_at\": 1428516397.2914684,\"temperature\": 22.0,\"temperature_updated_at\": 1428516397.2914257,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.5,\"deadband_updated_at\": 1428516397.2914317,\"min_min_set_point\": null,\"min_min_set_point_updated_at\": null,\"max_min_set_point\": null,\"max_min_set_point_updated_at\": null,\"min_max_set_point\": null,\"min_max_set_point_updated_at\": null,\"max_max_set_point\": null,\"max_max_set_point_updated_at\": null,\"modes_allowed\": [\"auto\",\"heat_only\",\"cool_only\"],\"modes_allowed_updated_at\": 1428516397.2914805,\"units\": \"f\",\"units_updated_at\": 1428516397.2914197,\"eco_target\": false,\"eco_target_updated_at\": 1428516397.2914433,\"manufacturer_structure_id\": \"kdCrRKp3UahHp8xWEoJBRYX9xnQWDsoU1sb5ej9Mp5Zb41WEIOKJtg\",\"manufacturer_structure_id_updated_at\": 1428516397.2914503,\"has_fan\": true,\"has_fan_updated_at\": 1428516397.2914622,\"fan_duration\": 0,\"fan_duration_updated_at\": 1428516397.2914863,\"last_error\": null,\"last_error_updated_at\": 1427241058.6980464,\"desired_mode\": \"cool_only\",\"desired_mode_updated_at\": 1427593066.90498,\"desired_powered\": true,\"desired_powered_updated_at\": 1428462838.6427567,\"desired_min_set_point\": 21.0,\"desired_min_set_point_updated_at\": 1428427791.3297703,\"desired_max_set_point\": 22.0,\"desired_max_set_point_updated_at\": 1428497187.9092989,\"desired_users_away\": false,\"desired_users_away_updated_at\": 1428440888.9921448,\"desired_fan_timer_active\": false,\"desired_fan_timer_active_updated_at\": 1427241058.6981435},\"lat_lng\": [null,null],\"location\": \"\",\"smart_schedule_enabled\": false},");

                    //Add Refuel
                    responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"propane_tank_id\": \"6521\",\"name\": \"Test Refuel\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419569612,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"5055752531a8aac104827ec4ba2a3366038ee15a|propane_tank-6521|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"device_manufacturer\": \"quirky_ge\",\"model_name\": \"Refuel\",\"upc_id\": \"17\",\"last_reading\": {\"connection\": true,\"battery\": 0.52,\"remaining\": 0.5},\"lat_lng\": [33.162101,-97.090547],\"location\": \"76210\",\"mac_address\": \"0c2a6907025a\",\"serial\": \"ACAB00033589\",\"tare\": 18.0,\"tank_changed_at\": 1421352479},");

                    //Add Nest Thermostat for Auto mode
                    responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"49410\",\"name\": \"Test Nest\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1427241058,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"0e67d43624e47b3633273f1236b7cde2c1823ac7|thermostat-49410|user-81926\"}},\"user_ids\": [\"81926\"],\"triggers\": [],\"desired_state\": {\"mode\": \"auto\",\"powered\": true,\"min_set_point\": 21.0,\"max_set_point\": 22.0,\"users_away\": false,\"fan_timer_active\": false},\"manufacturer_device_model\": \"nest\",\"manufacturer_device_id\": \"pHNukJTND3MHRBT9zks77kxx11Qobba_\",\"device_manufacturer\": \"nest\",\"model_name\": \"Learning Thermostat\",\"upc_id\": \"168\",\"hub_id\": null,\"local_id\": null,\"radio_type\": null,\"linked_service_id\": \"92972\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428516397.2914052,\"mode\": \"auto\",\"mode_updated_at\": 1428516397.2914376,\"powered\": true,\"powered_updated_at\": 1428516397.2914565,\"min_set_point\": 21.0,\"min_set_point_updated_at\": 1428461324.5980272,\"max_set_point\": 22.0,\"max_set_point_updated_at\": 1428516397.2914746,\"users_away\": false,\"users_away_updated_at\": 1428516397.2914917,\"fan_timer_active\": false,\"fan_timer_active_updated_at\": 1428516397.2914684,\"temperature\": 22.0,\"temperature_updated_at\": 1428516397.2914257,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.5,\"deadband_updated_at\": 1428516397.2914317,\"min_min_set_point\": null,\"min_min_set_point_updated_at\": null,\"max_min_set_point\": null,\"max_min_set_point_updated_at\": null,\"min_max_set_point\": null,\"min_max_set_point_updated_at\": null,\"max_max_set_point\": null,\"max_max_set_point_updated_at\": null,\"modes_allowed\": [\"auto\",\"heat_only\",\"cool_only\"],\"modes_allowed_updated_at\": 1428516397.2914805,\"units\": \"f\",\"units_updated_at\": 1428516397.2914197,\"eco_target\": false,\"eco_target_updated_at\": 1428516397.2914433,\"manufacturer_structure_id\": \"kdCrRKp3UahHp8xWEoJBRYX9xnQWDsoU1sb5ej9Mp5Zb41WEIOKJtg\",\"manufacturer_structure_id_updated_at\": 1428516397.2914503,\"has_fan\": true,\"has_fan_updated_at\": 1428516397.2914622,\"fan_duration\": 0,\"fan_duration_updated_at\": 1428516397.2914863,\"last_error\": null,\"last_error_updated_at\": 1427241058.6980464,\"desired_mode\": \"auto\",\"desired_mode_updated_at\": 1427593066.90498,\"desired_powered\": true,\"desired_powered_updated_at\": 1428462838.6427567,\"desired_min_set_point\": 21.0,\"desired_min_set_point_updated_at\": 1428427791.3297703,\"desired_max_set_point\": 22.0,\"desired_max_set_point_updated_at\": 1428497187.9092989,\"desired_users_away\": false,\"desired_users_away_updated_at\": 1428440888.9921448,\"desired_fan_timer_active\": false,\"desired_fan_timer_active_updated_at\": 1427241058.6981435},\"lat_lng\": [null,null],\"location\": \"\",\"smart_schedule_enabled\": false},");
                }
                #endif
                
                jsonResponse = JObject.Parse(responseString);
                if (jsonResponse != null)
                {
                    return jsonResponse;
                }
            }
        }
        catch
        {

        }
        return null;
    }
}
