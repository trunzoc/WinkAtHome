using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace WinkAtHome
{
    public partial class WinkAtHome : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loggedin"] == null || SettingMgmt.getSetting("winkUsername") != Common.Decrypt(Session["loggedin"].ToString()))
            {
                Response.Redirect("~/Login.aspx");
            }

            if (!IsPostBack)
            {
                //CHECK SECURITY/SETTINGS VALIDITY
                if (SettingMgmt.getSetting("winkUsername").ToLower() == "username" || SettingMgmt.getSetting("winkPassword").ToLower() == "password")
                    HttpContext.Current.Response.Redirect("~/Settings.aspx");

                //SET PAGE OPTIONS
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
            }

            //SELECT REFERRING MENU ITEM
            if (Request.CurrentExecutionFilePath != null)
            {
                string referrer = Request.CurrentExecutionFilePath;
                referrer = referrer.Substring(referrer.LastIndexOf('/') + 1);
                referrer = referrer.Substring(0, referrer.LastIndexOf(".aspx"));

                UserControl ucMenu = (UserControl)Page.Master.FindControl("ucMenu");
                RadMenu lbMenu = (RadMenu)ucMenu.FindControl("RadMenu1");
                RadMenuItem item = lbMenu.Items.FindItemByValue(referrer.ToLower());
                if (item != null)
                {
                    item.Selected = true;
                }
                else
                {
                    lbMenu.ClearSelectedItem();
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

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            if (Request.Cookies["login"] != null)
            {
                HttpCookie aCookie = new HttpCookie("login");
                aCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(aCookie);
            }

            Session.Abandon();

            Response.Redirect("~/Login.aspx");
        }

    }
}