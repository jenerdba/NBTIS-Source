using Microsoft.AspNetCore.Http;

namespace EmailService.Models
{
    public class EmailModel
    {
        public string? ApplicationName { get; set; }

        public string? AccessCode { get; set; }

        public required string From { get; set; }

        public required string To { get; set; }

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        public required string Subject { get; set; }

        public required string Body { get; set; }

        public string? EndUserIPAddress { get; set; }

        public string? EmailType { get; set; }

        public List<IFormFile>? Attachments { get; set; }
    }
}
