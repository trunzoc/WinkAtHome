using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace WinkAtHome.Controls
{
    public partial class Menu : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try 
            {
                if (!IsPostBack)
                {
                    //POPULTE DEVICE TYPES
                    foreach (RadMenuItem item in RadMenu1.Items)
                    {
                        if (item.Value.ToLower() == "devices")
                        {
                            item.Items.Clear();
                            List<string> deviceTypes = Wink.Device.getDeviceTypes();

                            if (deviceTypes != null)
                            {
                                foreach (string type in deviceTypes)
                                {
                                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                                    RadMenuItem deviceitem = new RadMenuItem();
                                    deviceitem.Text = textInfo.ToTitleCase(type.Replace("_", " "));
                                    deviceitem.Value = type.ToLower();

                                    item.Items.Add(deviceitem);
                                }
                            }
                        }
                    }

                    //SELECT REFERRING MENU ITEM
                    if (Request.CurrentExecutionFilePath != null)
                    {
                        string referrer = Request.CurrentExecutionFilePath;
                        referrer = referrer.Substring(referrer.LastIndexOf('/') + 1);
                        referrer = referrer.Substring(0, referrer.LastIndexOf(".aspx"));

                        RadMenuItem item = RadMenu1.Items.FindItemByValue(referrer.ToLower());
                        if (item != null)
                        {
                            item.Selected = true;
                        }
                        else
                        {
                            RadMenu1.ClearSelectedItem();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        protected void RadMenu1_ItemClick(object sender, RadMenuEventArgs e)
        {
            RadMenuItem item = e.Item;
            string pagename = string.Empty;
            string querystring = string.Empty;

            var parent = item.Parent;

            if (parent is RadMenuItem)
            {
                pagename = ((RadMenuItem)parent).Value.ToLower();
                if (pagename == "devices")
                {
                    querystring = "?devicetype=" + item.Value;
                }
            }
            else
            {
                pagename = item.Value;
            }


            string URL = "~/" + pagename + ".aspx" + querystring;
            Response.Redirect(URL);
        }
    }
}