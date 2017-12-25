using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace price_alert_v2
{
    internal class TwilioSender
    {
        internal static async Task SendMessageAsync(string toPhone, string message)
        {
            TwilioClient.Init(Constants.Twilio_accountSid, Constants.Twilio_authToken);
            await MessageResource.CreateAsync(
                toPhone,
                from: new PhoneNumber(Constants.Twilio_number),
                body: message);
        }
    }
}