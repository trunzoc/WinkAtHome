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
                bool showPubNub = false;

                string strShowPubNub = SettingMgmt.getSetting("Show-Pubnub-Log-In-Monitor");
                if (!string.IsNullOrWhiteSpace(strShowPubNub))
                {
                    if (strShowPubNub.ToLower() == "true" && SettingMgmt.hasPubNub)
                        showPubNub = true;
                }
                rowPubNub.Visible = showPubNub;
            }
        }
    }
}