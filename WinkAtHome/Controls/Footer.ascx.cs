using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class Footer : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool showLocal = true;//(Common.isLocalHost)

            rowEmailSupport.Visible = showLocal;
            rowBottomAds.Visible = showLocal;
            rowTracking.Visible = showLocal;
        }
    }
}