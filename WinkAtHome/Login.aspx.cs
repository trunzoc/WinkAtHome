using System;
using System.Collections.Generic;
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
            if (SettingMgmt.getSetting("winkUsername",true).ToLower() == "username" || SettingMgmt.getSetting("winkPassword", true).ToLower() == "password")
            {
                Response.Redirect("~/Settings.aspx");
            }
            else if (Request.Cookies["login"] != null)
            {
                HttpCookie aCookie = Request.Cookies["login"];
                if (aCookie != null)
                {
                    if (SettingMgmt.getSetting("winkUsername",true) == Common.Decrypt(aCookie.Value))
                    {
                        Session["loggedin"] = aCookie.Value;
                        Response.Redirect("~/Default.aspx");
                    }
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (tbUsername.Text.ToLower() == SettingMgmt.getSetting("winkUsername",true).ToLower() && tbPassword.Text == SettingMgmt.getSetting("winkPassword",true))
            {
                Session["loggedin"] = Common.Encrypt(SettingMgmt.getSetting("winkUsername",true));

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
    }
}