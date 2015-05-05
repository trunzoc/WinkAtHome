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

                if (!IsPostBack && Session["loggedin"] == null)
                {
                    if (Request.Cookies["login"] != null)
                    {
                        HttpCookie aCookie = Request.Cookies["login"];
                        if (aCookie != null)
                        {
                            string strloggedin = aCookie.Value;

                            string strDec = Common.Decrypt(strloggedin);

                            try
                            {
                                string strUser = strDec.Substring(0, strDec.IndexOf("|||"));
                                string strPass = strDec.Substring(strDec.IndexOf("|||") +3);

                                string username = Common.Decrypt(strUser);
                                string password = Common.Decrypt(strPass);

                                if (Wink.validateWinkCredentials(username, password))
                                {
                                    Session["loggedin"] = strloggedin;
                                    Session["username"] = strUser;
                                    Session["password"] = strPass;

                                    Response.Redirect("~/Default.aspx", false);
                                }
                            }
                            catch { }
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
                Wink.clearWink();
                SettingMgmt.Settings = null;

                bool validated = Wink.validateWinkCredentials(tbUsername.Text, tbPassword.Text);

                if (validated)
                {
                    Session["loggedin"] = Common.Encrypt(Common.Encrypt(tbUsername.Text) + "|||" + Common.Encrypt(tbPassword.Text));
                    Session["username"] = Common.Encrypt(tbUsername.Text);
                    Session["password"] = Common.Encrypt(tbPassword.Text);

                    if (cbRemember.Checked)
                    {
                        HttpCookie aCookie = new HttpCookie("login");
                        aCookie.Value = Session["loggedin"].ToString();
                        aCookie.Expires = DateTime.MaxValue;
                        Response.Cookies.Add(aCookie);
                    }


                    Response.Redirect("~/Default.aspx", false);
                }
            }
            catch (Exception ex)
            {
                throw ex; //EventLog.WriteEntry("WinkAtHome.Login.btnLogin_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}