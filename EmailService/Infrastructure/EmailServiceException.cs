namespace EmailService.Infrastructure
{
    public class EmailServiceException : Exception
    {
        public EmailServiceException(string error) : base(error)
        {

        }

        public override string? StackTrace => string.Empty;
    }
}
