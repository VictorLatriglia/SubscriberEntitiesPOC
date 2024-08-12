namespace EntitiesEnvironment.Environment
{
    public class DamageMessage : IMessage
    {
        public string Message { get; }

        public Guid BroadcasterId { get; }

        public double Damage { get; }

        public double Accuracy { get; set; }

        public Guid DirectedAtId { get; }
        public string BroadcasterName { get; }

        public DamageMessage(
            Guid broadcasterId,
            string broadcasterName,
            double damage,
            double accuracy,
            Guid directedAtId,
            bool isRetaliation
        )
        {
            BroadcasterId = broadcasterId;
            Damage = damage;
            DirectedAtId = directedAtId;
            BroadcasterName = broadcasterName;
            Accuracy = accuracy;
            Message =
                $"Shooting initiaded by {broadcasterName} with a damage of {damage}, OUCH! --This was a {(isRetaliation ? "Retaliation" : "CHAOS!!!!!")} shot";
        }
    }
}
