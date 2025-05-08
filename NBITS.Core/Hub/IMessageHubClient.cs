namespace NBTIS.Core.Hub
{
    public interface IMessageHubClient
    {
        Task SendProgress(List<string> message);
    }
}

