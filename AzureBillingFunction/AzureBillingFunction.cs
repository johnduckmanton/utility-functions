using System;
using System.Threading.Tasks;
using CodeHollow.AzureBillingApi;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzureBillingFunction
{
	public static class AzureBillingFunction
	{
		internal const string CURRENCY_CHAR = "&pound;";
		const string TRIGGER = "0 0 9 * * 1";

		static string MAIL_FROM_ADDRESS = Environment.GetEnvironmentVariable("MAIL_FROM_ADDRESS");
		static string MAIL_FROM_NAME = Environment.GetEnvironmentVariable("MAIL_FROM_NAME");
		static string MAIL_TO_ADDRESS = Environment.GetEnvironmentVariable("MAIL_TO_ADDRESS");
		static string MAIL_TO_NAME = Environment.GetEnvironmentVariable("MAIL_TO_NAME");
		static string APIKEY = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
		static string CLIENT_ID = Environment.GetEnvironmentVariable("BILLING_API_CLIENT");
		static string CLIENT_SECRET = Environment.GetEnvironmentVariable("BILLING_API_SECRET");
		static string AZURE_SUBSCRIPTION_ID = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
		static string TENANT_NAME = Environment.GetEnvironmentVariable("TENANT_NAME");
		[FunctionName("AzureBillingFunction")]
		public static void Run([TimerTrigger(TRIGGER)]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
		{
			log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
			try
			{
				Client c = new Client(TENANT_NAME, CLIENT_ID,
					CLIENT_SECRET, AZURE_SUBSCRIPTION_ID, "http://localhost/billingapi");

				var path = System.IO.Path.Combine(context.FunctionDirectory, "MailReport.html");
				string html = BillingReportGenerator.GetHtmlReport(c, path, log);

				SendMail(MAIL_TO_ADDRESS, MAIL_TO_NAME, html, log).Wait();
			}
			catch (Exception ex)
			{
				log.Error(ex.Message, ex);
			}
		}

		private static async Task SendMail(string toMail, string toName, string htmlContent, TraceWriter log)
		{
			log.Info($"### Sending HTML report as email via SendGrid...");

    		var client = new SendGridClient(APIKEY);
			var from = new EmailAddress(MAIL_FROM_ADDRESS, MAIL_FROM_NAME);
			var subject = "Weekly Azure Billing Report";
			var to = new EmailAddress(toMail, toName);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
			var response = await client.SendEmailAsync(msg);
		}
	}
}
