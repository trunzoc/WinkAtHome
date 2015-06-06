using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Configuration;
using System.Data.SQLite;

namespace WinkAtHome
{
    public partial class WinkAtHome : System.Web.UI.MasterPage 
    {
        Wink myWink = HttpContext.Current.Session["_wink"] == null ? new Wink() : (Wink)HttpContext.Current.Session["_wink"];
        private static int PubSubCleanupInterval = 1;

        List<WinkEvent> deviceEvents = new List<WinkEvent>();
        List<WinkEvent> groupEvents = new List<WinkEvent>();
        List<WinkEvent> shortcutEvents = new List<WinkEvent>();
        List<WinkEvent> robotEvents = new List<WinkEvent>();
        List<WinkEvent> subscriptionMessagesLong = new List<WinkEvent>();
        List<WinkEvent> subscriptionMessagesShort = new List<WinkEvent>();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Session["_winkToken"] == null || Session["_wink"] == null)
                    Response.Redirect("~/Login.aspx");

                if (!IsPostBack)
                {

                    //CHECK LOCAL VERSION FOR UPDATES
                    ibVersion.Text = Common.currentVersion;
                    if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnvironmentName"]))
                    {
                        ibVersion.Text += "\r\n" + ConfigurationManager.AppSettings["EnvironmentName"];
                    }

                    if (Common.isLocalHost)
                    {
                        bool hasUpdate = Common.checkForUpdate();
                        if (hasUpdate)
                        {
                            ibVersion.Text = "UPDATE AVAILABLE!";
                            ibVersion.Enabled = true;
                            lblCurrentVersion.Text = Common.currentVersion;
                            lblNewVersion.Text = Common.newVersion;
                            tbReleaseNotes.Text = Common.updateNotes;
                            hlDownloadUpdate.NavigateUrl = Common.updateFilePath;
                            mpeUpdate.Show();
                        }
                    }

                    tbRefreshed.Text = Common.getLocalTime().ToString();

                    //SET PAGE OPTIONS
                    string timerrefresh = SettingMgmt.getSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                    if (timerrefresh != null)
                    {
                        tbTimer.Text = timerrefresh;
                        tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
                    }

                    string menustate = SettingMgmt.getSetting("Menu-Default-State");
                    if (menustate != null)
                    {
                        if (menustate == "hide")
                        {
                            tblCollapsed.Visible = true;
                            tblExpand.Visible = false;
                        }
                        else
                        {
                            tblCollapsed.Visible = false;
                            tblExpand.Visible = true;
                        }

                        cellMenu.BackColor = tblExpand.Visible ? System.Drawing.ColorTranslator.FromHtml("#eeeeee") : System.Drawing.ColorTranslator.FromHtml("#22b9ec");
                    }

                    //SUBSCRIPTION STUFF
                    if (Session["_subscriptionLogShort"] == null)
                        Session["_subscriptionLogShort"] = new List<WinkEvent>();
                    if (Session["_subscriptionLogLong"] == null)
                        Session["_subscriptionLogLong"] = new List<WinkEvent>();

                    hfSubscriptionCallbackURL.Value = ConfigurationManager.AppSettings["SubscriptionCallbackURL"] + "MessageCheck/" + myWink.winkUser.userID + "/" + Session.SessionID;
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void ibSettingsClose_Click(object sender, EventArgs e)
        {
            //tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
            //SettingMgmt.saveSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), rblenabled.SelectedValue);
                
            tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
            SettingMgmt.saveSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), tbTimer.Text);
        }
        public void lbLogout_Click(object sender, EventArgs e)
        {
            try
            {
                if (Request.Cookies["token"] != null)
                {
                    HttpCookie aCookie = new HttpCookie("token");
                    aCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(aCookie);
                }

                Session.Abandon();

                new WinkHelper().clearWink();
                SettingMgmt.Settings = null;

                Response.Redirect("~/Login.aspx");
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.lbLogout_Click", ex.Message, EventLogEntryType.Error);
            }
        }
        protected void ibExpand_Click(object sender, EventArgs e)
        {
            try
            {
                string cmdArg = string.Empty;
                if (sender is ImageButton)
                {
                    ImageButton btn = (ImageButton)sender;
                    cmdArg = btn.CommandArgument;
                }
                else if (sender is Button)
                {
                    Button btn = (Button)sender;
                    cmdArg = btn.CommandArgument;
                }
                else if (sender is LinkButton)
                {
                    LinkButton btn = (LinkButton)sender;
                    cmdArg = btn.CommandArgument;
                }

                if (cmdArg == "hide")
                {
                    tblCollapsed.Visible = true;
                    tblExpand.Visible = false;
                    SettingMgmt.saveSetting("Menu-Default-State", "hide");
                }
                else
                {
                    tblCollapsed.Visible = false;
                    tblExpand.Visible = true;
                    SettingMgmt.saveSetting("Menu-Default-State", "show");
                }

                cellMenu.BackColor = tblExpand.Visible ? System.Drawing.ColorTranslator.FromHtml("#eeeeee") : System.Drawing.ColorTranslator.FromHtml("#22b9ec");

                Response.Redirect(Request.RawUrl);
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.ibExpand_Click", ex.Message, EventLogEntryType.Error);
            }
        }
        protected void ibUpdateClose_Click(object sender, EventArgs e)
        {
            Session["modalshowing"] = "false";

            mpeUpdate.Hide();
        }
        protected void ibVersion_Click(object sender, EventArgs e)
        {
            Session["modalshowing"] = "true";

            mpeUpdate.Show();
        }

        protected void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;

                string modalshowing = "false";
                if (Session["modalshowing"] != null)
                    modalshowing = Session["modalshowing"].ToString();

                if (modalshowing == "false")
                {
                    new WinkHelper().reloadWink();
                    Response.Redirect(Request.RawUrl);
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tmrRefresh_Tick", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnSubscriptions_Click(object sender, EventArgs e)
        {
            handleSubscriptions();
        }
         protected void handleSubscriptions()
        {
            try
            {
                string modalshowing = "false";
                if (Session["modalshowing"] != null)
                    modalshowing = Session["modalshowing"].ToString();

                if (modalshowing == "false")
                {
                    List<WinkEvent> events = WinkEventHelper.getSubscriptionMessages(myWink.winkUser.userID, Session.SessionID);

                    //SORT OBJECT TYPES
                    if (events.Count > 0)
                    {
                        foreach (WinkEvent winkevent in events)
                        {
                            if (winkevent != null)
                            {
                                string type = winkevent.objectType.ToLower();
                                if (type == "group")
                                    groupEvents.Add(winkevent);
                                else if (type == "shortcut")
                                    shortcutEvents.Add(winkevent);
                                else if (type == "robot")
                                    robotEvents.Add(winkevent);
                                else if (type.Contains("device-"))
                                    deviceEvents.Add(winkevent);
                                else
                                {
                                    subscriptionMessagesLong.Add(winkevent);
                                    subscriptionMessagesShort.Add(winkevent);
                                }
                            }
                        }

                        string[] markAsRead = events.Select(c => c.messageID).ToArray();
                        WinkEventHelper.markSubscriptionMessagesRead(markAsRead, Session.SessionID);

                        foreach (WinkEvent winkevent in deviceEvents)
                        {
                            myWink.DeviceHasChanges = true;
                            Wink.Device device = myWink.Devices.SingleOrDefault(d => d.id == winkevent.objectID);

                            string deviceResult = winkevent.text;
                            deviceResult = "{\"data\": [" + deviceResult + "]}";
                            JObject currentJSON = JObject.Parse(deviceResult);
                            new WinkHelper.DeviceHelper().DeviceUpdate(currentJSON);

                            string newStatus = string.Empty;
                            foreach (var state in device.desired_states)
                            {
                                newStatus += ", " + state.Key + ": " + state.Value;
                            }

                            if (newStatus.Length > 2)
                                newStatus = newStatus.Substring(2);

                            WinkEvent sublongevent = new WinkEvent();
                            sublongevent.messageID = Guid.NewGuid().ToString();
                            sublongevent.messageReceived = winkevent.messageReceived;
                            sublongevent.userID = winkevent.userID;
                            sublongevent.objectType = device.displayName;
                            sublongevent.text = winkevent.text;

                            subscriptionMessagesLong.Add(sublongevent);

                            WinkEvent subshortevent = new WinkEvent();
                            subshortevent.messageID = Guid.NewGuid().ToString();
                            subshortevent.messageReceived = winkevent.messageReceived;
                            subshortevent.userID = winkevent.userID;
                            subshortevent.objectType = device.displayName;
                            subshortevent.text = newStatus;

                            subscriptionMessagesShort.Add(subshortevent);
                        }
                    }

                    updateAllMasterPanels();
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tmrCheckChanges_Tick", ex.Message, EventLogEntryType.Error);
            }
        }

        public void updateAllMasterPanels(bool updateDevices = false, bool updateGroups = false, bool updateRobots = false, bool updateShortcuts = false)
        {
            try
            {
                #region objects
                //UPDATE DEVICES
                if (myWink.DeviceHasChanges || updateDevices)
                {
                    myWink.DeviceHasChanges = false;

                    //UPDATE DEVICES PANEL
                    UserControl ucDevices = (UserControl)cphMain.FindControl("ucDevices");
                    if (ucDevices != null)
                    {
                        var control = ucDevices as Controls.Devices;
                        control.BindData();

                        UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                        upData.Update();
                    }

                    //UPDATE SENSORS PANEL
                    UserControl ucSensors = (UserControl)cphMain.FindControl("ucSensors");
                    if (ucSensors != null)
                    {
                        var control = ucSensors as Controls.Devices;
                        control.BindData();

                        UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                        upData.Update();
                    }
                }

                //UPDATE GROUPS
                if (myWink.GroupHasChanges || updateGroups)
                {
                    myWink.GroupHasChanges = false;

                    UserControl ucGroups = (UserControl)cphMain.FindControl("ucGroups");
                    if (ucGroups != null)
                    {
                        var control = ucGroups as Controls.Groups;
                        //control.BindData();

                        UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                        upData.Update();
                    }
                }

                //UPDATE ROBOTS
                if (myWink.RobotHasChanges || updateRobots)
                {
                    myWink.RobotHasChanges = false;

                    UserControl ucRobots = (UserControl)cphMain.FindControl("ucRobots");
                    if (ucRobots != null)
                    {
                        var control = ucRobots as Controls.Robots;
                        control.BindData();

                        UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                        upData.Update();
                    }
                }

                //UPDATE SHORTCUTS
                if (myWink.ShortcutHasChanges || updateShortcuts)
                {
                    myWink.ShortcutHasChanges = false;

                    UserControl ucShortcuts = (UserControl)cphMain.FindControl("ucShortcuts");
                    if (ucShortcuts != null)
                    {
                        var control = ucShortcuts as Controls.Shortcuts;
                        control.BindData();

                        UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                        upData.Update();
                    }
                }
                #endregion

                //UPDATE FULL SUBSCRIPTION DISPLAYS
                if (subscriptionMessagesLong.Count > 0)
                {
                    List<WinkEvent> sessionList = (List<WinkEvent>)Session["_subscriptionLogLong"];

                    foreach (WinkEvent winkevent in subscriptionMessagesLong)
                    {
                        sessionList.Insert(0, winkevent);
                    }

                    if (sessionList.Count > 500)
                        sessionList.RemoveRange(500, sessionList.Count - 500);

                    Session["_subscriptionLogLong"] = sessionList;
                }
                if (subscriptionMessagesShort.Count > 0)
                {
                    List<WinkEvent> sessionList = (List<WinkEvent>)Session["_subscriptionLogShort"];

                    foreach (WinkEvent winkevent in subscriptionMessagesShort)
                    {
                        sessionList.Insert(0, winkevent);
                    }

                    if (sessionList.Count > 500)
                        sessionList.RemoveRange(500, sessionList.Count - 500);

                    Session["_subscriptionLogShort"] = sessionList;
                }

                UserControl ucSubscription = (UserControl)cphMain.FindControl("ucSubscription");
                if (ucSubscription != null)
                {
                    DataList dlEvents = (DataList)ucSubscription.FindControl("dlEvents");
                    if (subscriptionMessagesLong.Count > 0 || dlEvents.Items.Count == 0)
                    {
                        var control = ucSubscription as Controls.SubscriptionDisplay;
                        control.BindData();

                        UpdatePanel upData = (UpdatePanel)ucSubscription.FindControl("upData");
                        upData.Update();
                    }
                }

                UserControl ucSubscriptionLeft = (UserControl)cphLeft.FindControl("ucSubscriptionLeft");
                if (ucSubscriptionLeft != null)
                {
                    DataList dlEvents = (DataList)ucSubscriptionLeft.FindControl("dlEvents");
                    if (subscriptionMessagesLong.Count > 0 || dlEvents.Items.Count == 0)
                    {
                        var control = ucSubscriptionLeft as Controls.SubscriptionDisplay;
                        control.BindData();

                        UpdatePanel upData = (UpdatePanel)ucSubscriptionLeft.FindControl("upData");
                        upData.Update();
                    }
                }

                upRefresh.Update();
                tbRefreshed.Text = Common.getLocalTime().ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}