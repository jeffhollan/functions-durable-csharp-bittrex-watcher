using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace price_alert_v2
{
    internal class Bittrex
    {
        internal static HttpClient httpClient = new HttpClient();
        private static JArray markets;
        internal static async Task<string> GetMarketName(string requestedCoin)
        {
            if (markets == null)
            {
                await CallMarkets();
            }

            // Check to see if there is a ticker for USDT, if not will just do a BTC market
            var usdtMarket = markets
                .FirstOrDefault(m => string.Equals(((string)m["MarketName"]), "USDT-" + requestedCoin, StringComparison.OrdinalIgnoreCase));

            string market = "BTC-" + requestedCoin.ToUpper();
            if(usdtMarket != null) {
               market = (string)usdtMarket["MarketName"];
            }

            return market;
        }

        private static async Task CallMarkets()
        {
            var result = await httpClient.GetAsync(Constants.GetMarketsUrl);
            markets = (JArray)(await result.Content.ReadAsAsync<JObject>())["result"];
        }
    }
}