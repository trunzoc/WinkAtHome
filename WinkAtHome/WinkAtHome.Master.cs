using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Reflection;
using System.Collections.Concurrent;
using PubNubMessaging.Core;
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
        Wink myWink;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Common.prepareDatabase();

                if (Session["_winkToken"] == null || Session["_wink"] == null)
                    Response.Redirect("~/Login.aspx");

                if (myWink == null)
                    myWink = (Wink)Session["_wink"];
                
                if (!IsPostBack)
                {

                    //Response.Write("SessionID: " + Session.SessionID + "<br />");
                    //Response.Write("Token: " + myWink.Token + "<br />");
                    //Response.Write("User: " + myWink.winkUser.email + "<br />");
                    //if (Session["userID"] != null)
                    //    Response.Write("UserID: " + Session["userID"].ToString() + "<br />");

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

                    if (Common.isLocalHost)
                    {
                        rowMenuAdds.Visible = false;
                    }

                    lblRefreshed.Text = Common.getLocalTime().ToString();

                    //SET PAGE OPTIONS
                    string timerrefresh = SettingMgmt.getSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                    if (timerrefresh != null)
                    {
                        tbTimer.Text = timerrefresh;
                        tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
                    }

                    string timerenabled = SettingMgmt.getSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""));
                    if (timerenabled != null)
                    {
                        rblenabled.SelectedValue = timerenabled;
                        tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
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


                    //SET PUBNUB
                    if (SettingMgmt.hasPubNub)
                    {
                        PubNub pubnub = PubNub.myPubNub;
                        pubnub.Open();
                        tmrCheckChanges.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;

                reload();
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tmrRefresh_Tick", ex.Message, EventLogEntryType.Error);
            }

        }

        protected void ibSettingsClose_Click(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
            SettingMgmt.saveSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), rblenabled.SelectedValue);
                
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
        public void reload()
        {
            try
            {
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
                throw; //EventLog.WriteEntry("WinkAtHome.Master.reload", ex.Message, EventLogEntryType.Error);
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
        protected void tmrCheckChanges_Tick(object sender, EventArgs e)
        {
            try
            {
                string modalshowing = "false";
                if (Session["modalshowing"] != null)
                    modalshowing = Session["modalshowing"].ToString();

                if (modalshowing == "false")
                {
                    JObject currentRecord;
                    while (PubNub.myPubNub.DeviceQueue.TryDequeue(out currentRecord))
                    {
                        myWink.DeviceHasChanges = true;
                        new WinkHelper.DeviceHelper().DeviceUpdate(currentRecord);
                    }

                    if (myWink.DeviceHasChanges)
                    {
                        myWink.DeviceHasChanges = false;

                        //UPDATE DEVICES
                        UserControl ucDevices = (UserControl)cphMain.FindControl("ucDevices");
                        if (ucDevices != null)
                        {
                            var control = ucDevices as Controls.Devices;
                            control.BindData();

                            UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                            upData.Update();
                        }

                        //UPDATE SENSORS
                        UserControl ucSensors = (UserControl)cphMain.FindControl("ucSensors");
                        if (ucSensors != null)
                        {
                            var control = ucSensors as Controls.Devices;
                            control.BindData();

                            UpdatePanel upData = (UpdatePanel)control.FindControl("upData");
                            upData.Update();
                        }

                        UpdatePanel1.Update();
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