namespace EntitiesEnvironment.Environment
{
    public class DamageMessage : IMessage
    {
        public string Message { get; }

        public INpcEntity SendingEntity { get; }

        public double Damage { get; }
        public bool HitsTarget { get; set; }
        public double Accuracy { get; set; }

        public Guid DirectedAtId { get; }

        public DamageMessage(
            INpcEntity sendingEntity,
            double damage,
            double accuracy,
            Guid directedAtId,
            bool isRetaliation
        )
        {
            SendingEntity = sendingEntity;
            Damage = damage;
            DirectedAtId = directedAtId;
            Accuracy = accuracy;
            Message =
                $"Shooting initiaded by {sendingEntity.NpcName} with a damage of {damage}, OUCH! --This was a {(isRetaliation ? "Retaliation" : "CHAOS!!!!!")} shot";
        }
    }
}
