using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome
{
    public partial class Settings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SettingMgmt.getSetting("winkUsername",true).ToLower() == "username" || SettingMgmt.getSetting("winkPassword",true) == "password")
            {
                lblMessage.Text = "You must set your Username and Password before you can continue.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            else if (Session["loggedin"] == null || SettingMgmt.getSetting("winkUsername",true) != Common.Decrypt(Session["loggedin"].ToString()))
            {
                Response.Redirect("~/Login.aspx");
            }

            if (!IsPostBack)
            {
                lblVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                if (Request.QueryString["warning"] != null)
                {
                    string message = Request.QueryString["warning"];
                    lblMessage.Text = message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }

                dlSettings.DataSource = SettingMgmt.Settings;
                dlSettings.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (validatePassword())
            {
                foreach (DataListItem item in dlSettings.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        Label lbl = (Label)item.FindControl("lblKey");
                        TextBox tb = (TextBox)item.FindControl("tbValue");
                        HiddenField hf = (HiddenField)item.FindControl("hfValue");

                        if (tb.Text != hf.Value)
                            SettingMgmt.saveSetting(lbl.Text, tb.Text);
                    }
                }
            }
        }

        protected void btnWipe_Click(object sender, EventArgs e)
        {
            SettingMgmt.wipeSettings();

            if (Request.Cookies["login"] != null)
            {
                HttpCookie aCookie = new HttpCookie("login");
                aCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(aCookie);
            }

            Session.Abandon();

            Response.Redirect("~/Settings.aspx");
        }

        protected void btnManualEdit_Click(object sender, EventArgs e)
        {
            string strSettings = string.Empty;
            foreach(SettingMgmt.Setting setting in SettingMgmt.Settings)
            {
                strSettings += ",\"" + setting.key + "\":\"" + setting.value + "\"\n";
            }
            strSettings = "{\n" + strSettings.Substring(1) + "}";
            tbEdit.Text = strSettings;
            rowEdit.Visible = true;
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            string strSettings = tbEdit.Text.Replace("\n", "").Replace("\r", "");
            SettingMgmt.saveManualEdit(strSettings);
            Response.Redirect(Request.RawUrl);
        }

        protected void btnRawDevData_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string cmdarg = btn.CommandArgument;

            if (cmdarg == "devices")
            {
                JObject json = Wink.getDeviceJSON();
                if (json != null)
                    tbEdit.Text = json.ToString();
                else
                    tbEdit.Text = "There was an error getting Device JSON data";

                rowEdit.Visible = true;
            }
            else if (cmdarg == "robots")
            {
                JObject json = Wink.getRobotJSON();
                if (json != null)
                    tbEdit.Text = json.ToString();
                else
                    tbEdit.Text = "There was an error getting Device JSON data";

                rowEdit.Visible = true;
            }
        }

        protected void btnDefault_Click(object sender, EventArgs e)
        {
            if (validatePassword())
            {
                string strPage = string.Empty;
                if (sender is ImageButton)
                {
                    ImageButton btn = (ImageButton)sender;
                    strPage = btn.CommandArgument;
                }
                else if (sender is Button)
                {
                    Button btn = (Button)sender;
                    strPage = btn.CommandArgument;
                }
                else if (sender is LinkButton)
                {
                    LinkButton btn = (LinkButton)sender;
                    strPage = btn.CommandArgument;
                }
                else
                    strPage = "~/Default.aspx";

                Response.Redirect(strPage);
            }
        }

        protected bool validatePassword()
        {
            string user = null;
            string pass = null;

            foreach (DataListItem item in dlSettings.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    Label lblKey = (Label)item.FindControl("lblKey");
                    if (lblKey != null)
                    {
                        TextBox tbValue = (TextBox)item.FindControl("tbValue");

                        if (lblKey.Text.ToLower() == "winkusername")
                            user = tbValue.Text;
                        if (lblKey.Text.ToLower() == "winkpassword")
                            pass = tbValue.Text;
                    }
                }
            }

            string strToken = Wink.winkGetToken(true,user,pass);
            lblMessage.Text = "Username & Password verified";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            return true;
        }
    }
}