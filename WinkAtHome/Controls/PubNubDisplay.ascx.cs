using PubNubMessaging.Core;
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
    public partial class PubNubDisplay : System.Web.UI.UserControl
    {
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
        public Int32 LogLength
        {
            get
            {
                object o = ViewState["LogLength"];
                if (o != null)
                {
                    return (Int32)o;
                }
                return 15000;
            }
            set
            {
                ViewState["LogLength"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    hfSettingBase.Value = Request.RawUrl.Substring(Request.RawUrl.LastIndexOf('/') + 1) + "-PubNub-MV" + ((Table)Page.Master.FindControl("tblExpand")).Visible.ToString();

                    bool visible = true;
                    string dataVisible = SettingMgmt.getSetting(hfSettingBase.Value + "-Visible");
                    if (dataVisible != null)
                    {
                        bool.TryParse(dataVisible, out visible);
                        rowData.Visible = visible;
                        cbShow.Checked = visible;
                    }

                    string length = SettingMgmt.getSetting(hfSettingBase.Value + "-LogLength");
                    if (length != null)
                    {
                        int loglength = 15000;
                        int.TryParse(length, out loglength);
                        LogLength = loglength;
                    }
                    tbLogLength.Text = LogLength.ToString();

                    txtMessage.Height = DisplayHeight;
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.PubNubDisplay.Page_Load", ex.Message, EventLogEntryType.Error);
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
            
            int loglength = 15000;
            int.TryParse(tbLogLength.Text, out loglength);
            SettingMgmt.saveSetting(hfSettingBase.Value + "-LogLength", loglength.ToString());
            LogLength = loglength;

            mpeSettings.Hide();
        }

        protected void tmrCheckChanges_Tick(object sender, EventArgs e)
        {
            try
            {
                string modalshowing = "false";
                if (Session["modalshowing"] != null)
                    modalshowing = Session["modalshowing"].ToString();

                if (modalshowing == "false")
                {
                    string currentRecord;
                    while (PubNub.myPubNub.RecordQueue.TryDequeue(out currentRecord))
                    {
                        txtMessage.Text = currentRecord + Environment.NewLine + txtMessage.Text;
                    }

                    if (txtMessage.Text.Length > LogLength)
                    {
                        txtMessage.Text = txtMessage.Text.Substring(0, LogLength) + Environment.NewLine + "...(truncated)";
                    }
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tmrCheckChanges_Tick", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}


