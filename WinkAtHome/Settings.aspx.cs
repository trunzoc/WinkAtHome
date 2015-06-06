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

                if (Session["_winkToken"] == null)
                {
                    Response.Redirect("~/Login.aspx", false);
                }

                if (!IsPostBack)
                {
                    tbVersion.Text = Common.currentVersion;
                    tbDBPath.Text = Common.isLocalHost ? Common.dbPath : "Shhh.  It's a secret";
                    tbAccessToken.Text = Session["_winkToken"].ToString();

                    if (Request.QueryString["warning"] != null)
                    {
                        string message = Request.QueryString["warning"];
                        lblMessage.Text = message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                    BindData();
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.Page_Load", ex.Message, EventLogEntryType.Error);
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
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.BindData", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
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

                BindData();

                lblMessage.Text = "Settings Saved Succesfully";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception)
            {
                lblMessage.Text = "Settings Were Not Saved";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.btnSave_Click", ex.Message, EventLogEntryType.Error);
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
                Response.Redirect("~/Login.aspx");
            }
            catch (Exception)
            {
                lblMessage.Text = "Settings Were Not Wiped";
                lblMessage.ForeColor = System.Drawing.Color.Red;

                throw; //EventLog.WriteEntry("WinkAtHome.Settings.btnWipe_Click", ex.Message, EventLogEntryType.Error);
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
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.btnManualEdit_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string strSettings = tbEdit.Text.Replace("\n", "").Replace("\r", "");
                SettingMgmt.saveManualEdit(strSettings);

                BindData();

                lblMessage.Text = "Settings Saved Succesfully";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception)
            {
                lblMessage.Text = "Settings Were Not Saved";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.btnSaveEdit_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnRawDevData_Click(object sender, EventArgs e)
        {
            try
            {
                Wink myWink = (Wink)Session["_wink"];

                Button btn = (Button)sender;
                string cmdarg = btn.CommandArgument;

                if (cmdarg == "devices")
                {
                    JObject json = new WinkHelper.DeviceHelper().DeviceGetJSON();
                    if (json != null)
                        tbEdit.Text = json.ToString();
                    else
                        tbEdit.Text = "There was an error getting Device JSON data";

                }
                else if (cmdarg == "robots")
                {
                    JObject json = new WinkHelper.RobotHelper().RobotGetJSON();
                    if (json != null)
                        tbEdit.Text = json.ToString();
                    else
                        tbEdit.Text = "There was an error getting Robot JSON data";

                }
                else if (cmdarg == "groups")
                {
                    JObject json = new WinkHelper.GroupHelper().GroupGetJSON();
                    if (json != null)
                        tbEdit.Text = json.ToString();
                    else
                        tbEdit.Text = "There was an error getting Group JSON data";

                }
                else if (cmdarg == "shortcuts")
                {
                    JObject json = new WinkHelper.ShortcutHelper().ShortcutGetJSON();
                    if (json != null)
                        tbEdit.Text = json.ToString();
                    else
                        tbEdit.Text = "There was an error getting Group JSON data";

                }
                rowEditText.Visible = true;
                rowEditButton.Visible = false;
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.btnRawDevData_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnDefault_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Settings.btnDefault_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}