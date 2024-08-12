namespace EntitiesEnvironment.Environment
{
    public interface INpcEntity : IObserver<IMessage>
    {
        void Shoot(Guid directedAtId, bool isRetaliation);
        void Speak(string message);
        public string NpcName { get; }
        public ConsoleColor Color { get; }
        Guid BroadcasterId { get; }
        Faction Faction { get; }
        double Accuracy { get; }
        double Damage { get; }
        double Health { get; }
        bool IsAlive { get; }
    }

    public enum Faction
    {
        Ally,
        Enemy
    }
}
