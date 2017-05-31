using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace RobotRemoteControlBot.Dialogs

{
    [Serializable]
    [LuisModel("e7335122-7819-48d2-8e9f-794bb1129419", "db7fab4525544462899e39311acb9edb")]

    public class RootDialog : LuisDialog<object>
    {

        static ServiceClient serviceClient;
        static string connectionString = "HostName=adojeiothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=83Gfbj8wfjsErg+2Fihv5gLuEFHz4YX8pT0wsoZNGn4=";

        // LUIS endpoint URL://
        //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/e7335122-7819-48d2-8e9f-794bb1129419?subscription-key=db7fab4525544462899e39311acb9edb&verbose=true&timezoneOffset=0&q= 

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            String response = $"Hi There! I am a bot created to control your robot. Tell me how you want your robot to move? ";
            response += $" Say something like - turn left - for the robot to turn left";
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("MoveRobot")]
        public async Task Search(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            var reply = context.MakeMessage();
            EntityRecommendation tunPositionEntity;
            if (result.TryFindEntity("TurnPosition", out tunPositionEntity))
            {
                await context.PostAsync(" trying to turn robot " + tunPositionEntity.Entity);

                // now call device methods to turn the robot left 
                serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
                await InvokeMethod(tunPositionEntity.Entity);

            }
            else
            {
                reply.Text = $"Hmm, I don't understand you...  \U0001F633. ";
                reply.Text += $"Say something like - turn left - for the robot to turn left";
            }
          
            await context.PostAsync(reply);
            context.Wait(this.MessageReceived);
        }

        private static async Task InvokeMethod(String command)
        {
            command = "TurnRobot" + command;
            var methodInvocation = new CloudToDeviceMethod(command) { ResponseTimeout = TimeSpan.FromSeconds(30) };

            methodInvocation.SetPayloadJson(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                message = "moving the robot"
            }));



            try
            {
                var response = await serviceClient.InvokeDeviceMethodAsync("Robot", methodInvocation);
                Console.WriteLine("Response status: {0}, payload:", response.Status);
                Console.WriteLine(response.GetPayloadAsJson());
            }
            catch (IotHubException)
            {
                // write code here 
            }


        }
    }
}