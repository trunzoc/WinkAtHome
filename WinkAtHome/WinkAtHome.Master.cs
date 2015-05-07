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

namespace WinkAtHome
{
    public partial class WinkAtHome : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Common.prepareDatabase();

                if (Session["loggedin"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                }

                if (!IsPostBack)
                {
                    bool hasUpdate = Common.checkForUpdate();
                    if (hasUpdate && Common.isLocalHost)
                    {
                        ibVersion.Text = "UPDATE AVAILABLE!";
                        ibVersion.Enabled = true;
                        lblCurrentVersion.Text = Common.currentVersion;
                        lblNewVersion.Text = Common.newVersion;
                        tbReleaseNotes.Text = Common.updateNotes;
                        hlDownloadUpdate.NavigateUrl = Common.updateFilePath;
                        mpeUpdate.Show();
                    }
                    else
                        ibVersion.Text = Common.currentVersion;
                    

                    lblRefreshed.Text = DateTime.Now.ToString();

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

                    PubNub pubnub = PubNub.myPubNub;
                    Response.Write(pubnub.hasPubNub);
                    if (pubnub.hasPubNub)
                    {
                        PubNub pubnubSocket = new PubNub();
                        pubnubSocket.Open();
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
        protected void tbTimer_TextChanged(object sender, EventArgs e)
        {
            try
            {
                tmrRefresh.Interval = Convert.ToInt32(tbTimer.Text) * 60000;
                SettingMgmt.saveSetting("RefreshTimer-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), tbTimer.Text);
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.tbTimer_TextChanged", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void rblenabled_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                tmrRefresh.Enabled = Convert.ToBoolean(rblenabled.SelectedValue);
                SettingMgmt.saveSetting("RefreshEnabled-" + Request.RawUrl.Replace("/", "").Replace(".aspx", ""), rblenabled.SelectedValue);
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.rblenabled_SelectedIndexChanged", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            try
            {
                if (Request.Cookies["login"] != null)
                {
                    HttpCookie aCookie = new HttpCookie("login");
                    aCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(aCookie);
                }

                Session.Abandon();

                Wink.clearWink();
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
                    Wink.reloadWink();
                    Response.Redirect(Request.RawUrl);
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Master.reload", ex.Message, EventLogEntryType.Error);
            }
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
                    if (Wink.hasDeviceChanges)
                    {
                        Wink.hasDeviceChanges = false;

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

                        //UPDATE PUBNUB PANEL
                        UserControl ucPubNub = (UserControl)cphMain.FindControl("ucPubNub");
                        if (ucPubNub != null)
                        {
                            var control = ucPubNub as Controls.PubNubDisplay;
                            control.UpdateResultView();
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

    }
}