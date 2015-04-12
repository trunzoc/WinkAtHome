using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace WinkAtHome.Controls
{
    public partial class Groups : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindData();
                
                string columns = SettingMgmt.getSetting("Groups-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns");
                if (columns != null)
                    tbColumns.Text = columns;

                string dataVisible = SettingMgmt.getSetting("Groups-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Visible");
                if (dataVisible != null)
                {
                    bool visible = true;
                    bool.TryParse(dataVisible, out visible);
                    rowData.Visible = visible;
                }
            }
        }

        private void BindData()
        {
            dlGroups.DataSource = null;
            dlGroups.DataBind();

            dlGroups.DataSource = Wink.Groups;
            dlGroups.DataBind();
        }

        protected void dlGroups_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Wink.Group group = ((Wink.Group)e.Item.DataItem);
                ImageButton img = (ImageButton)e.Item.FindControl("imgIcon");
                RadSlider rs = (RadSlider)e.Item.FindControl("rsBrightness");
                HiddenField hfMainCommand = (HiddenField)e.Item.FindControl("hfMainCommand");
                HiddenField hfCurrentStatus = (HiddenField)e.Item.FindControl("hfCurrentStatus");
                HiddenField hfLevelCommand = (HiddenField)e.Item.FindControl("hfLevelCommand");

                //BIND INFO BUTTON
                var props = typeof(Wink.Group).GetProperties();
                var properties = new List<KeyValuePair<string, string>>();
                foreach (var prop in props)
                {
                    if (prop.Name != "json")
                    {
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                        string propname = textInfo.ToTitleCase(prop.Name.Replace("_", " "));
                        var propvalue = prop.GetValue(group, null);
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



                List<Wink.GroupStatus> status = group.status;
                IList<string> keys = status.Select(p => p.name).ToList();
                bool state = false;
                string degree = "n/a";

                if (keys.Contains("powered") || keys.Contains("locked"))
                {
                    Wink.GroupStatus stat = status.Single(p => p.name == "powered" || p.name == "locked");


                    bool stateisbool = bool.TryParse(stat.current_status, out state);
                    if (!stateisbool)
                        state = (Convert.ToDouble(stat.current_status) > 0);

                    hfMainCommand.Value = stat.name;
                    hfCurrentStatus.Value = state.ToString();
                }

                if (keys.Contains("brightness") || keys.Contains("position"))
                {
                    Wink.GroupStatus stat = status.Single(p => p.name == "brightness" || p.name == "position");
                    degree = Math.Round(Convert.ToDouble(stat.current_status) * 100).ToString();
                    hfLevelCommand.Value = stat.name;
                }

                img.ImageUrl = "~/Images/Groups/" + state.ToString() + ".png";

                if (degree != "n/a" && state)
                {
                    rs.Visible = true;
                    rs.Value = Convert.ToDecimal(degree);
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
            DataListItem li = (DataListItem)ib.NamingContainer;
            string groupID = ib.CommandArgument;
            string command = string.Empty;

            HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");

            string newstate = (!Convert.ToBoolean(hfCurrentStatus.Value)).ToString().ToLower();
            string newlevel = string.Empty;
            string newlevelcommand = string.Empty;

            if (newstate == "true")
                newlevel = "1";
            else
                newlevel = "0";

            if (hfLevelCommand.Value == "brightness")
                newlevelcommand = ",\"brightness\":" + newlevel;

            command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + newlevelcommand + "}}";
            Wink.sendGroupCommand(groupID, command);

            updateStuff(groupID, hfMainCommand.Value, newstate, hfLevelCommand.Value, newlevel);
        }

        protected void rsBrightness_ValueChanged(object sender, EventArgs e)
        {
            RadSlider rs = (RadSlider)sender;
            DataListItem li = (DataListItem)rs.Parent;
            ImageButton ib = (ImageButton)li.FindControl("imgIcon"); ;
            string groupID = ib.CommandArgument;
            string command = string.Empty;
            Decimal newlevel = Math.Round(rs.Value) / 100;

            HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");

            string newstate = newlevel == 0 ? "false" : "true";

            command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + ",\"" + hfLevelCommand.Value + "\":" + newlevel + "}}";
            Wink.sendGroupCommand(groupID, command);

            updateStuff(groupID, hfMainCommand.Value, newstate, hfLevelCommand.Value, newlevel.ToString());
        }

        protected void updateStuff(string groupID, string maincommand, string newstate, string levelcommand, string newlevel = "")
        {
            Wink.Group group = Wink.Group.getGroupByID(groupID);
            Wink.GroupStatus status = group.status.Single(p => p.name == maincommand);
            status.current_status = newstate == "true" ? "1" : "0";

            Wink.GroupStatus statuslvl = group.status.SingleOrDefault(p => p.name == levelcommand);
            if (statuslvl != null)
                statuslvl.current_status = newlevel;

            foreach (Wink.GroupMember member in group.members)
            {
                Wink.Device device = Wink.Device.getDeviceByID(member.id);
                if (device != null)
                {
                    Wink.DeviceStatus devstatp = device.status.SingleOrDefault(p => p.name == maincommand);
                    if (devstatp != null)
                        devstatp.current_status = newstate;

                    Wink.DeviceStatus devstatd = device.status.SingleOrDefault(p => p.name == levelcommand);
                    if (devstatd != null)
                        devstatd.current_status = newlevel;
                }
            }

            if (Request.RawUrl.ToLower().Contains("default.aspx"))
            {
                Response.Redirect(Request.RawUrl);
            }
            else
            {
                BindData();
            }
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            SettingMgmt.saveSetting("Groups-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns", tbColumns.Text);
        }

        protected void ibExpand_Click(object sender, ImageClickEventArgs e)
        {
            rowData.Visible = !rowData.Visible;
            SettingMgmt.saveSetting("Groups-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Visible", rowData.Visible.ToString());
        }
    }
}