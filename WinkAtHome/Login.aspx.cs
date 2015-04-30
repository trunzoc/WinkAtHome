using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Common.prepareDatabase();

                if (SettingMgmt.getSetting("winkUsername").ToLower() == "username" || SettingMgmt.getSetting("winkPassword").ToLower() == "password")
                {
                    Response.Redirect("~/Settings.aspx");
                }
                else if (Request.Cookies["login"] != null)
                {
                    HttpCookie aCookie = Request.Cookies["login"];
                    if (aCookie != null)
                    {
                        if (SettingMgmt.getSetting("winkUsername") == Common.Decrypt(aCookie.Value))
                        {
                            Session["loggedin"] = aCookie.Value;
                            Response.Redirect("~/Default.aspx");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Login.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbUsername.Text.ToLower() == SettingMgmt.getSetting("winkUsername").ToLower() && tbPassword.Text == SettingMgmt.getSetting("winkPassword"))
                {
                    Session["loggedin"] = Common.Encrypt(SettingMgmt.getSetting("winkUsername"));

                    if (cbRemember.Checked)
                    {
                        HttpCookie aCookie = new HttpCookie("login");
                        aCookie.Value = Session["loggedin"].ToString();
                        aCookie.Expires = DateTime.MaxValue;
                        Response.Cookies.Add(aCookie);
                    }

                    Response.Redirect("~/Default.aspx");
                }
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Login.btnLogin_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}