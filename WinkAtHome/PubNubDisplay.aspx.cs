using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI.WebControls;

namespace WinkAtHome
{
    public partial class PubNubDisplay : System.Web.UI.Page
    {
        protected static Pubnub pubnub;

        static string channel = "";//"hub_15053,remote_25923,lock_26503,lightbulb_35665,binaryswitch_41775,remote_4409,lightbulb_453617,lightbulb_453672,lightbulb_473489,unknowndevice_4782,unknowndevice_4795,lightbulb_486695,binaryswitch_49169,lightbulb_507307,lightbulb_507413,sensorpod_56667,sensorpod_56675,sensorpod_56679,sensorpod_56696,sensorpod_56701,sensorpod_56709,sensorpod_57429,sensorpod_57430";
        static string channelGroup = "";
        static bool ssl = true;
        static string origin = "pubsub.pubnub.com";
        static string publishKey = SettingMgmt.getSetting("PubNub-PublishKey");
        static string subscriberKey = SettingMgmt.getSetting("PubNub-SubscribeKey");
        static string secretKey = SettingMgmt.getSetting("PubNub-SecretKey");
        static string cipherKey = "";
        static string uuid = "";
        static string authKey = "";
        static bool resumeOnReconnect = true;

        static int subscribeTimeoutInSeconds = 310;
        static int operationTimeoutInSeconds = 15;
        static int networkMaxRetries = 50;
        static int networkRetryIntervalInSeconds = 10;
        static int localClientheartbeatIntervalInSeconds = 10;
        static int presenceHeartbeat = 63;
        static int presenceHeartbeatInterval = 60;

        ManualResetEvent mre = new ManualResetEvent(false);
        private object _lockObject = new object();
        private static List<string> _pubnubResult = new List<string>();

        internal class RecordStatusHolder
        {
            public string Record
            {
                get;
                set;
            }

            public bool Status
            {
                get;
                set;
            }
        }

        private static ConcurrentQueue<string> _recordQueue = new ConcurrentQueue<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!(String.IsNullOrWhiteSpace(SettingMgmt.getSetting("PubNub-PublishKey"))
                    || String.IsNullOrWhiteSpace(SettingMgmt.getSetting("PubNub-SubscribeKey"))
                    || String.IsNullOrWhiteSpace(SettingMgmt.getSetting("PubNub-SecretKey"))))
                {
                channel = Wink.getSubscriptionTopics();

                ProcessPubnubRequest("subscribe");
                }
            }
        }

        private void CheckUserInputs()
        {
            if (pubnub == null)
            {
                pubnub = new Pubnub(publishKey, subscriberKey, secretKey, cipherKey, ssl);
                pubnub.Origin = origin;
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
        }

        private void AddToPubnubResultContainer(string result)
        {
            _recordQueue.Enqueue(result);
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayUserCallbackMessage(string result)
        {
            UpdateTimer.Enabled = false;
            AddToPubnubResultContainer("REGULAR CALLBACK");
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");

            try
            {
                string strResult = "{\"data\": " + result + "}";
                JObject json = JObject.Parse(strResult);

                var data = json["data"].First;
                string name = data["name"].ToString();
                JObject last_reading = JObject.Parse(data["last_reading"].ToString());

                Wink.Device controlled = Wink.Device.getDeviceByName(name);
                Wink.getLast_Readings(controlled, last_reading);
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

        }


        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayConnectCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("CONNECT CALLBACK");
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }

        protected void DisplayDisconnectCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("DISCONNECT CALLBACK");
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayErrorMessage(string result)
        {
        }
        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayErrorMessage(PubnubClientError result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("ERROR CALLBACK");
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

        protected void btnSubscribe_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnUnsubscribe_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        void ProcessPubnubRequest(string requestType)
        {
            CheckUserInputs();

            UpdateTimer.Enabled = true;

            switch (requestType.ToLower())
            {
                case "subscribe":
                    if (channelGroup == "")
                    {
                        pubnub.Subscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.Subscribe<string>(channel, channelGroup, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                case "unsubscribe":
                    if (channelGroup == "")
                    {
                        pubnub.Unsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.Unsubscribe<string>(channel, channelGroup, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                default:
                    break;
            }
        }

        protected void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateResultView();
        }

        private void UpdateResultView()
        {
            string recordTest;
            if (_recordQueue.TryPeek(out recordTest))
            {
                //if (txtMessage.Text.Length > 1000)
                //{
                //    string trucatedMessage = "..(truncated)..." + txtMessage.Text.Substring(txtMessage.Text.Length - 300);
                //    txtMessage.Text = trucatedMessage;
                //}

                string currentRecord;
                while (_recordQueue.TryDequeue(out currentRecord))
                {
                    txtMessage.Text += string.Format("{0}{1}", currentRecord, Environment.NewLine);
                    System.Diagnostics.Debug.WriteLine(currentRecord);
                }
            }

            UpdatePanelRight.Update();
        }

    }
}