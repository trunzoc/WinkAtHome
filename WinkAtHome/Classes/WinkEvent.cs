using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading;

namespace WinkAtHome
{

    [ServiceContract]
    public interface iWinkEvent
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "{userID}/{objectType}/{objectID}", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream InsertEvent(string userID, string objectType, string objectID, Stream body);

        [OperationContract]
        [WebGet(UriTemplate = "{userID}/{objectType}/{objectID}?hub.mode={action}&hub.topic={topic}&hub.challenge={challenge}&hub.lease_seconds={lease}")]
        Stream validateSub(string userID, string objectType, string objectID, string action, string topic, string challenge, string lease);
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WinkEventService : iWinkEvent
    {
        private static ConcurrentDictionary<Guid, WinkEvent> _messageQueue = new ConcurrentDictionary<Guid, WinkEvent>();
        public static ConcurrentDictionary<Guid, WinkEvent> MessageQueue
        {
            get
            {
                return _messageQueue;
            }
        }
        private static DateTime lastCleaned = DateTime.Now;
        private static int cleanupInterval = -15;

        public Stream InsertEvent(string userID, string objectType, string objectID, Stream body)
        {
            Guid id = Guid.NewGuid();
            string response = string.Empty;

            try
            {
                string json = new StreamReader(body).ReadToEnd();

                WinkEvent winkevent = new WinkEvent();
                winkevent.messageID = id;
                winkevent.messageReceived = DateTime.Now;
                winkevent.userID = userID;
                winkevent.objectType = objectType;
                winkevent.objectID = objectID;
                winkevent.json = json;

                _messageQueue.TryAdd(id, winkevent);

                response = userID + ":" + objectType + ":" + objectID + ":" + json;

                if (lastCleaned < DateTime.Now.AddMinutes(cleanupInterval))
                {
                    lastCleaned = DateTime.Now;
                    foreach (KeyValuePair<Guid, WinkEvent> pair in _messageQueue)
                    {
                        WinkEvent toDelete;
                        if (pair.Value.messageReceived < DateTime.Now.AddMinutes(cleanupInterval))
                            _messageQueue.TryRemove(pair.Key, out toDelete);
                    }
                }
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.InnerException + "\r\n" + ex.Message;

                WinkEvent winkevent = new WinkEvent();
                winkevent.json = response;

                _messageQueue.TryAdd(id, winkevent);
            }

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(response);
            sw.Flush();
            ms.Position = 0;
            return ms;
        }
        public Stream validateSub(string userID, string objectType, string objectID, string action, string topic, string challenge, string lease)
        {
            Guid id = Guid.NewGuid();

            WinkEvent winkevent = new WinkEvent();
            winkevent.messageID = id;
            winkevent.messageReceived = DateTime.Now;
            winkevent.userID = userID;
            winkevent.objectType = "ACTION: " + action + " " + objectType + " " + objectID;
            winkevent.objectID = objectID;
            winkevent.json = "topic: " + topic + ", challenge: " + challenge + ", lease: " + lease;

            _messageQueue.TryAdd(id, winkevent);

            if (action == "subscribe")
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(challenge);
                sw.Flush();
                ms.Position = 0;
                return ms;
            }
            else
                return null;
        }

    }
    public class WinkEvent
    {
        public Guid messageID;
        public DateTime messageReceived;
        public string userID;
        public string objectType;
        public string objectID;
        public string json;
    }
}