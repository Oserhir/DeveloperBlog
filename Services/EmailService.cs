using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

using TheBlogProject.Services.Interfaces;
using TheBlogProject.ViewModels;

namespace TheBlogProject.Services
{
    public class EmailService : IBlogEmailSender
    {

        #region Properties
        private readonly MailSettings _mailSettings;
        #endregion


        #region Constructor
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
            
        }
        #endregion


        public Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            var email = new MimeMessage();

            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder()
            {
                HtmlBody = htmlMessage
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }
    }
}
