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

        }

        protected void RadMenu1_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (RadMenuItem item in RadMenu1.Items)
                {
                    if (item.Value.ToLower() == "devices")
                    {
                        List<string> deviceTypes = Wink.Device.getDeviceTypes();

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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void RadMenu1_ItemClick(object sender, RadMenuEventArgs e)
        {
            RadMenuItem item = e.Item;
            string pagename = string.Empty;

            var parent = item.Parent;

            if (parent is RadMenuItem)
            {
                pagename = ((RadMenuItem)parent).Value.ToLower();
            }
            else
            {
                pagename = item.Value;
            }


            string URL = "~/" + pagename + ".aspx";
            Server.Transfer(URL);
        }
    }
}