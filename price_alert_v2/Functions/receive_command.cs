using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System.Threading.Tasks;
using System;

namespace price_alert_v2
{
    public static class receive_command
    {
        private static Random rnd1 = new Random();
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Constants.StorageConnectionString);
        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("alias");

        [FunctionName("receive_command")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req, 
            TraceWriter log,
            [OrchestrationClient]DurableOrchestrationClient starter)
        {
            log.Info("Command triggered via HTTP.");

            var args = req.Form["body"].ToString().Split(' ');
            string fromPhone = req.Form["from"];
            switch (args[0].ToLower())
            {
                case "watch":
                    await StartWatcher(starter, args, fromPhone);
                    break;
                case "stop":
                    await TerminateWatcherAsync(starter, args, fromPhone);
                    break;
                default:
                    return new BadRequestResult();
            }
            return new OkResult();
        }

        private static async Task TerminateWatcherAsync(DurableOrchestrationClient starter, string[] args, string fromPhone)
        {
            var result = await table.ExecuteAsync(TableOperation.Retrieve<Alias>(fromPhone, args[1]));
            if(result.Result != null)
            {
                await starter.TerminateAsync(((Alias)result.Result).Id, "User requested terminate");
                await TwilioSender.SendMessageAsync(fromPhone, $"Terminated instance {args[1]}");
            }
            else
            {
                await TwilioSender.SendMessageAsync(fromPhone, $"No instance exists with id: {args[1]}");
            }
        }

        private static async Task StartWatcher(DurableOrchestrationClient starter, string[] args, string fromPhone)
        {
            string market = await Bittrex.GetMarketName(args[1]);
            string instanceId = await starter.StartNewAsync("watcher", new WatcherRequest
            {
                market = market,
                threshold = double.Parse(args[2]),
                maxDuration = TimeSpan.FromMinutes(double.Parse(Constants.MaxDuration)),
                args = args
            });
            Alias alias = new Alias
            {
                PartitionKey = fromPhone,
                RowKey = rnd1.Next(1000).ToString(),
                Id = instanceId
            };
            await table.ExecuteAsync(TableOperation.Insert(alias));
            await TwilioSender.SendMessageAsync(fromPhone, $"Now watching {market} to pass {args[2]}. To cancel, text STOP {alias.RowKey}");
        }

        public class Alias : TableEntity
        {
            public string Id { get; set; }
        }
    }
}
