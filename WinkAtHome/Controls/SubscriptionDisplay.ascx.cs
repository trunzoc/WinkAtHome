using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class SubscriptionDisplay : System.Web.UI.UserControl
    {
        private int defaultLogLength = 50;
        
        public Int32 DisplayHeight
        {
            get
            {
                object o = ViewState["DisplayHeight"];
                if (o != null)
                {
                    return (Int32)o;
                }
                return 500;
            }
            set
            {
                ViewState["DisplayHeight"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    hfSettingBase.Value = Request.RawUrl.Substring(Request.RawUrl.LastIndexOf('/') + 1) + "-Subscription-MV" + ((Table)Page.Master.FindControl("tblExpand")).Visible.ToString();

                    bool visible = true;
                    string dataVisible = SettingMgmt.getSetting(hfSettingBase.Value + "-Visible");
                    if (dataVisible != null)
                    {
                        bool.TryParse(dataVisible, out visible);
                        rowData.Visible = visible;
                        cbShow.Checked = visible;
                    }

                    int loglength = defaultLogLength;
                    string length = SettingMgmt.getSetting(hfSettingBase.Value + "-LogLength");
                    if (length != null)
                    {
                        int.TryParse(length, out loglength);
                    }
                    hfLogLength.Value = loglength.ToString();
                    tbLogLength.Text = hfLogLength.Value;

                    txtMessage.Height = DisplayHeight;

                    if (Session["_subscriptionLog"] == null)
                        Session["_subscriptionLog"] = new List<string>();
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.SubscriptionDisplay.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void ibSettings_Click(object sender, ImageClickEventArgs e)
        {
            Session["modalshowing"] = "true";

            mpeSettings.Show();
        }

        protected void ibSettingsClose_Click(object sender, EventArgs e)
        {
            Session["modalshowing"] = "false";

            rowData.Visible = cbShow.Checked;
            SettingMgmt.saveSetting(hfSettingBase.Value + "-Visible", cbShow.Checked.ToString());

            int loglength = defaultLogLength;
            int.TryParse(tbLogLength.Text, out loglength);
            SettingMgmt.saveSetting(hfSettingBase.Value + "-LogLength", loglength.ToString());
            hfLogLength.Value = loglength.ToString();

            mpeSettings.Hide();
        }

        protected void ibPause_Click(object sender, ImageClickEventArgs e)
        {
            System.Web.UI.Timer tmrSubscriptions = (System.Web.UI.Timer)Page.Master.FindControl("tmrSubscriptions");

            tmrSubscriptions.Enabled = !tmrSubscriptions.Enabled;

            if (tmrSubscriptions.Enabled)
                ibPause.ImageUrl = "~/Images/pause.png";
            else
                ibPause.ImageUrl = "~/Images/play.png";
        }

        protected void ibErase_Click(object sender, ImageClickEventArgs e)
        {
            txtMessage.Text = "";
            Session["_subscriptionLog"] = string.Empty;
        }

        protected void ibReconnect_Click(object sender, ImageClickEventArgs e)
        {
            new WinkHelper.SubscriptionHelper().refreshSubscriptions();
        }
    }
}


