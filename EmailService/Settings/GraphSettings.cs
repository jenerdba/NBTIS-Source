namespace EmailService.Settings
{
    public class GraphSettings
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }

        public string[] Scopes { get; set; }

        public string Sender { get; set; }
    }
}
