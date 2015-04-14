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
            if (SettingMgmt.getSetting("winkUsername").ToLower() == "username" || SettingMgmt.getSetting("winkPassword") == "password")
            {
                rowWarning.Visible = true;
            }
            else if (Session["loggedin"] == null || SettingMgmt.getSetting("winkUsername") != Common.Decrypt(Session["loggedin"].ToString()))
            {
                Response.Redirect("~/Login.aspx");
            }
            else
            {
                rowWarning.Visible = false;
            }

            if (!IsPostBack)
            {
                dlSettings.DataSource = SettingMgmt.Settings;
                dlSettings.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
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

        protected void btnWipe_Click(object sender, EventArgs e)
        {
            SettingMgmt.wipeSettings();
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

        protected void lbControl_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Control.aspx");
        }

        protected void btnRawDevData_Click(object sender, EventArgs e)
        {
            JObject json = Wink.getDeviceJSON();
            if (json != null)
                tbEdit.Text = json.ToString();
            else
                tbEdit.Text = "There was an error getting Device JSON data";

            rowEdit.Visible = true;

        }

        protected void lbMonitor_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Monitor.aspx");
        }
    }
}