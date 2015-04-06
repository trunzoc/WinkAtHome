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

        protected void lbDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void lbSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Settings.aspx");
        }

    }
}