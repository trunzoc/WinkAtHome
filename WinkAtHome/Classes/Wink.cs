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
        public string units_type;
        public string unit_value;

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

                #region debug
#if DEBUG
                //DVD
                json = JObject.Parse("{\"data\": [{\"garage_door_id\": \"8552\",\"name\": \"Garage Door\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1420250978,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"af309d2e12b86bd1e5e63123db745dad703e46fb|garage_door-8552|user-123172\"}},\"user_ids\": [\"123172\"],\"triggers\": [],\"desired_state\": {\"position\": 0.5},\"manufacturer_device_model\": \"chamberlain_vgdo\",\"manufacturer_device_id\": \"1180839\",\"device_manufacturer\": \"chamberlain\",\"model_name\": \"MyQ Garage Door Controller\",\"upc_id\": \"26\",\"linked_service_id\": \"59900\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428535030.025161,\"position\": 0.5,\"position_updated_at\": 1428535020.76,\"position_opened\": \"N/A\",\"position_opened_updated_at\": 1428534916.709,\"battery\": 1.0,\"battery_updated_at\": 1428534350.3417819,\"fault\": false,\"fault_updated_at\": 1428534350.3417749,\"control_enabled\": true,\"control_enabled_updated_at\": 1428534350.3417563,\"desired_position\": 0.0,\"desired_position_updated_at\": 1428535030.0404377},\"lat_lng\": [33.162135,-97.090945],\"location\": \"\",\"order\": 0},{\"hub_id\": \"123325\",\"name\": \"Hub\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227748,\"hidden_at\": null,\"capabilities\": {\"oauth2_clients\": [\"wink_hub\"]},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"529370b8a463cd5d7eb1c2b0fa4012d00a50a656|hub-123325|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"pairing_mode\": null},\"manufacturer_device_model\": \"wink_hub\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"wink\",\"model_name\": \"Hub\",\"upc_id\": \"15\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428536664.2008882,\"agent_session_id\": \"9e4d89ad709347c6403bed40d55c578d\",\"agent_session_id_updated_at\": 1428518268.715148,\"remote_pairable\": null,\"remote_pairable_updated_at\": null,\"updating_firmware\": false,\"updating_firmware_updated_at\": 1428518268.0768747,\"app_rootfs_version\": \"00.77\",\"app_rootfs_version_updated_at\": 1428518269.3207974,\"firmware_version\": \"0.77.0\",\"firmware_version_updated_at\": 1428518269.3207758,\"update_needed\": false,\"update_needed_updated_at\": 1428518269.3208041,\"mac_address\": \"34:23:BA:F6:35:D8\",\"mac_address_updated_at\": 1428518269.3207829,\"ip_address\": \"10.1.1.201\",\"ip_address_updated_at\": 1428518269.3207898,\"hub_version\": \"00.01\",\"hub_version_updated_at\": 1428518269.3207622,\"pairing_mode\": null,\"pairing_mode_updated_at\": 1427227748.3987196,\"desired_pairing_mode\": null,\"desired_pairing_mode_updated_at\": 1427227748.398726},\"lat_lng\": [null,null],\"location\": \"\",\"configuration\": {\"kidde_radio_code\": 0},\"update_needed\": false,\"uuid\": \"3ec4f0ad-109e-4ebe-a274-5d9768e6b84c\"},{\"light_bulb_id\": \"478420\",\"name\": \"Living room floor lamp\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227790,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"44fcf028ea9c73e9ea6af24b82e36cb632118374|light_bulb-478420|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"1\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428518540.0306776,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227806.16346,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428518540.0306881,\"powered\": false,\"powered_updated_at\": 1428518540.0306954,\"brightness\": 0.92,\"brightness_updated_at\": 1428518540.0307035,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472947.8617652,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.1631668},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478421\",\"name\": \"Dales Desk\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227790,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"e195f23aeea7800aaeb6336edea638767ba99b8d|light_bulb-478421|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": true,\"brightness\": 0.98},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"2\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428535132.99332,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227801.7766044,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428535132.9933288,\"powered\": true,\"powered_updated_at\": 1428535132.993335,\"brightness\": 0.98,\"brightness_updated_at\": 1428535132.9933429,\"desired_powered\": true,\"desired_powered_updated_at\": 1428528603.1202045,\"desired_brightness\": 0.98,\"desired_brightness_updated_at\": 1428535112.0340307},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478422\",\"name\": \"Annes Desk\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227790,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"65ed528c6bc8acb0185f90d9982954b1966c6cbf|light_bulb-478422|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": true,\"brightness\": 1.0},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"3\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428536680.3404841,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227807.3688362,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428536680.3405023,\"powered\": true,\"powered_updated_at\": 1428536680.3405173,\"brightness\": 1.0,\"brightness_updated_at\": 1428536680.340534,\"desired_powered\": true,\"desired_powered_updated_at\": 1428536663.8815196,\"desired_brightness\": 1.0,\"desired_brightness_updated_at\": 1428536663.8815293},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478423\",\"name\": \"Dales Bedside\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227791,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"39f4698c9de76f3d269293996121fea79acb5c5c|light_bulb-478423|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"5\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428534754.2444475,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227811.281913,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428534754.2444575,\"powered\": false,\"powered_updated_at\": 1428534754.2444651,\"brightness\": 0.92,\"brightness_updated_at\": 1428534754.2444727,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472922.1611123,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.3369167},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478424\",\"name\": \"Annes Bedside\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227794,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"282b2982b2f129351fcbee2ea8c1c1ab32647447|light_bulb-478424|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"6\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428523346.5536232,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227820.0656626,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428523346.5536332,\"powered\": false,\"powered_updated_at\": 1428523346.5536401,\"brightness\": 0.92,\"brightness_updated_at\": 1428523346.5536482,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472957.6509764,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.3777168},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478425\",\"name\": \"Hall 1\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227794,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"55708fc574c1cc2ea163f0dfd08853b4be53eb53|light_bulb-478425|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"9\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428521542.671829,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227813.4945884,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428521542.6718392,\"powered\": false,\"powered_updated_at\": 1428521542.6718462,\"brightness\": 0.92,\"brightness_updated_at\": 1428521542.6718552,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472951.2001102,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472951.2001269},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478426\",\"name\": \"Hall 2\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227794,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"94c12d0b66d9c638d0b410edb051684909a36346|light_bulb-478426|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"10\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428518542.8424141,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227817.3887572,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428518542.8424242,\"powered\": false,\"powered_updated_at\": 1428518542.8424311,\"brightness\": 0.92,\"brightness_updated_at\": 1428518542.8424387,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472922.4328008,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.4655426},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478427\",\"name\": \"Living room lamp\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227795,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"826893c576f52836ca9968e492df110a82351ad8|light_bulb-478427|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"12\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428520939.5528028,\"firmware_version\": \"0.1b03 / 0.4b00\",\"firmware_version_updated_at\": 1428520939.5528338,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428520939.5528123,\"powered\": false,\"powered_updated_at\": 1428520939.5528202,\"brightness\": 0.92,\"brightness_updated_at\": 1428520939.5528271,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472949.1534016,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.5071518},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478428\",\"name\": \"Hall 3\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227795,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"bd444af6442ee20ce316f5f91a14f78b2faf8220|light_bulb-478428|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"11\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428518539.8047106,\"firmware_version\": null,\"firmware_version_updated_at\": 1427227821.7048221,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428518539.8047209,\"powered\": false,\"powered_updated_at\": 1428518539.804728,\"brightness\": 0.92,\"brightness_updated_at\": 1428518539.8047359,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472949.9585643,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.5484588},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478429\",\"name\": \"Front hall 3\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227796,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"78dd678994e091868ed0de4d4f32189b0f509aba|light_bulb-478429|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"13\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428534756.4984963,\"firmware_version\": \"0.1b03 / 0.4b00\",\"firmware_version_updated_at\": 1428534756.4985278,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428534756.4985058,\"powered\": false,\"powered_updated_at\": 1428534756.4985142,\"brightness\": 0.92,\"brightness_updated_at\": 1428534756.4985211,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472961.60687,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.5925322},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478430\",\"name\": \"Front hall 1\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227797,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"0f96c74cc64e5e60f991829543762454743195f1|light_bulb-478430|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"15\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428518542.293112,\"firmware_version\": \"0.1b03 / 0.4b00\",\"firmware_version_updated_at\": 1428518542.293143,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428518542.2931216,\"powered\": false,\"powered_updated_at\": 1428518542.2931294,\"brightness\": 0.92,\"brightness_updated_at\": 1428518542.2931361,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472922.6836944,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.630353},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"light_bulb_id\": \"478431\",\"name\": \"Front hall 2\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1427227803,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"210be6efabc51130de50a6f28fdb5353022b2655|light_bulb-478431|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"powered\": false,\"brightness\": 0.92},\"manufacturer_device_model\": \"ge_\",\"manufacturer_device_id\": null,\"device_manufacturer\": \"ge\",\"model_name\": \"GE light bulb\",\"upc_id\": \"73\",\"hub_id\": \"123325\",\"local_id\": \"14\",\"radio_type\": \"zigbee\",\"linked_service_id\": null,\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428518541.556289,\"firmware_version\": \"0.1b03 / 0.4b00\",\"firmware_version_updated_at\": 1428518541.5563202,\"firmware_date_code\": \"20140812\",\"firmware_date_code_updated_at\": 1428518541.5562987,\"powered\": false,\"powered_updated_at\": 1428518541.5563066,\"brightness\": 0.92,\"brightness_updated_at\": 1428518541.5563138,\"desired_powered\": false,\"desired_powered_updated_at\": 1428472957.5910468,\"desired_brightness\": 0.92,\"desired_brightness_updated_at\": 1428472917.6678269},\"lat_lng\": [null,null],\"location\": \"\",\"order\": 0},{\"propane_tank_id\": \"6521\",\"name\": \"Refuel\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419569612,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"5055752531a8aac104827ec4ba2a3366038ee15a|propane_tank-6521|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"device_manufacturer\": \"quirky_ge\",\"model_name\": \"Refuel\",\"upc_id\": \"17\",\"last_reading\": {\"connection\": true,\"battery\": 0.52,\"remaining\": 0.5},\"lat_lng\": [33.162101,-97.090547],\"location\": \"76210\",\"mac_address\": \"0c2a6907025a\",\"serial\": \"ACAB00033589\",\"tare\": 18.0,\"tank_changed_at\": 1421352479},{\"thermostat_id\": \"27239\",\"name\": \"HOME\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419909349,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"f5cb03e4101d1668ff7933a703a864b4984fce5a|thermostat-27239|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"mode\": \"heat_only\",\"powered\": true,\"min_set_point\": 20.0,\"max_set_point\": 22.777777777777779},\"manufacturer_device_model\": \"MANHATTAN\",\"manufacturer_device_id\": \"798165\",\"device_manufacturer\": \"honeywell\",\"model_name\": \"Honeywell Wi-Fi Smart Thermostat\",\"upc_id\": \"151\",\"hub_id\": null,\"local_id\": \"00D02D49A90A\",\"radio_type\": null,\"linked_service_id\": \"57563\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428536316.8312173,\"mode\": \"heat_only\",\"mode_updated_at\": 1428536316.8312581,\"powered\": true,\"powered_updated_at\": 1428536316.831265,\"min_set_point\": 20.0,\"min_set_point_updated_at\": 1428536316.8312485,\"max_set_point\": 22.777777777777779,\"max_set_point_updated_at\": 1428536316.8312287,\"temperature\": 22.777777777777779,\"temperature_updated_at\": 1428536316.8312783,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.6666666666666667,\"deadband_updated_at\": 1428536316.831311,\"min_min_set_point\": 4.4444444444444446,\"min_min_set_point_updated_at\": 1428536316.8313046,\"max_min_set_point\": 29.444444444444443,\"max_min_set_point_updated_at\": 1428536316.8312914,\"min_max_set_point\": 16.666666666666668,\"min_max_set_point_updated_at\": 1428536316.8312984,\"max_max_set_point\": 37.222222222222221,\"max_max_set_point_updated_at\": 1428536316.831285,\"modes_allowed\": [\"auto\",\"cool_only\",\"heat_only\"],\"modes_allowed_updated_at\": 1428536316.8313177,\"units\": \"f\",\"units_updated_at\": 1428536316.8312719,\"desired_mode\": \"heat_only\",\"desired_mode_updated_at\": 1428365474.3775809,\"desired_powered\": true,\"desired_powered_updated_at\": 1424823532.9645114,\"desired_min_set_point\": 20.0,\"desired_min_set_point_updated_at\": 1428509375.1094887,\"desired_max_set_point\": 22.777777777777779,\"desired_max_set_point_updated_at\": 1428509375.109503},\"lat_lng\": [33.162074,-97.090928],\"location\": \"\",\"smart_schedule_enabled\": false}],\"errors\": [],\"pagination\": {\"count\": 16},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"009ff9de3a742a55c2bfd1998f3a81027726e79b\"}}}");
#endif
                #endregion


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
                                    }
                                }
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

                                            device.status.Add(deviceStatus);
                                        }
                                    }
                                }
                            }

                            if (keys.Contains("units") && data["units"].First != null)
                            {
                                JObject units = (JObject)data["units"];
                                if (units != null)
                                {
                                    foreach (var unit in units)
                                    {
                                        device.units_type = unit.Key;
                                        device.unit_value = unit.Value.ToString();
                                    }
                                }
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
