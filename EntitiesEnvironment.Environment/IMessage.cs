namespace EntitiesEnvironment.Environment
{
    public interface IMessage
    {
        public string Message { get; }
        public Guid BroadcasterId { get; }
        public string BroadcasterName { get; }
    }
}
