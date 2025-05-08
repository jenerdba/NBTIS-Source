using EmailService.Models;
using EmailService.Services;
using EmailService.Settings;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Services
{
    public interface IEmailManagerService
    {
        Task<sendMailResponse> Send(EmailModel model);
    }

    public class EmailManagerService : IEmailManagerService
    {
        private readonly IEmailSoapService _emailSoapService;
        private readonly GraphSettings _settings;

        public EmailManagerService(
            GraphSettings settings,
            IEmailSoapService emailSoapService)
        {
            _settings = settings;
            _emailSoapService = emailSoapService;
        }


        public async Task<sendMailResponse> Send(EmailModel model)
        {
            var attachments = await GetAttachmentsAsync(model);

            return await _emailSoapService.sendMail(
                model.From,
                model.To,
                model.Cc,
                model.Bcc,
                model.Subject,
                model.Body,
                model.EndUserIPAddress,
                attachments);
        }

        private async Task<EmailAttachmentModel[]?> GetAttachmentsAsync(EmailModel model)
        {
            if (model.Attachments == null)
                return null;

            var tasks = model.Attachments.Select(async file =>
            {
                var fileData = string.Empty;

                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    fileData = Convert.ToBase64String(ms.ToArray());
                }

                var attachment = new EmailAttachmentModel
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    ContentBytesBase64 = fileData
                };

                return attachment;
            });

            var attachments = await Task.WhenAll(tasks);
            return attachments;
        }

    }
}
