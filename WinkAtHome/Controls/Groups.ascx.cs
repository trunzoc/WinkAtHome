using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            hfSettingBase.Value = Request.RawUrl.Substring(Request.RawUrl.LastIndexOf('/') + 1) + "-Groups-MV" + ((Table)Page.Master.FindControl("tblExpand")).Visible.ToString();
            if (!IsPostBack)
            {
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

                string hideEmpty = SettingMgmt.getSetting("Hide-Empty-Groups").ToLower();
                if (hideEmpty != null)
                {
                    bool visible = true;
                    bool.TryParse(hideEmpty, out visible);
                    cbHideEmpty.Checked = visible;
                }

                BindData();
            }
        }

        private void BindData()
        {
            dlGroups.DataSource = null;
            dlGroups.DataBind();

            List<Wink.Group> groups;

            if (SettingMgmt.getSetting("Hide-Empty-Groups").ToLower() == "true")
            {
                groups = Wink.Groups.Where(p => !p.isempty).ToList();
            }
            else
                groups = Wink.Groups;

            groups = groups.OrderBy(c => c.position).ThenBy(c => c.displayName).ToList();

            dlGroups.DataSource = groups;
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

                TextBox tbPosition = (TextBox)e.Item.FindControl("tbPosition");
                tbPosition.Text = group.position > 1000 ? "" : (group.position).ToString();

                TextBox tbDisplayName = (TextBox)e.Item.FindControl("tbDisplayName");
                tbDisplayName.Text = group.displayName;


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



                List<Wink.Group.GroupStatus> status = group.status;
                IList<string> keys = status.Select(p => p.name).ToList();
                bool state = false;
                string degree = "n/a";

                if (keys.Contains("powered") || keys.Contains("locked"))
                {
                    Wink.Group.GroupStatus stat = status.Single(p => p.name == "powered" || p.name == "locked");


                    bool stateisbool = bool.TryParse(stat.current_status, out state);
                    if (!stateisbool)
                        state = (Convert.ToDouble(stat.current_status) > 0);

                    hfMainCommand.Value = stat.name;
                    hfCurrentStatus.Value = state.ToString();
                }

                if (keys.Contains("brightness") || keys.Contains("position"))
                {
                    Wink.Group.GroupStatus stat = status.Single(p => p.name == "brightness" || p.name == "position");
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
            Wink.Group.sendGroupCommand(groupID, command);

            updateStuff(groupID, hfMainCommand.Value, newstate, hfLevelCommand.Value, newlevel);
        }

        protected void rsBrightness_ValueChanged(object sender, EventArgs e)
        {
            RadSlider rs = (RadSlider)sender;
            DataListItem li = (DataListItem)rs.NamingContainer;
            ImageButton ib = (ImageButton)li.FindControl("imgIcon"); ;
            string groupID = ib.CommandArgument;
            string command = string.Empty;
            Decimal newlevel = Math.Round(rs.Value) / 100;

            HiddenField hfMainCommand = (HiddenField)li.FindControl("hfMainCommand");
            HiddenField hfCurrentStatus = (HiddenField)li.FindControl("hfCurrentStatus");
            HiddenField hfLevelCommand = (HiddenField)li.FindControl("hfLevelCommand");

            string newstate = newlevel == 0 ? "false" : "true";

            command = "{\"desired_state\": {\"" + hfMainCommand.Value + "\":" + newstate + ",\"" + hfLevelCommand.Value + "\":" + newlevel + "}}";
            Wink.Group.sendGroupCommand(groupID, command);

            updateStuff(groupID, hfMainCommand.Value, newstate, hfLevelCommand.Value, newlevel.ToString());
        }

        protected void updateStuff(string groupID, string maincommand, string newstate, string levelcommand, string newlevel = "")
        {
            Wink.Group group = Wink.Group.getGroupByID(groupID);
            Wink.Group.GroupStatus status = group.status.Single(p => p.name == maincommand);
            status.current_status = newstate == "true" ? "1" : "0";

            Wink.Group.GroupStatus statuslvl = group.status.SingleOrDefault(p => p.name == levelcommand);
            if (statuslvl != null)
                statuslvl.current_status = newlevel;

            foreach (Wink.Group.GroupMember member in group.members)
            {
                Wink.Device device = Wink.Device.getDeviceByID(member.id);
                if (device != null)
                {
                    Wink.Device.DeviceStatus devstatp = device.status.SingleOrDefault(p => p.name == maincommand);
                    if (devstatp != null)
                        devstatp.current_status = newstate;

                    Wink.Device.DeviceStatus devstatd = device.status.SingleOrDefault(p => p.name == levelcommand);
                    if (devstatd != null)
                        devstatd.current_status = newlevel;
                }
            }

            Response.Redirect(Request.RawUrl);
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

            SettingMgmt.saveSetting("Hide-Empty-Groups", cbHideEmpty.Checked.ToString());

            mpeSettings.Hide();

            BindData();
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

                Wink.Group item = Wink.Group.getGroupByID(ib.CommandArgument);

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
                            foreach (DataListItem dli in dlGroups.Items)
                            {
                                HiddenField hfGroupID = (HiddenField)dli.FindControl("hfGroupID");
                                existingList.Add(hfGroupID.Value);
                            }
                            string newItem = item.id;

                            existingList.RemoveAll(s => s == newItem);
                            existingList.Insert(pos - 1, newItem);

                            foreach (string ID in existingList)
                            {
                                int position = existingList.IndexOf(ID) + 1;
                                Wink.Group.setGroupPosition(ID, position);
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
                        Wink.Group.setGroupDisplayName(item.id, tbDisplayName.Text);
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
                throw ex; //throw ex; //EventLog.WriteEntry("WinkAtHome.Groups.btnClose_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}