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
                rptSettings.DataSource = SettingMgmt.Settings;
                rptSettings.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            foreach (RepeaterItem item in rptSettings.Items)
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
}