namespace NBTIS.Core.Settings
{
    public class OktaSettings
    {
        public required string OktaDomain { get; set; }

        public required string ClientId { get; set; }

        public required string ClientSecret { get; set; }

        public required string AuthorizationServerId { get; set; }
    }
}
