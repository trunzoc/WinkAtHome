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

                if (devicetype == "thermostats") //Start Thermostats
                {
                    tblThermostat.Visible = true;
                    if (keys.Contains("powered"))
                    {
                        ImageButton ibThermPower = (ImageButton)e.Item.FindControl("ibThermPower");
                        string powered = status.Single(p => p.name == "powered").current_status.ToLower();
                        ibThermPower.ImageUrl = "~/Images/Thermostats/power" + powered + ".png";
                    }

                    //SET TEMP/HUMIDITY HERE

                    if (keys.Contains("mode"))
                    {
                        string mode = status.Single(p => p.name == "mode").current_status.ToLower();
                        
                        ImageButton ibThermMode = (ImageButton)e.Item.FindControl("ibTherm" + mode);
                        ibThermMode.ImageUrl = "~/Images/Thermostats/" + mode + "true.png";
                        
                        if (mode == "auto")
                        {
                            Table tblMode = (Table)e.Item.FindControl("tblThermauto");
                            tblMode.Visible = true;

                            if (keys.Contains("min_set_point"))
                            {
                                string mintemp = status.Single(p => p.name == "min_set_point").current_status.ToLower();
                                Label lblTempCool = (Label)e.Item.FindControl("lblTempCool" + mode);
                                lblTempCool.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                            }

                            if (keys.Contains("max_set_point"))
                            {
                                string maxtemp = status.Single(p => p.name == "max_set_point").current_status.ToLower();
                                Label lblTempHeat = (Label)e.Item.FindControl("lblTempHeat" + mode);
                                lblTempHeat.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                            }
                        }
                        else
                        {
                            Table tblMode = (Table)e.Item.FindControl("tblCoolHeat");
                            tblMode.Visible = true;
                            
                            ImageButton ibThermUp = (ImageButton)e.Item.FindControl("ibThermUp");
                            ibThermUp.ImageUrl = "~/Images/Thermostats/" + mode + "up.png";

                            ImageButton ibThermDown = (ImageButton)e.Item.FindControl("ibThermDown");
                            ibThermDown.ImageUrl = "~/Images/Thermostats/" + mode + "down.png";

                            Image imgCool = (Image)e.Item.FindControl("imgCool");
                            imgCool.ImageUrl = "~/Images/Thermostats/cool" + mode + ".png";

                            Image imgHeat = (Image)e.Item.FindControl("imgHeat");
                            imgHeat.ImageUrl = "~/Images/Thermostats/heat" + mode + ".png";

                            if (keys.Contains("max_set_point") && mode == "cool")
                            {
                                string maxtemp = status.Single(p => p.name == "max_set_point").current_status.ToLower();
                                Label lblTemp = (Label)e.Item.FindControl("lblTemp");
                                lblTemp.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(maxtemp)).ToString() + "&deg;";
                                lblTemp.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                            }

                            if (keys.Contains("min_set_point") && mode == "heat")
                            {
                                string mintemp = status.Single(p => p.name == "min_set_point").current_status.ToLower();
                                Label lblTemp = (Label)e.Item.FindControl("lblTemp");
                                lblTemp.Text = Common.FromCelsiusToFahrenheit(Convert.ToDouble(mintemp)).ToString() + "&deg;";
                                lblTemp.ForeColor = (mode == "heat") ? System.Drawing.Color.Red : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                            }
                        }







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
                    else if (keys.Contains("connection"))
                    {
                        Wink.DeviceStatus stat = status.Single(p => p.name == "connection");
                        state = stat.current_status.ToLower();
                        hfMainCommand.Value = stat.name;
                        hfCurrentStatus.Value = state;
                    }

                    if (keys.Contains("brightness") || keys.Contains("position"))
                    {
                        Wink.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position");
                        degree = (Convert.ToDouble(stat.current_status) * 100).ToString();
                        hfLevelCommand.Value = stat.name;
                    }

                    if (devicetype == "light_bulbs" || devicetype == "binary_switches")
                    {
                        img.ImageUrl = "~/Images/Devices/lights" + state + ".png";
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

                    if (degree != "n/a" && state == "true")
                    {
                        rs.Visible = true;
                        rs.Value = Convert.ToInt32(degree);
                        rs.ToolTip = degree + "%";
                    }
                    else if (degree == "n/a")
                    {
                        rs.Visible = false;
                    }
                } //end normal devices
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.Parent;
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
            RadSlider rs = (RadSlider)sender;
            DataListItem li = (DataListItem)rs.Parent;
            ImageButton ib = (ImageButton)li.FindControl("imgIcon"); ;
            string deviceID = ib.CommandArgument;
            string command = string.Empty;
            Decimal newlevel = rs.Value / 100;

            HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");

            string newstate = newlevel == 0 ? "false" : "true";

            command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + ",\"" + hfLevelCommand.Value + "\":" + newlevel + "}}";
            Wink.sendDeviceCommand(deviceID, command);


            Wink.Device device = Wink.Device.getDeviceByID(deviceID);
            Wink.DeviceStatus status = device.status.Single(p => p.name == hfMainCommand.Value);
            status.current_status = newstate;

            Wink.DeviceStatus statuslvl = device.status.Single(p => p.name == hfLevelCommand.Value);
            statuslvl.current_status = newlevel.ToString();

            BindData();
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
                SettingMgmt.saveSetting("Devices-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + ControllableOnly.ToString() + "Columns", tbColumns.Text);
        }
    }
}