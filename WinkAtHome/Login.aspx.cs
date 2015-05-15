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
                    pnlLocalLogin.Visible = Common.isLocalHost;
                    pnlWinkLogin.Visible = !Common.isLocalHost;
                    btnLogin.CommandArgument = Common.isLocalHost ? "local" : "wink";

                    if (ConfigurationManager.AppSettings["Maintenance"].ToLower() == "true")
                    {
                        btnLogin.Enabled = false;
                        btnLogin.Text = "Closed for Miantenance";
                        return;
                    }

                    Wink wink = Wink.myWink;

                    if (Session["_winkToken"] == null || Request.QueryString["code"] != null)
                    {
                        if (Request.QueryString["code"] != null)
                        {
                            string tempToken = Request.QueryString["code"].ToString();
                            if (wink.validateWinkCredentialsByAuthCode(tempToken))
                            {
                                Response.Redirect("~/Default.aspx", false);
                                return;
                            }
                        }
                        else if (Request.Cookies["token"] != null)
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
                                        return;
                                    }
                                }
                                catch { }
                            }
                        }
                        else if (!Common.isLocalHost)
                        {
                            string loginURL = "https://winkapi.quirky.com/oauth2/authorize?client_id=" + ConfigurationManager.AppSettings["APIClientID"] + "&redirect_uri=" + ConfigurationManager.AppSettings["LoginRedirect"];
                            Response.Redirect(loginURL, false);
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
                Button btn = (Button)sender;

                Wink wink = Wink.myWink;

                if (btn.CommandArgument == "local")
                {
                    bool validated = wink.validateWinkCredentialsByUsername(tbUsername.Text, tbPassword.Text);

                    if (validated)
                    {
                        HttpCookie aCookie = new HttpCookie("token");
                        aCookie.Value = Common.Encrypt(Session["_winkToken"].ToString());
                        if (cbRemember.Checked)
                            aCookie.Expires = DateTime.Now.AddMonths(1);
                        else
                            aCookie.Expires = DateTime.Now.AddMinutes(10);

                        Response.Cookies.Add(aCookie);

                        Response.Redirect("~/Default.aspx", false);
                    }
                }
                else if (btn.CommandArgument=="wink")
                {
                    string loginURL = "https://winkapi.quirky.com/oauth2/authorize?client_id=" + ConfigurationManager.AppSettings["APIClientID"] + "&redirect_uri=" + ConfigurationManager.AppSettings["LoginRedirect"];
                    Response.Redirect(loginURL, false);
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Login.btnLogin_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}