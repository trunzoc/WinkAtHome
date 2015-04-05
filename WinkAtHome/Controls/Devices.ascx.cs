using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;


namespace WinkAtHome.Controls
{
    public partial class Devices : System.Web.UI.UserControl
    {
        public bool ControllableOnly {get; set;}

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ControllableOnly)
                {
                    lblHeader.Text = "Controllable Devices";
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
            dlDevices.DataSource = null;
            dlDevices.DataBind();

            dlDevices.DataSource = ControllableOnly ? Wink.Devices.Where(p => p.controllable == true) : Wink.Devices;
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

                Label lblType = (Label)e.Item.FindControl("lblType");
                if (lblType != null) lblType.Text = device.type;

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
                }

                if (keys.Contains("brightness") || keys.Contains("position"))
                {
                    Wink.DeviceStatus stat = status.Single(p => p.name == "brightness" || p.name == "position");
                    degree = (Convert.ToDouble(stat.current_status) * 100).ToString();
                    hfLevelCommand.Value = stat.name;
                }


                if (devicetype == "light_bulbs" || devicetype == "binary_switches")
                {
                    img.ImageUrl = "~/Images/Lights/" + state + ".png";
                }
                else if (devicetype == "locks")
                {
                    img.ImageUrl = "~/Images/Locks/" + state + ".png";
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