using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;

namespace price_alert_v2
{
    public static class watcher
    {
        [FunctionName("watcher")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {
            log.Info("Starting watcher - getting initial ticker");
            WatcherRequest input = context.GetInput<WatcherRequest>();
            double current = await context.CallActivityAsync<double>("watcher_getticker", input.market);
            DateTime maxTime = context.CurrentUtcDateTime.Add(input.maxDuration);
            while (current < input.threshold && context.CurrentUtcDateTime < maxTime) {
                await context.CreateTimer(context.CurrentUtcDateTime.AddMinutes(double.Parse(Constants.DelayInterval)), CancellationToken.None);
                log.Info("Getting ticker");
                current = await context.CallActivityAsync<double>("watcher_getticker", input.market);
                log.Info("Current price " + current);
            }
            log.Info("Exited while loop");
            if(current < input.threshold)
            {
                await context.CallActivityAsync("send_event", new Message {
                    phone = input.phone,
                    text = "Crossed at " + context.CurrentUtcDateTime.ToString() + " with price " + current
                });
            }
            else
            {
                await context.CallActivityAsync("send_event", new Message {
                    phone = input.phone,
                    text = "Timed out with price " + current
                });
            }
            
        }

        [FunctionName("watcher_getticker")]
        public static async Task<double> GetTicker([ActivityTrigger] string name, TraceWriter log)
        {
            var result = await (await Bittrex.httpClient.GetAsync(Constants.TickerUrl + "?market=" + name)).Content.ReadAsAsync<JObject>();
            return (double)result["result"]["Bid"];
        }

        [FunctionName("send_event")]
        public static async Task SendMessage([ActivityTrigger] Message message, TraceWriter log)
        {
            await TwilioSender.SendMessageAsync(message.phone, message.text);
        }


    }
}