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
            if (!IsPostBack)
            {
                dlRobots.DataSource = Wink.Robots;
                dlRobots.DataBind();

                string columns = SettingMgmt.getSetting("Robots-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns");
                if (columns != null)
                {
                    tbColumns.Text = columns;
                    dlRobots.RepeatColumns = Convert.ToInt32(tbColumns.Text);
                }

                string dataVisible = SettingMgmt.getSetting("Robots-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Visible");
                if (dataVisible != null)
                {
                    bool visible = true;
                    bool.TryParse(dataVisible, out visible);
                    rowData.Visible = visible;
                }
            }
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
            }
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            dlRobots.RepeatColumns = Convert.ToInt32(tbColumns.Text);
            SettingMgmt.saveSetting("Robots-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns", tbColumns.Text);
        }

        protected void ibExpand_Click(object sender, ImageClickEventArgs e)
        {
            rowData.Visible = !rowData.Visible;
            SettingMgmt.saveSetting("Robots-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Visible", rowData.Visible.ToString());
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