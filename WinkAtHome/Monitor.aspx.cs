using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome
{
    public partial class Monitor : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool showSubscriptions = false;
                string strShowSubscriptions = SettingMgmt.getSetting("Show-Subscription-Log-In-Monitor");
                if (!string.IsNullOrWhiteSpace(strShowSubscriptions))
                {
                    if (strShowSubscriptions.ToLower() == "true")
                        showSubscriptions = true;
                }
                rowSubscriptions.Visible = showSubscriptions;
            }
        }
    }
}