using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WinkAtHome;

public delegate void pubnubEventHandler(object sender, EventArgs e);

public class PubNubSocket
{
    public event pubnubEventHandler pubnubReceived;
    protected virtual void OnpubnubReceived(EventArgs e)
    {
        if (pubnubReceived != null)
            pubnubReceived(this, e);
    }


    protected static Pubnub pubnub;

    static string channel = "";//"hub_15053,remote_25923,lock_26503,lightbulb_35665,binaryswitch_41775,remote_4409,lightbulb_453617,lightbulb_453672,lightbulb_473489,unknowndevice_4782,unknowndevice_4795,lightbulb_486695,binaryswitch_49169,lightbulb_507307,lightbulb_507413,sensorpod_56667,sensorpod_56675,sensorpod_56679,sensorpod_56696,sensorpod_56701,sensorpod_56709,sensorpod_57429,sensorpod_57430";
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

    public void open()
    {
        if (!(String.IsNullOrWhiteSpace(SettingMgmt.getSetting("PubNub-PublishKey"))
            || String.IsNullOrWhiteSpace(SettingMgmt.getSetting("PubNub-SubscribeKey"))
            || String.IsNullOrWhiteSpace(SettingMgmt.getSetting("PubNub-SecretKey"))))
        {
            channel = Wink.getSubscriptionTopics();

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
            pubnub.Subscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
        }
    }

    protected void DisplayUserCallbackMessage(string result)
    {
        try
        {
            string strResult = "{\"data\": " + result + "}";
            JObject json = JObject.Parse(strResult);

            var data = json["data"].First;
            string name = data["name"].ToString();
            JObject last_reading = JObject.Parse(data["last_reading"].ToString());

            Wink.Device controlled = Wink.Device.getDeviceByName(name);
            Wink.getLast_Readings(controlled, last_reading);

            OnpubnubReceived(new EventArgs());
        }
        catch (Exception e)
        {
            string error = e.Message;
        }
    }

    protected void DisplayConnectCallbackMessage(string result)
    {
    }
    protected void DisplayDisconnectCallbackMessage(string result)
    {
    }
    protected void DisplayErrorMessage(string result)
    {
    }
    protected void DisplayErrorMessage(PubnubClientError result)
    {
    }
}
