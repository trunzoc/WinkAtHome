using System;
using System.Collections.Generic;
using System.Configuration;
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
                if (!IsPostBack)
                {
                    if (ConfigurationManager.AppSettings["Maintenance"].ToLower() == "true")
                    {
                        btnLogin.Enabled = false;
                        btnLogin.Text = "Closed for Miantenance";
                        return;
                    }
                    
                    Wink wink = Wink.myWink;

                    if (Request.QueryString["code"] != null)
                    {
                        string tempToken = Request.QueryString["code"].ToString();
                        if (wink.validateWinkCredentialsByAuthCode(tempToken))
                        {
                            Response.Redirect("~/Default.aspx", false);
                        }
                    }
                    else if (Session["_winkToken"] == null)
                    {
                        if (Request.Cookies["token"] != null)
                        {
                            HttpCookie aCookie = Request.Cookies["token"];
                            if (aCookie != null)
                            {
                                string strEncyptedToken = aCookie.Value;

                                string strDecryptedToken = Common.Decrypt(strEncyptedToken);

                                try
                                {
                                    if (Wink.myWink.validateWinkCredentialsByToken(strDecryptedToken))
                                    {
                                        Session["_winkToken"] = strDecryptedToken;

                                        Response.Redirect("~/Default.aspx", false);
                                    }
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            if (!Common.isLocalHost)
                            {
                                string url = "https://winkapi.quirky.com/oauth2/authorize?client_id=" + ConfigurationManager.AppSettings["APIClientID"];
                                Response.Redirect(url, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Login.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                bool validated = Wink.myWink.validateWinkCredentialsByUsername(tbUsername.Text, tbPassword.Text);

                if (validated)
                {
                    Session["loggedin"] = Common.Encrypt(Common.Encrypt(tbUsername.Text) + "|||" + Common.Encrypt(tbPassword.Text));
                    Session["username"] = Common.Encrypt(tbUsername.Text);
                    Session["password"] = Common.Encrypt(tbPassword.Text);

                    HttpCookie aCookie = new HttpCookie("token");
                    aCookie.Value = Session["loggedin"].ToString();
                    if (cbRemember.Checked)
                        aCookie.Expires = DateTime.Now.AddMonths(1);
                    else
                        aCookie.Expires = DateTime.Now.AddMinutes(10);

                    Response.Cookies.Add(aCookie);

                    Response.Redirect("~/Default.aspx");
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Login.btnLogin_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}