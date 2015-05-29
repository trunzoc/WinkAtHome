using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Reflection;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Configuration;

namespace WinkAtHome
{
    public partial class WinkAtHome : System.Web.UI.MasterPage 
    {
        Wink myWink = HttpContext.Current.Session["_wink"] == null ? new Wink() : (Wink)HttpContext.Current.Session["_wink"];
        List<WinkEvent> deviceEvents = new List<WinkEvent>();
        List<WinkEvent> groupEvents = new List<WinkEvent>();
        List<WinkEvent> shortcutEvents = new List<WinkEvent>();
        List<WinkEvent> robotEvents = new List<WinkEvent>();
        List<string> subscriptionMessages = new List<string>();
        public List<KeyValuePair<Guid, DateTime>> completedEvents
        {
            get
            {
                object o = Session["completedEvents"];
                if (o != null)
                {
                    return (List<KeyValuePair<Guid, DateTime>>)o;
                }
                Session["completedEvents"] = new List<KeyValuePair<Guid, DateTime>>();
                return (List<KeyValuePair<Guid, DateTime>>)Session["completedEvents"];
            }
            set
            {
                Session["completedEvents"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Session["_winkToken"] == null || Session["_wink"] == null)
                    Response.Redirect("~/Login.aspx");

                if (!IsPostBack)
                {
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

                    lblRefreshed.Text = Common.getLocalTime().ToString();

                    //SET PAGE OPTIONS
                    string timerrefresh = SettingMgmt.getSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                    if (timerrefresh != null)
                    {
                        tbTimer.Text = timerrefresh;
                        tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
                    }

                    //string timerenabled = SettingMgmt.getSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                    //if (timerenabled != null)
                    //{
                    //    rblenabled.SelectedValue = timerenabled;
                    //    tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
                    //}

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
                }
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tmrRefresh_Tick", ex.Message, EventLogEntryType.Error);
            }

        }

        protected void tmrSubscriptions_Tick(object sender, EventArgs e)
        {
            try
            {
                string modalshowing = "false";
                if (Session["modalshowing"] != null)
                    modalshowing = Session["modalshowing"].ToString();

                if (modalshowing == "false")
                {
                    //SORT OBJECT TYPES
                    if (WinkEventService.MessageQueue.Count > 0)
                    {
                        Dictionary<Guid, WinkEvent> events = WinkEventService.MessageQueue.Where(q => q.Value.userID == myWink.winkUser.userID).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        if (events.Count > 0)
                        {
                            foreach (KeyValuePair<Guid, WinkEvent> pair in events)
                            {
                                KeyValuePair<Guid, DateTime> thisone = completedEvents.SingleOrDefault(d => d.Key == pair.Key);
                                if (thisone.Key == new Guid())
                                {
                                    WinkEvent winkevent;
                                    WinkEventService.MessageQueue.TryGetValue(pair.Key, out winkevent);
                                    if (winkevent != null)
                                    {
                                        completedEvents.Add(new KeyValuePair<Guid, DateTime>(pair.Key, DateTime.Now));
                                        winkevent.messageReceived = Common.getLocalTime(winkevent.messageReceived);
                                        if (winkevent.objectType.ToLower().Contains("action:"))
                                        {
                                            subscriptionMessages.Add(winkevent.objectType + ": " + winkevent.messageReceived.ToString() + Environment.NewLine + winkevent.json);
                                        }
                                        else
                                        {
                                            switch (winkevent.objectType)
                                            {
                                                case "group":
                                                    groupEvents.Add(winkevent);
                                                    break;
                                                case "shortcut":
                                                    shortcutEvents.Add(winkevent);
                                                    break;
                                                case "robot":
                                                    robotEvents.Add(winkevent);
                                                    break;
                                                default:
                                                    deviceEvents.Add(winkevent);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (WinkEvent winkevent in deviceEvents)
                            {
                                myWink.DeviceHasChanges = true;
                                Wink.Device device = myWink.Devices.SingleOrDefault(d => d.id == winkevent.objectID);

                                subscriptionMessages.Add("DEVICE ACTION (" + device.name + "): " + winkevent.messageReceived.ToString() + Environment.NewLine + winkevent.json);

                                string deviceResult = winkevent.json;
                                deviceResult = "{\"data\": [" + deviceResult + "]}";
                                JObject currentJSON = JObject.Parse(deviceResult);
                                new WinkHelper.DeviceHelper().DeviceUpdate(currentJSON);
                                break;
                            }
                            //while (mySubscriptions.ShortcutQueue.TryDequeue(out currentJSON))
                            //{
                            //    myWink.ShortcutHasChanges = true;
                            //    new WinkHelper.ShortcutHelper().ShortcutUpdate(currentJSON);
                            //}
                            //while (mySubscriptions.GroupQueue.TryDequeue(out currentJSON))
                            //{
                            //    myWink.GroupHasChanges = true;
                            //    new WinkHelper.GroupHelper().GroupUpdate(currentJSON);
                            //}
                            //while (mySubscriptions.RobotQueue.TryDequeue(out currentJSON))
                            //{
                            //    myWink.RobotHasChanges = true;
                            //    new WinkHelper.RobotHelper().RobotUpdate(currentJSON);
                            //}

                            //if (mySubscriptions.MessageQueue.Count > 0)
                            //{
                            //}

                            updateAllMasterPanels();
                        }
                    }

                    List<KeyValuePair<Guid, DateTime>> deleteUs = completedEvents.Where(c => c.Value < DateTime.Now.AddSeconds(-30)).ToList();
                    foreach (KeyValuePair<Guid, DateTime> pair in deleteUs)
                    {
                        WinkEvent winkevent;
                        WinkEventService.MessageQueue.TryRemove(pair.Key, out winkevent);
                        completedEvents.Remove(pair);
                    }
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tmrCheckChanges_Tick", ex.Message, EventLogEntryType.Error);
            }
        }

        public void updateAllMasterPanels(bool updateDevices = false, bool updateGroups = false, bool updateRobots = false, bool updateShortcuts = false)
        {
            try
            {
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

                //UPDATE SUBSCRIPTION MESSAGES
                UserControl ucSubscription = (UserControl)cphMain.FindControl("ucSubscription");
                if (ucSubscription != null)
                {
                    TextBox txtMessage = (TextBox)ucSubscription.FindControl("txtMessage");
                    HiddenField hfLogLength = (HiddenField)ucSubscription.FindControl("hfLogLength");
                    if (subscriptionMessages.Count > 0 || txtMessage.Text.Length == 0)
                    {
                        List<string> sessionList = (List<string>)Session["_subscriptionLog"];

                        foreach (string message in subscriptionMessages)
                        {
                            sessionList.Insert(0, message + Environment.NewLine + Environment.NewLine);
                        }

                        if (sessionList.Count > 500)
                            sessionList.RemoveRange(500, sessionList.Count - 500);

                        Session["_subscriptionLog"] = sessionList;

                        //DRAW MESSAGES
                        string length = hfLogLength.Value;
                        int LogLength = 50;
                        int.TryParse(length, out LogLength);

                        if (sessionList.Count > LogLength)
                            txtMessage.Text = String.Join("", sessionList.GetRange(0, LogLength)) + "...(truncated)";
                        else
                            txtMessage.Text = String.Join("", sessionList);

                        UpdatePanel upData = (UpdatePanel)ucSubscription.FindControl("upData");
                        upData.Update();
                    }
                }

                upRefresh.Update();
                lblRefreshed.Text = Common.getLocalTime().ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}