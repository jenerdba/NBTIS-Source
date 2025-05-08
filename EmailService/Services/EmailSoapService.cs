using EmailService.Infrastructure;
using EmailService.Models;
using EmailService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using System.ServiceModel;

namespace EmailService.Services
{
    [ServiceContract(Namespace = "http://web.ws")]
    public interface IEmailSoapService
    {
        [OperationContract]
        Task<sendMailResponse> sendMail(
            string from,
            string to,
            string? cc,
            string? bcc,
            string subject,
            string text,
            string? endUserIPAddress,
            EmailAttachmentModel[]? emailAttachments);
    }

    public class EmailSoapService : IEmailSoapService
    {
        private readonly ILogger<EmailSoapService> _logger;
        private readonly IEmailGraphService _emailGraphService;
        private readonly AppSettings _appSettings;

        public EmailSoapService(
            ILogger<EmailSoapService> logger,
            IEmailGraphService emailGraphService,
            AppSettings appSettings)
        {
            _logger = logger;
            _emailGraphService = emailGraphService;
            _appSettings = appSettings;
        }

        public async Task<sendMailResponse> sendMail(
            string from,
            string to,
            string? cc,
            string? bcc,
            string subject,
            string text,
            string? endUserIPAddress,
            EmailAttachmentModel[]? emailAttachments)
        {

            var response = new sendMailResponse();

            try
            {
                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = text
                    },
                    From = new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = from
                        }
                    },
                    ToRecipients = to?.Split(';').Distinct().Select(s =>
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = s
                            }
                        }).ToList(),
                    Attachments = GetAttachments(emailAttachments)
                };

                await _emailGraphService.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                throw new EmailServiceException(ex.Message);
            }

            return new sendMailResponse
            {
                Message = "Success"
            };
        }

        private List<Attachment> GetAttachments(EmailAttachmentModel[]? emailAttachments)
        {
            if (emailAttachments == null)
                return new List<Attachment>();

            return emailAttachments.Select(file => new FileAttachment
            {
                OdataType = "#microsoft.graph.fileAttachment",
                ContentType = file.ContentType,
                Size = (int)file.Length,
                Name = file.FileName,
                ContentBytes = Convert.FromBase64String(file.ContentBytesBase64)
            }).Cast<Attachment>().ToList();
        }
    }
}
