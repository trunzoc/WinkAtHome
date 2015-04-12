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
                lblRefreshed.Text = DateTime.Now.ToString();
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
        }

        protected void lbDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void tmrRefresh_Tick(object sender, EventArgs e)
        {
            tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
            Wink.reloadWink();
            Response.Redirect(Request.RawUrl);
            
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

        protected void lbMonitor_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Monitor.aspx");
        }

    }
}