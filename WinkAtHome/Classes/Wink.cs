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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WinkAtHome;

public class WinkHelper
{
    Wink myWink = HttpContext.Current != null ? HttpContext.Current.Session["_wink"] == null ? new Wink() : (Wink)HttpContext.Current.Session["_wink"] : null;
    public DateTime lastDeviceSubscribed
    {
        get
        {
            object o = HttpContext.Current.Session["lastDeviceSubscribed"];
            if (o != null)
            {
                return (DateTime)o;
            }
            HttpContext.Current.Session["lastDeviceSubscribed"] = new DateTime();
            return (DateTime)HttpContext.Current.Session["lastDeviceSubscribed"];
        }
        set
        {
            HttpContext.Current.Session["lastDeviceSubscribed"] = value;
        }
    }
    public DateTime lastGroupSubscribed
    {
        get
        {
            object o = HttpContext.Current.Session["lastGroupSubscribed"];
            if (o != null)
            {
                return (DateTime)o;
            }
            HttpContext.Current.Session["lastGroupSubscribed"] = new DateTime();
            return (DateTime)HttpContext.Current.Session["lastGroupSubscribed"];
        }
        set
        {
            HttpContext.Current.Session["lastGroupSubscribed"] = value;
        }
    }

#region public Functions
    public bool validateWinkCredentialsByUsername(string username, string password)
    {
        try
        {
            if (HttpContext.Current.Session["_wink"] == null)
                HttpContext.Current.Session["_wink"] = new Wink();

            string token = new WinkHelper.TokenHelper().winkGetTokenByUsername(username, password, true, ConfigurationManager.AppSettings["APIClientID"], ConfigurationManager.AppSettings["APIClientSecret"]);
            if (token != null)
            {
                myWink.winkUser.password = Common.Encrypt(password);

                return true;
            }
        }
        catch (Exception)
        {
        }

        return false;
    }
    public bool validateWinkCredentialsByAuthCode(string AuthCode)
    {
        try
        {
            if (HttpContext.Current.Session["_wink"] == null)
                HttpContext.Current.Session["_wink"] = new Wink();

            string token = new WinkHelper.TokenHelper().winkGetTokenByAuthToken(AuthCode, true, ConfigurationManager.AppSettings["APIClientID"], ConfigurationManager.AppSettings["APIClientSecret"]);
            if (token != null)
            {
                return true;
            }
        }
        catch (Exception)
        {
        }

        return false;
    }
    public bool validateWinkCredentialsByToken(string Token)
    {
        try
        {
            if (HttpContext.Current.Session["_wink"] == null)
                HttpContext.Current.Session["_wink"] = new Wink();

            HttpContext.Current.Session["_winkToken"] = Token;
            myWink.Token = Token;

            Wink.User user = new WinkHelper.UserHelper().winkGetUser(true);

            if (user != null)
                return true;
        }
        catch (Exception)
        {
        }

        return false;
    }
    public void clearWink()
    {
        HttpContext.Current.Session["_wink"] = null;
    }
    public void reloadWink(bool clearFirst = true)
    {
        new WinkHelper.DeviceHelper().winkGetDevices(null, true);
        new WinkHelper.ShortcutHelper().winkGetShortcuts(null, true);
        new WinkHelper.GroupHelper().winkGetGroups(null, true);
        new WinkHelper.RobotHelper().winkGetRobots(null, true);
    }
    public Dictionary<string, string>[] winkGetServerStatus()
    {
        try
        {
            Dictionary<string, string> dictStatus = new Dictionary<string, string>();
            Dictionary<string, string> dictIncidents = new Dictionary<string, string>();
            Dictionary<string, string> dictInfo = new Dictionary<string, string>();

            JObject jsonResponse = winkCallAPI(ConfigurationManager.AppSettings["winkStatusURL"], "", "", false);

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
    protected JObject winkCallAPI(string url, string method = "", string sendcommand = "", bool requiresToken = true, string TokenOverride = null)
    {
        try
        {
            String responseString = string.Empty;
            JObject jsonResponse = new JObject();

            using (var xhr = new WebClient())
            {
                xhr.Headers[HttpRequestHeader.ContentType] = "application/json";

                if (requiresToken)
                {
                    string token = TokenOverride != null ? TokenOverride : HttpContext.Current.Session["_winkToken"].ToString();
                    xhr.Headers.Add("Authorization", "Bearer " + token);
                }

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
                    if (myWink.winkUser != null && myWink.winkUser.email.ToLower().Contains("trunzo"))
                    {
                        //Add Garage Door
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"garage_door_id\": \"8552\",\"name\": \"zTest Chamberlain\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1420250978,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"af309d2e12b86bd1e5e63123db745dad703e46fb|garage_door-8552|user-123172\"}},\"user_ids\": [\"123172\"],\"triggers\": [],\"desired_state\": {\"position\": 1.0},\"manufacturer_device_model\": \"chamberlain_vgdo\",\"manufacturer_device_id\": \"1180839\",\"device_manufacturer\": \"chamberlain\",\"model_name\": \"MyQ Garage Door Controller\",\"upc_id\": \"26\",\"linked_service_id\": \"59900\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428535030.025161,\"position\": 1.0,\"position_updated_at\": 1428535020.76,\"position_opened\": \"N/A\",\"position_opened_updated_at\": 1428534916.709,\"battery\": 1.0,\"battery_updated_at\": 1428534350.3417819,\"fault\": false,\"fault_updated_at\": 1428534350.3417749,\"control_enabled\": true,\"control_enabled_updated_at\": 1428534350.3417563,\"desired_position\": 0.0,\"desired_position_updated_at\": 1428535030.0404377},\"lat_lng\": [33.162135,-97.090945],\"location\": \"\",\"order\": 0},");

                        //Add Honeywell Thermostat
                                responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"27239\",\"name\": \"zTest Honeywell\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1419909349,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"f5cb03e4101d1668ff7933a703a864b4984fce5a|thermostat-27239|user-123172\"}},\"user_ids\": [\"123172\",\"157050\"],\"triggers\": [],\"desired_state\": {\"mode\": \"heat_only\",\"powered\": true,\"min_set_point\": 20.0,\"max_set_point\": 22.777777777777779},\"manufacturer_device_model\": \"MANHATTAN\",\"manufacturer_device_id\": \"798165\",\"device_manufacturer\": \"honeywell\",\"model_name\": \"Honeywell Wi-Fi Smart Thermostat\",\"upc_id\": \"151\",\"hub_id\": null,\"local_id\": \"00D02D49A90A\",\"radio_type\": null,\"linked_service_id\": \"57563\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428536316.8312173,\"mode\": \"heat_only\",\"mode_updated_at\": 1428536316.8312581,\"powered\": true,\"powered_updated_at\": 1428536316.831265,\"min_set_point\": 20.0,\"min_set_point_updated_at\": 1428536316.8312485,\"max_set_point\": 22.777777777777779,\"max_set_point_updated_at\": 1428536316.8312287,\"temperature\": 22.777777777777779,\"temperature_updated_at\": 1428536316.8312783,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.6666666666666667,\"deadband_updated_at\": 1428536316.831311,\"min_min_set_point\": 4.4444444444444446,\"min_min_set_point_updated_at\": 1428536316.8313046,\"max_min_set_point\": 29.444444444444443,\"max_min_set_point_updated_at\": 1428536316.8312914,\"min_max_set_point\": 16.666666666666668,\"min_max_set_point_updated_at\": 1428536316.8312984,\"max_max_set_point\": 37.222222222222221,\"max_max_set_point_updated_at\": 1428536316.831285,\"modes_allowed\": [\"auto\",\"cool_only\",\"heat_only\"],\"modes_allowed_updated_at\": 1428536316.8313177,\"units\": \"f\",\"units_updated_at\": 1428536316.8312719,\"desired_mode\": \"heat_only\",\"desired_mode_updated_at\": 1428365474.3775809,\"desired_powered\": true,\"desired_powered_updated_at\": 1424823532.9645114,\"desired_min_set_point\": 20.0,\"desired_min_set_point_updated_at\": 1428509375.1094887,\"desired_max_set_point\": 22.777777777777779,\"desired_max_set_point_updated_at\": 1428509375.109503},\"lat_lng\": [33.162074,-97.090928],\"location\": \"\",\"smart_schedule_enabled\": false},");

                        //Add Nest Thermostat
                                responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"thermostat_id\": \"49534\",\"name\": \"zTest Nest\",\"locale\": \"en_us\",\"units\": {\"temperature\": \"f\"},\"created_at\": 1427241058,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"0e67d43624e47b3633273f1236b7cde2c1823ac7|thermostat-49410|user-81926\"}},\"user_ids\": [\"81926\"],\"triggers\": [],\"desired_state\": {\"mode\": \"cool_only\",\"powered\": true,\"min_set_point\": 21.0,\"max_set_point\": 22.0,\"users_away\": false,\"fan_timer_active\": false},\"manufacturer_device_model\": \"nest\",\"manufacturer_device_id\": \"pHNukJTND3MHRBT9zks77kxx11Qobba_\",\"device_manufacturer\": \"nest\",\"model_name\": \"Learning Thermostat\",\"upc_id\": \"168\",\"hub_id\": null,\"local_id\": null,\"radio_type\": null,\"linked_service_id\": \"92972\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1428516397.2914052,\"mode\": \"cool_only\",\"mode_updated_at\": 1428516397.2914376,\"powered\": true,\"powered_updated_at\": 1428516397.2914565,\"min_set_point\": 21.0,\"min_set_point_updated_at\": 1428461324.5980272,\"max_set_point\": 22.0,\"max_set_point_updated_at\": 1428516397.2914746,\"users_away\": false,\"users_away_updated_at\": 1428516397.2914917,\"fan_timer_active\": false,\"fan_timer_active_updated_at\": 1428516397.2914684,\"temperature\": 22.0,\"temperature_updated_at\": 1428516397.2914257,\"external_temperature\": null,\"external_temperature_updated_at\": null,\"deadband\": 1.5,\"deadband_updated_at\": 1428516397.2914317,\"min_min_set_point\": null,\"min_min_set_point_updated_at\": null,\"max_min_set_point\": null,\"max_min_set_point_updated_at\": null,\"min_max_set_point\": null,\"min_max_set_point_updated_at\": null,\"max_max_set_point\": null,\"max_max_set_point_updated_at\": null,\"modes_allowed\": [\"auto\",\"heat_only\",\"cool_only\"],\"modes_allowed_updated_at\": 1428516397.2914805,\"units\": \"f\",\"units_updated_at\": 1428516397.2914197,\"eco_target\": false,\"eco_target_updated_at\": 1428516397.2914433,\"manufacturer_structure_id\": \"kdCrRKp3UahHp8xWEoJBRYX9xnQWDsoU1sb5ej9Mp5Zb41WEIOKJtg\",\"manufacturer_structure_id_updated_at\": 1428516397.2914503,\"has_fan\": true,\"has_fan_updated_at\": 1428516397.2914622,\"fan_duration\": 0,\"fan_duration_updated_at\": 1428516397.2914863,\"last_error\": null,\"last_error_updated_at\": 1427241058.6980464,\"desired_mode\": \"cool_only\",\"desired_mode_updated_at\": 1427593066.90498,\"desired_powered\": true,\"desired_powered_updated_at\": 1428462838.6427567,\"desired_min_set_point\": 21.0,\"desired_min_set_point_updated_at\": 1428427791.3297703,\"desired_max_set_point\": 22.0,\"desired_max_set_point_updated_at\": 1428497187.9092989,\"desired_users_away\": false,\"desired_users_away_updated_at\": 1428440888.9921448,\"desired_fan_timer_active\": false,\"desired_fan_timer_active_updated_at\": 1427241058.6981435},\"lat_lng\": [null,null],\"location\": \"\",\"smart_schedule_enabled\": false},");

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

                        //Rachio Iro
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{\"sprinkler_id\": \"1483\",\"name\": \"Sprinkler\",\"locale\": \"en_us\",\"units\": {},\"created_at\": 1429295028,\"hidden_at\": null,\"capabilities\": {},\"subscription\": {\"pubnub\": {\"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\",\"channel\": \"ecbd8151da8524635b2dfd777c4f55f33a042285|sprinkler-1483|user-186645\"}},\"user_ids\": [\"186645\"],\"desired_state\": {\"master_valve\": false,\"rain_sensor\": false,\"schedule_enabled\": false,\"run_zone_indices\": [],\"run_zone_durations\": []},\"manufacturer_device_model\": \"rachio_iro\",\"manufacturer_device_id\": \"b26d4e70-f4df-481a-9149-c9bac4c3a09e\",\"device_manufacturer\": \"rachio\",\"model_name\": \"Iro\",\"upc_id\": \"152\",\"linked_service_id\": \"100792\",\"last_reading\": {\"connection\": true,\"connection_updated_at\": 1429477160.725,\"master_valve\": false,\"master_valve_updated_at\": 1429477160.725,\"rain_sensor\": false,\"rain_sensor_updated_at\": 1429477160.725,\"schedule_enabled\": false,\"schedule_enabled_updated_at\": 1429477160.725,\"run_zone_indices\": [],\"run_zone_indices_updated_at\": 1429361044.5683227,\"run_zone_durations\": [],\"run_zone_durations_updated_at\": 1429361044.5683227,\"desired_master_valve\": false,\"desired_master_valve_updated_at\": 1429361044.6298194,\"desired_rain_sensor\": false,\"desired_rain_sensor_updated_at\": 1429361042.534966,\"desired_schedule_enabled\": false,\"desired_schedule_enabled_updated_at\": 1429361044.6298397,\"desired_run_zone_indices\": [],\"desired_run_zone_indices_updated_at\": 1429361042.5349822,\"desired_run_zone_durations\": [],\"desired_run_zone_durations_updated_at\": 1429361042.5349896},\"lat_lng\": [41.38983,-81.42602],\"location\": \"\",\"zones\": [{\"name\": \"Top Driveway\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6298468,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6298535,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6298602,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6298668,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.629873,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6298814},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 0,\"zone_id\": \"12501\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Driveway Bottom\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6298881,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6298945,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6299007,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6299071,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6299136,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6299202},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 1,\"zone_id\": \"12502\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Tree Lawn\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6299269,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6299338,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6299408,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6299474,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6299543,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.629961},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 2,\"zone_id\": \"12503\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Back Right\",\"desired_state\": {\"enabled\": false,\"enabled_updated_at\": 1429361044.6299675,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6299734,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6299799,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6299863,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6299949,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6300077},\"last_reading\": {\"enabled\": false,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 3,\"zone_id\": \"12504\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Mailbox\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6300216,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6300349,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6300464,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.630054,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6300609,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6300678},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 4,\"zone_id\": \"12505\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Back Center\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6300743,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6300812,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6300881,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6300948,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6301012,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6301079},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 5,\"zone_id\": \"12506\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Back Left\",\"desired_state\": {\"enabled\": true,\"enabled_updated_at\": 1429361044.6301141,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6301203,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6301262,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6301327,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6301389,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6301456},\"last_reading\": {\"enabled\": true,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 6,\"zone_id\": \"12507\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"},{\"name\": \"Zone 8\",\"desired_state\": {\"enabled\": false,\"enabled_updated_at\": 1429361044.630152,\"shade\": \"none\",\"shade_updated_at\": 1429361044.6301584,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429361044.6301646,\"soil\": \"top_soil\",\"soil_updated_at\": 1429361044.6301739,\"slope\": \"flat\",\"slope_updated_at\": 1429361044.6301811,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429361044.6301878},\"last_reading\": {\"enabled\": false,\"enabled_updated_at\": 1429477160.725,\"shade\": \"none\",\"shade_updated_at\": 1429477160.725,\"nozzle\": \"fixed_spray_head\",\"nozzle_updated_at\": 1429477160.725,\"soil\": \"top_soil\",\"soil_updated_at\": 1429477160.725,\"slope\": \"flat\",\"slope_updated_at\": 1429477160.725,\"vegetation\": \"grass\",\"vegetation_updated_at\": 1429477160.725,\"powered\": false,\"powered_updated_at\": 1429361044.5683227},\"zone_index\": 7,\"zone_id\": \"12508\",\"parent_object_type\": \"sprinkler\",\"parent_object_id\": \"1483\"}]},");

                        //Ascend
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"garage_door_id\": \"16896\", \"name\": \"zTest Ascend Garage Door\", \"locale\": \"en_us\", \"units\": {}, \"created_at\": 1430344326, \"hidden_at\": null, \"capabilities\": {}, \"subscription\": { \"pubnub\": { \"subscribe_key\": \"sub-c-f7bf7f7e-0542-11e3-a5e8-02ee2ddab7fe\", \"channel\": \"9fe350e72a807190df334cd839a00992f59446b2|garage_door-16896|user-145398\" } }, \"user_ids\": [ \"145398\" ], \"triggers\": [], \"desired_state\": { \"position\": 0.0, \"laser\": false, \"calibration_enabled\": false }, \"manufacturer_device_model\": \"quirky_ge_ascend\", \"manufacturer_device_id\": null, \"device_manufacturer\": \"quirky_ge\", \"model_name\": \"Ascend\", \"upc_id\": \"182\", \"linked_service_id\": null, \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1430362778.3658881, \"position\": 0.0, \"position_updated_at\": 1430362778.3658676, \"position_opened\": \"N/A\", \"position_opened_updated_at\": 1430351386.88027, \"battery\": null, \"battery_updated_at\": null, \"fault\": false, \"fault_updated_at\": 1430362778.3658946, \"control_enabled\": null, \"control_enabled_updated_at\": null, \"laser\": false, \"laser_updated_at\": 1430362778.3658564, \"buzzer\": null, \"buzzer_updated_at\": null, \"led\": null, \"led_updated_at\": null, \"moving\": null, \"moving_updated_at\": null, \"calibrated\": true, \"calibrated_updated_at\": 1430362778.3658812, \"calibration_enabled\": false, \"calibration_enabled_updated_at\": 1430362778.3658745, \"last_error\": null, \"last_error_updated_at\": 1430362778.3659008, \"desired_position\": 0.0, \"desired_position_updated_at\": 1430362531.397609, \"desired_laser\": false, \"desired_laser_updated_at\": 1430345748.1740961, \"desired_calibration_enabled\": false, \"desired_calibration_enabled_updated_at\": 1430345759.9318135 }, \"lat_lng\": [ 41.675863, -81.287492 ], \"location\": \"44060\", \"mac_address\": null, \"serial\": \"20000c2a69088729\", \"order\": null},");

                        //T
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"53479\", \"name\" : \"Bedroom Sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1427985576, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"19\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431625783.7867568, \"agent_session_id\" : \"FALSE\", \"agent_session_id_updated_at\" : 1431625783.7867672, \"motion\" : true, \"motion_updated_at\" : 1431625783.7867749, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431625783.7867839, \"agent_session_id_changed_at\" : 1431625783.7867672, \"motion_changed_at\" : 1431625783.7867749 }, \"lat_lng\" : [ 39.024424, -77.038657 ], \"location\" : \"\", \"uuid\" : \"e9eaeaf1-2ce4-46d4-adb6-6f3f560a0594\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"53481\", \"name\" : \"Bar sensor\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1427985745, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"motion\", \"type\" : \"boolean\" } ] }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"ecolink_pir_zwavve2\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"Ecolink\", \"model_name\" : \"Motion Sensor\", \"upc_id\" : \"173\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"21\", \"radio_type\" : \"zwave\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431626950.371877, \"agent_session_id\" : \"FALSE\", \"agent_session_id_updated_at\" : 1431626950.3718965, \"motion\" : false, \"motion_updated_at\" : 1431626950.3719103, \"motion_true\" : \"N/A\", \"motion_true_updated_at\" : 1431626950.3719225, \"agent_session_id_changed_at\" : 1431626950.3718965, \"motion_changed_at\" : 1431626950.3719103 }, \"lat_lng\" : [ 39.024424, -77.038657 ], \"location\" : \"\", \"uuid\" : \"6e4fb2d0-0c36-4a20-8da4-dd2250e55d6a\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"60402\", \"name\" : \"Basement Door\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1430514447, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"opened\", \"type\" : \"boolean\" }, { \"field\" : \"battery\", \"type\" : \"percentage\" }, { \"field\" : \"tamper_detected\", \"type\" : \"boolean\" } ], \"polling_interval\" : 4200, \"home_security_device\" : true, \"offline_notification\" : true }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"quirky_ge_tripper\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"quirky_ge\", \"model_name\" : \"Tripper\", \"upc_id\" : \"184\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"34\", \"radio_type\" : \"zigbee\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431626601.5417626, \"agent_session_id\" : null, \"agent_session_id_updated_at\" : 1430514451.8775995, \"firmware_version\" : \"1.8b00 / 5.1b21\", \"firmware_version_updated_at\" : 1431626601.5417826, \"firmware_date_code\" : \"20150120\", \"firmware_date_code_updated_at\" : 1431626601.5417743, \"opened\" : false, \"opened_updated_at\" : 1431626601.541836, \"battery\" : 1.0, \"battery_updated_at\" : 1431626601.5418434, \"tamper_detected\" : false, \"tamper_detected_updated_at\" : 1431626601.5418506, \"tamper_detected_true\" : null, \"tamper_detected_true_updated_at\" : null, \"battery_voltage\" : 29, \"battery_voltage_updated_at\" : 1431626601.54179, \"battery_alarm_mask\" : 15, \"battery_alarm_mask_updated_at\" : 1431626601.5417976, \"battery_voltage_min_threshold\" : 24, \"battery_voltage_min_threshold_updated_at\" : 1431626601.5418057, \"battery_voltage_threshold_1\" : 24, \"battery_voltage_threshold_1_updated_at\" : 1431626601.5418136, \"battery_voltage_threshold_2\" : 24, \"battery_voltage_threshold_2_updated_at\" : 1431626601.5418212, \"battery_voltage_threshold_3\" : 25, \"battery_voltage_threshold_3_updated_at\" : 1431626601.5418289, \"opened_changed_at\" : 1431626601.541836, \"battery_voltage_changed_at\" : 1431549393.1340518 }, \"lat_lng\" : [ 39.024242, -77.038966 ], \"location\" : \"\", \"uuid\" : \"52d5f095-6fc1-4256-8f4e-ce3c1d8b9215\" }, { \"last_event\" : { \"brightness_occurred_at\" : null, \"loudness_occurred_at\" : null, \"vibration_occurred_at\" : null }, \"sensor_pod_id\" : \"60965\", \"name\" : \"Front door\", \"locale\" : \"en_us\", \"units\" : {}, \"created_at\" : 1430666191, \"hidden_at\" : null, \"capabilities\" : { \"sensor_types\" : [{ \"field\" : \"opened\", \"type\" : \"boolean\" }, { \"field\" : \"battery\", \"type\" : \"percentage\" }, { \"field\" : \"tamper_detected\", \"type\" : \"boolean\" } ], \"polling_interval\" : 4200, \"home_security_device\" : true, \"offline_notification\" : true }, \"triggers\" : [], \"desired_state\" : {}, \"manufacturer_device_model\" : \"quirky_ge_tripper\", \"manufacturer_device_id\" : null, \"device_manufacturer\" : \"quirky_ge\", \"model_name\" : \"Tripper\", \"upc_id\" : \"184\", \"gang_id\" : null, \"hub_id\" : \"45262\", \"local_id\" : \"35\", \"radio_type\" : \"zigbee\", \"last_reading\" : { \"connection\" : true, \"connection_updated_at\" : 1431619864.8199573, \"agent_session_id\" : null, \"agent_session_id_updated_at\" : 1430666193.5535953, \"firmware_version\" : \"1.8b00 / 5.1b21\", \"firmware_version_updated_at\" : 1431619864.8199759, \"firmware_date_code\" : \"20150120\", \"firmware_date_code_updated_at\" : 1431619864.8199678, \"opened\" : true, \"opened_updated_at\" : 1431619864.8200295, \"battery\" : 1.0, \"battery_updated_at\" : 1431619864.8200362, \"tamper_detected\" : false, \"tamper_detected_updated_at\" : 1431619864.8200431, \"tamper_detected_true\" : \"N/A\", \"tamper_detected_true_updated_at\" : 1430666193.5537961, \"battery_voltage\" : 28, \"battery_voltage_updated_at\" : 1431619864.819983, \"battery_alarm_mask\" : 15, \"battery_alarm_mask_updated_at\" : 1431619864.8199911, \"battery_voltage_min_threshold\" : 24, \"battery_voltage_min_threshold_updated_at\" : 1431619864.819999, \"battery_voltage_threshold_1\" : 24, \"battery_voltage_threshold_1_updated_at\" : 1431619864.8200069, \"battery_voltage_threshold_2\" : 24, \"battery_voltage_threshold_2_updated_at\" : 1431619864.820015, \"battery_voltage_threshold_3\" : 25, \"battery_voltage_threshold_3_updated_at\" : 1431619864.8200226, \"opened_changed_at\" : 1431569313.1120057, \"battery_voltage_changed_at\" : 1431535108.6598671 }, \"lat_lng\" : [ 39.024228, -77.038927 ], \"location\" : \"\", \"uuid\" : \"d396485b-4125-4e0b-8891-c847618b0d0c\" },");
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{  \"light_bulb_id\": \"642148\",  \"name\": \"Patio\",  \"locale\": \"en_us\",  \"units\": {},  \"created_at\": 1432238565,  \"hidden_at\": null,  \"capabilities\": {},  \"triggers\": [],  \"desired_state\": {    \"powered\": true,    \"brightness\": 1.0  },  \"manufacturer_device_model\": \"lutron_p_pkg1_w_wh_d\",  \"manufacturer_device_id\": null,  \"device_manufacturer\": \"lutron\",  \"model_name\": \"Caseta Wireless Dimmer & Pico\",  \"upc_id\": \"3\",  \"gang_id\": null,  \"hub_id\": \"45262\",  \"local_id\": \"39\",  \"radio_type\": \"lutron\",  \"linked_service_id\": null,  \"last_reading\": {    \"connection\": true,    \"connection_updated_at\": 1432238566.2925382,    \"powered\": null,    \"powered_updated_at\": null,    \"brightness\": null,    \"brightness_updated_at\": null,    \"desired_powered\": true,    \"desired_powered_updated_at\": 1432309700.7329776,    \"desired_brightness\": 1.0,    \"desired_brightness_updated_at\": 1432309700.73299,    \"connection_changed_at\": 1432238566.2925382,    \"desired_powered_changed_at\": 1432309700.7329776,    \"desired_brightness_changed_at\": 1432309700.73299  },  \"lat_lng\": [    39.024314,    -77.038935  ],  \"location\": \"\",  \"order\": 0},");

                        //NIMBUS
                        //        responseString = responseString.Replace("{\"data\":[", "{\"data\":[" + "{ \"last_reading\": { \"connection\": true, \"connection_updated_at\": 1430176862.4790323 }, \"dials\": [ { \"name\": \"Time\", \"value\": 57874.0, \"position\": 122.2833333333333, \"label\": \"4:04 PM\", \"labels\": [ \"4:04 PM\", \"New York\" ], \"brightness\": 25, \"channel_configuration\": { \"locale\": \"en_US\", \"timezone\": \"America/New_York\", \"channel_id\": \"1\", \"object_type\": null, \"object_id\": null }, \"dial_configuration\": { \"max_position\": 720, \"max_value\": 86400, \"min_position\": 0, \"scale_type\": \"linear\", \"min_value\": 0, \"rotation\": \"cw\", \"num_ticks\": 12 }, \"dial_index\": 0, \"dial_id\": \"36845\", \"refreshed_at\": 1432065874, \"parent_object_type\": \"cloud_clock\", \"parent_object_id\": \"9099\" }, { \"name\": \"Weather\", \"value\": 6.07, \"position\": 32.778, \"label\": \"74 °F\", \"labels\": [ \"74 >f\", \"Partly Cloudy\" ], \"brightness\": 25, \"channel_configuration\": { \"location\": \"10001\", \"locale\": \"en_US\", \"lat_lng\": [ 40.75185393893581, -74.005638816321976 ], \"units\": { \"temperature\": \"f\" }, \"reading_type\": \"weather_conditions\", \"channel_id\": \"2\", \"object_type\": null, \"object_id\": null }, \"dial_configuration\": { \"max_position\": 135, \"max_value\": 25, \"min_position\": -135, \"scale_type\": \"linear\", \"min_value\": -25, \"rotation\": \"cw\", \"num_ticks\": 12 }, \"dial_index\": 1, \"dial_id\": \"36846\", \"refreshed_at\": 1432065517, \"parent_object_type\": \"cloud_clock\", \"parent_object_id\": \"9099\" }, { \"name\": \"Email\", \"value\": 0.0, \"position\": 0.0, \"label\": \"AM Light Rain\", \"labels\": [ \"AM Light Rain\" ], \"brightness\": 25, \"channel_configuration\": { \"channel_id\": \"10\", \"object_type\": null, \"object_id\": null }, \"dial_configuration\": { \"max_position\": 360, \"max_value\": 360, \"min_position\": 0, \"scale_type\": \"linear\", \"min_value\": 0, \"rotation\": \"cw\", \"num_ticks\": 12 }, \"dial_index\": 2, \"dial_id\": \"36847\", \"refreshed_at\": 1418962052, \"parent_object_type\": \"cloud_clock\", \"parent_object_id\": \"9099\" }, { \"name\": \"Email\", \"value\": 0.0, \"position\": 0.0, \"label\": \"LAComputerCompany@apple2.subscribermail.com\", \"labels\": [ \"LAComputerCompany@apple2.subscribermail.com\" ], \"brightness\": 25, \"channel_configuration\": { \"channel_id\": \"10\", \"object_type\": null, \"object_id\": null }, \"dial_configuration\": { \"max_position\": 360, \"max_value\": 360, \"min_position\": 0, \"scale_type\": \"linear\", \"min_value\": 0, \"rotation\": \"cw\", \"num_ticks\": 12 }, \"dial_index\": 3, \"dial_id\": \"36848\", \"refreshed_at\": 1418962244, \"parent_object_type\": \"cloud_clock\", \"parent_object_id\": \"9099\" } ], \"alarms\": [ { \"next_at\": null, \"enabled\": false, \"recurrence\": \"DTSTART;TZID=America/New_York:20141217T155400\", \"alarm_id\": \"5170\", \"name\": null, \"media_id\": \"1\" } ], \"cloud_clock_id\": \"9099\", \"name\": \"Nimbus\", \"locale\": \"en_us\", \"units\": { \"temperature\": \"f\" }, \"created_at\": 1418762984, \"hidden_at\": null, \"capabilities\": {}, \"triggers\": [], \"device_manufacturer\": \"quirky_ge\", \"model_name\": \"Nimbus\", \"upc_id\": \"21\", \"lat_lng\": [ 0.0, 0.0 ], \"location\": \"\", \"mac_address\": \"0c2a6904f569\", \"serial\": \"ADAA00025756\"},");
                                
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
#endregion

#region token
    public class TokenHelper : WinkHelper
    {
        public string winkGetTokenByUsername(string Username = null, string Password = null, bool forceRefresh = false, string forceClientID = null, string forceClientSecret = null)
        {
            try
            {
                if (forceRefresh || HttpContext.Current == null || (HttpContext.Current != null && HttpContext.Current.Session["_winkToken"] == null))
                {
                    string winkUsername = Username == null ? myWink.winkUser.email : Username;
                    string winkPassword = Password == null ? Common.Decrypt(myWink.winkUser.password) : Password;
                    string winkClientID = forceClientID == null ? ConfigurationManager.AppSettings["APIClientID"] : forceClientID;
                    string winkClientSecret = forceClientSecret == null ? ConfigurationManager.AppSettings["APIClientSecret"] : forceClientSecret;

                    string oAuthURL = ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkOAuthURL"];
                    string sendstring = "{\"client_id\":\"" + winkClientID + "\",\"client_secret\":\"" + winkClientSecret + "\",\"username\":\"" + winkUsername + "\",\"password\":\"" + winkPassword + "\",\"grant_type\":\"password\"}";

                    JObject jsonResponse = winkCallAPI(oAuthURL, "POST", sendstring, false);

                    myWink.Token = jsonResponse["access_token"].ToString();

                    HttpContext.Current.Session["_winkToken"] = myWink.Token;

                    new WinkHelper.UserHelper().winkGetUser(true);
                }
                else
                    myWink.Token = HttpContext.Current.Session["_winkToken"].ToString();


                return myWink.Token;
            }
            catch
            {
            }

            return null;
        }
        public string winkGetTokenByAuthToken(string AuthToken, bool forceRefresh = false, string forceClientID = null, string forceClientSecret = null)
        {
            try
            {
                if (forceRefresh || HttpContext.Current == null || (HttpContext.Current != null && HttpContext.Current.Session["_winkToken"] == null))
                {
                    string winkClientID = forceClientID == null ? ConfigurationManager.AppSettings["APIClientID"] : forceClientID;
                    string winkClientSecret = forceClientSecret == null ? ConfigurationManager.AppSettings["APIClientSecret"] : forceClientSecret;

                    string oAuthURL = ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkOAuthURL"];
                    string sendstring = "{\"client_id\":\"" + winkClientID + "\",\"client_secret\":\"" + winkClientSecret + "\",\"grant_type\":\"authorization_code\",\"code\":\"" + AuthToken + "\"}";

                    JObject jsonResponse = winkCallAPI(oAuthURL, "POST", sendstring, false);

                    myWink.Token = jsonResponse["access_token"].ToString();

                    HttpContext.Current.Session["_winkToken"] = myWink.Token;

                    new WinkHelper.UserHelper().winkGetUser(true);
                }
                else
                    myWink.Token = HttpContext.Current.Session["_winkToken"].ToString();


                return myWink.Token;
            }
            catch
            {
            }

            return null;
        }
    }
#endregion

#region user
    public class UserHelper : WinkHelper
    {
        public string userID()
        {
            string userid = myWink.winkUser.userID;
            return userid;
        }
        public Wink.User winkGetUser(bool forceRefresh = false)
        {
            try
            {
                myWink.winkUser = new Wink.User();

                if (forceRefresh || HttpContext.Current.Session["_winkUser"] == null)
                {
                    string URL = ConfigurationManager.AppSettings["winkRootURL"] + "users/me";

                    JObject jsonResponse = winkCallAPI(URL);

                    if (jsonResponse != null)
                    {
                        myWink.winkUser.userID = jsonResponse["data"]["user_id"].ToString();
                        myWink.winkUser.first_name = jsonResponse["data"]["first_name"].ToString();
                        myWink.winkUser.last_name = jsonResponse["data"]["last_name"].ToString();
                        myWink.winkUser.email = jsonResponse["data"]["email"].ToString();

                        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                        {
                            connection.Open();

                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                command.CommandText = "INSERT OR REPLACE INTO Users (UserID, Email, Last_Login) VALUES (@UserID,@Email,@Last_Login);";
                                command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                command.Parameters.Add(new SQLiteParameter("@Email", myWink.winkUser.email));
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

                        HttpContext.Current.Session["_winkUser"] = myWink.winkUser;
                    }
                    else
                        return null;
                }
                else
                    myWink.winkUser = (Wink.User)HttpContext.Current.Session["_winkUser"];

                return myWink.winkUser;
            }
            catch
            {
                return null;
            }
        }
    }
#endregion

#region Subscriptions
    public class SubscriptionHelper: WinkHelper
    {
        //public string getSubscriptionChannels()
        //{
        //    List<String> listChannels = new List<string>();

        //    if (myWink.Devices != null && myWink.Devices.Count > 0)
        //    {
        //        List<String> deviceChannels = myWink.Devices.Select(d => d.subscriptionChannel).ToList();
        //        listChannels.AddRange(deviceChannels);
        //    }
        //    if (myWink.Shortcuts != null && myWink.Shortcuts.Count > 0)
        //    {
        //        List<String> shortcutChannels = myWink.Shortcuts.Select(s => s.subscriptionChannel).ToList();
        //        listChannels.AddRange(shortcutChannels);
        //    }
        //    if (myWink.Groups != null && myWink.Groups.Count > 0)
        //    {
        //        List<String> groupChannels = myWink.Groups.Select(s => s.subscriptionChannel).ToList();
        //        listChannels.AddRange(groupChannels);
        //    }

        //    listChannels.RemoveAll(l => l == "");

        //    if (listChannels.Count > 0)
        //    {
        //        string strTopics = String.Join(",", listChannels.Distinct());
        //        return strTopics;
        //    }

        //    return null;
        //}
        
        //public Object getObjectBySubscriptionByChannel(string SubscriptionChannel)
        //{
        //    Object device = myWink.Devices.FirstOrDefault(d => d.subscriptionChannel == SubscriptionChannel);
        //    if (device != null)
        //        return device;
        //    else
        //    {
        //        Object group = myWink.Groups.FirstOrDefault(d => d.subscriptionChannel == SubscriptionChannel);
        //        if (group != null)
        //            return group;
        //        else
        //        {
        //            Object robot = myWink.Robots.FirstOrDefault(d => d.subscriptionChannel == SubscriptionChannel);
        //            if (robot != null)
        //                return robot;
        //            else
        //            {
        //                Object shortcut = myWink.Shortcuts.FirstOrDefault(d => d.subscriptionChannel == SubscriptionChannel);
        //                if (shortcut != null)
        //                    return shortcut;
        //            }
        //        }
        //    }
        //    return null;
        //}

        public void refreshSubscriptions(string type = "all")
        {
            try
            {
                if (type == "all" || type == "devices")
                {
                    ThreadPool.QueueUserWorkItem(o => new DeviceHelper().DeviceGetSubscriptions(myWink));
                    lastDeviceSubscribed = DateTime.Now;
                }
                if (type == "all" || type == "groups")
                {
                    ThreadPool.QueueUserWorkItem(o => new GroupHelper().GroupGetSubscriptions(myWink));
                    lastGroupSubscribed = DateTime.Now;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
#endregion

#region Device
    public class DeviceHelper : WinkHelper
    {
        public Wink.Device getDeviceByID(string deviceID)
        {
            Wink.Device device = myWink.Devices.FirstOrDefault(Device => Device.id.Equals(deviceID));
            return device;
        }
        public Wink.Device getDeviceByName(string deviceName)
        {
            Wink.Device device = myWink.Devices.FirstOrDefault(Device => Device.name.ToLower().Equals(deviceName.ToLower()));
            return device;
        }
        public List<Wink.Device> getDevicesByHubID(string hubID)
        {
            List<Wink.Device> devices = myWink.Devices.Where(device => device.hub_id == hubID).ToList();

            return devices;
        }
        public List<string> getDeviceTypes(bool forMenu = false)
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
        public string setDeviceDisplayName(string DeviceID, string DisplayName)
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

                Wink.Device device = myWink.Devices.FirstOrDefault(d => d.id == DeviceID);
                device.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setDeviceDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public void setDevicePosition(string DeviceID, int Position)
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
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setDevicePosition", ex.Message, EventLogEntryType.Error);
            }
        }
        public JObject DeviceGetJSON()
        {
            JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetAllDevicesURL"]);
            if (json != null)
                return json;

            return null;
        }
        public void DeviceUpdate(JObject json)
        {
            winkGetDevices(json);
        }
        public void DeviceSendCommand(string deviceID, string command)
        {
            try
            {
                Wink.Device device = new WinkHelper.DeviceHelper().getDeviceByID(deviceID);
                if (device != null)
                {
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + device.type + "/" + device.id;
                    winkCallAPI(url, "PUT", command);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private Wink.Device getDesired_States(Wink.Device device, JObject states)
        {
            if (states != null)
            {
                foreach (var state in states)
                {
                    device.desired_states.Remove(state.Key);
                    device.desired_states.Add(state.Key, state.Value.ToString());
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
        private Wink.Device getLast_Readings(Wink.Device device, JObject readings)
        {
            List<Wink.Device.DeviceStatus> states = new List<Wink.Device.DeviceStatus>();
            device.status = new List<Wink.Device.DeviceStatus>();

            if (readings != null)
            {
                foreach (var reading in readings)
                {
                    if (!reading.Key.Contains("_updated_at") && !reading.Key.Contains("_changed_at"))
                    {
                        Wink.Device.DeviceStatus deviceStatus = new Wink.Device.DeviceStatus();
                        deviceStatus.id = device.id;
                        deviceStatus.name = reading.Key;
                        deviceStatus.current_status = reading.Value.ToString();

                        if (readings[reading.Key + "_updated_at"] != null)
                        {
                            string lastupdated = readings[reading.Key + "_updated_at"].ToString();
                            deviceStatus.last_updated = Common.FromUnixTime(lastupdated, true);
                        }
                        if (readings[reading.Key + "_changed_at"] != null)
                        {
                            string lastchanged = readings[reading.Key + "_changed_at"].ToString();
                            deviceStatus.last_changed = Common.FromUnixTime(lastchanged, true);
                        }
                        device.status.Add(deviceStatus);
                    }
                }

                string power = readings["powered"] != null ? readings["powered"].ToString() : null;
                KeyValuePair<string, string> desiredpower = device.desired_states.SingleOrDefault(d => d.Key == "powered");
                if (!string.IsNullOrWhiteSpace(desiredpower.Value) && desiredpower.Value != power)
                {
                    Wink.Device.DeviceStatus deviceStatus = device.status.SingleOrDefault(s => s.name == "powered");
                    deviceStatus.current_status = desiredpower.Value;
                }

                string brightness = readings["brightness"] != null ? readings["brightness"].ToString() : null;
                KeyValuePair<string, string> desiredbrightness = device.desired_states.SingleOrDefault(d => d.Key == "brightness");
                if (!string.IsNullOrWhiteSpace(desiredbrightness.Value) && desiredbrightness.Value != brightness)
                {
                    Wink.Device.DeviceStatus deviceStatus = device.status.SingleOrDefault(s => s.name == "brightness");
                    deviceStatus.current_status = desiredbrightness.Value;
                }
            }

            return device;
        }
        public void DeviceGetSubscriptions(Wink wink = null)
        {
            try
            {
                Dictionary<string, string> devices = new Dictionary<string, string>();
                string userID = null;
                string token = null;

                if (wink != null)
                {
                    devices = wink.Devices.ToDictionary(d => d.id, d => d.type);
                    userID = wink.winkUser.userID;
                    token = wink.Token;
                }
                else if (myWink != null)
                {
                    devices = myWink.Devices.ToDictionary(d => d.id, d => d.type);
                    userID = myWink.winkUser.userID;
                    token = myWink.Token;
                }

                foreach (KeyValuePair<string,string> device in devices)
                {
                    string APIURL = ConfigurationManager.AppSettings["winkRootURL"] + device.Value + "/" + device.Key + "/subscriptions";
                    string callbackURL = ConfigurationManager.AppSettings["SubscriptionCallbackURL"] + userID + "/device-" + device.Value + "/" + device.Key;
                    string sendCommand = "{\"callback\":\"" + callbackURL + "\",\"secret\":\"" + "MDkyZmZmZWMtNDM1Yi00MjI4LThhM2UtZjI4OGFjNWExNjU3" + "\"}";
                    JObject subJSON = winkCallAPI(APIURL, "POST", sendCommand, true, token);
                }

                if (userID != null)
                {
                    WinkEventHelper.storeNewSubscriptionMessage(userID, "Subscription Event", "", "Device Subscriptions Have Been Refreshed");
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        public void winkGetDevices(JObject jsonObject = null, bool forceRefresh = false)
        {
            try
            {
                if (myWink.Devices == null || myWink.Devices.Count == 0 || jsonObject != null || forceRefresh)
                {
                    List<Wink.Device> devices = new List<Wink.Device>();
                    JObject json = null;

                    if (myWink.Devices == null || forceRefresh)
                    {
                        json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetAllDevicesURL"]);
                    }
                    else if (jsonObject != null)
                    {
                        json = jsonObject;
                        string jsonstring = json.ToString();
                        string strDeviceID = jsonstring.Substring(jsonstring.IndexOf("object_id\": \"") + 13);
                        strDeviceID = strDeviceID.Remove(strDeviceID.IndexOf("\""));
                        devices = myWink.Devices.Where(d => d.id == strDeviceID).ToList();
                    }

                    if (json != null)
                    {
                        Int32 sensorAlertTimeout = Convert.ToInt32(SettingMgmt.getSetting("Robot-Alert-Minutes-Since-Last-Trigger"));

                        foreach (JObject data in json["data"])
                        {
                            IEnumerable<string> ikeys = data.Properties().Select(p => p.Name);
                            if (ikeys != null)
                            {
                                List<string> keys = ikeys.ToList();

                                string desired_states = string.Empty;
                                string last_readings = string.Empty;

                                string typeName = keys[0];

                                Wink.Device device = new Wink.Device();

                                device.json = data.ToString();
                                device.name = data["name"] != null ? data["name"].ToString() : "error: name";
                                if (jsonObject != null)
                                {
                                    device = new WinkHelper.DeviceHelper().getDeviceByName(device.name);
                                    myWink.Devices.Remove(device);
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

                                    if (keys.Contains("manufacturer_device_model"))
                                    {
                                        device.model_name = data["manufacturer_device_model"].ToString();
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
                                    getDesired_States(device, states);
                                }
                                else
                                {
                                    device.issensor = true;
                                }

                                if (keys.Contains("last_reading"))
                                {
                                    JObject readings = (JObject)data["last_reading"];
                                    getLast_Readings(device, readings);
                                }

                                #region NON-SENSOR DEVICE-SPECIFIC CONFIGURATIONS
                                //DEVICE EXCEPTIONS

                                //Power Pivot Genius
                                if (keys.Contains("powerstrip_id"))
                                {
                                    device.id = data["powerstrip_id"].ToString();
                                    device.type = "powerstrips";
                                    device.menu_type = device.type;
                                    device.issensor = false;
                                    foreach (Wink.Device.DeviceStatus status in device.status)
                                        status.id = device.id;

                                    foreach (var outlet in data["outlets"])
                                    {
                                        Wink.Device outletdevice = new Wink.Device();
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
                                        getDesired_States(outletdevice, states);

                                        JObject readings = (JObject)outlet["last_reading"];
                                        getLast_Readings(outletdevice, readings);

                                        Wink.Device.DeviceStatus connstatus = device.status.Single(s => s.name == "connection");
                                        outletdevice.status.Add(connstatus);

                                        devices.Add(outletdevice);

                                        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                        {
                                            connection.Open();

                                            using (SQLiteCommand command = new SQLiteCommand(connection))
                                            {
                                                command.CommandText = "UPDATE Devices SET name=@name, Mfg=@Mfg, Model=@Model, ModelName=@ModelName WHERE UserID=@UserID AND DeviceID = @ID;";
                                                command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                                command.Parameters.Add(new SQLiteParameter("@ID", outletdevice.id));
                                                command.Parameters.Add(new SQLiteParameter("@name", outletdevice.name));
                                                command.Parameters.Add(new SQLiteParameter("@Mfg", outletdevice.manufacturer));
                                                command.Parameters.Add(new SQLiteParameter("@Model", outletdevice.model));
                                                command.Parameters.Add(new SQLiteParameter("@ModelName", outletdevice.model_name));
                                                command.ExecuteNonQuery();

                                                command.CommandText = "INSERT OR IGNORE INTO Devices (UserID, DeviceID, name, Mfg, Model, ModelName) VALUES (@UserID, @ID, @name, @Mfg, @Model, @ModelName);";
                                                command.ExecuteNonQuery();
                                            }
                                        }
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
                                    Wink.Device.DeviceStatus status = device.status.FirstOrDefault(s => s.name == "powering_mode");
                                    if (status.current_status == "none")
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
                                    foreach (Wink.Device.DeviceStatus status in device.status)
                                        status.id = device.id;
                                }

                                //NIMBUS
                                if (device.model != null && device.model.ToLower() == "nimbus")
                                {
                                    device.id = data["cloud_clock_id"] != null ? data["cloud_clock_id"].ToString() : "error: key_id";
                                    device.type = "cloud_clocks";
                                    device.menu_type = "nimbus";
                                    device.issensor = false;
                                    foreach (Wink.Device.DeviceStatus status in device.status)
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
                                    foreach (Wink.Device.DeviceStatus status in device.status)
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
                                        Wink.Device.DeviceStatus tankstatus = new Wink.Device.DeviceStatus();
                                        tankstatus.id = device.id;
                                        tankstatus.name = "tank_changed_at";
                                        tankstatus.current_status = Common.FromUnixTime(data["tank_changed_at"].ToString(), true).ToString();
                                        tankstatus.last_updated = Common.FromUnixTime(data["tank_changed_at"].ToString(), true);
                                        device.status.Add(tankstatus);
                                    }

                                    Wink.Device.DeviceStatus stat = device.status.FirstOrDefault(p => p.name == "remaining");
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

                                    Wink.Device.DeviceStatus costat = device.status.FirstOrDefault(p => p.name == "co_detected");
                                    if (costat != null)
                                        strStatuses += costat.current_status;

                                    Wink.Device.DeviceStatus smstat = device.status.FirstOrDefault(p => p.name == "smoke_detected");
                                    if (smstat != null)
                                        strStatuses += smstat.current_status;

                                    Wink.Device.DeviceStatus teststat = device.status.FirstOrDefault(p => p.name == "test_activated");
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
                                            JToken capabilityList = data["capabilities"];
                                            if (capabilityList != null)
                                            {
                                                JToken sensor_types = capabilityList["sensor_types"];
                                                if (sensor_types != null)
                                                {
                                                    foreach (JToken type in sensor_types)
                                                    {
                                                        string sensorname = type["field"].ToString();
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
                                            Wink.Device.DeviceStatus status = device.status.FirstOrDefault(s => s.name == capability);
                                            if (status != null)
                                            {
                                                string strValue = status.current_status;
                                                Int32 intValue = 0;
                                                Int32.TryParse(strValue, out intValue);

                                                if (status.name == "temperature")
                                                {
                                                    double temp = 0;
                                                    double.TryParse(strValue, out temp);
                                                    temp = Common.FromCelsiusToFahrenheit(temp);
                                                    status.current_status = Convert.ToString(temp);
                                                }

                                                if (intValue > 1000000000)
                                                    status.current_status = Common.FromUnixTime(strValue, true).ToString();

                                                Wink.Device.DeviceStatus changedStatus = device.status.FirstOrDefault(s => s.name == capability + "_changed_at");
                                                if (changedStatus != null)
                                                {
                                                    DateTime changedAt = Common.FromUnixTime(changedStatus.current_status, true);
                                                    status.last_changed = changedAt;
                                                }


                                                if (status.name.ToLower() != "battery" && status.name.ToLower() != "external_power"
                                                    && (status.last_changed == null || status.last_changed > DateTime.Now.AddMinutes(sensorAlertTimeout * -1)))
                                                {
                                                    if (status.current_status.ToLower() == "true" || status.current_status.ToLower() == "1")
                                                        device.sensortripped = "true";
                                                }
                                                device.sensor_states.Add(status);
                                            }
                                        }
                                    }
                                }
                                #endregion

                                devices.Add(device);

                                #region UPDATE DATABASE
                                //UPDATE DEVICE DB
                                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                                {
                                    connection.Open();

                                    using (SQLiteCommand command = new SQLiteCommand(connection))
                                    {
                                        command.CommandText = "UPDATE Devices SET name=@name, Mfg=@Mfg, Model=@Model, ModelName=@ModelName WHERE UserID=@UserID AND DeviceID = @ID;";
                                        command.Parameters.Add(new SQLiteParameter("@UserID", myWink.winkUser.userID));
                                        command.Parameters.Add(new SQLiteParameter("@ID", device.id));
                                        command.Parameters.Add(new SQLiteParameter("@name", device.name));
                                        command.Parameters.Add(new SQLiteParameter("@Mfg", device.manufacturer));
                                        command.Parameters.Add(new SQLiteParameter("@Model", device.model));
                                        command.Parameters.Add(new SQLiteParameter("@ModelName", device.model_name));
                                        command.ExecuteNonQuery();

                                        command.CommandText = "INSERT OR IGNORE INTO Devices (UserID, DeviceID, name, Mfg, Model, ModelName) VALUES (@UserID, @ID, @name, @Mfg, @Model, @ModelName);";
                                        command.ExecuteNonQuery();
                                    }
                                }
                                #endregion

                                if (jsonObject != null)
                                {
                                    myWink.Devices.Add(device);
                                }
                            }
                        }
                    }

                    if (jsonObject == null)
                        myWink.Devices = devices.OrderBy(c => !c.iscontrollable).ThenBy(c => c.name).ToList();
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

                        foreach (Wink.Device device in myWink.Devices)
                        {
                            //VALUE FROM DATABASE
                            DataRow[] rowArray = dt.Select("DeviceID = '" + device.id + "'");
                            if (rowArray.Length > 0)
                            {
                                DataRow row = rowArray[0];

                                device.position = Convert.ToInt32(row["Position"].ToString());

                                if (!string.IsNullOrWhiteSpace(row["DisplayName"].ToString()))
                                    device.displayName = row["DisplayName"].ToString();
                            }
                        }
                    }
                }
                #endregion

                #region FINAL PROCESSING
                foreach (Wink.Device device in myWink.Devices)
                {
                    Wink.Device hubDevice = myWink.Devices.FirstOrDefault(h => h.id == device.hub_id);
                    if (hubDevice != null)
                        device.hub_name = hubDevice.displayName;
                }
                #endregion

                if (lastDeviceSubscribed < DateTime.Now.AddMinutes(-60))
                {
                    ThreadPool.QueueUserWorkItem(o => DeviceGetSubscriptions(myWink));
                    lastDeviceSubscribed = DateTime.Now;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
#endregion

#region Shortcut
    public class ShortcutHelper : WinkHelper
    {
        public Wink.Shortcut getShortcutByID(string ShortcutID)
        {
            Wink.Shortcut shortcut = myWink.Shortcuts.FirstOrDefault(s => s.id.Equals(ShortcutID));
            return shortcut;
        }
        public Wink.Shortcut getShortcutByName(string shortcutName)
        {
            Wink.Shortcut shortcut = myWink.Shortcuts.FirstOrDefault(s => s.name.ToLower().Equals(shortcutName.ToLower()));
            return shortcut;
        }
        public string setShortcutDisplayName(string ShortcutID, string DisplayName)
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
                Wink.Shortcut shortcut = myWink.Shortcuts.FirstOrDefault(d => d.id == ShortcutID);
                shortcut.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setShortcutDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public void setShortcutPosition(string ShortcutID, int Position)
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
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setShortcutPosition", ex.Message, EventLogEntryType.Error);
            }
        }
        public void ShortcutActivate(string shortcutID)
        {
            try
            {
                Wink.Shortcut shortcut = new WinkHelper.ShortcutHelper().getShortcutByID(shortcutID);

                if (shortcut != null)
                {
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + "scenes/" + shortcut.id + "/activate/";
                    winkCallAPI(url, "POST");

                    List<Wink.Shortcut.ShortcutMember> members = shortcut.members;
                    foreach (Wink.Shortcut.ShortcutMember member in members)
                    {
                        List<Wink.Device> devices = new List<Wink.Device>();

                        if (member.type == "group")
                        {
                            Wink.Group group = new WinkHelper.GroupHelper().getGroupByID(member.id);
                            if (group != null)
                            {
                                foreach (KeyValuePair<string, string> entry in member.actions)
                                {
                                    Wink.Group.GroupStatus status = group.status.FirstOrDefault(n => n.name == entry.Key);
                                    if (status != null)
                                        status.current_status = entry.Value;
                                }

                                foreach (Wink.Group.GroupMember groupmember in group.members)
                                {
                                    Wink.Device device = new WinkHelper.DeviceHelper().getDeviceByID(groupmember.id);
                                    if (device != null)
                                        devices.Add(device);
                                }
                            }
                        }
                        else
                        {
                            Wink.Device device = new WinkHelper.DeviceHelper().getDeviceByID(member.id);
                            if (device != null)
                                devices.Add(device);
                        }

                        foreach (Wink.Device device in devices)
                        {
                            foreach (KeyValuePair<string, string> entry in member.actions)
                            {
                                Wink.Device.DeviceStatus status = device.status.FirstOrDefault(p => p.name == entry.Key);
                                if (status != null)
                                    status.current_status = entry.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ShortcutUpdate(JObject json)
        {
            winkGetShortcuts(json);
        }
        public List<Wink.Shortcut> winkGetShortcuts(JObject jsonObject = null, bool forceRefresh = false)
        {
            try
            {
                if (myWink.Shortcuts == null || myWink.Shortcuts.Count == 0 || jsonObject != null || forceRefresh)
                {
                    List<Wink.Shortcut> shortcuts = new List<Wink.Shortcut>();
                    JObject json = null;

                    if (myWink.Shortcuts == null || forceRefresh)
                    {
                        json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetShortcutsURL"]);
                    }
                    else if (jsonObject != null)
                    {
                        json = jsonObject;
                        string jsonstring = json.ToString();
                        string strShortcutID = jsonstring.Substring(jsonstring.IndexOf("scene_id\": \"") + 12);
                        strShortcutID = strShortcutID.Remove(strShortcutID.IndexOf("\""));
                        shortcuts = myWink.Shortcuts.Where(d => d.id == strShortcutID).ToList();
                    }

                    if (json != null)
                    {
                        foreach (JObject data in json["data"])
                        {
                            IList<string> keys = data.Properties().Select(p => p.Name).ToList();

                            Wink.Shortcut shortcut = new Wink.Shortcut();
                            shortcut.id = data["scene_id"].ToString();
                            
                            if (jsonObject != null)
                            {
                                shortcut = new WinkHelper.ShortcutHelper().getShortcutByID(shortcut.id);
                                myWink.Shortcuts.Remove(shortcut);
                            }
                                
                            shortcut.name = data["name"].ToString();
                            shortcut.displayName = shortcut.name;
                            shortcut.json = data.ToString();

                            if (keys.Contains("members"))
                            {
                                var members = data["members"];
                                foreach (var member in members)
                                {
                                    Wink.Shortcut.ShortcutMember newmember = new Wink.Shortcut.ShortcutMember();
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

                            shortcuts.Add(shortcut);

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
                            
                            if (jsonObject != null)
                            {
                                myWink.Shortcuts.Add(shortcut);
                            }
                        }
                    }

                    if (jsonObject == null)
                        myWink.Shortcuts = shortcuts.OrderBy(c => c.name).ToList();
                }

                #region RETRIEVE DATABASE VALUES
                foreach (Wink.Shortcut shortcut in myWink.Shortcuts)
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
                            }
                        }
    
                    }
                }
                #endregion

                return myWink.Shortcuts;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
#endregion

#region Group
    public class GroupHelper : WinkHelper
    {
        public Wink.Group getGroupByID(string GroupID)
        {
            Wink.Group group = myWink.Groups.FirstOrDefault(s => s.id.Equals(GroupID));
            return group;
        }
        public Wink.Group getGroupByName(string GroupName)
        {
            Wink.Group group = myWink.Groups.FirstOrDefault(s => s.name.ToLower().Equals(GroupName.ToLower()));
            return group;
        }

        public string setGroupDisplayName(string GroupID, string DisplayName)
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
                Wink.Group group = myWink.Groups.FirstOrDefault(d => d.id == GroupID);
                group.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setGroupDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public void setGroupPosition(string GroupID, int Position)
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
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setGroupPosition", ex.Message, EventLogEntryType.Error);
            }
        }
        public void GroupSendCommand(string groupID, string command)
        {
            try
            {
                Wink.Group group = new WinkHelper.GroupHelper().getGroupByID(groupID);
                if (group != null)
                {
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + "groups/" + groupID + "/activate/";
                    winkCallAPI(url, "POST", command);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void GroupUpdate(JObject json)
        {
            winkGetGroups(json);
        }
        public void GroupGetSubscriptions(Wink wink = null)
        {
            try
            {
                List<string> groups = new List<string>();
                string userID = null;
                string token = null;

                if (wink != null)
                {
                    groups = wink.Groups.Select(d => d.id).ToList();
                    userID = wink.winkUser.userID;
                    token = wink.Token;
                }
                else if (myWink != null)
                {
                    groups = myWink.Groups.Select(d => d.id).ToList();
                    userID = myWink.winkUser.userID;
                    token = myWink.Token;
                }

                foreach (string group in groups)
                {
                    string APIURL = ConfigurationManager.AppSettings["winkRootURL"] + "groups/" + group + "/subscriptions";
                    string callbackURL = ConfigurationManager.AppSettings["SubscriptionCallbackURL"] + userID + "/group/" + group;
                    string sendCommand = "{\"callback\":\"" + callbackURL + "\",\"secret\":\"" + "MDkyZmZmZWMtNDM1Yi00MjI4LThhM2UtZjI4OGFjNWExNjU3" + "\"}";
                    JObject subJSON = winkCallAPI(APIURL, "POST", sendCommand, true, token);
                }

                if (userID != null)
                {
                    WinkEventHelper.storeNewSubscriptionMessage(userID, "Subscription Event", "", "Group Subscriptions Have Been Refreshed");
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        public void winkGetGroups(JObject jsonObject = null, bool forceRefresh = false)
        {
            try
            {
                if (myWink.Groups == null || myWink.Groups.Count == 0 || jsonObject != null || forceRefresh)
                {
                    List<Wink.Group> groups = new List<Wink.Group>();
                    JObject json = null;

                    if (myWink.Shortcuts == null || forceRefresh)
                    {
                        json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetGroupsURL"]);
                    }
                    else if (jsonObject != null)
                    {
                        json = jsonObject;
                        string jsonstring = json.ToString();
                        string strGroupID = jsonstring.Substring(jsonstring.IndexOf("group_id\": \"") + 12);
                        strGroupID = strGroupID.Remove(strGroupID.IndexOf("\""));
                        groups = myWink.Groups.Where(d => d.id == strGroupID).ToList();
                    }

                    if (json != null)
                    {
                        foreach (JObject data in json["data"])
                        {
                            IList<string> keys = data.Properties().Select(p => p.Name).ToList();

                            Wink.Group group = new Wink.Group();
                            group.id = data["group_id"].ToString();

                            if (jsonObject != null)
                            {
                                if (keys.Contains("last_reading"))
                                {
                                    var readings = data["last_reading"];
                                    string brightness = readings["brightness"] != null ? readings["brightness"].ToString() : null;
                                    string desiredbrightness = readings["desired_brightness"] != null ? readings["desired_brightness"].ToString() : null;
                                    string power = readings["powered"] != null ? readings["powered"].ToString() : null;
                                    string desiredpower = readings["desired_powered"] != null ? readings["desired_powered"].ToString() : null;

                                    if ((!string.IsNullOrWhiteSpace(desiredbrightness) && (desiredbrightness != brightness)) || (!string.IsNullOrWhiteSpace(desiredpower) && (desiredpower != power)))
                                        return;
                                }

                                group = new WinkHelper.GroupHelper().getGroupByID(group.id);
                                myWink.Groups.Remove(group);
                            }

                            group.name = data["name"].ToString();
                            group.displayName = group.name;
                            group.json = data.ToString();

                            if (keys.Contains("members"))
                            {
                                var members = data["members"];
                                foreach (var member in members)
                                {
                                    Wink.Group.GroupMember newmember = new Wink.Group.GroupMember();
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
                                    Wink.Group.GroupStatus newreading = new Wink.Group.GroupStatus();
                                    newreading.id = group.id;
                                    newreading.name = reading.Name;
                                    newreading.last_updated = Common.FromUnixTime(reading.Value["updated_at"].ToString(), true);

                                    if (reading.Value["true_count"] != null)
                                        newreading.current_status = reading.Value["true_count"].ToString();
                                    else if (reading.Value["average"] != null)
                                        newreading.current_status = reading.Value["average"].ToString();

                                    group.status.Add(newreading);
                                }
                            }

                            if (group.members.Count == 0)
                                group.isempty = true;

                            groups.Add(group);

                            #region UPDATE DATABASE
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
                            #endregion

                            if (jsonObject != null)
                            {
                                myWink.Groups.Add(group);
                            }
                        }
                    }
                    
                    if (jsonObject == null)
                        myWink.Groups = groups.OrderBy(c => c.name).ToList();
                }
                #region RETRIEVE DATABASE VALUES
                foreach (Wink.Group group in myWink.Groups)
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
                            }
                        }
    
                    }
                }
                #endregion

                //if (lastGroupSubscribed < DateTime.Now.AddMinutes(-60))
                //{
                //    ThreadPool.QueueUserWorkItem(o => GroupGetSubscriptions(myWink));
                //    lastGroupSubscribed = DateTime.Now;
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
#endregion

#region Robot
    public class RobotHelper : WinkHelper
    {
        public Wink.Robot getRobotByID(string RobotID)
        {
            Wink.Robot robot = myWink.Robots.FirstOrDefault(s => s.id.Equals(RobotID));
            return robot;
        }
        public Wink.Robot getRobotByName(string robotName)
        {
            Wink.Robot robot = myWink.Robots.FirstOrDefault(s => s.name.ToLower().Equals(robotName.ToLower()));
            return robot;
        }
        public string setRobotDisplayName(string RobotID, string DisplayName)
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
                Wink.Robot robot = myWink.Robots.FirstOrDefault(d => d.id == RobotID);
                robot.displayName = DisplayName;

                return DisplayName;
            }
            catch (Exception)
            {
                return null;
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setRobotDisplayName", ex.Message, EventLogEntryType.Error);
            }
        }
        public void setRobotPosition(string RobotID, int Position)
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
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Wink.setRobotPosition", ex.Message, EventLogEntryType.Error);
            }
        }
        public void RobotChangeState(string robotID, bool newEnabledState)
        {
            try
            {
                Wink.Robot robot = new WinkHelper.RobotHelper().getRobotByID(robotID);

                if (robot != null)
                {
                    string newstate = newEnabledState.ToString().ToLower();
                    string url = ConfigurationManager.AppSettings["winkRootURL"] + "robots/" + robot.id;
                    string sendcommand = "{\"enabled\":" + newstate + "}";

                    winkCallAPI(url, "PUT", sendcommand);

                    robot.enabled = newstate;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public JObject RobotGetJSON()
        {
            JObject json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetRobotsURL"]);
            if (json != null)
                return json;

            return null;
        }
        public void RobotUpdate(JObject json)
        {
            winkGetRobots(json);
        }
        public List<Wink.Robot> winkGetRobots(JObject jsonObject = null, bool forceRefresh = false)
        {
            try
            {
                if (myWink.Robots == null || myWink.Robots.Count == 0 || jsonObject != null || forceRefresh)
                {
                    List<Wink.Robot> robots = new List<Wink.Robot>();
                    JObject json = null;

                    if (myWink.Robots == null || forceRefresh)
                    {
                        json = winkCallAPI(ConfigurationManager.AppSettings["winkRootURL"] + ConfigurationManager.AppSettings["winkGetRobotsURL"]);
                    }
                    else if (jsonObject != null)
                    {
                        json = jsonObject;
                        string jsonstring = json.ToString();
                        string strRobotID = jsonstring.Substring(jsonstring.IndexOf("robot_id\": \"") + 12);
                        strRobotID = strRobotID.Remove(strRobotID.IndexOf("\""));
                        robots = myWink.Robots.Where(d => d.id == strRobotID).ToList();
                    }

                    if (json != null)
                    {
                        foreach (JObject data in json["data"])
                        {
                            IList<string> keys = data.Properties().Select(p => p.Name).ToList();

                            Wink.Robot robot = new Wink.Robot();
                            bool hasData = false;

                            robot.id = data["robot_id"].ToString();

                            if (jsonObject != null)
                            {
                                robot = new WinkHelper.RobotHelper().getRobotByID(robot.id);
                                myWink.Robots.Remove(robot);
                            }

                            robot.name = data["name"].ToString();
                            robot.displayName = robot.name;
                            robot.enabled = data["enabled"].ToString();
                            robot.json = data.ToString();
                            robot.isschedule = (data["automation_mode"].ToString() == "schedule");

                            DateTime cutoff = Convert.ToDateTime("1/1/2012");
                            DateTime lastfired = Common.FromUnixTime(data["last_fired"].ToString(), true); ;
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
                                            DateTime nextrun = Common.FromUnixTime(data["causes"][0]["next_at"].ToString(), true);
                                            robot.next_run = (nextrun < cutoff ? "Not Scheduled" : nextrun.ToString());
                                        }
                                    }

                                    //RENAME "MY ROBOT" WHEN IS SENSOR BATTERY WARNING
                                    if (robot.name.ToLower() == "my robot")
                                    {
                                        Wink.Device device = myWink.Devices.FirstOrDefault(d => d.id == data["causes"][0]["observed_object_id"].ToString());

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

                            robots.Add(robot);

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

                            if (jsonObject != null)
                            {
                                myWink.Robots.Add(robot);
                            }
                        }
                    }
                    if (jsonObject == null)
                        myWink.Robots = robots.OrderBy(c => c.name).ToList();
                }

                #region RETRIEVE DATABASE VALUES
                foreach (Wink.Robot robot in myWink.Robots)
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
                            }
                        }
    
                    }
                }
                #endregion

                return myWink.Robots;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
#endregion
}


public class Wink
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SimplePropertyAttribute : Attribute
    {
    }

#region token
    public string Token
    {
        get { return _token; }
        set { _token = value; }
    }
    private string _token;
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
        get { return _user; }
        set { _user = value; }
    }
    public User _user;

#endregion

#region Device
    public class Device : Wink
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
        public string model_name { get; set; }
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
        public int position = 1001;
        public Dictionary<string, string> desired_states = new Dictionary<string, string>();
        public List<DeviceStatus> sensor_states = new List<DeviceStatus>();
        public List<DeviceStatus> status = new List<DeviceStatus>();
        public class DeviceStatus
        {
            public string id;
            public string name;
            public string current_status;
            public DateTime? last_updated;
            public DateTime? last_changed;

        }
    }
    public List<Device> Devices
    {
        get
        {
            return _devices;
        }
        set
        {
            _devices = value;
        }
    }
    private List<Device> _devices;
    public bool DeviceHasChanges;    
#endregion

#region Shortcut
    public class Shortcut : Wink
    {
        [SimpleProperty]
        public string id { get; set; }
        [SimpleProperty]
        public string name { get; set; }
        [SimpleProperty]
        public string displayName { get; set; }

        public string json;
        public int position = 1001;

        public List<ShortcutMember> members = new List<ShortcutMember>();
        public class ShortcutMember
        {
            public string id;
            public string type;
            public Dictionary<string, string> actions = new Dictionary<string, string>();
        }
    }

    public List<Shortcut> Shortcuts
    {
        get
        {
            return _shortcuts;
        }
        set
        {
            _shortcuts = value;
        }
    }
    private List<Shortcut> _shortcuts;
    public bool ShortcutHasChanges;
#endregion
    
#region Group 
    public class Group : Wink
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
        }
    }
 
    public List<Group> Groups
    {
        get
        {
            return _groups;
        }
        set
        {
            _groups = value;
        }
    }
    private List<Group> _groups;
    public bool GroupHasChanges;
#endregion

#region Robot
    public class Robot : Wink
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
        public int position = 1001;

        public List<string> members = new List<string>();
    }

    public List<Robot> Robots
    {
        get
        {
            return _robots;
        }
        set
        {
            _robots = value;
        }
    }
    private List<Robot> _robots;
    public bool RobotHasChanges;
#endregion

}
