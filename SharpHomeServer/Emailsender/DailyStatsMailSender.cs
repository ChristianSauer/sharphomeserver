using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Scriban;
using SharpHomeServer.Data;

namespace SharpHomeServer.EmailSender
{
    public record Entry(string Name, string Metric, string Legend, double Value);

    public class DailyStatsMailSender: BackgroundService
    {

        public DailyStatsMailSender(IChartDataProvider provider, ILogger<DailyStatsMailSender> logger, IOptions<EmailSenderOptions> options)
        {
            Provider = provider;
            Logger = logger;
            Options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _template = Template.Parse(GetTemplate());
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // todo delay to wait!

            if (!Options.Enabled)
            {
                Logger.LogInformation("Daily stats emailing disabled");
                return;
            }

            var model = AggregateData();

            SendStatsAsEmail(model);
        }

        private EmailTemplateData AggregateData()
        {
            var (start, end) = GetYesterday();

            var model = new EmailTemplateData
            {
                Start = start,
                End = end,
                Entries = Provider.Charts.Charts.Select(x => GetEntry(x, start, end))
            };

            return model;
        }

        private static (DateTime, DateTime) GetYesterday()
        {
            var yesterdayStart = DateTime.UtcNow.Date; // todo fix!
            var yesterdayEnd = DateTime.UtcNow.Date.Add(new TimeSpan(23, 59, 59));

            return (yesterdayStart, yesterdayEnd);
        }

        private void SendStatsAsEmail(EmailTemplateData model)
        {
            var body = _template.Render(model, x => x.Name);
            var (start, _) = GetYesterday();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Options.FromName, Options.FromEmail));
            message.To.AddRange(Options.Recipients.Select(x => new MailboxAddress(x.Name, x.Email)));
            message.Subject = $"Daily Stats for {start:d}";

            message.Body = new TextPart("html")
            {
                Text = body
            };

            try
            {
                using var client = new SmtpClient();
                client.Connect(Options.Server.Url, Options.Server.Port);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(Options.Server.User, Options.Server.Password);

                client.Send(message);
                client.Disconnect(true);
            }
            catch (Exception e)
            {
                Logger.LogError("Could not sent email, see error for details: {}", e);
            }
        }

        private static string GetTemplate()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(baseDirectory, "EmailSender/EmailTemplate.scriban");
            return File.ReadAllText(path);
        }

        private Entry GetEntry(ChartOption chart, DateTime yesterdayStart, DateTime yesterdayEnd)
        {
            var metric = chart.Unit;
            var group = ValidGroupByTimes.Days.ToString("g");
            var value = Provider.GetReadingTimeSeries(chart.DocumentId, chart.TimeSeries, group, yesterdayStart, yesterdayEnd)
                .y_values
                .SingleOrDefault();

            return new Entry(chart.Name, metric, chart.Legend, value);
        }

        private IChartDataProvider Provider { get; }
        private ILogger Logger { get; }
        private EmailSenderOptions Options { get; }

        private readonly Template _template;
    }
}
