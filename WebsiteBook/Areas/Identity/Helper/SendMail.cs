using System.Net;
using System.Net.Mail;


namespace FashionShopDemo.Areas.Identity.Helper
{
    public class SendMail
    {
        private readonly ILogger<SendMail> _logger;
        private readonly ConstantHelper _constantHelper;
        public SendMail(ILogger<SendMail> logger, ConstantHelper constantHelper)
        {
            _logger = logger;
            _constantHelper = constantHelper;
        }

        public bool SendEmail(string to, string subject, string attachFile, string body)
        {
            try
            {
                using (var message = new MailMessage(_constantHelper.EmailSender, to, subject, body))
                {
                 //  if (!string.IsNullOrEmpty(attachFile)) // Sửa điều kiện
                 //   {
                 //       var attachment = new Attachment(attachFile);
                 //       message.Attachments.Add(attachment);
                 //   }
                    using (var client = new SmtpClient(_constantHelper.HostEmail, _constantHelper.PortEmail))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(_constantHelper.EmailSender, _constantHelper.PasswordSender);
                        client.Send(message);
                    }
                }
                return true;
            }
            catch (SmtpException ex) 
            {
                _logger.LogError(ex, "SMTP error occurred while sending email"); 
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email"); 
                return false;
            }
        }
    }
}
