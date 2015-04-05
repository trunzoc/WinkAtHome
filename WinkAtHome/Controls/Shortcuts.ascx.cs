using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class Shortcuts : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dlShortcuts.DataSource = Wink.Shortcuts;
                dlShortcuts.DataBind();
                
                string columns = SettingMgmt.getSetting("Shortcuts-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns");
                if (columns != null)
                    tbColumns.Text = columns;
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.Parent;
            string shortcutID = ib.CommandArgument;

            Wink.activateShortcut(shortcutID);
            Response.Redirect(Request.RawUrl);
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            SettingMgmt.saveSetting("Shortcuts-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns", tbColumns.Text);
        }
    }
}