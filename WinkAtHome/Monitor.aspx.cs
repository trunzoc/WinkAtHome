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
            string strShowPubNub = SettingMgmt.getSetting("Show-Pubnub-Log-In-Monitor");
            if (strShowPubNub.ToLower() == "true" && PubNub.myPubNub.hasPubNub)
                rowPubNub.Visible = true;
            else
                rowPubNub.Visible = false;
        }
    }
}