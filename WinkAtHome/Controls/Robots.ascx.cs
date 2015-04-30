using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class Robots : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                hfSettingBase.Value = Request.RawUrl.Substring(Request.RawUrl.LastIndexOf('/') + 1) + "-Robots-MV" + ((Table)Page.Master.FindControl("tblExpand")).Visible.ToString();
                if (!IsPostBack)
                {

                    string columns = SettingMgmt.getSetting(hfSettingBase.Value + "-Columns");
                    if (columns != null)
                    {
                        tbColumns.Text = columns;
                        dlRobots.RepeatColumns = Convert.ToInt32(tbColumns.Text);
                    }

                    string dataVisible = SettingMgmt.getSetting(hfSettingBase.Value + "-Visible");
                    if (dataVisible != null)
                    {
                        bool visible = true;
                        bool.TryParse(dataVisible, out visible);
                        rowData.Visible = visible;
                        cbShow.Checked = visible;
                    }

                    string hideEmpty = SettingMgmt.getSetting("Hide-Empty-Robots").ToLower();
                    if (hideEmpty != null)
                    {
                        bool visible = true;
                        bool.TryParse(hideEmpty, out visible);
                        cbHideEmpty.Checked = visible;
                    }

                    string alerttimeout = SettingMgmt.getSetting("Robot-Alert-Minutes-Since-Last-Trigger");
                    if (alerttimeout != null)
                    {
                        tbAlertTimeout.Text = alerttimeout;
                    }
                    

                    BindData();
                }
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Robots.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        private void BindData()
        {
            try
            {
                dlRobots.DataSource = null;
                dlRobots.DataBind();

                List<Wink.Robot> robots = new List<Wink.Robot>();

                if (SettingMgmt.getSetting("Hide-Empty-Robots").ToLower() == "true")
                {
                    robots = Wink.Robots.Where(p => !p.isempty).ToList();
                }
                else
                    robots = Wink.Robots;

                robots = robots.OrderBy(c => c.position).ThenBy(c => c.displayName).ToList();

                dlRobots.DataSource = robots;
                dlRobots.DataBind();
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Robots.BindData", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void dlRobots_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    Wink.Robot robot = ((Wink.Robot)e.Item.DataItem);

                    //BIND INFO BUTTON
                    var props = typeof(Wink.Robot).GetProperties();
                    var properties = new List<KeyValuePair<string, string>>();
                    foreach (var prop in props)
                    {
                        if (prop.Name != "json")
                        {
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                            string propname = textInfo.ToTitleCase(prop.Name.Replace("_", " "));
                            var propvalue = prop.GetValue(robot, null);
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

                    TextBox tbPosition = (TextBox)e.Item.FindControl("tbPosition");
                    tbPosition.Text = robot.position > 1000 ? "" : (robot.position).ToString();

                    TextBox tbDisplayName = (TextBox)e.Item.FindControl("tbDisplayName");
                    tbDisplayName.Text = robot.displayName;

                    //Assign Image
                    ImageButton imgIcon = (ImageButton)e.Item.FindControl("imgIcon");
                    imgIcon.ImageUrl = "~/Images/Robots/" + (robot.isschedule ? "schedule" : "robot") + robot.enabled + ".png";

                    if (robot.isschedule)
                        imgIcon.Enabled = false;


                    //Set Last/Next Trigger Time
                    TextBox tbName = (TextBox)e.Item.FindControl("tbName");
                    string tooltip = "Last Triggered: " + (robot.last_fired.ToString().Contains("1/1/1970") ? "Never" : robot.last_fired.ToString());
                    if (robot.isschedule)
                        tooltip += Environment.NewLine + "Next Scheduled Run: " + (robot.next_run.ToString().Contains("1/1/1970") ? "Never" : robot.next_run.ToString());

                    tbName.ToolTip = tooltip;

                    //Resize for long names
                    int i = tbName.Text.Length;
                    int rowsize = (i / 23) + 2;
                    tbName.Rows = rowsize;

                    //Set Alert icon
                    string alertTimeout = SettingMgmt.getSetting("Robot-Alert-Minutes-Since-Last-Trigger");
                    Int32 timeout = 60;
                    Int32.TryParse(alertTimeout, out timeout);

                    DateTime lastTrigger = Convert.ToDateTime(robot.last_fired);
                    DateTime dateAlert = DateTime.Now.AddMinutes(timeout * -1);

                    if (lastTrigger > dateAlert)
                    {
                        Image imgAlert = (Image)e.Item.FindControl("imgAlert");
                        imgAlert.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Robots.dlRobots_ItemDataBound", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton ib = (ImageButton)sender;
                DataListItem li = (DataListItem)ib.NamingContainer;
                string robotID = ib.CommandArgument;

                bool newstate = !Convert.ToBoolean(ib.CommandName);

                Wink.Robot.changeRobotState(robotID, newstate);
                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Robots.imgIcon_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void ibInfo_Click(object sender, EventArgs e)
        {
            try
            {
                ImageButton ib = (ImageButton)sender;

                Session["modalshowing"] = "true";

                ModalPopupExtender mpeInfo = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeInfo");
                mpeInfo.Show();
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Robots.ibInfo_Click", ex.Message, EventLogEntryType.Error);
            }
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

                Wink.Robot item = Wink.Robot.getRobotByID(ib.CommandArgument);

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
                            foreach (DataListItem dli in dlRobots.Items)
                            {
                                HiddenField hfRobotID = (HiddenField)dli.FindControl("hfRobotID");
                                existingList.Add(hfRobotID.Value);
                            }
                            string newItem = item.id;

                            existingList.RemoveAll(s => s == newItem);
                            existingList.Insert(pos - 1, newItem);

                            foreach (string ID in existingList)
                            {
                                int position = existingList.IndexOf(ID) + 1;
                                Wink.Robot.setRobotPosition(ID, position);
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
                        Wink.Robot.setRobotDisplayName(item.id, tbDisplayName.Text);
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
                throw ex; //EventLog.WriteEntry("WinkAtHome.Robots.btnClose_Click", ex.Message, EventLogEntryType.Error);
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

            SettingMgmt.saveSetting("Hide-Empty-Robots", cbHideEmpty.Checked.ToString());

            SettingMgmt.saveSetting("Robot-Alert-Minutes-Since-Last-Trigger", tbAlertTimeout.Text);

            mpeSettings.Hide();

            BindData();
        }
    }
}