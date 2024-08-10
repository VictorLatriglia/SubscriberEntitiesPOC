namespace EntitiesEnvironment.Environment
{
    public class DeathMessage : IMessage
    {
        public string Message { get; }

        public Guid BroadcasterId { get; }
        public string BroadcasterName { get; }

        public DeathMessage(Guid broadcasterId, string broadcasterName)
        {
            BroadcasterId = broadcasterId;
            Message = $"OH no!!! entity {broadcasterName} has died!!! RIP!";
            BroadcasterName = broadcasterName;
        }
    }
}
