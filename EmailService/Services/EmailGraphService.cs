using Azure.Identity;
using EmailService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Users.Item.SendMail;
using System.Text.RegularExpressions;

namespace EmailService.Services
{
    public interface IEmailGraphService
    {
        Task SendEmailAsync(Message message);
    }

    public class EmailGraphService : IEmailGraphService
    {
        private readonly ILogger<EmailGraphService> _logger;
        private readonly GraphSettings _settings;

        public EmailGraphService(
            ILogger<EmailGraphService> logger,
            GraphSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task SendEmailAsync(Message message)
        {
            ValidateSendEmailAsync(message);

            try
            {
                var clientSecretCredential =
                    new ClientSecretCredential(
                        _settings.TenantId,
                        _settings.ClientId,
                        _settings.ClientSecret,
                        new TokenCredentialOptions
                        {
                            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                        });

                var graphClient = new GraphServiceClient(clientSecretCredential, _settings.Scopes);

                var requestBody = new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                };

                // Should from email be config value _settings.Sender?
                // My user has permissions to send on behalf. Is that an issue?
                var userSender = graphClient.Users[message.From?.EmailAddress?.Address];
                await userSender.SendMail.PostAsync(requestBody);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, ex.Message);

                //if (ex is ODataError)
                //{
                //    throw;
                //}

                //throw new ApplicationException("An error occurred.");
            }
        }

        private void ValidateSendEmailAsync(Message message)
        {
            var error = string.Empty;

            if (string.IsNullOrEmpty(message?.Subject))
            {
                error += "Email subject required. ";
            }

            if (string.IsNullOrEmpty(message?.Body?.Content))
            {
                error += "Email body required. ";
            }

            if (!IsValidEmail(message?.From?.EmailAddress?.Address ?? string.Empty))
            {
                error += "Invalid from email. ";
            }

            if (!message?.ToRecipients?.Any(recipient => IsValidEmail(recipient?.EmailAddress?.Address ?? string.Empty)) ?? true)
            {
                error += "Invalid recipient email. ";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new FormatException(error.TrimEnd());
            }
        }

        private bool IsValidEmail(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return false;
            }

            var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

            var regex = new Regex(pattern);
            return regex.IsMatch(emailAddress);
        }
    }
}







