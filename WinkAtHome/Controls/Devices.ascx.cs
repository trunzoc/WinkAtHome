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
        public bool ControllableOnly {get; set;}
        public string typeToShow { get; set; }

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
                else if (typeToShow != null)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    lblHeader.Text = "Devices: " + textInfo.ToTitleCase(typeToShow.Replace("_", " "));
                }
                else
                {
                    lblHeader.Text = "All Devices";
                }

                string columns = SettingMgmt.getSetting("Devices-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + ControllableOnly.ToString() + "Columns");
                if (columns != null)
                    tbColumns.Text = columns;

                BindData();
            }
        }

        private void BindData()
        {
            if (!string.IsNullOrWhiteSpace(hfDeviceType.Value))
                typeToShow = hfDeviceType.Value;

            dlDevices.DataSource = null;
            dlDevices.DataBind();

            List<Wink.Device> devices = new List<Wink.Device>();
            if (ControllableOnly)
            {
                devices = Wink.Devices.Where(p => p.controllable == true).ToList();
            }
            else if (typeToShow != null)
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

                if (device.name == "Test Refuel" || device.name == "HOME" || device.name == "Garage Door")
                {
                    string bob = "bob";
                }

                ImageButton img = (ImageButton)e.Item.FindControl("imgIcon");
                RadSlider rs = (RadSlider)e.Item.FindControl("rsBrightness");
                HiddenField hfMainCommand = (HiddenField)e.Item.FindControl("hfMainCommand");
                HiddenField hfCurrentStatus = (HiddenField)e.Item.FindControl("hfCurrentStatus");
                HiddenField hfLevelCommand = (HiddenField)e.Item.FindControl("hfLevelCommand");
                HiddenField hfJSON = (HiddenField)e.Item.FindControl("hfJSON");
                hfJSON.Value = device.json;

                Table tblDefault = (Table)e.Item.FindControl("tblDefault");
                Table tblThermostat = (Table)e.Item.FindControl("tblThermostat");

                Label lblType = (Label)e.Item.FindControl("lblType");
                if (lblType != null) lblType.Text = device.type;

                List<Wink.DeviceStatus> status = device.status;
                IList<string> keys = status.Select(p => p.name).ToList();
                string state = string.Empty;
                string degree = "n/a";

                //BIND INFO BUTTON
                var props = typeof(Wink.Device).GetProperties();
                var properties = new List<KeyValuePair<string, string>>();
                foreach (var prop in props)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    
                    string propname = textInfo.ToTitleCase(prop.Name.Replace("_", " "));
                    var propvalue = prop.GetValue(device,null);
                    if (propvalue != null)
                        properties.Add(new KeyValuePair<string, string>(propname, propvalue.ToString()));
                }
                DataList dlProperties = (DataList)e.Item.FindControl("dlProperties");
                if (dlProperties != null)
                {
                    dlProperties.DataSource = properties;
                    dlProperties.DataBind();
                }

                //SETUP DEVICES DISPLAY
                if (devicetype == "thermostats") //Start Thermostats
                {
                    HiddenField hfSetHighTemp = (HiddenField)e.Item.FindControl("hfSetHighTemp");
                    HiddenField hfSetLowTemp = (HiddenField)e.Item.FindControl("hfSetLowTemp");
                    HiddenField hfSetMode = (HiddenField)e.Item.FindControl("hfSetMode");
                    HiddenField hfSetPower = (HiddenField)e.Item.FindControl("hfSetPower");
                    
                    tblThermostat.Visible = true;
                    if (keys.Contains("powered"))
                    {
                        string powered = status.Single(p => p.name == "powered").current_status.ToLower();
                        ImageButton ibThermPower = (ImageButton)e.Item.FindControl("ibThermPower");
                        ibThermPower.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";

                        ImageButton ibThermPowerSet = (ImageButton)e.Item.FindControl("ibThermPowerSet");
                        ibThermPowerSet.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";
                        hfSetPower.Value = powered;
                    }

                    if (keys.Contains("temperature") && !string.IsNullOrWhiteSpace(status.Single(p => p.name == "temperature").current_status))
                    {
                        Double temp = Common.FromCelsiusToFahrenheit(Convert.ToDouble(status.Single(p => p.name == "temperature").current_status));
                        Label lblThermStats = (Label)e.Item.FindControl("lblThermStats");
                        lblThermStats.Text = temp.ToString() + "&deg;";

                        Label lblThermStatsSet = (Label)e.Item.FindControl("lblThermStatsSet");
                        lblThermStatsSet.Text = temp.ToString() + "&deg;";
                    }

                    if (keys.Contains("mode"))
                    {
                        string mode = status.Single(p => p.name == "mode").current_status.ToLower().Replace("_only","");
                        Image imgThermostatModeAuto = (Image)e.Item.FindControl("imgThermostatModeAuto");
                        imgThermostatModeAuto.ImageUrl = "~/Images/Thermostats/" + mode + "true.png";

                        Image imgThermostatModeHeatCool = (Image)e.Item.FindControl("imgThermostatModeHeatCool");
                        imgThermostatModeHeatCool.ImageUrl = "~/Images/Thermostats/" + mode + mode + ".png";

                        ImageButton ibThermMode = (ImageButton)e.Item.FindControl("ibTherm" + mode);
                        ibThermMode.ImageUrl = "~/Images/Thermostats/" + mode + "true.png";

                        hfSetMode.Value = mode;
                        
                        string mintemp = string.Empty;
                        string maxtemp = string.Empty;
                        if (mode == "auto")
                        {
                            Table tblMode = (Table)e.Item.FindControl("tblThermauto");
                            tblMode.Visible = true;
                            Table tblModeSet = (Table)e.Item.FindControl("tblThermautoSet");
                            tblModeSet.Visible = true;

                            if (keys.Contains("min_set_point"))
                            {
                                mintemp = status.Single(p => p.name == "min_set_point").current_status.ToLower();
                                Label lblTempCool = (Label)e.Item.FindControl("lblTempCool" + mode);
                                lblTempCool.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";

                                Label lblTempCoolSet = (Label)e.Item.FindControl("lblTempCoolSet" + mode);
                                lblTempCoolSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                            }

                            if (keys.Contains("max_set_point"))
                            {
                                maxtemp = status.Single(p => p.name == "max_set_point").current_status.ToLower();
                                Label lblTempHeat = (Label)e.Item.FindControl("lblTempHeat" + mode);
                                lblTempHeat.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";

                                Label lblTempHeatSet = (Label)e.Item.FindControl("lblTempHeatSet" + mode);
                                lblTempHeatSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                            }
                        }
                        else
                        {
                            Table tblMode = (Table)e.Item.FindControl("tblCoolHeat");
                            tblMode.Visible = true;
                            Table tblModeSet = (Table)e.Item.FindControl("tblCoolHeatSet");
                            tblModeSet.Visible = true;

                            ImageButton ibThermUp = (ImageButton)e.Item.FindControl("ibThermUp");
                            ibThermUp.ImageUrl = "~/Images/Thermostats/" + mode + "up.png";

                            ImageButton ibThermDown = (ImageButton)e.Item.FindControl("ibThermDown");
                            ibThermDown.ImageUrl = "~/Images/Thermostats/" + mode + "down.png";

                            Image imgCool = (Image)e.Item.FindControl("imgCool");
                            imgCool.ImageUrl = "~/Images/Thermostats/cool" + mode + ".png";

                            Image imgHeat = (Image)e.Item.FindControl("imgHeat");
                            imgHeat.ImageUrl = "~/Images/Thermostats/heat" + mode + ".png";

                            if (keys.Contains("max_set_point"))
                            {
                                maxtemp = status.Single(p => p.name == "max_set_point").current_status.ToLower();
                                if (mode == "cool")
                                {
                                    Label lblTemp = (Label)e.Item.FindControl("lblTemp");
                                    lblTemp.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                                    lblTemp.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

                                    Label lblTempSet = (Label)e.Item.FindControl("lblTempSet");
                                    lblTempSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                                    lblTempSet.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                                }
                            }

                            if (keys.Contains("min_set_point"))
                            {
                                mintemp = status.Single(p => p.name == "min_set_point").current_status.ToLower();
                                if (mode == "heat")
                                {
                                    Label lblTemp = (Label)e.Item.FindControl("lblTemp");
                                    lblTemp.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                                    lblTemp.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

                                    Label lblTempSet = (Label)e.Item.FindControl("lblTempSet");
                                    lblTempSet.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                                    lblTempSet.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                                }
                            }
                        }
                        hfSetLowTemp.Value = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString();
                        hfSetHighTemp.Value = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString();
                    }
                }
                else //Start Normal Devices
                {
                    tblDefault.Visible = true;

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

                        if (device.controllable)
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
                        degree = (Convert.ToDouble(stat.current_status) * 100).ToString();
                        hfLevelCommand.Value = stat.name;
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

                    if (degree != "n/a" && device.controllable)
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
                } //end normal devices
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

                if (hfLevelCommand.Value == "brightness" && newstate == "true")
                {
                    newstate = hfMainCommand.Value + "\":" + (newlevel == 0 ? "false" : "true") + ",\"";
                }

                command = "{\"desired_state\": {\"" + newstate + hfLevelCommand.Value + "\":" + newlevel + "}}";
                Wink.sendDeviceCommand(deviceID, command);


                Wink.Device device = Wink.Device.getDeviceByID(deviceID);
                Wink.DeviceStatus status = device.status.Single(p => p.name == hfMainCommand.Value);
                status.current_status = newstate;

                Wink.DeviceStatus statuslvl = device.status.Single(p => p.name == hfLevelCommand.Value);
                statuslvl.current_status = newlevel.ToString();

                BindData();
            }
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            SettingMgmt.saveSetting("Devices-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + ControllableOnly.ToString() + "Columns", tbColumns.Text);
            BindData();
        }

        protected void ibThermModeChange_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            HiddenField hfSetMode = (HiddenField)ib.NamingContainer.FindControl("hfSetMode");
            HiddenField hfSetLowTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetLowTemp");
            HiddenField hfSetHighTemp = (HiddenField)ib.NamingContainer.FindControl("hfSetHighTemp");

            string mode = ib.CommandArgument;
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
        }

        protected void ibThermPower_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            HiddenField hfSetPower = (HiddenField)ib.NamingContainer.FindControl("hfSetPower");
            bool power = Convert.ToBoolean(hfSetPower.Value);
            string newpower = (!power).ToString().ToLower();

            ib.ImageUrl = "~/Images/Thermostats/power" + newpower + ".png";
            hfSetPower.Value = newpower;
        }

        protected void lbApplyThermostat_Click(object sender, EventArgs e)
        {
            LinkButton ib = (LinkButton)sender;
            DataListItem li = (DataListItem)ib.NamingContainer;
            HiddenField hfSetMode = (HiddenField)ib.NamingContainer.FindControl("hfSetMode");
            HiddenField hfSetPower = (HiddenField)ib.NamingContainer.FindControl("hfSetPower");
            Label lblTempCoolSetauto = (Label)ib.NamingContainer.FindControl("lblTempCoolSetauto");
            Label lblTempHeatSetauto = (Label)ib.NamingContainer.FindControl("lblTempHeatSetauto");
            Label lblTempSet = (Label)ib.NamingContainer.FindControl("lblTempSet");
                
            string deviceID = ib.CommandArgument;
            string command = string.Empty;
            string thermmode = string.Empty;
            string thermpower = string.Empty;
            Double basetemp = 0;
            Double thermmin = 0;
            Double thermmax = 0;

            thermmode = hfSetMode.Value.ToLower() == "auto" ? hfSetMode.Value.ToLower() : hfSetMode.Value.ToLower() + "_only";
            thermpower = hfSetPower.Value.ToLower();

            if (hfSetMode.Value == "auto")
            {
                thermmin = Common.FromFahrenheitToCelsius(Convert.ToDouble(lblTempCoolSetauto.Text.Replace("&deg;", "")));
                thermmax = Common.FromFahrenheitToCelsius(Convert.ToDouble(lblTempHeatSetauto.Text.Replace("&deg;", "")));
            }
            else if (hfSetMode.Value == "cool")
            {
                basetemp = Convert.ToDouble(lblTempSet.Text.Replace("&deg;", ""));
                thermmax = Common.FromFahrenheitToCelsius(basetemp);
                thermmin = Common.FromFahrenheitToCelsius(basetemp - 2);
            }
            else if (hfSetMode.Value == "heat")
            {
                basetemp = Convert.ToDouble(lblTempSet.Text.Replace("&deg;", ""));
                thermmin = Common.FromFahrenheitToCelsius(basetemp);
                thermmax = Common.FromFahrenheitToCelsius(basetemp + 2);
            }
            else
            {
            }

            command = "{\"desired_state\": {\"" + "\"mode\":\"" + thermmode + "\",\"powered\":" + thermpower + ",\"modes_allowed\":null,\"min_set_point\":" + thermmin + ",\"max_set_point\":" + thermmax + "}}";
            Wink.sendDeviceCommand(deviceID, command);

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

            //LinkButton lb = (LinkButton)ib.NamingContainer.FindControl("lbApplyThermostat_Click");
            //lb.Text = "CHANGES APPLIED";


            ModalPopupExtender mpe = (ModalPopupExtender)ib.NamingContainer.FindControl("mdeThermostats");
            mpe.Hide();
            BindData();
        }
    }
}