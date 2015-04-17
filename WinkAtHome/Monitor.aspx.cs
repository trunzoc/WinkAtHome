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
        [WebMethod]
        public static string DropDevice(string[] deviceID)
        {
            Guid orderId = Guid.NewGuid();
            string guid = orderId.ToString();
            return guid;
        }


        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}