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
                    if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnvironmentName"]))
                        ibVersion.Text += ConfigurationManager.AppSettings["EnvironmentName"];

                    pnlLocalLogin.Visible = Common.isLocalHost;
                    pnlWinkLogin.Visible = !Common.isLocalHost;
                    btnLogin.CommandArgument = Common.isLocalHost ? "local" : "wink";

                    if (ConfigurationManager.AppSettings["Maintenance"].ToLower() == "true")
                    {
                        btnLogin.Enabled = false;
                        btnLogin.Text = "Closed for Miantenance";
                        return;
                    }

                    if (Session["_wink"] == null)
                        Session["_wink"] = new Wink();

                    Wink wink = (Wink)Session["_wink"];

                    if (Session["_winkToken"] == null || Request.QueryString["code"] != null)
                    {
                        if (Request.QueryString["code"] != null && Request.QueryString["state"] != null && Session["userID"] != null)
                        {
                            if (Request.QueryString["state"] == Session["userID"].ToString())
                            {
                                string tempToken = Request.QueryString["code"].ToString();
                                if (new WinkHelper().validateWinkCredentialsByAuthCode(tempToken))
                                {
                                    bool remember = false;
                                    if (Session["remember"] != null)
                                        Boolean.TryParse(Session["remember"].ToString(), out remember);

                                    HttpCookie aCookie = new HttpCookie("token");
                                    aCookie.Value = Common.Encrypt(Session["_winkToken"].ToString());
                                    if (remember)
                                        aCookie.Expires = DateTime.Now.AddMonths(1);
                                    else
                                        aCookie.Expires = DateTime.Now.AddMinutes(10);

                                    Response.Cookies.Add(aCookie);
                                    new WinkHelper().reloadWink();

                                    Response.Redirect("~/Default.aspx", false);
                                    return;
                                }
                            }
                            else
                            {
                                lblMessage.Text = "There was an Error validating.  Please try again" + "<br />Session userID: " + Session["userID"].ToString() + "<br />Winked userID: " + Request.QueryString["state"];
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
                                    if (new WinkHelper().validateWinkCredentialsByToken(strDecryptedToken))
                                    {
                                        Session["_winkToken"] = strDecryptedToken;
                                        new WinkHelper().reloadWink();

                                        Response.Redirect("~/Default.aspx", false);
                                        return;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Login.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string strUserID = Guid.NewGuid().ToString();
                Session["userID"] = strUserID;

                Button btn = (Button)sender;

                Wink wink = (Wink)Session["_wink"];

                if (btn.CommandArgument == "local")
                {
                    bool validated = new WinkHelper().validateWinkCredentialsByUsername(tbUsername.Text, tbPassword.Text);

                    if (validated)
                    {
                        HttpCookie aCookie = new HttpCookie("token");
                        aCookie.Value = Common.Encrypt(Session["_winkToken"].ToString());
                        if (cbRemember.Checked)
                            aCookie.Expires = DateTime.Now.AddMonths(1);
                        else
                            aCookie.Expires = DateTime.Now.AddMinutes(10);

                        Response.Cookies.Add(aCookie);

                        new WinkHelper().reloadWink();

                        Response.Redirect("~/Default.aspx", false);
                    }
                }
                else if (btn.CommandArgument=="wink")
                {
                    Session["remember"] = cbRemember.Checked;


                    string loginURL = "https://winkapi.quirky.com/oauth2/authorize?client_id=" + ConfigurationManager.AppSettings["APIClientID"] + "&redirect_uri=" + ConfigurationManager.AppSettings["LoginRedirect"] + "&state=" + strUserID;
                    Response.Redirect(loginURL, false);
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Login.btnLogin_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}