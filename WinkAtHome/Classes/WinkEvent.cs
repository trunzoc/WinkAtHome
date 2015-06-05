using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
        void ReceiveEvent(string userID, string objectType, string objectID, Stream body);

        [OperationContract]
        [WebGet(UriTemplate = "{userID}/{objectType}/{objectID}?hub.mode={action}&hub.topic={topic}&hub.challenge={challenge}&hub.lease_seconds={lease}")]
        Stream validateSub(string userID, string objectType, string objectID, string action, string topic, string challenge, string lease);

        [OperationContract]
        [WebGet(UriTemplate = "/MessageCheck/{userID}/{sessionID}/{nonce}", ResponseFormat = WebMessageFormat.Json)]
        Int32 CheckForUserMessages(string userID, string sessionID, string nonce);
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WinkEventService : iWinkEvent
    {
        public void ReceiveEvent(string userID, string objectType, string objectID, Stream body)
        {
            try
            {
                string text = new StreamReader(body).ReadToEnd();
                WinkEventHelper.storeNewSubscriptionMessage(userID, objectType, objectID, text);
            }
            catch (Exception)
            {

            }
        }

        public Stream validateSub(string userID, string objectType, string objectID, string action, string topic, string challenge, string lease)
        {
            try
            {
                WinkEventHelper.storeNewSubscriptionMessage(userID, "Subscription Event", objectID, action + " " + objectType + " " + objectID + ": topic: " + topic + ", challenge: " + challenge + ", lease: " + lease);

                if (action == "subscribe")
                {
                    MemoryStream ms = new MemoryStream();
                    StreamWriter sw = new StreamWriter(ms);
                    sw.Write(challenge);
                    sw.Flush();
                    ms.Position = 0;
                    return ms;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public Int32 CheckForUserMessages(string userID, string sessionID, string nonce)
        {
            int RowCount = -1;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        //PROCESS SETTINGS
                        command.CommandText = "SELECT COUNT(*) FROM PubSubMessages WHERE UserID = @UserID AND IFNULL(checkSessionIDs,'') NOT LIKE '%" + sessionID + "%' AND Archived <> 'true'";
                        command.Parameters.Add(new SQLiteParameter("@UserID", userID));

                        var result = command.ExecuteScalar();

                        Int32.TryParse(result.ToString(), out RowCount);
                    }

                }
            }
            catch 
            {
            }

            return RowCount;
        }
    }

    public class WinkEventHelper
    {
        private static DateTime lastCleaned = DateTime.Now;
        private static int cleanupInterval = 15;

        public static void storeNewSubscriptionMessage(string userID, string objectType, string objectID, string text)
        {
            Guid id = Guid.NewGuid();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "INSERT INTO PubSubMessages (ID, messageReceived, userID, objectType, objectID, text) VALUES (@ID, @messageReceived, @userID, @objectType, @objectID, @text);";
                        command.Parameters.Add(new SQLiteParameter("@ID", id.ToString()));
                        command.Parameters.Add(new SQLiteParameter("@messageReceived", DateTime.Now));
                        command.Parameters.Add(new SQLiteParameter("@userID", userID));
                        command.Parameters.Add(new SQLiteParameter("@objectType", objectType));
                        command.Parameters.Add(new SQLiteParameter("@objectID", objectID));
                        command.Parameters.Add(new SQLiteParameter("@text", text));
                        command.ExecuteNonQuery();

                        //CLEANUP OLD MESSAGES
                        if (lastCleaned < DateTime.Now.AddMinutes(cleanupInterval * -1))
                        {
                            command.CommandText = "UPDATE PubSubMessages SET Archived = 'true', whenArchived = @now WHERE  Archived = 'false' AND messageReceived < @cleanupTime;";
                            command.Parameters.Add(new SQLiteParameter("@now", DateTime.Now));
                            command.Parameters.Add(new SQLiteParameter("@cleanupTime", DateTime.Now.AddMinutes(cleanupInterval * -1)));
                            command.ExecuteNonQuery();

                            lastCleaned = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public static List<WinkEvent> getSubscriptionMessages(string userID, string sessionID)
        {
            try
            {
                List<WinkEvent> winkevents = new List<WinkEvent>();

                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "select * from PubSubMessages WHERE UserID = @UserID AND IFNULL(checkSessionIDs, '') NOT LIKE '%" + sessionID + "%' AND Archived <> 'true' ORDER BY messageReceived DESC";
                        command.Parameters.Add(new SQLiteParameter("@UserID", userID));

                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            WinkEvent winkevent = new WinkEvent();
                            winkevent.messageID = reader["ID"].ToString();
                            winkevent.messageReceived = Convert.ToDateTime(reader["messageReceived"].ToString());
                            winkevent.userID = reader["userID"].ToString();
                            winkevent.objectType = reader["objectType"].ToString();
                            winkevent.objectID = reader["objectID"].ToString();
                            winkevent.text = reader["text"].ToString();

                            winkevents.Add(winkevent);
                        }
                    }

                }
                return winkevents;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static void markSubscriptionMessagesRead(string[] messageIDs, string sessionID)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Common.dbPath + ";Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        string IDs = "'" + string.Join("','", messageIDs) + "'";

                        //PROCESS SETTINGS
                        command.CommandText = "UPDATE PubSubMessages SET checkSessionIDs = IFNULL(checkSessionIDs,'') || '|' || @sessionID || '|'  WHERE ID in (" + IDs + ");";
                        command.Parameters.Add(new SQLiteParameter("@sessionID", sessionID));
                        command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class WinkEvent
    {
        public string messageID;
        public DateTime messageReceived;
        public string userID;
        public string objectType;
        public string objectID;
        public string text;
    }
}