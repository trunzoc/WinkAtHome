using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;


namespace WinkAtHome.Controls
{
    public partial class Devices : System.Web.UI.UserControl
    {
        public bool ControllableOnly = false;
        public string typeToShow = "all";
        public bool SensorsOnly = false;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["devicetype"] != null)
                {
                    typeToShow = Request.QueryString["devicetype"].ToLower();
                    hfDeviceType.Value = typeToShow;
                }

                if (ControllableOnly)
                {
                    lblHeader.Text = "Devices: Controllable Only";
                }
                else if (SensorsOnly)
                {
                    lblHeader.Text = "Sensors";
                }
                else if (typeToShow != null)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    lblHeader.Text = "Devices: " + textInfo.ToTitleCase(typeToShow.Replace("_", " "));
                }
                else
                {
                    lblHeader.Text = "All Devices";
                }

                string columns = SettingMgmt.getSetting(Request.RawUrl.Replace("/", "") + "-Devices-" + "-" + ControllableOnly.ToString() + "-" + SensorsOnly.ToString() + "-" + typeToShow + "-Columns");
                if (columns != null)
                    tbColumns.Text = columns;

                string dataVisible = SettingMgmt.getSetting(Request.RawUrl.Replace("/", "") + "-Devices-" + "-" + ControllableOnly.ToString() + "-" + SensorsOnly.ToString() + "-" + typeToShow + "-Visible");
                if (dataVisible != null)
                {
                    bool visible = true;
                    bool.TryParse(dataVisible, out visible);
                    rowData.Visible = visible;
                }

                BindData();
            }
        }

        public void BindData()
        {
            if (!string.IsNullOrWhiteSpace(hfDeviceType.Value))
                typeToShow = hfDeviceType.Value;

            dlDevices.DataSource = null;
            dlDevices.DataBind();

            List<Wink.Device> devices = new List<Wink.Device>();
            if (ControllableOnly)
            {
                devices = Wink.Devices.Where(p => p.iscontrollable == true).ToList();
                UserControl ucSensors = (UserControl)Page.Master.FindControl("cphMain").FindControl("ucSensors");
                if (ucSensors != null)
                {
                    DataList dl = (DataList)ucSensors.FindControl("dlDevices");
                    if (dl != null)
                    {
                        List<Wink.Device> sensordevices = Wink.Devices.Where(p => p.issensor == true).ToList();
                        dl.DataSource = sensordevices;
                        dl.DataBind();
                    }
                }
            }
            else if (SensorsOnly)
            {
                devices = Wink.Devices.Where(p => p.issensor == true).ToList();
            }
            else if (typeToShow != "all")
            {
                devices = Wink.Devices.Where(p => p.type==typeToShow).ToList();
            }
            else
            {
                devices = Wink.Devices;
            }

            dlDevices.DataSource = devices;
            dlDevices.DataBind();
        }

        protected void dlDevices_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Wink.Device device = ((Wink.Device)e.Item.DataItem);
                string devicetype = device.type;

                HiddenField hfDeviceID = (HiddenField)e.Item.FindControl("hfDeviceID");
                hfDeviceID.Value = device.id;

                List<Wink.DeviceStatus> status = device.status;
                IList<string> keys = status.Select(p => p.name).ToList();

                //BIND INFO BUTTON
                var props = typeof(Wink.Device).GetProperties();
                var properties = new List<KeyValuePair<string, string>>();
                foreach (var prop in props)
                {
                    if (prop.Name != "json")
                    {
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                        string propname = textInfo.ToTitleCase(prop.Name.Replace("_", " "));
                        var propvalue = prop.GetValue(device, null);
                        if (propvalue != null)
                            properties.Add(new KeyValuePair<string, string>(propname, propvalue.ToString()));
                    }
                }
                DataList dlProperties = (DataList)e.Item.FindControl("dlProperties");
                if (dlProperties != null)
                {
                    dlProperties.DataSource = properties;
                    dlProperties.DataBind();
                }

                //SET BATTERY ICON
                if (keys.Contains("battery"))
                {
                    Image imgBattery = (Image)e.Item.FindControl("imgBattery");
                    if (imgBattery != null)
                    {
                        imgBattery.Visible = true;

                        Wink.DeviceStatus stat = status.Single(p => p.name == "battery");
                        double batLevel = 0;
                        Double.TryParse(stat.current_status, out batLevel);

                        batLevel = batLevel * 100;

                        string imgLevel = "0";
                        if (batLevel <= 10)
                            imgLevel = "0";
                        else if (batLevel <= 30)
                            imgLevel = "25";
                        else if (batLevel <= 60)
                            imgLevel = "50";
                        else if (batLevel <= 90)
                            imgLevel = "75";
                        else
                            imgLevel = "100";

                        imgBattery.ToolTip = "Battery Level: " + batLevel + "%\r\nLast Updated: " + stat.last_updated;


                        string imgPath = Request.PhysicalApplicationPath + "\\Images\\Battery\\Battery" + imgLevel + ".png";
                        if (File.Exists(imgPath))
                        {
                            string url = "~/Images/Battery/Battery" + imgLevel + ".png";
                            imgBattery.ImageUrl = url;
                        }
                    }
                }

                //SETUP DEVICES DISPLAY
                if (SensorsOnly) // Bind Sensors for SensorOnly view
                {
                    displaySensors(e.Item);
                }
                else if (devicetype == "thermostats") //Bind Thermostats
                {
                    displayThermostats(e.Item);
                }
                else //Bind Other Devices
                {
                    displayDevices(e.Item);
                }
            }
        }

        protected void displayDevices(DataListItem item)
        {
            ImageButton img = (ImageButton)item.FindControl("imgIcon");
            RadSlider rs = (RadSlider)item.FindControl("rsBrightness");
            HiddenField hfMainCommand = (HiddenField)item.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)item.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)item.FindControl("hfLevelCommand");
            Table tblDefault = (Table)item.FindControl("tblDefault");
            tblDefault.Visible = true;

            Wink.Device device = ((Wink.Device)item.DataItem);
            string devicetype = device.type;

            List<Wink.DeviceStatus> status = device.status;
            IList<string> keys = status.Select(p => p.name).ToList();
            string state = string.Empty;
            string degree = "n/a";

            if (keys.Contains("powered") || keys.Contains("locked"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "powered" || p.name == "locked");
                state = stat.current_status.ToLower();
                hfMainCommand.Value = stat.name;
                hfCurrentStatus.Value = state;
                img.Enabled = true;
            }
            else if (keys.Contains("brightness") || keys.Contains("position") || keys.Contains("remaining"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position" || p.name == "remaining");
                Double converted = Convert.ToDouble(stat.current_status) * 100;
                state = converted > 0 ? "true" : "false";
                hfMainCommand.Value = stat.name;
                hfCurrentStatus.Value = state;

                if (device.iscontrollable)
                    img.Enabled = true;
            }
            else if (keys.Contains("connection"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "connection");
                state = stat.current_status.ToLower();
                hfMainCommand.Value = stat.name;
                hfCurrentStatus.Value = state;
            }

            if (keys.Contains("brightness") || keys.Contains("position") || keys.Contains("remaining"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position" || p.name == "remaining");
                hfLevelCommand.Value = stat.name;
                degree = (Convert.ToDouble(stat.current_status) * 100).ToString();
            }

            if (devicetype == "light_bulbs" || devicetype == "binary_switches")
            {
                img.ImageUrl = "~/Images/Devices/lights" + state + ".png";
            }
            else if (hfLevelCommand.Value == "position" || hfLevelCommand.Value == "remaining")
            {
                string imgDegree = "100";
                double deg = Convert.ToDouble(degree);
                if (deg <= 10)
                    imgDegree = "0";
                else if (deg <= 30)
                    imgDegree = "25";
                else if (deg <= 60)
                    imgDegree = "50";
                else if (deg <= 90)
                    imgDegree = "75";
                else
                    imgDegree = "100";

                string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\" + devicetype + imgDegree + ".png";
                if (File.Exists(imgPath))
                {
                    string url = "~/Images/Devices/" + devicetype + imgDegree + ".png";
                    img.ImageUrl = url;
                }
            }
            else
            {
                string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\" + devicetype + state + ".png";
                if (File.Exists(imgPath))
                {
                    string url = "~/Images/Devices/" + devicetype + state + ".png";
                    img.ImageUrl = url;
                }

            }

            if (device.isvariable)
            {
                rs.Visible = true;
                if (state == "true")
                {
                    rs.Value = Convert.ToInt32(degree);
                    rs.ToolTip = degree + "%";
                }
            }
            else
            {
                rs.Visible = false;
            }
        }

        protected void displaySensors(DataListItem item)
        {
            ImageButton img = (ImageButton)item.FindControl("imgIcon");
            Table tblDefault = (Table)item.FindControl("tblDefault");
            tblDefault.Visible = true;

            Wink.Device device = ((Wink.Device)item.DataItem);
            string devicetype = device.type;

            List<Wink.DeviceStatus> status = device.status;
            IList<string> keys = status.Select(p => p.name).ToList();
            string state = string.Empty;
            string degree = "n/a";

            if (keys.Contains("powered") || keys.Contains("locked"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "powered" || p.name == "locked");
                state = stat.current_status.ToLower();
            }
            else if (keys.Contains("brightness") || keys.Contains("position") || keys.Contains("remaining"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position" || p.name == "remaining");
                Double converted = Convert.ToDouble(stat.current_status) * 100;
                state = converted > 0 ? "true" : "false";
            }
            else if (keys.Contains("connection"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "connection");
                state = stat.current_status.ToLower();
            }

            if (keys.Contains("brightness") || keys.Contains("position") || keys.Contains("remaining"))
            {
                Wink.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position" || p.name == "remaining");
                degree = (Convert.ToDouble(stat.current_status) * 100).ToString();
            }

            if (devicetype == "light_bulbs" || devicetype == "binary_switches")
            {
                img.ImageUrl = "~/Images/Devices/lights" + state + ".png";
            }
            else if (degree != "n/a")
            {
                string imgDegree = "100";
                double deg = Convert.ToDouble(degree);
                if (deg <= 10)
                    imgDegree = "0";
                else if (deg <= 30)
                    imgDegree = "25";
                else if (deg <= 60)
                    imgDegree = "50";
                else if (deg <= 90)
                    imgDegree = "75";
                else
                    imgDegree = "100";

                string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\" + devicetype + imgDegree + ".png";
                if (File.Exists(imgPath))
                {
                    string url = "~/Images/Devices/" + devicetype + imgDegree + ".png";
                    img.ImageUrl = url;
                }
            }
            else
            {
                string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\" + devicetype + state + ".png";
                if (File.Exists(imgPath))
                {
                    string url = "~/Images/Devices/" + devicetype + state + ".png";
                    img.ImageUrl = url;
                }

            }
        }

        protected void displayThermostats(DataListItem item)
        {
            HiddenField hfOrigHighTemp = (HiddenField)item.FindControl("hfOrigHighTemp");
            HiddenField hfOrigLowTemp = (HiddenField)item.FindControl("hfOrigLowTemp");
            HiddenField hfOrigMode = (HiddenField)item.FindControl("hfOrigMode");
            HiddenField hfOrigPower = (HiddenField)item.FindControl("hfOrigPower");

            HiddenField hfSetHighTemp = (HiddenField)item.FindControl("hfSetHighTemp");
            HiddenField hfSetLowTemp = (HiddenField)item.FindControl("hfSetLowTemp");
            HiddenField hfSetMode = (HiddenField)item.FindControl("hfSetMode");
            HiddenField hfSetPower = (HiddenField)item.FindControl("hfSetPower");
            
            Table tblThermostat = (Table)item.FindControl("tblThermostat");
            tblThermostat.Visible = true;
            
            Wink.Device device = ((Wink.Device)item.DataItem);

            List<Wink.DeviceStatus> status = device.status;
            IList<string> keys = status.Select(p => p.name).ToList();

            if (keys.Contains("powered"))
            {
                string powered = status.Single(p => p.name == "powered").current_status.ToLower();
                ImageButton ibThermPower = (ImageButton)item.FindControl("ibThermPower");
                ibThermPower.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";

                ImageButton ibThermPowerSet = (ImageButton)item.FindControl("ibThermPowerSet");
                ibThermPowerSet.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";
                hfOrigPower.Value = powered;
            }

            if (keys.Contains("temperature") && !string.IsNullOrWhiteSpace(status.Single(p => p.name == "temperature").current_status))
            {
                Double temp = Common.FromCelsiusToFahrenheit(Convert.ToDouble(status.Single(p => p.name == "temperature").current_status));
                Label lblThermStats = (Label)item.FindControl("lblThermStats");
                lblThermStats.Text = temp.ToString() + "&deg;";

                Label lblThermStatsSet = (Label)item.FindControl("lblThermStatsSet");
                lblThermStatsSet.Text = temp.ToString() + "&deg;";
            }

            if (keys.Contains("mode"))
            {
                string mode = status.Single(p => p.name == "mode").current_status.ToLower().Replace("_only", "");
                Image imgThermostatModeAuto = (Image)item.FindControl("imgThermostatModeAuto");
                imgThermostatModeAuto.ImageUrl = "~/Images/Thermostats/" + mode + "true.png";

                Image imgThermostatModeHeatCool = (Image)item.FindControl("imgThermostatModeHeatCool");
                imgThermostatModeHeatCool.ImageUrl = "~/Images/Thermostats/" + mode + mode + ".png";

                ImageButton ibThermMode = (ImageButton)item.FindControl("ibTherm" + mode);
                ibThermMode.ImageUrl = "~/Images/Thermostats/" + mode + "true.png";

                hfOrigMode.Value = mode;

                string mintemp = string.Empty;
                string maxtemp = string.Empty;
                if (mode == "auto")
                {
                    Table tblMode = (Table)item.FindControl("tblThermauto");
                    tblMode.Visible = true;
                    Table tblModeSet = (Table)item.FindControl("tblThermautoSet");
                    tblModeSet.Visible = true;

                    if (keys.Contains("min_set_point"))
                    {
                        mintemp = status.Single(p => p.name == "min_set_point").current_status.ToLower();
                        Label lblTempCool = (Label)item.FindControl("lblTempCool" + mode);
                        lblTempCool.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";

                        Label lblTempCoolSet = (Label)item.FindControl("lblTempCoolSet" + mode);
                        lblTempCoolSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                    }

                    if (keys.Contains("max_set_point"))
                    {
                        maxtemp = status.Single(p => p.name == "max_set_point").current_status.ToLower();
                        Label lblTempHeat = (Label)item.FindControl("lblTempHeat" + mode);
                        lblTempHeat.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";

                        Label lblTempHeatSet = (Label)item.FindControl("lblTempHeatSet" + mode);
                        lblTempHeatSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                    }
                }
                else
                {
                    Table tblMode = (Table)item.FindControl("tblCoolHeat");
                    tblMode.Visible = true;
                    Table tblModeSet = (Table)item.FindControl("tblCoolHeatSet");
                    tblModeSet.Visible = true;

                    ImageButton ibThermUp = (ImageButton)item.FindControl("ibThermUp");
                    ibThermUp.ImageUrl = "~/Images/Thermostats/" + mode + "up.png";

                    ImageButton ibThermDown = (ImageButton)item.FindControl("ibThermDown");
                    ibThermDown.ImageUrl = "~/Images/Thermostats/" + mode + "down.png";

                    Image imgCool = (Image)item.FindControl("imgCool");
                    imgCool.ImageUrl = "~/Images/Thermostats/cool" + mode + ".png";

                    Image imgHeat = (Image)item.FindControl("imgHeat");
                    imgHeat.ImageUrl = "~/Images/Thermostats/heat" + mode + ".png";

                    if (keys.Contains("max_set_point"))
                    {
                        maxtemp = status.Single(p => p.name == "max_set_point").current_status.ToLower();
                        if (mode == "cool")
                        {
                            Label lblTemp = (Label)item.FindControl("lblTemp");
                            lblTemp.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                            lblTemp.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

                            Label lblTempSet = (Label)item.FindControl("lblTempSet");
                            lblTempSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                            lblTempSet.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                        }
                    }

                    if (keys.Contains("min_set_point"))
                    {
                        mintemp = status.Single(p => p.name == "min_set_point").current_status.ToLower();
                        if (mode == "heat")
                        {
                            Label lblTemp = (Label)item.FindControl("lblTemp");
                            lblTemp.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                            lblTemp.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

                            Label lblTempSet = (Label)item.FindControl("lblTempSet");
                            lblTempSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                            lblTempSet.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                        }
                    }
                }
                hfOrigLowTemp.Value = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString();
                hfOrigHighTemp.Value = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString();

                hfSetHighTemp.Value = hfOrigHighTemp.Value;
                hfSetLowTemp.Value = hfOrigLowTemp.Value;
                hfSetMode.Value = hfOrigMode.Value;
                hfSetPower.Value = hfOrigPower.Value;
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.NamingContainer;
            string deviceID = ib.CommandArgument;
            string command = string.Empty;

            HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");

            string newstate = (!Convert.ToBoolean(hfCurrentStatus.Value)).ToString().ToLower();
            string newlevel = string.Empty;

            if (hfLevelCommand.Value == "brightness" && newstate == "true")
            {
                newlevel = ",\"brightness\":1";
            }
            else if (hfLevelCommand.Value == "position")
            {
                if (newstate == "true")
                    newstate = "1";
                else
                    newstate = "0";
            }

            command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + newlevel + "}}";
            Wink.sendDeviceCommand(deviceID, command);

            Wink.Device device = Wink.Device.getDeviceByID(deviceID);
            Wink.DeviceStatus status = device.status.Single(p => p.name == hfMainCommand.Value);
            status.current_status = newstate;

            if (!string.IsNullOrWhiteSpace(newlevel))
            {
                Wink.DeviceStatus statuslvl = device.status.Single(p => p.name == hfLevelCommand.Value);
                statuslvl.current_status = "1";
            }

            BindData();
        }

        protected void rsBrightness_ValueChanged(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                RadSlider rs = (RadSlider)sender;
                DataListItem li = (DataListItem)rs.NamingContainer;
                ImageButton ib = (ImageButton)li.FindControl("imgIcon"); ;
                string deviceID = ib.CommandArgument;
                string command = string.Empty;
                Decimal newlevel = rs.Value / 100;

                HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
                HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
                HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");


                string newstate = string.Empty;

                if (hfLevelCommand.Value == "brightness")
                {
                    newstate = (newlevel == 0 ? "false" : "true");
                }

                command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + ",\"" + hfLevelCommand.Value + "\":" + newlevel + "}}";
                Wink.sendDeviceCommand(deviceID, command);


                Wink.Device device = Wink.Device.getDeviceByID(deviceID);
                Wink.DeviceStatus status = device.status.Single(p => p.name == hfMainCommand.Value);
                status.current_status = newstate;

                Wink.DeviceStatus statuslvl = device.status.Single(p => p.name == hfLevelCommand.Value);
                statuslvl.current_status = newlevel.ToString();

                BindData();
            }
        }


        protected void ibThermPower_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            HiddenField hfSetPower = (HiddenField)ib.NamingContainer.FindControl("hfSetPower");
            Label lblNotes = (Label)ib.NamingContainer.FindControl("lblNotes");
            bool power = Convert.ToBoolean(hfSetPower.Value);
            string newpower = (!power).ToString().ToLower();

            ib.ImageUrl = "~/Images/Thermostats/power" + newpower + ".png";
            hfSetPower.Value = newpower;

            if (power)
            {
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoCoolUp")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoHeatUp")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoCoolDown")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoHeatDown")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermUp")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermDown")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermcool")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermauto")).Enabled = false;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermheat")).Enabled = false;
                lblNotes.Text = "Turn power on to edit additional settings";
            }
            else
            {
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoCoolUp")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoHeatUp")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoCoolDown")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermAutoHeatDown")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermUp")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermDown")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermcool")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermauto")).Enabled = true;
                ((ImageButton)ib.NamingContainer.FindControl("ibThermheat")).Enabled = true;
                lblNotes.Text = "";
            }

            string command = "{\"desired_state\": {\"mode\":null,\"powered\":" + newpower + ",\"modes_allowed\":null,\"min_set_point\":null,\"max_set_point\":null}}";
            Wink.sendDeviceCommand(((HiddenField)ib.NamingContainer.FindControl("hfDeviceID")).Value, command);
        }

        protected void ibThermModeChange_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            HiddenField hfSetMode = (HiddenField)ib.NamingContainer.FindControl("hfSetMode");
            HiddenField hfSetLowTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetLowTemp");
            HiddenField hfSetHighTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetHighTemp");

            string mode = ib.CommandArgument.ToLower();
            hfSetMode.Value = mode;

            Table tblModeAutoSet = (Table)ib.NamingContainer.FindControl("tblThermautoSet");
            Table tblModeHCSet = (Table)ib.NamingContainer.FindControl("tblCoolHeatSet");

            ImageButton ibThermcool = (ImageButton)ib.NamingContainer.FindControl("ibThermcool");
            ibThermcool.ImageUrl = "~/Images/Thermostats/coolfalse.png";
            ImageButton ibThermauto = (ImageButton)ib.NamingContainer.FindControl("ibThermauto");
            ibThermauto.ImageUrl = "~/Images/Thermostats/autofalse.png";
            ImageButton ibThermheat = (ImageButton)ib.NamingContainer.FindControl("ibThermheat");
            ibThermheat.ImageUrl = "~/Images/Thermostats/heatfalse.png";
            ib.ImageUrl = "~/Images/Thermostats/" + mode + "true.png";

            if (mode == "auto")
            {
                tblModeAutoSet.Visible = true;
                tblModeHCSet.Visible = false;


                Label lblTempCoolSet = (Label)ib.NamingContainer.FindControl("lblTempCoolSetauto");
                lblTempCoolSet.Text = hfSetLowTemp.Value + "&deg;";

                Label lblTempHeatSet = (Label)ib.NamingContainer.FindControl("lblTempHeatSetauto");
                lblTempHeatSet.Text = hfSetHighTemp.Value + "&deg;";
            }
            else
            {
                tblModeAutoSet.Visible = false;
                tblModeHCSet.Visible = true;

                ImageButton ibThermUp = (ImageButton)ib.NamingContainer.FindControl("ibThermUp");
                ibThermUp.ImageUrl = "~/Images/Thermostats/" + mode + "up.png";

                ImageButton ibThermDown = (ImageButton)ib.NamingContainer.FindControl("ibThermDown");
                ibThermDown.ImageUrl = "~/Images/Thermostats/" + mode + "down.png";

                Image imgCool = (Image)ib.NamingContainer.FindControl("imgCool");
                imgCool.ImageUrl = "~/Images/Thermostats/cool" + mode + ".png";

                Image imgHeat = (Image)ib.NamingContainer.FindControl("imgHeat");
                imgHeat.ImageUrl = "~/Images/Thermostats/heat" + mode + ".png";


                string maxtemp = hfSetHighTemp.Value;
                string mintemp = hfSetLowTemp.Value;
                Label lblTempSet = (Label)ib.NamingContainer.FindControl("lblTempSet");
                lblTempSet.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                
                if (mode == "cool")
                    lblTempSet.Text = maxtemp + "&deg;";
                else if (mode == "heat")
                {
                    lblTempSet.Text = mintemp + "&deg;";
                }
            }

            string sendmode = mode == "auto" ? mode : mode + "_only";
            string command = "{\"desired_state\": {\"mode\":\""+ sendmode + "\",\"powered\":true,\"modes_allowed\":null,\"min_set_point\":null,\"max_set_point\":null}}";
            Wink.sendDeviceCommand(((HiddenField)ib.NamingContainer.FindControl("hfDeviceID")).Value, command);

        }

        protected void ibThermChange_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            Label lblTempSet = null;
            HiddenField hfSetNewTemp = null;

            if (ib.CommandArgument.Contains("heat"))
            {
                lblTempSet = (Label)ib.NamingContainer.FindControl("lblTempHeatSetauto");
                hfSetNewTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetLowTemp");
            }
            else if (ib.CommandArgument.Contains("cool"))
            {
                lblTempSet = (Label)ib.NamingContainer.FindControl("lblTempCoolSetauto");
                hfSetNewTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetHighTemp");
            }
            else
            {
                lblTempSet = (Label)ib.NamingContainer.FindControl("lblTempSet");
                hfSetNewTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetTemp");
            }
           
            if (lblTempSet != null)
            {
                Int16 temp = Convert.ToInt16(lblTempSet.Text.Replace("&deg;", ""));

                if (ib.CommandArgument.Contains("up"))
                    temp++;
                if (ib.CommandArgument.Contains("down"))
                    temp--;

                hfSetNewTemp.Value = temp.ToString();
                lblTempSet.Text = temp.ToString() + "&deg;";
            }

            string command = "{\"desired_state\": {\"mode\":null,\"powered\":true,\"modes_allowed\":null,\"min_set_point\":null,\"max_set_point\":null}}";
            Wink.sendDeviceCommand(((HiddenField)ib.NamingContainer.FindControl("hfDeviceID")).Value, command);
        }

        protected void lbApplyThermostat_Click(object sender, EventArgs e)
        {
            LinkButton ib = (LinkButton)sender;
            DataListItem dli = (DataListItem)ib.NamingContainer;

            HiddenField hfOrigHighTemp = (HiddenField)dli.FindControl("hfOrigHighTemp");
            HiddenField hfOrigLowTemp = (HiddenField)dli.FindControl("hfOrigLowTemp");
            HiddenField hfOrigMode = (HiddenField)dli.FindControl("hfOrigMode");
            HiddenField hfOrigPower = (HiddenField)dli.FindControl("hfOrigPower");

            HiddenField hfSetHighTemp = (HiddenField)dli.FindControl("hfSetHighTemp");
            HiddenField hfSetLowTemp = (HiddenField)dli.FindControl("hfSetLowTemp");
            HiddenField hfSetMode = (HiddenField)dli.FindControl("hfSetMode");
            HiddenField hfSetPower = (HiddenField)dli.FindControl("hfSetPower");

            Label lblTempCoolSetauto = (Label)dli.FindControl("lblTempCoolSetauto");
            Label lblTempHeatSetauto = (Label)dli.FindControl("lblTempHeatSetauto");
            Label lblTempSet = (Label)dli.FindControl("lblTempSet");
                
            string deviceID = ib.CommandArgument;
            string command = string.Empty;
            
            string thermmode = "null";
            string thermpower = "null";
            string highTemp = "null";
            string lowTemp = "null";
            string singleTemp = "null";

            Double basetemp = 0;
            Double thermmin = 0;
            Double thermmax = 0;

            thermpower = hfSetPower.Value.ToLower();

            if (thermpower == "false")
            {
                command = "{\"desired_state\": {\"mode\":null,\"powered\":" + thermpower + ",\"modes_allowed\":null,\"min_set_point\":null,\"max_set_point\":null}}";
                Wink.sendDeviceCommand(deviceID, command);
            }
            else
            {
                if (hfOrigMode.Value != hfSetMode.Value)
                {
                    thermmode = hfSetMode.Value.ToLower() == "auto" ? hfSetMode.Value.ToLower() : hfSetMode.Value.ToLower() + "_only";
                }
                command = "{\"desired_state\": {\"" + "\"mode\":\"" + thermmode + "\",\"powered\":true,\"modes_allowed\":null,\"min_set_point\":" + thermmin + ",\"max_set_point\":" + thermmax + "}}";
                //Wink.sendDeviceCommand(deviceID, command);
            }


            if (hfSetMode.Value == "auto")
            {
                highTemp = lblTempHeatSetauto.Text.Replace("&deg;", "");
                lowTemp = lblTempCoolSetauto.Text.Replace("&deg;", "");
            }
            else
            {
                singleTemp = lblTempSet.Text.Replace("&deg;", "");
            }


            if (hfSetMode.Value == "auto")
            {
                thermmin = Common.FromFahrenheitToCelsius(Convert.ToDouble(lowTemp));
                thermmax = Common.FromFahrenheitToCelsius(Convert.ToDouble(highTemp));
            }
            else if (hfSetMode.Value == "cool")
            {
                basetemp = Convert.ToDouble(singleTemp);
                thermmax = Common.FromFahrenheitToCelsius(basetemp);
                thermmin = Common.FromFahrenheitToCelsius(basetemp - 2);
            }
            else if (hfSetMode.Value == "heat")
            {
                basetemp = Convert.ToDouble(singleTemp);
                thermmin = Common.FromFahrenheitToCelsius(basetemp);
                thermmax = Common.FromFahrenheitToCelsius(basetemp + 2);
            }


            Wink.Device device = Wink.Device.getDeviceByID(deviceID);
            
            Wink.DeviceStatus modestatus = device.status.Single(p => p.name == "mode");
            if (modestatus != null)
                modestatus.current_status = thermmode;

            Wink.DeviceStatus poweredstatus = device.status.Single(p => p.name == "powered");
            if (poweredstatus != null)
                poweredstatus.current_status = thermpower;

            Wink.DeviceStatus maxstatus = device.status.Single(p => p.name == "max_set_point");
            if (maxstatus != null)
                maxstatus.current_status = thermmax.ToString();

            Wink.DeviceStatus minstatus = device.status.Single(p => p.name == "min_set_point");
            if (minstatus != null)
                minstatus.current_status = thermmin.ToString();

            TableCell cellApply = (TableCell)ib.NamingContainer.FindControl("cellApply");
            cellApply.BackColor = System.Drawing.Color.Green;

            ModalPopupExtender mpe = (ModalPopupExtender)ib.NamingContainer.FindControl("mdeThermostats");
            mpe.Hide();
            BindData();
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            SettingMgmt.saveSetting(Request.RawUrl.Replace("/", "") + "-Devices-" + "-" + ControllableOnly.ToString() + "-" + SensorsOnly.ToString() + "-" + typeToShow + "-Columns", tbColumns.Text);
            BindData();
        }

        protected void ibExpand_Click(object sender, ImageClickEventArgs e)
        {
            rowData.Visible = !rowData.Visible;
            SettingMgmt.saveSetting(Request.RawUrl.Replace("/", "") + "-Devices-" + "-" + ControllableOnly.ToString() + "-" + SensorsOnly.ToString() + "-" + typeToShow + "-Visible", rowData.Visible.ToString());
        }
    }
}