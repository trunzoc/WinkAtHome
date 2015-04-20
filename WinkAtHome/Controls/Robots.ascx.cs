using AjaxControlToolkit;
using System;
using System.Collections.Generic;
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
                }

                BindData();
            }
        }

        private void BindData()
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

            dlRobots.DataSource = robots;
            dlRobots.DataBind();

        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.NamingContainer;
            string robotID = ib.CommandArgument;

            bool newstate = !Convert.ToBoolean(ib.CommandName);

            Wink.changeRobotState(robotID, newstate);
            Response.Redirect(Request.RawUrl);
        }

        protected void dlRobots_ItemDataBound(object sender, DataListItemEventArgs e)
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
                string alertTimeout = SettingMgmt.getSetting("Robot-Alert-Minutes-Since-Last-Trigger", true);
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

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            dlRobots.RepeatColumns = Convert.ToInt32(tbColumns.Text);
            SettingMgmt.saveSetting(hfSettingBase.Value + "-Columns", tbColumns.Text);
        }

        protected void ibExpand_Click(object sender, ImageClickEventArgs e)
        {
            rowData.Visible = !rowData.Visible;
            SettingMgmt.saveSetting(hfSettingBase.Value + "-Visible", rowData.Visible.ToString());
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
            LinkButton ib = (LinkButton)sender;

            Session["modalshowing"] = "false";

            ModalPopupExtender mpeInfo = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeInfo");
            mpeInfo.Hide();
        }
    }
}