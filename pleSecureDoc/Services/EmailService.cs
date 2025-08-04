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

            var fromEmail = _config["Email:Username"] ?? throw new InvalidOperationException("Email:Username configuration is missing.");
            MailAddress fromAddress;
            try
            {
                fromAddress = new MailAddress(fromEmail.Trim());
            }
            catch (FormatException)
            {
                throw new FormatException($"Invalid sender email format in config: {fromEmail}");
            }

            // Ensure these configuration values are not null before parsing/using
            var smtpServer = _config["Email:SmtpServer"] ?? throw new InvalidOperationException("Email:SmtpServer configuration is missing.");
            var portString = _config["Email:Port"] ?? "587"; // Provide a default if missing
            int port = int.Parse(portString);
            var password = _config["Email:Password"] ?? string.Empty;

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, password),
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

        // New method for sending face verification email
        public async Task SendFaceVerificationEmail(string employerEmail, string employeeId, string employeeEmail)
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

            var fromEmail = _config["Email:Username"] ?? throw new InvalidOperationException("Email:Username configuration is missing.");
            MailAddress fromAddress;
            try
            {
                fromAddress = new MailAddress(fromEmail.Trim());
            }
            catch (FormatException)
            {
                throw new FormatException($"Invalid sender email format in config: {fromEmail}");
            }

            var smtpServer = _config["Email:SmtpServer"] ?? throw new InvalidOperationException("Email:SmtpServer configuration is missing.");
            var portString = _config["Email:Port"] ?? "587";
            int port = int.Parse(portString);
            var password = _config["Email:Password"] ?? string.Empty;

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            var baseUrl = _config["AppSettings:BaseUrl"];
            var verificationLink = $"{baseUrl}/Face/Confirm?requestId={employeeId}"; // Assuming employeeId is used as requestId for verification
            var body = $"Dear Employer, an employee ({employeeEmail}) has requested a document. Please verify your face to approve the request. <a href='{verificationLink}'>Click here to verify</a>.";

            var mailMessage = new MailMessage
            {
                From = fromAddress,
                Subject = "Document Approval: Face Verification Required",
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toAddress);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
