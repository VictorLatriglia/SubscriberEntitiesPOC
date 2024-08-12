namespace EntitiesEnvironment.Environment
{
    public class DeathMessage : IMessage
    {
        public string Message { get; }

        public INpcEntity SendingEntity { get; }

        public DeathMessage(INpcEntity sendingEntity)
        {
            SendingEntity = sendingEntity;
            Message = $"OH no!!! entity {sendingEntity.NpcName} has died!!! RIP!";
        }
    }
}
