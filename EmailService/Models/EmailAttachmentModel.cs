namespace EmailService.Models
{
    public class EmailAttachmentModel
    {
        public required string ContentType { get; set; }

        public long Length { get; set; }

        public required string FileName { get; set; }

        public required string ContentBytesBase64 { get; set; }
    }
}
