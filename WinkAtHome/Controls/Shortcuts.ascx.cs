using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class Shortcuts : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                hfSettingBase.Value = Request.RawUrl.Substring(Request.RawUrl.LastIndexOf('/') + 1) + "-Shortcuts-MV" + ((Table)Page.Master.FindControl("tblExpand")).Visible.ToString();
                if (!IsPostBack)
                {

                    string columns = SettingMgmt.getSetting(hfSettingBase.Value + "-Columns");
                    if (columns != null)
                        tbColumns.Text = columns;

                    string dataVisible = SettingMgmt.getSetting(hfSettingBase.Value + "-Visible");
                    if (dataVisible != null)
                    {
                        bool visible = true;
                        bool.TryParse(dataVisible, out visible);
                        rowData.Visible = visible;
                        cbShow.Checked = visible;
                    }

                    BindData();
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Shortcuts.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        private void BindData()
        {
            try
            {
                dlShortcuts.DataSource = null;
                dlShortcuts.DataBind();

                dlShortcuts.DataSource = Wink.Shortcuts.OrderBy(c => c.position).ThenBy(c => c.displayName).ToList();
                dlShortcuts.DataBind();
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Shortcuts.BindData", ex.Message, EventLogEntryType.Error);
            }
        }

         protected void dlShortcuts_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    Wink.Shortcut shortcut = ((Wink.Shortcut)e.Item.DataItem);

                    TextBox tbPosition = (TextBox)e.Item.FindControl("tbPosition");
                    tbPosition.Text = shortcut.position > 1000 ? "" : (shortcut.position).ToString();

                    TextBox tbDisplayName = (TextBox)e.Item.FindControl("tbDisplayName");
                    tbDisplayName.Text = shortcut.displayName;

                    //BIND INFO BUTTON
                    var props = typeof(Wink.Shortcut).GetProperties();
                    var properties = new List<KeyValuePair<string, string>>();
                    foreach (var prop in props)
                    {
                        if (prop.Name != "json")
                        {
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                            string propname = textInfo.ToTitleCase(prop.Name.Replace("_", " "));
                            var propvalue = prop.GetValue(shortcut, null);
                            if (propvalue != null)
                                properties.Add(new KeyValuePair<string, string>(propname, propvalue.ToString()));
                        }
                    }
                    DataList dlProperties = (DataList)e.Item.FindControl("dlProperties");
                    if (dlProperties != null)
                    {
                        dlProperties.DataSource = properties;
                        dlProperties.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Shortcuts.dlShortcuts_ItemDataBound", ex.Message, EventLogEntryType.Error);
            }
        }

       protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton ib = (ImageButton)sender;
                DataListItem li = (DataListItem)ib.NamingContainer;
                string shortcutID = ib.CommandArgument;

                Wink.Shortcut.activateShortcut(shortcutID);
                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Shortcuts.imgIcon_Click", ex.Message, EventLogEntryType.Error);
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

            SettingMgmt.saveSetting(hfSettingBase.Value + "-Columns", tbColumns.Text);

            mpeSettings.Hide();

            BindData();
        }

        protected void ibInfo_Click(object sender, EventArgs e)
        {
            try
            {
                ImageButton ib = (ImageButton)sender;

                Session["modalshowing"] = "true";

                ModalPopupExtender mpeInfo = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeInfo");
                mpeInfo.Show();
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Shortcuts.ibInfo_Click", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton ib = (LinkButton)sender;
                TextBox tbPosition = (TextBox)ib.NamingContainer.FindControl("tbPosition");
                TextBox tbDisplayName = (TextBox)ib.NamingContainer.FindControl("tbDisplayName");
                Label lblPositionBad = (Label)ib.NamingContainer.FindControl("lblPositionBad");
                ModalPopupExtender mpeInfo = (ModalPopupExtender)ib.NamingContainer.FindControl("mpeInfo");

                Wink.Shortcut item = Wink.Shortcut.getShortcutByID(ib.CommandArgument);

                bool savePosSuccess = false;
                bool saveNameSuccess = false;

                if (item != null)
                {
                    //SAVE POSITION
                    try
                    {
                        Int32 pos = 9999;
                        if (string.IsNullOrWhiteSpace(tbPosition.Text))
                        {
                            savePosSuccess = true;
                        }
                        else if (Int32.TryParse(tbPosition.Text, out pos) && pos > 0 && pos < 1001)
                        {
                            List<string> existingList = new List<string>();
                            foreach (DataListItem dli in dlShortcuts.Items)
                            {
                                HiddenField hfShortcutID = (HiddenField)dli.FindControl("hfShortcutID");
                                existingList.Add(hfShortcutID.Value);
                            }
                            string newItem = item.id;

                            existingList.RemoveAll(s => s == newItem);
                            existingList.Insert(pos - 1, newItem);

                            foreach (string ID in existingList)
                            {
                                int position = existingList.IndexOf(ID) + 1;
                                Wink.Shortcut.setShortcutPosition(ID, position);
                            }

                            lblPositionBad.Visible = false;
                            savePosSuccess = true;
                        }
                        else
                            lblPositionBad.Visible = true;
                    }
                    catch (Exception ex)
                    {
                        lblPositionBad.Visible = true;
                    }

                    //SAVE DISPLAY NAME
                    try
                    {
                        Wink.Shortcut.setShortcutDisplayName(item.id, tbDisplayName.Text);
                        saveNameSuccess = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (saveNameSuccess && savePosSuccess)
                {
                    Session["modalshowing"] = "false";

                    mpeInfo.Hide();

                    BindData();
                }
                else
                    mpeInfo.Show();
            }
            catch (Exception ex)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.Shortcuts.btnClose_Click", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}