namespace NBTIS.Web.ViewModels
{
    public class SendEmailViewModel
    {
        public required string To { get; set; }

        public required string From { get; set; }

        public required string Subject { get; set; }

        public required string Body { get; set; }

        public string? EmailType { get; set; }
    }
}
