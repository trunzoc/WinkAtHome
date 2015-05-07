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
                    
                    AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
                    System.Web.UI.Timer tmrCheckChanges = (System.Web.UI.Timer)Page.Master.FindControl("tmrCheckChanges");
                    if (tmrCheckChanges != null && visible)
                    {
                        trigger.ControlID = tmrCheckChanges.ID;
                        UpdatePanelPubNub.Triggers.Add(trigger);
                    }


                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.PubNubDisplay.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        public void UpdateResultView()
        {
            try
            {
                string recordTest;
                if (PubNub.RecordQueue.TryPeek(out recordTest))
                {
                    if (txtMessage.Text.Length > 10000)
                    {
                        string trucatedMessage = "..(truncated)..." + txtMessage.Text.Substring(txtMessage.Text.Length - 9000);
                        txtMessage.Text = trucatedMessage;
                    }

                    string currentRecord;
                    while (PubNub.RecordQueue.TryDequeue(out currentRecord))
                    {
                        txtMessage.Text += string.Format("{0}{1}", currentRecord, Environment.NewLine);
                        System.Diagnostics.Debug.WriteLine(currentRecord);
                    }
                }

                UpdatePanelPubNub.Update();
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.PubNubDisplay.UpdateResultView", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void ibSettings_Click(object sender, ImageClickEventArgs e)
        {
            Session["modalshowing"] = "true";

            mpeSettings.Show();
        }

        protected void btnSettingsClose_Click(object sender, EventArgs e)
        {
            Session["modalshowing"] = "false";

            rowData.Visible = cbShow.Checked;
            SettingMgmt.saveSetting(hfSettingBase.Value + "-Visible", cbShow.Checked.ToString());

            mpeSettings.Hide();
        }
    }
}