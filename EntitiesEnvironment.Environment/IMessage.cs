namespace EntitiesEnvironment.Environment
{
    public interface IMessage
    {
        public string Message { get; }
        INpcEntity SendingEntity { get; }
    }
}
