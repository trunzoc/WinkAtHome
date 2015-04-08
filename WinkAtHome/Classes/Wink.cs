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
    public class Device
    {
        public string name;
        public string id;
        public string type;
        public List<string> desired_states = new List<string>();
        public List<DeviceStatus> status = new List<DeviceStatus>();
        public bool controllable;
        public string json;

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

                foreach (JObject data in json["data"])
                {
                    IList<string> keys = data.Properties().Select(p => p.Name).ToList();
                    string typeName = keys[0];

                    Device device = new Device();
                    device.json = data.ToString();
                    device.controllable = false;

                    if (keys.Contains("key_id") && keys.Contains("parent_object_type") && data["parent_object_type"].ToString().ToLower() == "lock")
                    {
                        device.id = data["key_id"].ToString();
                        device.name = data["name"].ToString();
                        device.type = "lock_pins";
                    }
                    else
                    {
                        device.id = data[typeName].ToString();
                        device.name = data["name"].ToString();
                        device.type = typeName.Replace("_id", "s").Replace("switchs", "switches");
                        
                        if (keys.Contains("desired_state"))
                        {
                            JObject states = (JObject)data["desired_state"];
                            foreach (var state in states)
                            {
                                device.desired_states.Add(state.Key);
                            }

                            if (!(device.desired_states.Count == 0 || device.type == "hubs" || device.type == "unknown_devices"))
                            {
                                device.controllable = true;
                            }
                        }
                    }

                    if (keys.Contains("last_reading"))
                    {
                        JObject readings = (JObject)data["last_reading"];
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
                    Devices.Add(device);
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
    #endregion

    #region Shortcut
    public class Shortcut
    {
        public string id;
        public string name;
        public List<ShortcutMember> members = new List<ShortcutMember>();

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

        jsonResponse = JObject.Parse(responseString);
        return jsonResponse;
    }
}
