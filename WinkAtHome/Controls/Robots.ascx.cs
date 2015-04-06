using System;
using System.Collections.Generic;
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
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.Parent;
            string robotID = ib.CommandArgument;

            bool newstate = !Convert.ToBoolean(ib.CommandName);

            Wink.changeRobotState(robotID, newstate);
            Response.Redirect(Request.RawUrl);
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            dlRobots.RepeatColumns = Convert.ToInt32(tbColumns.Text);
            SettingMgmt.saveSetting("Robots-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns", tbColumns.Text);
        }
    }
}