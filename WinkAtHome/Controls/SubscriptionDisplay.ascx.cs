using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class SubscriptionDisplay : System.Web.UI.UserControl
    {
        Wink myWink = HttpContext.Current.Session["_wink"] == null ? new Wink() : (Wink)HttpContext.Current.Session["_wink"];
        private int defaultLogLength = 50;
        
        public Int32 Height
        {
            get
            {
                object o = ViewState["Height"];
                if (o != null)
                {
                    return (Int32)o;
                }
                return 0;
            }
            set
            {
                ViewState["Height"] = value;
            }
        }
        public Int32 Width
        {
            get
            {
                object o = ViewState["Width"];
                if (o != null)
                {
                    return (Int32)o;
                }
                return 0;
            }
            set
            {
                ViewState["Width"] = value;
            }
        }

        public Int32 logLength
        {
            get
            {
                object o = ViewState["logLength"];
                if (o != null)
                {
                    return (Int32)o;
                }
                return defaultLogLength;
            }
            set
            {
                ViewState["logLength"] = value;
            }
        }
        public bool showLongLogDetail
        {
            get
            {
                object o = ViewState["showLongLogDetail"];
                if (o != null)
                {
                    return (bool)o;
                }
                return true;
            }
            set
            {
                ViewState["showLongLogDetail"] = value;
            }
        }
        public bool showFullHeader
        {
            get
            {
                object o = ViewState["showFullHeader"];
                if (o != null)
                {
                    return (bool)o;
                }
                return true;
            }
            set
            {
                ViewState["showFullHeader"] = value;
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
                    logLength = loglength;
                    tbLogLength.Text = loglength.ToString();

                    hfMainHeight.Value = Height.ToString();
                    if (Height > 0)
                        pnlEvents.Height = Height;
                    if (Width > 0)
                        pnlEvents.Width = Width;

                    rowFullHeader.Visible = showFullHeader;
                    rowShortHeader.Visible = !showFullHeader;

                    BindData();
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.SubscriptionDisplay.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        public void BindData()
        {
            dlEvents.DataSource = null;
            dlEvents.DataBind();

            List<WinkEvent> sessionList = ((List<WinkEvent>)Session["_subscriptionLog" + (showLongLogDetail ? "Long" : "Short")]).OrderByDescending(i => i.messageReceived).ToList();
            Int32 getLength = logLength > sessionList.Count ? sessionList.Count : logLength;

            if (getLength < sessionList.Count)
            {
                dlEvents.ShowHeader = true;
            }

            dlEvents.DataSource = sessionList.GetRange(0, getLength);
            dlEvents.DataBind();

            //Page.ClientScript.RegisterStartupScript(this.GetType(), "resizePanel", "setHeight()", true);
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "resizePanel", "setHeight()", true); 
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
            logLength = loglength;

            mpeSettings.Hide();
        }

        protected void ibPause_Click(object sender, ImageClickEventArgs e)
        {
            HiddenField hfPauseSubscriptions = (HiddenField)Page.Master.FindControl("hfPauseSubscriptions");

            bool paused = false;
            bool.TryParse(hfPauseSubscriptions.Value, out paused);

            paused = !paused;
            hfPauseSubscriptions.Value = paused.ToString();

            if (paused)
            {
                ibPause.ImageUrl = "~/Images/play.png";
                ibPauseShort.ImageUrl = "~/Images/play.png";
            }
            else
            {
                ibPause.ImageUrl = "~/Images/pause.png";
                ibPauseShort.ImageUrl = "~/Images/pause.png";
            }
        }

        protected void ibErase_Click(object sender, ImageClickEventArgs e)
        {
            Session["_subscriptionLogShort"] = new List<WinkEvent>();
            Session["_subscriptionLogLong"] = new List<WinkEvent>();
            BindData();
        }

        protected void ibReconnect_Click(object sender, ImageClickEventArgs e)
        {
            WinkEventHelper.storeNewSubscriptionMessage(myWink.winkUser.userID, "Subscription Event", "", "Subscriptions Refresh Started");
        
            new WinkHelper.SubscriptionHelper().refreshSubscriptions();
        }

    }
}


