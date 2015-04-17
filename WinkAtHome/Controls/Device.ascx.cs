using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class Device : System.Web.UI.UserControl
    {
        public Wink.Device Device = new Wink.Device();

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}