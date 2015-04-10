using System;
using System.Collections.Generic;
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
            if (!IsPostBack)
            {
                dlShortcuts.DataSource = Wink.Shortcuts;
                dlShortcuts.DataBind();
                
                string columns = SettingMgmt.getSetting("Shortcuts-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns");
                if (columns != null)
                    tbColumns.Text = columns;
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton ib = (ImageButton)sender;
            DataListItem li = (DataListItem)ib.NamingContainer;
            string shortcutID = ib.CommandArgument;

            Wink.activateShortcut(shortcutID);
            Response.Redirect(Request.RawUrl);
        }

        protected void tbColumns_TextChanged(object sender, EventArgs e)
        {
            SettingMgmt.saveSetting("Shortcuts-" + Request.RawUrl.Replace("/", "").Replace(".aspx", "") + "Columns", tbColumns.Text);
        }

        protected void dlShortcuts_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Wink.Shortcut shortcut = ((Wink.Shortcut)e.Item.DataItem);

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
    }
}