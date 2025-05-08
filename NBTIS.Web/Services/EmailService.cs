using NBTIS.Core.Settings;
using NBTIS.Web.ViewModels;


namespace NBTIS.Web.Services
{
    public interface IEmailService
    {
        Task<HttpResponseMessage> SendEmail(SendEmailViewModel vm);
    }

    public class EmailService : IEmailService
    {
        public async Task<HttpResponseMessage> SendEmail(SendEmailViewModel vm)
        {
            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StringContent(vm.From), "From");
            multipartContent.Add(new StringContent(vm.To), "To");
            multipartContent.Add(new StringContent(vm.Subject), "Subject");
            multipartContent.Add(new StringContent(vm.Body), "Body");

            using var client = new HttpClient();
            client.BaseAddress = new Uri(AppSettings.Current.EmailServiceUrl);

            return await client.PostAsync("Email", multipartContent);
        }
    }
}
