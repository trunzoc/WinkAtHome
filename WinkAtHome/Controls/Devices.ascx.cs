using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;


namespace WinkAtHome.Controls
{
    public partial class Devices : System.Web.UI.UserControl
    {
        private string dbPath = Common.dbPath;

        [PersistenceMode(PersistenceMode.Attribute)]
        public string typeToShow
        {
            get
            {
                object o = ViewState["typeToShow"];
                if (o != null)
                {
                    return (string)o;
                }
                return "all";
            }
            set
            {
                ViewState["typeToShow"] = value;
            }
        }
        public bool ControllableOnly
        {
            get
            {
                object o = ViewState["ControllableOnly"];
                if (o != null)
                {
                    return (bool)o;
                }
                return false;
            }
            set
            {
                ViewState["ControllableOnly"] = value;
            }
        }
        public bool SensorsOnly
        {
            get
            {
                object o = ViewState["SensorsOnly"];
                if (o != null)
                {
                    return (bool)o;
                }
                return false;
            }
            set
            {
                ViewState["SensorsOnly"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            hfSettingBase.Value = Request.RawUrl.Substring(Request.RawUrl.LastIndexOf('/') + 1) + "-Devices-MV" + ((Table)Page.Master.FindControl("tblExpand")).Visible.ToString() + "-CO" + ControllableOnly.ToString() + "-SO" + SensorsOnly.ToString() + "-Type" + typeToShow;
            if (!IsPostBack)
            {
                if (Request.QueryString["devicetype"] != null)
                {
                    string type = Request.QueryString["devicetype"].ToLower();
                    if (type == "controllable")
                        ControllableOnly = true;
                    else if (type == "sensors")
                        SensorsOnly = true;
                    else
                    {
                        typeToShow = type;
                        hfDeviceType.Value = typeToShow;
                    }
                }

                string columns = SettingMgmt.getSetting(hfSettingBase.Value + "-Columns");
                if (columns != null)
                    tbColumns.Text = columns;

                string dataVisible = SettingMgmt.getSetting(hfSettingBase.Value + "-Visible");
                if (dataVisible != null)
                {
                    bool visible = true;
                    bool.TryParse(dataVisible, out visible);
                    rowData.Visible = visible;
                    cbShow.Checked = visible;
                }

                BindData();
            }
            else
            {

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
                lblHeader.Text = "Devices: Controllable Only";
                devices = Wink.myWink.Devices.Where(p => p.iscontrollable == true).ToList();
            }
            else if (SensorsOnly)
            {
                lblHeader.Text = "Sensors";
                devices = Wink.myWink.Devices.Where(p => p.issensor == true).ToList();
            }
            else if (typeToShow != "all")
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                lblHeader.Text = "Devices: " + textInfo.ToTitleCase(typeToShow.Replace("_", " "));
                devices = Wink.myWink.Devices.Where(p => p.menu_type == typeToShow).ToList();
            }
            else
            {
                lblHeader.Text = "All Devices";
                devices = Wink.myWink.Devices.Where(p => p.issensor != true || p.menu_type == "hubs").ToList();
            }

            devices = devices.OrderBy(c => c.position).ThenBy(c => c.displayName).ToList();

            dlDevices.DataSource = devices;
            dlDevices.DataBind();
        }

        protected void dlDevices_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Wink.Device device = ((Wink.Device)e.Item.DataItem);
                string devicetype = device.type;

                TextBox tbPosition = (TextBox)e.Item.FindControl("tbPosition");
                tbPosition.Text = device.position > 1000? "":(device.position).ToString();

                TextBox tbDisplayName = (TextBox)e.Item.FindControl("tbDisplayName");
                tbDisplayName.Text = device.displayName;

                List<Wink.Device.DeviceStatus> status = device.status;
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

                //Last_Readings
                DataTable dtStatus = new DataTable();
                dtStatus.Columns.Add("Reading Name");
                dtStatus.Columns.Add("Last Read");
                dtStatus.Columns.Add("Last Updated");
                foreach (Wink.Device.DeviceStatus stat in status)
                {
                    DataRow row = dtStatus.NewRow();
                    row[0] = stat.name;
                    row[1] = stat.current_status;
                    row[2] = stat.last_updated.ToString();
                    dtStatus.Rows.Add(row);
                }

                if (dtStatus.Rows.Count > 0)
                {
                    TableRow rowLastReadings = (TableRow)e.Item.FindControl("rowLastReadings");
                    rowLastReadings.Visible = true;

                    GridView gv = (GridView)e.Item.FindControl("gvLastReadings");
                    gv.DataSource = dtStatus;
                    gv.DataBind();
                }

                //if (device.desired_states.Count > 0)
                //{
                //    TableRow row = (TableRow)e.Item.FindControl("rowDesiredStates");
                //    row.Visible = true;
                    
                //    ListBox lbDesiredStates = (ListBox)e.Item.FindControl("lbDesiredStates");
                //    lbDesiredStates.DataSource = device.desired_states;
                //    lbDesiredStates.DataBind();
                //}


                //SET BATTERY ICON
                if (keys.Contains("battery"))
                {
                    Wink.Device.DeviceStatus stat = status.Single(p => p.name == "battery");

                    if (!string.IsNullOrWhiteSpace(stat.current_status))
                    {
                        Image imgBattery = (Image)e.Item.FindControl("imgBattery");
                        if (imgBattery != null)
                        {
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

                            imgBattery.Visible = true;
                            string imgPath = Request.PhysicalApplicationPath + "\\Images\\Battery\\Battery" + imgLevel + ".png";
                            if (File.Exists(imgPath))
                            {
                                string url = "~/Images/Battery/Battery" + imgLevel + ".png";
                                imgBattery.ImageUrl = url;
                            }
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

            List<Wink.Device.DeviceStatus> status = device.status;
            IList<string> keys = status.Select(p => p.name).ToList();
            string state = string.Empty;
            bool hasConnection = false;
            bool noConnectionValue = false;
            string degree = "n/a";

            if (keys.Contains("connection"))
            {
                Wink.Device.DeviceStatus stat = status.Single(p => p.name == "connection");
                if (stat == null || (stat != null && string.IsNullOrWhiteSpace(stat.current_status)))
                {
                    hasConnection = false;
                    state = "false";
                    hfCurrentStatus.Value = state;

                    Label lblName = (Label)item.FindControl("lblName");
                    lblName.ForeColor = System.Drawing.Color.Red;
                    lblName.Text += "<br />NOT CONNECTED";
                }
                else
                    hasConnection = true;
            }
            else
            {
                noConnectionValue = true;
            }

            if (hasConnection || noConnectionValue)
            {
                if (keys.Contains("powered") || keys.Contains("locked"))
                {
                    Wink.Device.DeviceStatus stat = status.Single(p => p.name == "powered" || p.name == "locked");
                    state = stat.current_status.ToLower();
                    hfMainCommand.Value = stat.name;
                    hfCurrentStatus.Value = state;
                    if (device.iscontrollable)
                        img.Enabled = true;
                }
                else if (keys.Contains("brightness") || keys.Contains("position") || keys.Contains("remaining"))
                {
                    Wink.Device.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position" || p.name == "remaining");
                    Double converted = Convert.ToDouble(stat.current_status) * 100;
                    state = converted > 0 ? "true" : "false";
                    hfMainCommand.Value = stat.name;
                    hfCurrentStatus.Value = state;

                    if (device.iscontrollable)
                        img.Enabled = true;
                }
                else if (hasConnection)
                {
                    state = hasConnection.ToString().ToLower();
                    hfMainCommand.Value = "connection";
                    hfCurrentStatus.Value = state;
                }

                if (keys.Contains("brightness") || keys.Contains("position") || keys.Contains("remaining"))
                {
                    Wink.Device.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position" || p.name == "remaining");
                    hfLevelCommand.Value = stat.name;
                    double testdouble = 0;
                    Double.TryParse(stat.current_status, out testdouble);
                    degree = (testdouble * 100).ToString();
                }

                if (devicetype == "light_bulbs" || devicetype == "binary_switches")
                {
                    if (device.model.ToLower().StartsWith("outl"))
                    {
                        string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\outlets" + state + ".png";
                        if (File.Exists(imgPath))
                        {
                            string url = "~/Images/Devices/outlets" + state + ".png";
                            img.ImageUrl = url;
                        }
                    }
                    else
                    {
                        string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\" + device.manufacturer + state + ".png";
                        if (File.Exists(imgPath))
                        {
                            string url = "~/Images/Devices/" + device.manufacturer + state + ".png";
                            img.ImageUrl = url;
                        }
                        else
                            img.ImageUrl = "~/Images/Devices/lights" + state + ".png";
                    }
                }
                else if (devicetype == "outlets")
                {
                    string imgPath = Request.PhysicalApplicationPath + "\\Images\\Devices\\" + device.menu_type + device.type + state + ".png";
                    if (File.Exists(imgPath))
                    {
                        string url = "~/Images/Devices/" + device.menu_type + device.type + state + ".png";
                        img.ImageUrl = url;
                    }
                    else
                        img.ImageUrl = "~/Images/Devices/outlets" + state + ".png";
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
            }

            if (device.isvariable)
            {
                rs.Visible = true;
                rs.Enabled = hasConnection;
                if (state == "true")
                {
                    decimal dim = 100;
                    decimal.TryParse(degree, out dim);
                    dim = Math.Round(dim);

                    rs.Value = dim;
                    rs.ToolTip = dim + "%";
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
            bool alert = false;

            Wink.Device device = ((Wink.Device)item.DataItem);
            string devicetype = device.sensor_type;

            List<Wink.Device.DeviceStatus> status = device.status;
            IList<string> keys = status.Select(p => p.name).ToList();
            string state = string.Empty;

            if (keys.Contains("connection"))
            {
                Wink.Device.DeviceStatus stat = status.Single(p => p.name == "connection");
                bool reverse = !Convert.ToBoolean(stat.current_status);
                alert = reverse;
                state = reverse.ToString().ToLower();
            }

            //Sesnor_States
            DataTable dtStatus = new DataTable();
            dtStatus.Columns.Add("Reading Name");
            dtStatus.Columns.Add("Last Read");
            dtStatus.Columns.Add("Last Updated");
            foreach (Wink.Device.DeviceStatus stat in device.sensor_states)
            {
                DataRow row = dtStatus.NewRow();
                row[0] = stat.name;
                row[1] = stat.current_status;
                row[2] = stat.last_updated.ToString();
                dtStatus.Rows.Add(row);
            }

            if (dtStatus.Rows.Count > 0)
            {
                TableRow rowSensorStates = (TableRow)item.FindControl("rowSensorStates");
                rowSensorStates.Visible = true;

                GridView gv = (GridView)item.FindControl("gvSensorStates");
                gv.DataSource = dtStatus;
                gv.DataBind();
            }

            double dblTest;
            if (device.sensortripped == "true")
            {
                state = "true";
                alert = true;
            }
            else if (device.sensortripped == "test")
            {
                state = "test";
            }
            else if (double.TryParse(device.sensortripped, out dblTest))
            {
                state = device.sensortripped;
                if (dblTest < 25)
                    alert = true;
            }

            string imgPath = Request.PhysicalApplicationPath + "\\Images\\Sensors\\" + devicetype + state + ".png";
            if (File.Exists(imgPath))
            {
                string url = "~/Images/Sensors/" + devicetype + state + ".png";
                img.ImageUrl = url;
            }
            else
            {
                img.ImageUrl = "";
            }

            ((Image)item.FindControl("imgAlert")).Visible = alert;
        }

        protected void displayThermostats(DataListItem item)
        {
            HiddenField hfDeadband = (HiddenField)item.FindControl("hfDeadband");
 
            HiddenField hfSetHighTemp = (HiddenField)item.FindControl("hfSetHighTemp");
            HiddenField hfSetLowTemp = (HiddenField)item.FindControl("hfSetLowTemp");
            HiddenField hfSetMode = (HiddenField)item.FindControl("hfSetMode");
            HiddenField hfSetPower = (HiddenField)item.FindControl("hfSetPower");

            Table tblThermostat = (Table)item.FindControl("tblThermostat");
            tblThermostat.Visible = true;
            
            Wink.Device device = ((Wink.Device)item.DataItem);

            List<Wink.Device.DeviceStatus> status = device.status;
            IList<string> keys = status.Select(p => p.name).ToList();

            if (keys.Contains("deadband"))
            {
                hfDeadband.Value = status.Single(p => p.name == "deadband").current_status.ToLower();
            }

            if (keys.Contains("powered"))
            {
                string powered = status.Single(p => p.name == "powered").current_status.ToLower();
                ImageButton ibThermPower = (ImageButton)item.FindControl("ibThermPower");
                ibThermPower.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";

                ImageButton ibThermPowerSet = (ImageButton)item.FindControl("ibThermPowerSet");
                ibThermPowerSet.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";
                hfSetPower.Value = powered;
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

                hfSetMode.Value = mode;

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

                Double min;
                Double max;
                Double.TryParse(mintemp, out min);
                Double.TryParse(maxtemp, out max);

                if (min > 0)
                    hfSetLowTemp.Value = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString();

                if (max > 0)
                    hfSetHighTemp.Value = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString();
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
            Wink.Device.sendDeviceCommand(deviceID, command);

            Wink.Device device = Wink.Device.getDeviceByID(deviceID);
            Wink.Device.DeviceStatus status = device.status.Single(p => p.name == hfMainCommand.Value);
            status.current_status = newstate;

            if (!string.IsNullOrWhiteSpace(newlevel))
            {
                Wink.Device.DeviceStatus statuslvl = device.status.Single(p => p.name == hfLevelCommand.Value);
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
                Wink.Device.sendDeviceCommand(deviceID, command);


                Wink.Device device = Wink.Device.getDeviceByID(deviceID);
                Wink.Device.DeviceStatus status = device.status.Single(p => p.name == hfMainCommand.Value);
                status.current_status = newstate;

                Wink.Device.DeviceStatus statuslvl = device.status.Single(p => p.name == hfLevelCommand.Value);
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
                lblNotes.Text = "The command to turn power on has been sent";
            }

            HiddenField hfDeviceID = (HiddenField)ib.NamingContainer.FindControl("hfDeviceID");
            string command = "{\"desired_state\": {\"mode\":null,\"powered\":" + newpower + ",\"modes_allowed\":null,\"min_set_point\":null,\"max_set_point\":null}}";
            Wink.Device.sendDeviceCommand(hfDeviceID.Value, command);

            Wink.Device device = Wink.Device.getDeviceByID(hfDeviceID.Value);
            Wink.Device.DeviceStatus status = device.status.Single(p => p.name == "powered");
            status.current_status = newpower;

        }

        protected void ibThermModeChange_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            Label lblNotes = (Label)ib.NamingContainer.FindControl("lblNotes");
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

            HiddenField hfDeviceID = (HiddenField)ib.NamingContainer.FindControl("hfDeviceID");
            string sendmode = mode == "auto" ? mode : mode + "_only";
            string command = "{\"desired_state\": {\"mode\":\""+ sendmode + "\",\"powered\":true,\"modes_allowed\":null,\"min_set_point\":null,\"max_set_point\":null}}";
            Wink.Device.sendDeviceCommand(hfDeviceID.Value, command);

            lblNotes.Text = "The command to turn the mode to " + mode + " has been sent";

            Wink.Device device = Wink.Device.getDeviceByID(hfDeviceID.Value);
            Wink.Device.DeviceStatus status = device.status.Single(p => p.name == "mode");
            status.current_status = mode;
        }

        protected void ibThermChange_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            Label lblNotes = (Label)ib.NamingContainer.FindControl("lblNotes");
            HiddenField hfSetMode = (HiddenField)ib.NamingContainer.FindControl("hfSetMode");

            string mode = hfSetMode.Value;

            Label lblTempSet = null;
            HiddenField hfSetNewTemp = null;

            Int16 temp = 0;

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
                temp = Convert.ToInt16(lblTempSet.Text.Replace("&deg;", ""));

                if (ib.CommandArgument.Contains("up"))
                    temp++;
                if (ib.CommandArgument.Contains("down"))
                    temp--;

                hfSetNewTemp.Value = temp.ToString();
                lblTempSet.Text = temp.ToString() + "&deg;";
            }

            if (temp > 0)
            {
                HiddenField hfDeadband = (HiddenField)ib.NamingContainer.FindControl("hfDeadband");
                Double changeVal = 2;
                Double.TryParse(hfDeadband.Value, out changeVal);

                Double tempC = Common.FromFahrenheitToCelsius(temp);
                Double tempLow;
                Double tempHigh;
                
                if (mode == "cool")
                {
                    tempLow = tempC - changeVal;
                    tempHigh = tempC;
                }
                else if (mode == "heat")
                {
                    tempLow = tempC;
                    tempHigh = tempC + changeVal;
                }
                else
                {
                    Label lblTempHeatSetauto = (Label)ib.NamingContainer.FindControl("lblTempHeatSetauto");
                    Label lblTempCoolSetauto = (Label)ib.NamingContainer.FindControl("lblTempCoolSetauto");
                    tempLow = Common.FromFahrenheitToCelsius(Convert.ToDouble(lblTempCoolSetauto.Text.Replace("&deg;", "")));
                    tempHigh = Common.FromFahrenheitToCelsius(Convert.ToDouble(lblTempHeatSetauto.Text.Replace("&deg;", "")));
                }

                HiddenField hfDeviceID = (HiddenField)ib.NamingContainer.FindControl("hfDeviceID");
                string command = "{\"desired_state\": {\"mode\":null,\"powered\":true,\"modes_allowed\":null,\"min_set_point\":" + tempLow + ",\"max_set_point\":" + tempHigh + "}}";
                Wink.Device.sendDeviceCommand(hfDeviceID.Value, command);

                lblNotes.Text = "The command to change the temperature has been sent";

                Wink.Device device = Wink.Device.getDeviceByID(hfDeviceID.Value);
                Wink.Device.DeviceStatus statuslow = device.status.Single(p => p.name == "min_set_point");
                statuslow.current_status = tempLow.ToString();
                Wink.Device.DeviceStatus statushigh = device.status.Single(p => p.name == "max_set_point");
                statushigh.current_status = tempHigh.ToString();
            }
        }

        protected void ibThermostat_Click(object sender, EventArgs e)
        {
            LinkButton ib = (LinkButton)sender;

            Session["modalshowing"] = "true";

            ModalPopupExtender mdeThermostats = (ModalPopupExtender)ib.NamingContainer.FindControl("mdeThermostats");
            mdeThermostats.Show();
        }
        
        protected void ibInfo_Click(object sender, EventArgs e)
        {
            ImageButton ib = (ImageButton)sender;

            Session["modalshowing"] = "true";

            ModalPopupExtender mpeInfo = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeInfo");
            mpeInfo.Show();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton ib = (LinkButton)sender;
                TextBox tbPosition = (TextBox)ib.NamingContainer.FindControl("tbPosition");
                TextBox tbDisplayName = (TextBox)ib.NamingContainer.FindControl("tbDisplayName");
                Label lblPositionBad = (Label)ib.NamingContainer.FindControl("lblPositionBad");
                ModalPopupExtender mpeInfo = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeInfo");

                Wink.Device item = Wink.Device.getDeviceByID(ib.CommandArgument);

                bool savePosSuccess = false;
                bool saveNameSuccess = false;

                if (item != null)
                {
                    //SAVE POSITION
                    try
                    {
                        Int32 pos = 9999;
                        if (string.IsNullOrWhiteSpace(tbPosition.Text))
                        {
                            savePosSuccess = true;
                        }
                        else if (Int32.TryParse(tbPosition.Text, out pos) && pos > 0 && pos < 1001)
                        {
                            List<string> existingList = new List<string>();
                            foreach (DataListItem dli in dlDevices.Items)
                            {
                                HiddenField hfDeviceID = (HiddenField)dli.FindControl("hfDeviceID");
                                existingList.Add(hfDeviceID.Value);
                            }
                            string newItem = item.id;

                            existingList.RemoveAll(s => s == newItem);
                            existingList.Insert(pos - 1, newItem);

                            foreach (string ID in existingList)
                            {
                                int position = existingList.IndexOf(ID) + 1;
                                Wink.Device.setDevicePosition(ID, position);
                            }

                            lblPositionBad.Visible = false;
                            savePosSuccess = true;
                        }
                        else
                            lblPositionBad.Visible = true;
                    }
                    catch (Exception ex)
                    {
                        lblPositionBad.Visible = true;
                    }

                    //SAVE DISPLAY NAME
                    try
                    {
                        Wink.Device.setDeviceDisplayName(item.id, tbDisplayName.Text);
                        saveNameSuccess = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (saveNameSuccess && savePosSuccess)
                {
                    Session["modalshowing"] = "false";

                    mpeInfo.Hide();

                    BindData();
                }
                else
                    mpeInfo.Show();
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Devices.btnClose_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void ibSettings_Click(object sender, ImageClickEventArgs e)
        {
            Session["modalshowing"] = "true";

            mpeSettings.Show();
        }

        protected void ibSettingsClose_Click(object sender, EventArgs e)
        {
            Session["modalshowing"] = "false";

            rowData.Visible = cbShow.Checked;
            SettingMgmt.saveSetting(hfSettingBase.Value + "-Visible", cbShow.Checked.ToString());

            SettingMgmt.saveSetting(hfSettingBase.Value + "-Columns", tbColumns.Text);

            mpeSettings.Hide();

            BindData();
        }

        protected void ibHubDevices_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton ib = (ImageButton)sender;
                GridView gvHubDevices = (GridView)ib.NamingContainer.FindControl("gvHubDevices");

                List<Wink.Device> devices = Wink.Device.getDevicesByHubID(ib.CommandArgument);
                devices = devices.OrderBy(c => c.type).ThenBy(c => c.displayName).ToList();

                gvHubDevices.DataSource = devices;
                gvHubDevices.DataBind();

                ModalPopupExtender mpeHubDevices = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeHubDevices");
                mpeHubDevices.Show();
                Session["modalshowing"] = "true";
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Devices.ibHubDevices_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void lbCloseHubDevices_Click(object sender, EventArgs e)
        {
            LinkButton ib = (LinkButton)sender;

            ModalPopupExtender mpeHubDevices = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeHubDevices");
            mpeHubDevices.Hide();
            Session["modalshowing"] = "false";
        }
    }
}