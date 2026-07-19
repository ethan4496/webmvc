using WebMVC.Data;
using WebMVC.Interfaces;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using Newtonsoft.Json;

namespace WebMVC.BackgroundWorkers
{
    public class CampaignJob : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _env;

        public CampaignJob(IConfiguration config, IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
        {
            _config = config;
            _scopeFactory = scopeFactory;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("CampaignJob Started");

            var emailTemplateHtml = File.ReadAllText(Path.Combine(_env.WebRootPath, "templates", "EmailTemplate.html"));
            var defaultLogoHtml = File.ReadAllText(Path.Combine(_env.WebRootPath, "templates", "DefaultLogo.html"));

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Running: {DateTime.Now}");
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var now = DateTime.Now;

                    var campaigns = await db.Campaigns
                        .Where(x => x.Status == "active" && x.SendAt <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var campaign in campaigns)
                    {
                        var emailTemplate = await db.EmailTemplates.FindAsync([campaign.TemplateId], stoppingToken);

                        // var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == campaign.CreatedBy, stoppingToken);
                        var signature = await db.AccountSignatures.FirstOrDefaultAsync(x => x.AccountId == campaign.CreatedBy, stoppingToken);

                        var signatureHtml = BuildSignatureHtml(signature, defaultLogoHtml);
                        var contacts = await db.ContactListContacts
                            .Where(x => x.ContactListId == campaign.ContactId)
                            .Join(db.Contacts, clc => clc.ContactId, c => c.Id, (clc, c) => c)
                            .ToListAsync(stoppingToken);

                        const int batchSize = 30;
                        for (int i = 0; i < contacts.Count; i++)
                        {
                            if (i > 0 && i % batchSize == 0)
                            {
                                await db.SaveChangesAsync(stoppingToken);
                                Console.WriteLine($"Đã gửi {i} mail, nghỉ 15 phút...");
                                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                            }

                            var contact = contacts[i];
                            var log = new MailLog
                            {
                                CampaignId = campaign.Id,
                                SentAt = DateTime.Now,
                                Status = "success",
                                ContactId = contact.Id,
                            };
                            try
                            {
                                if (string.IsNullOrEmpty(contact.Email))
                                    continue;

                                var body = emailTemplateHtml
                                    .Replace("{content}", emailTemplate?.Body ?? "")
                                    .Replace("{signature}", signatureHtml)
                                    .Replace("{first_name}", contact.FirstName ?? "")
                                    .Replace("{last_name}", contact.LastName ?? "");

                                await emailService.SendAsync(contact.Email, campaign.Subject, body, campaign.EmailSent);
                            }
                            catch (Exception ex)
                            {
                                campaign.Status = "failed";
                                log.Status = "failed";
                                log.ErrorMessage = ex.Message;
                                Console.Write(ex.ToString());
                            }
                            db.MailLogs.Add(log);
                        }
                        campaign.Status = "sent";
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private static string BuildSignatureHtml(AccountSignature signature, string defaultLogoHtml)
        {
            if (signature == null) return "";
            var logoHtml = defaultLogoHtml.Replace("{{Logo}}", signature.Logo ?? "");
            return "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" width=\"100%\">"
                 + "<tr><td style=\"direction:ltr;font-size:0px;padding:0;text-align:center;\">"
                 + logoHtml + signature.Body
                 + "</td></tr></table>";
        }
    }
}
