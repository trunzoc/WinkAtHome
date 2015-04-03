using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class Menu : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void lbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect("~/" + lbMenu.SelectedValue + ".aspx");
        }
    }
}