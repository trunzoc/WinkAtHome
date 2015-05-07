using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WinkAtHome;

public delegate void pubnubEventHandler(object sender, EventArgs e);

public class PubNub
{
    public static PubNub myPubNub
    {
        get { return HttpContext.Current.Session["_pubnub"] == null ? new PubNub() : (PubNub)HttpContext.Current.Session["_pubnub"]; }
        set { HttpContext.Current.Session["_pubnub"] = value; }
    }
    public string publishKey { get { return SettingMgmt.getSetting("PubNub-PublishKey"); } }
    public string subscriberKey { get { return SettingMgmt.getSetting("PubNub-SubscribeKey"); } }
    public string secretKey { get { return SettingMgmt.getSetting("PubNub-SecretKey"); } }
    public bool hasPubNub { get { return !(String.IsNullOrWhiteSpace(publishKey) || String.IsNullOrWhiteSpace(subscriberKey) || String.IsNullOrWhiteSpace(secretKey)); } }

    private Pubnub pubnub;

    private static string channel = "";
    private static bool ssl = true;
    private static string cipherKey = "";
    private static string uuid = "";
    private static string authKey = "";
    private static bool resumeOnReconnect = true;
    private static int subscribeTimeoutInSeconds = 310;
    private static int operationTimeoutInSeconds = 15;
    private static int networkMaxRetries = 50;
    private static int networkRetryIntervalInSeconds = 10;
    private static int localClientheartbeatIntervalInSeconds = 10;
    private static int presenceHeartbeat = 63;
    private static int presenceHeartbeatInterval = 60;
   
    //MAKE NON_STATIC
    private static ConcurrentQueue<string> _recordQueue = new ConcurrentQueue<string>();
    public static ConcurrentQueue<string> RecordQueue
    {
        get
        {
            return _recordQueue;
        }
    }
    

    public void Open()
    {
        try
        {
            if (hasPubNub)
            {
                channel = Wink.getSubscriptionTopics();

                if (channel != null)
                {
                    if (pubnub == null)
                    {
                        pubnub = new Pubnub(publishKey, subscriberKey, secretKey, cipherKey, ssl);
                        pubnub.Origin = "pubsub.pubnub.com";
                    }
                    pubnub.SessionUUID = uuid;
                    pubnub.AuthenticationKey = authKey;
                    pubnub.SubscribeTimeout = subscribeTimeoutInSeconds;
                    pubnub.NonSubscribeTimeout = operationTimeoutInSeconds;
                    pubnub.NetworkCheckMaxRetries = networkMaxRetries;
                    pubnub.NetworkCheckRetryInterval = networkRetryIntervalInSeconds;
                    pubnub.LocalClientHeartbeatInterval = localClientheartbeatIntervalInSeconds;
                    pubnub.EnableResumeOnReconnect = resumeOnReconnect;
                    pubnub.PresenceHeartbeat = presenceHeartbeat;
                    pubnub.PresenceHeartbeatInterval = presenceHeartbeatInterval;
                    pubnub.Subscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.Open", ex.Message, EventLogEntryType.Error);
        }
    }

    public void Close()
    {
        try
        {
            pubnub.Unsubscribe<string>(channel, null, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.Close", ex.Message, EventLogEntryType.Error);
        }
    }

    private void AddToPubnubResultContainer(string result)
    {
        try
        {
            _recordQueue.Enqueue(result);
        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.AddToPubnubResultContainer", ex.Message, EventLogEntryType.Error);
        }
    }

    protected void DisplayUserCallbackMessage(string result)
    {
        try
        {
            AddToPubnubResultContainer("REGULAR CALLBACK: " + DateTime.Now.ToString());
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer(""); 
            
            string strResult = "{\"data\": " + result.Remove(result.LastIndexOf("},")) + "}]}";
            JObject json = JObject.Parse(strResult);

            Wink.Device.updateDevice(json);
            Wink.hasDeviceChanges = true;

        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.DisplayUserCallbackMessage", ex.Message, EventLogEntryType.Error);
        }
    }

    protected void DisplayConnectCallbackMessage(string result)
    {
        try
        {
            AddToPubnubResultContainer("CONNECT CALLBACK: " + DateTime.Now.ToString());
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.DisplayConnectCallbackMessage", ex.Message, EventLogEntryType.Error);
        }
    }
    protected void DisplayDisconnectCallbackMessage(string result)
    {
        try
        {
            AddToPubnubResultContainer("DISCONNECT CALLBACK: " + DateTime.Now.ToString());
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.DisplayDisconnectCallbackMessage", ex.Message, EventLogEntryType.Error);
        }
    }
    protected void DisplayErrorMessage(string result)
    {
    }
    protected void DisplayErrorMessage(PubnubClientError result)
    {
        try
        {
            AddToPubnubResultContainer("ERROR CALLBACK: " + DateTime.Now.ToString());
            AddToPubnubResultContainer(result.Description);
            AddToPubnubResultContainer("");

            switch (result.StatusCode)
            {
                case 103:
                    //Warning: Verify origin host name and internet connectivity
                    break;
                case 104:
                    //Critical: Verify your cipher key
                    break;
                case 106:
                    //Warning: Check network/internet connection
                    break;
                case 108:
                    //Warning: Check network/internet connection
                    break;
                case 109:
                    //Warning: No network/internet connection. Please check network/internet connection
                    break;
                case 110:
                    //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
                    break;
                case 111:
                    //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
                    break;
                case 112:
                    //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
                    break;
                case 113:
                    //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
                    break;
                case 114:
                    //Warning: Please verify your cipher key
                    break;
                case 115:
                    //Warning: Protocol Error. Please contact PubNub with error details.
                    break;
                case 116:
                    //Warning: ServerProtocolViolation. Please contact PubNub with error details.
                    break;
                case 117:
                    //Informational: Input contains invalid channel name
                    break;
                case 118:
                    //Informational: Channel not subscribed yet
                    break;
                case 119:
                    //Informational: Channel not subscribed for presence yet
                    break;
                case 120:
                    //Informational: Incomplete unsubscribe. Try again for unsubscribe.
                    break;
                case 121:
                    //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
                    break;
                case 122:
                    //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
                    break;
                case 123:
                    //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
                    break;
                case 124:
                    //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
                    break;
                case 125:
                    //Informational: Publish operation timeout occured.
                    break;
                case 126:
                    //Informational: HereNow operation timeout occured
                    break;
                case 127:
                    //Informational: Detailed History operation timeout occured
                    break;
                case 128:
                    //Informational: Time operation timeout occured
                    break;
                case 4000:
                    //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
                    break;
                case 4001:
                    //Warning: Bad Request. Please check the entered inputs or web request URL
                    break;
                case 4010:
                    //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
                    break;
                case 4030:
                    //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
                    break;
                case 4031:
                    //Warning: Incorrect public key or secret key.
                    break;
                case 4140:
                    //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
                    break;
                case 5000:
                    //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 5020:
                    //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 5040:
                    //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 0:
                    //Undocumented error. Please contact PubNub support with full error object details for further investigation
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            throw; //EventLog.WriteEntry("WinkAtHome.PubNub.DisplayErrorMessage", ex.Message, EventLogEntryType.Error);
        }
    }
}
