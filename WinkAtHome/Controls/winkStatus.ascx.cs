using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WinkAtHome.Controls
{
    public partial class winkStatus : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                Dictionary<string, string>[] dictStatuses = new WinkHelper().winkGetServerStatus();

                if (dictStatuses != null)
                {
                    rptStatus.DataSource = dictStatuses[0];
                    rptStatus.DataBind();

                    rptIncident.DataSource = dictStatuses[1];
                    rptIncident.DataBind();

                    lblStatusLastUpdate.Text = "Last Updated: " + dictStatuses[2]["LastUpdated"];
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.winkStatus.Page_Load", ex.Message, EventLogEntryType.Error);
            }
        }

        protected void rptStatus_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    Label lblStatus = (Label)e.Item.FindControl("lblStatus");
                    switch (lblStatus.Text.ToLower())
                    {
                        case "operational":
                            lblStatus.ForeColor = System.Drawing.Color.Green;
                            break;

                        case "partial_outage":
                            lblStatus.ForeColor = System.Drawing.Color.Orange;
                            break;
                        case "investigating":
                            lblStatus.ForeColor = System.Drawing.Color.Purple;
                            break;
                        default:
                            lblStatus.ForeColor = System.Drawing.Color.Red;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw; //EventLog.WriteEntry("WinkAtHome.winkStatus.rptStatus_ItemDataBound", ex.Message, EventLogEntryType.Error);
            }
        }
    }
}