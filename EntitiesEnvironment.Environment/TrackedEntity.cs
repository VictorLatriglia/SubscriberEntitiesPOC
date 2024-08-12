namespace EntitiesEnvironment.Environment
{
    internal class TrackedEntity : INpcEntity
    {
        public Guid BroadcasterId { get; set; }

        public Faction Faction { get; set; }

        public double Accuracy { get; set; }

        public double AnimosityLevel { get; set; }

        public double Damage { get; set; }

        public double Health { get; set; }

        public bool IsAlive => Health > 0;

        public string NpcName { get; set; }
        public ConsoleColor Color { get; set; }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(IMessage value)
        {
            throw new NotImplementedException();
        }

        public void Shoot(Guid directedAtId, bool isRetaliation)
        {
            throw new NotImplementedException();
        }
    }
}
