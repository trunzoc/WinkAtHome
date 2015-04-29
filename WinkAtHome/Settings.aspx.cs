using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
            {
                Common.prepareDatabase();

                if (SettingMgmt.getSetting("winkUsername").ToLower() == "username" || SettingMgmt.getSetting("winkPassword") == "password")
                {
                    lblMessage.Text = "You must set your Username and Password before you can continue.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
                else if (Session["loggedin"] == null || SettingMgmt.getSetting("winkUsername") != Common.Decrypt(Session["loggedin"].ToString()))
                {
                    Response.Redirect("~/Login.aspx");
                }

                if (!IsPostBack)
                {
                    lblVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    tbVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    tbDBPath.Text = Common.dbPath;

                    if (Request.QueryString["warning"] != null)
                    {
                        string message = Request.QueryString["warning"];
                        lblMessage.Text = message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;

                    }
                    BindData();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.Page_Load", ex.Message, EventLogEntryType.Error);
            }

        }

        public void BindData()
        {
            try
            {
                dlRequiredSettings.DataSource = SettingMgmt.Settings.Where(s => s.isRequired);
                dlRequiredSettings.DataBind();

                dlAdditionalSettings.DataSource = SettingMgmt.Settings.Where(s => !s.isRequired);
                dlAdditionalSettings.DataBind();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.BindData", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (validatePassword())
                {
                    foreach (DataListItem item in dlRequiredSettings.Items)
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
                    foreach (DataListItem item in dlAdditionalSettings.Items)
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

                BindData();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.btnSave_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnWipe_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.btnWipe_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnManualEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string strSettings = string.Empty;
                foreach (SettingMgmt.Setting setting in SettingMgmt.Settings)
                {
                    strSettings += ",\"" + setting.key + "\":\"" + setting.value + "\"\n";
                }
                strSettings = "{\n" + strSettings.Substring(1) + "}";
                tbEdit.Text = strSettings;
                rowEditText.Visible = true;
                rowEditButton.Visible = true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.btnManualEdit_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string strSettings = tbEdit.Text.Replace("\n", "").Replace("\r", "");
                SettingMgmt.saveManualEdit(strSettings);
                Response.Redirect(Request.RawUrl);

                BindData();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.btnSaveEdit_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnRawDevData_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string cmdarg = btn.CommandArgument;

                if (cmdarg == "devices")
                {
                    JObject json = Wink.Device.getDeviceJSON();
                    if (json != null)
                        tbEdit.Text = json.ToString();
                    else
                        tbEdit.Text = "There was an error getting Device JSON data";

                }
                else if (cmdarg == "robots")
                {
                    JObject json = Wink.Robot.getRobotJSON();
                    if (json != null)
                        tbEdit.Text = json.ToString();
                    else
                        tbEdit.Text = "There was an error getting Robot JSON data";

                }

                rowEditText.Visible = true;
                rowEditButton.Visible = false;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.btnRawDevData_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnDefault_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.btnDefault_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected bool validatePassword()
        {
            try
            {
                string user = null;
                string pass = null;

                foreach (DataListItem item in dlRequiredSettings.Items)
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

                if (Wink.validateWinkCredentials(user, pass))
                {
                    lblMessage.Text = "Username & Password verified";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("WinkAtHome.Settings.validatePassword", ex.Message, EventLogEntryType.Error);
                return false;
            }
        }
    }
}