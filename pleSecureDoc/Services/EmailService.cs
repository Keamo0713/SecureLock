using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace pleSecureDoc.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendApprovalEmail(string employerEmail, string requestId)
        {
            if (string.IsNullOrWhiteSpace(employerEmail))
            {
                throw new ArgumentException("Employer email is null or empty.");
            }

            MailAddress toAddress;
            try
            {
                toAddress = new MailAddress(employerEmail.Trim());
            }
            catch (FormatException)
            {
                throw new FormatException($"Invalid employer email format: {employerEmail}");
            }

            var fromEmail = _config["Email:Username"];
            MailAddress fromAddress;
            try
            {
                fromAddress = new MailAddress(fromEmail.Trim());
            }
            catch (FormatException)
            {
                throw new FormatException($"Invalid sender email format in config: {fromEmail}");
            }

            var smtpClient = new SmtpClient(_config["Email:SmtpServer"])
            {
                Port = int.Parse(_config["Email:Port"]),
                Credentials = new NetworkCredential(fromEmail, _config["Email:Password"]),
                EnableSsl = true,
            };

            var baseUrl = _config["AppSettings:BaseUrl"];
            var body = $"Document request received. <a href='{baseUrl}/Employer/ApproveRequest?requestId={requestId}'>Click here to approve</a>. Link valid for 5 minutes.";

            var mailMessage = new MailMessage
            {
                From = fromAddress,
                Subject = "Document Request Approval Needed",
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toAddress);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
