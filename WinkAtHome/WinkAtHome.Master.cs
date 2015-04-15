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
            if (SettingMgmt.getSetting("winkUsername", true).ToLower() == "username" || SettingMgmt.getSetting("winkPassword", true) == "password" || Session["loggedin"] == null || (Session["loggedin"] != null && SettingMgmt.getSetting("winkUsername", true) != Common.Decrypt(Session["loggedin"].ToString())))
            {
                Response.Redirect("~/Login.aspx");
            }
            
            if (!IsPostBack)
            {
                lblRefreshed.Text = DateTime.Now.ToString();
                //CHECK SECURITY/SETTINGS VALIDITY
                if (SettingMgmt.getSetting("winkUsername",true).ToLower() == "username" || SettingMgmt.getSetting("winkPassword",true).ToLower() == "password")
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

                string menustate = SettingMgmt.getSetting("Menu-Default-State");
                if (menustate != null)
                {
                    if (menustate == "hide")
                    {
                        tblCollapsed.Visible = true;
                        tblExpand.Visible = false;
                    }
                    else
                    {
                        tblCollapsed.Visible = false;
                        tblExpand.Visible = true;
                    }

                    cellMenu.BackColor = tblExpand.Visible ? System.Drawing.ColorTranslator.FromHtml("#eeeeee") : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

                }
            }
        }

        protected void lbControl_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Control.aspx");
        }

        protected void tmrRefresh_Tick(object sender, EventArgs e)
        {
            tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;

            string modalshowing = "false";
            if (Session["modalshowing"] != null)
                modalshowing = Session["modalshowing"].ToString();

            if (modalshowing == "false")
            {
                Wink.reloadWink();
                Response.Redirect(Request.RawUrl);
            }
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

        protected void ibExpand_Click(object sender, EventArgs e)
        {
            string cmdArg = string.Empty;
            if (sender is ImageButton)
            {
                ImageButton btn = (ImageButton)sender;
                cmdArg = btn.CommandArgument;
            }
            else if (sender is Button)
            {
                Button btn = (Button)sender;
                cmdArg = btn.CommandArgument;
            }
            else if (sender is LinkButton)
            {
                LinkButton btn = (LinkButton)sender;
                cmdArg = btn.CommandArgument;
            }

            if (cmdArg == "hide")
            {
                tblCollapsed.Visible = true;
                tblExpand.Visible = false;
                SettingMgmt.saveSetting("Menu-Default-State", "hide");
            }
            else
            {
                tblCollapsed.Visible = false;
                tblExpand.Visible = true;
                SettingMgmt.saveSetting("Menu-Default-State", "show");
            }

            cellMenu.BackColor = tblExpand.Visible ? System.Drawing.ColorTranslator.FromHtml("#eeeeee") : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

        }

        protected void lbSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Settings.aspx");
        }

    }
}