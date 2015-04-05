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
                hfCurrentURL.Value = Request.RawUrl;

                string timerrefresh = SettingMgmt.getSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                if (timerrefresh != null)
                {
                    tbTimer.Text = timerrefresh;
                    tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
                }

                string timerenabled = SettingMgmt.getSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                if (timerenabled != null)
                {
                    rblenabled.SelectedValue = timerenabled;
                    tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
                }


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
            tmrRefresh.Interval = 500;
        }

        protected void lbDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void tmrRefresh_Tick(object sender, EventArgs e)
        {
            tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "refresh", "clickTrigger()", true); 
        }

        protected void lbSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Settings.aspx");
        }

        protected void tbTimer_TextChanged(object sender, EventArgs e)
        {
            tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
            SettingMgmt.saveSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), tbTimer.Text);
        }

        protected void rblenabled_SelectedIndexChanged(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
            SettingMgmt.saveSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), rblenabled.SelectedValue);
        }

    }
}