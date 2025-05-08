using Microsoft.Extensions.Logging;
using NBTIS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailService.Services;
using EmailService.Models;
using Microsoft.AspNetCore.Http;
using NBTIS.Core.Enums;

namespace NBTIS.Core.Services
{
    public enum SubmissionNotificationType
    {
        Submitted,
        Merged,
        ApprovedByDivision,
        ReturnedByDivision,
        Accepted,
        Rejected
    }

    public interface IEmailNotificationService
    {
        // Generic email
        Task SendEmailAsync(EmailModel emailModel);

        // Submission Notification
        Task NotifySubmissionAsync(
             SubmittalLog logEntry,
             SubmittalType submittalType,
             string stateAgencyName,
             SubmissionNotificationType notificationType,
             List<IFormFile>? attachments = null
         );
    }

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailManagerService _emailService;
        private readonly ILogger<EmailNotificationService> _logger;

        private const string _applicationName = "NBTIS";
        private const string _defaultFrom = "NBTIS-no-reply@dot.gov";

        public EmailNotificationService(
            IEmailManagerService emailService,
            ILogger<EmailNotificationService> logger
        )
        {
            _emailService = emailService;
            _logger = logger;
        }
        public async Task SendEmailAsync(EmailModel model)
        {
            if (string.IsNullOrWhiteSpace(model.To))
            {
                _logger.LogWarning(
                  "Email skipped, no recipients. Subject: {Subject}",
                  model.Subject
                );
                return;
            }

            // fill in application name if not set
            model.ApplicationName ??= _applicationName;

            try
            {
                var resp = await _emailService.Send(model);
                if (resp?.Message != "Success")
                {
                    _logger.LogError(
                        "Failed to send email [{EmailType}] to {To}. Response: {Response}",
                        model.EmailType, model.To, resp?.Message ?? "<null>"
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "Email [{EmailType}] sent to {To}. Subject: {Subject}",
                        model.EmailType, model.To, model.Subject
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception sending email [{EmailType}] to {To}. Subject: {Subject}",
                    model.EmailType, model.To, model.Subject
                );
                throw;
            }
        }

        public Task NotifySubmissionAsync(
              SubmittalLog logEntry,
              SubmittalType submittalType,
              string stateAgencyName,
              SubmissionNotificationType notificationType,
              List<IFormFile>? attachments = null
          )
        {
            // Full vs Partial
            var submittalTypeText = submittalType.ToString();
            string statusText, actionVerb;

            switch (notificationType)
            {
                case SubmissionNotificationType.Submitted:
                    statusText = "SUBMITTED";
                    actionVerb = "submitted for division review";
                    break;
                case SubmissionNotificationType.Merged:
                    statusText = "MERGED";
                    actionVerb = "merged with an existing full submittal";
                    break;
                case SubmissionNotificationType.ApprovedByDivision:
                    statusText = "APPROVED BY DIVISION";
                    actionVerb = "approved by division";
                    break;
                case SubmissionNotificationType.ReturnedByDivision:
                    statusText = "RETURNED BY DIVISION";
                    actionVerb = "returned by division";
                    break;
                case SubmissionNotificationType.Accepted:
                    statusText = "ACCEPTED";
                    actionVerb = "accepted";
                    break;
                case SubmissionNotificationType.Rejected:
                    statusText = "REJECTED";
                    actionVerb = "rejected";
                    break;
                default:
                    statusText = string.Empty;
                    actionVerb = string.Empty;
                    break;
            }

            var subject = $"NBTIS - {stateAgencyName} {submittalTypeText} Submittal {statusText}";

            var body = $@"
<html>
  <body>
    <p>Hello,</p>

    <p>
      NBTIS {submittalTypeText.ToLowerInvariant()} submittal for 
      <strong>{stateAgencyName}</strong> has been {actionVerb}.
      Please see the attached Processing Report for full details.
    </p>

    <p>Thank you,<br/>The NBTIS Team</p>

    <p style=""color: #666666; font-size: 0.9em;"">
      (This is an automated message. Please do not reply.)
    </p>
  </body>
</html>"
    .Trim();


            // Collect recipients (skip null/empty)
            var tos = new[] { logEntry?.Approver, logEntry?.Reviewer, logEntry?.Submitter }
                     .Where(x => !string.IsNullOrWhiteSpace(x));
            var toList = string.Join(";", tos);

            var emailModel = new EmailModel
            {
                ApplicationName = _applicationName,
                From = _defaultFrom,
                To = toList,
                // optionally CC/BCC if you have other stakeholders:
                Cc = null,
                Bcc = null,
                Subject = subject,
                Body = body,
                EmailType = "SubmittalStatus",
                Attachments = attachments
            };

            return SendEmailAsync(emailModel);
        }
    }

}
