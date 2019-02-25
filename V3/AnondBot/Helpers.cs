using AnondBot.Models;
using Microsoft.Azure;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AnondBot
{
    /// <summary>
    /// Helpers functions for Anond
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Returns a TaskModule dialog for collecting anonymous reply from user
        /// </summary>
        /// <param name="activity">The incoming InvokeActivity</param>
        /// <returns>Returns TaskModule</returns>
        public static TaskModuleResponse GetTaskModuleForCollectingUserReply(Activity activity)
        {
            var responseCard = File.ReadAllText(HostingEnvironment.MapPath("~/Cards/GetMessageCard.json"));
            responseCard = responseCard.Replace("%personHash%", AnonimizeSender(activity.From));

            var taskModuleResponse = new TaskModuleResponse()
            {
                ResponseType = "task",
                Task = new TaskResponse()
                {
                    Type = "continue",
                    Value = new Value()
                    {
                        Card = new Attachment()
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = JsonConvert.DeserializeObject(responseCard)
                        }
                    }
                }
            };

            return taskModuleResponse;
        }

        /// <summary>
        /// Posts the reply provided in the Activity anonymously
        /// </summary>
        /// <param name="activity">Incoming InvokeActivity containing user's reply</param>
        /// <returns>Task that can be awaited upon</returns>
        public static async Task PostAnonReply(Activity activity)
        {
            dynamic activityValue = activity.Value;
            var teamId = activity.GetChannelData<TeamsChannelData>().Channel.Id;
            string messageReply = activityValue.data.undefined;

            // If replying to a reply, need to get the parent of the reply
            var parentMessageId = (activityValue.messagePayload.replyToId != null) ? activityValue.messagePayload.replyToId.ToString()
                : activityValue.messagePayload.id.ToString();

            string replyToId = string.Format("{0};messageid={1}", teamId, parentMessageId);

            using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                var replyActivity = activity.CreateReply();
                replyActivity.Type = ActivityTypes.Message;
                replyActivity.Attachments = new List<Attachment>()
                        {
                            new Attachment()
                            {
                                ContentType = "application/vnd.microsoft.card.adaptive",
                                Content = JsonConvert.DeserializeObject(
                                    File.ReadAllText(HostingEnvironment.MapPath("~/Cards/PostMessageCard.json"))
                                    .Replace("%personHash%", AnonimizeSender(activity.From))
                                    .Replace("%replyText%", messageReply))
                            }
                        };

                replyActivity.Conversation = new ConversationAccount()
                {
                    Id = replyToId,
                    IsGroup = true,
                };

                await connectorClient.Conversations.SendToConversationAsync(replyActivity);
            }
        }

        /// <summary>
        /// Returns TaskModule dialog to indicate task completion
        /// </summary>
        /// <param name="activity">Incoming InvokeActivity</param>
        /// <returns>TaskModule dialog</returns>
        public static TaskModuleResponse GetMessagePostedConfirmation(Activity activity)
        {
            var responseCard = File.ReadAllText(HostingEnvironment.MapPath("~/Cards/CardPostedConfirmation.json"));
            responseCard = responseCard.Replace("%personHash%", AnonimizeSender(activity.From));

            var taskModuleResponse = new TaskModuleResponse()
            {
                ResponseType = "task",
                Task = new TaskResponse()
                {
                    Type = "continue",
                    Value = new Value()
                    {
                        Card = new Attachment()
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = JsonConvert.DeserializeObject(responseCard)
                        }
                    }
                }
            };

            return taskModuleResponse;
        }

        /// <summary>
        /// Returns a cryptographically secure hash of the specified user account
        /// </summary>
        /// <param name="from">The user to be anonymized</param>
        /// <returns>Anonymized hash of the user</returns>
        private static string AnonimizeSender(ChannelAccount from)
        {
            // Compute HMACSHA1 hash of AAD-ID with app-secret as the hash
            var senderGuid = from.Properties["aadObjectId"].ToString();
            var salt = Encoding.UTF8.GetBytes(CloudConfigurationManager.GetSetting("MicrosoftAppPassword"));
            return Convert.ToBase64String(new Rfc2898DeriveBytes(senderGuid, salt, 10000).GetBytes(20)); // HMACSHA1
        }

    }


}