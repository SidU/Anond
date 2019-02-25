using AnondBot.Models;
using Microsoft.Bot.Connector;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AnondBot
{

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Microsoft.Bot.Connector.Activity activity)
        {
            if (activity.Type == ActivityTypes.Invoke) // Received an invoke.
            {                
                if (activity.Name == "composeExtension/fetchTask")
                {
                    // Since this is the initial fetch, return the initial task-module to collect the anonymous reply.
                    return Request.CreateResponse<TaskModuleResponse>(HttpStatusCode.OK, Helpers.GetTaskModuleForCollectingUserReply(activity));
                }
                else if (activity.Name == "composeExtension/submitAction")
                {
                    // Since this is a submit, user should have typed a reply in the Task Module and pressed "Submit" button.

                    dynamic activityValue = activity.Value;
                    string messageReply = activityValue.data.undefined;

                    // Ensure user typed a reply
                    if (string.IsNullOrEmpty(messageReply))
                    {
                        // Since the user didn't provide a reply text, ask the user to type the reply.
                        return Request.CreateResponse<TaskModuleResponse>(HttpStatusCode.OK, Helpers.GetTaskModuleForCollectingUserReply(activity));
                    }
                    else
                    {
                        // Post the reply.
                        await Helpers.PostAnonReply(activity);

                        // Return a confirmation task-module back to the user.
                        return Request.CreateResponse<TaskModuleResponse>(HttpStatusCode.OK, Helpers.GetMessagePostedConfirmation(activity));
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            return response;
        }       
    }
}