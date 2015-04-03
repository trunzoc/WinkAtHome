using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome
{
    public partial class Groups : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<Wink.Group> groups =  Wink.Groups;
        }
    }
}