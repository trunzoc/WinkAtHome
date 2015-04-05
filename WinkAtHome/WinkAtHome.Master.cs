using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome
{
    public partial class WinkAtHome : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!IsPostBack)
            {
                if (Request.UrlReferrer != null)
                {
                    string referrer = Request.FilePath;
                    referrer = referrer.Substring(referrer.LastIndexOf('/')+1);
                    referrer = referrer.Substring(0,referrer.LastIndexOf(".aspx"));

                    UserControl ucMenu = (UserControl)Page.Master.FindControl("ucMenu");
                    ListBox lbMenu = (ListBox)ucMenu.FindControl("lbMenu");
                    ListItem item = lbMenu.Items.FindByValue(referrer);
                    if (item != null)
                    {
                        lbMenu.SelectedValue = referrer;
                        lbMenu.SelectedItem.Attributes.Add("style", "background-color:#dddddd; color:#ffffff");
                    }
                    else
                    {
                        
                    }
                    lbMenu.ClearSelection();
                }
            }
        }

        protected void ibRefresh_Click(object sender, ImageClickEventArgs e)
        {
            Wink.clearWink();
            Response.Redirect(Request.RawUrl);
        }

        protected void lbDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "refresh", "clickTrigger()", true); 
        }

        protected void lbSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Settings.aspx");
        }

    }
}