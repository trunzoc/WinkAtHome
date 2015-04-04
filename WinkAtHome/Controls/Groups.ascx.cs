using System;
using System.Collections.Generic;
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
            }
        }

        private void BindData()
        {
            dlGroups.DataSource = null;
            dlGroups.DataBind();

            dlGroups.DataSource = Wink.Groups;
            dlGroups.DataBind();
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.Parent;
            string groupID = ib.CommandArgument;
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
            Wink.sendGroupCommand(groupID, command);

            Wink.Group group = Wink.Group.getGroupByID(groupID);
            Wink.GroupStatus status = group.status.Single(p => p.name == hfMainCommand.Value);
            status.current_status = newstate == "true" ? "1" : "0";

            if (!string.IsNullOrWhiteSpace(newlevel))
            {
                Wink.GroupStatus statuslvl = group.status.Single(p => p.name == hfLevelCommand.Value);
                statuslvl.current_status = "1";
            }

            BindData();
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
                    degree = (Convert.ToDouble(stat.current_status) * 100).ToString();
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

        protected void rsBrightness_ValueChanged(object sender, EventArgs e)
        {
            RadSlider rs = (RadSlider)sender;
            DataListItem li = (DataListItem)rs.Parent;
            ImageButton ib = (ImageButton)li.FindControl("imgIcon"); ;
            string groupID = ib.CommandArgument;
            string command = string.Empty;
            Decimal newlevel = rs.Value / 100;

            HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");

            string newstate = newlevel == 0 ? "false" : "true";

            command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + ",\"" + hfLevelCommand.Value + "\":" + newlevel + "}}";
            Wink.sendGroupCommand(groupID, command);


            Wink.Group group = Wink.Group.getGroupByID(groupID);
            Wink.GroupStatus status = group.status.Single(p => p.name == hfMainCommand.Value);
            status.current_status = newstate == "true" ? "1" : "0";

            Wink.GroupStatus statuslvl = group.status.Single(p => p.name == hfLevelCommand.Value);
            statuslvl.current_status = newlevel.ToString();

            BindData();
        }
    }
}