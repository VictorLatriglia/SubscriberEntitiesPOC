using EntitiesEnvironment.Environment;

namespace EntitiesEnvironment.Entities
{
    public class NpcEntity : INpcEntity
    {
        private readonly IDisposable? _cancellation;
        private readonly List<IMessage> messages = [];
        private readonly IEnvironment environment;

        public Guid BroadcasterId { get; }
        private readonly double probabilityToRetaliate;

        private readonly HashSet<Guid> KnownEntities = [];

        public Faction Faction { get; }

        public double Accuracy { get; private set; }

        public double AnimosityLevel { get; set; }

        public double Damage { get; }

        public double Health { get; set; }

        public bool IsAlive => Health > 0;

        public string NpcName { get; }
        public ConsoleColor Color { get; }

        private readonly Random rng = new();

        public NpcEntity(
            IEnvironment environment,
            Faction faction,
            string npcName,
            ConsoleColor color
        )
        {
            this.environment = environment;
            BroadcasterId = Guid.NewGuid();
            NpcName = npcName;
            Color = color;
            Faction = faction;
            Accuracy = rng.Next(1, 10);
            AnimosityLevel = rng.Next(1, 10);
            probabilityToRetaliate = rng.Next(1, 10);
            Damage = rng.Next(1, 10);
            Health = 10;
            _cancellation = environment.RegisterNpc(this, this);
        }

        public void OnCompleted() => messages.Clear();

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(IMessage value)
        {
            if (value.SendingEntity.BroadcasterId != BroadcasterId)
            {
                bool shotsFired = false;
                KnownEntities.Add(value.SendingEntity.BroadcasterId);
                if (value is DamageMessage dmgMessage && dmgMessage.DirectedAtId == BroadcasterId)
                {
                    AnimosityLevel += 1;
                    var probabilityOfRetaliation = 3;
                    Console.ForegroundColor = Color;
                    if (dmgMessage.HitsTarget)
                    {
                        double prevHealth = Health;
                        this.Health -= dmgMessage.Damage;
                        Console.WriteLine(
                            $"{NpcName} says: I got shot!! my health was {prevHealth}, now it's {Health}"
                        );
                    }
                    else
                    {
                        Console.WriteLine($"{NpcName} says: I got shot!! but they missed lmao");
                    }

                    if (!IsAlive)
                    {
                        return;
                    }
                    else if (probabilityToRetaliate + AnimosityLevel > probabilityOfRetaliation)
                    {
                        shotsFired = true;
                        Shoot(dmgMessage.SendingEntity.BroadcasterId, true);
                    }
                }
                var peace = 0;
                lock (KnownEntities)
                {
                    if (AnimosityLevel > peace && KnownEntities.Count > 0 && !shotsFired)
                    {
                        var randomPerson = rng.Next(0, KnownEntities.Count);
                        var entity = KnownEntities.ElementAt(randomPerson);
                        if (entity != Guid.Empty)
                            Shoot(entity, false);
                    }
                }
            }
            if (value is DeathMessage deathMessage)
            {
                if (value.SendingEntity.BroadcasterId == BroadcasterId)
                {
                    Die(); //Lol, rip
                    _cancellation!.Dispose();
                }
                else
                {
                    lock (KnownEntities)
                    {
                        KnownEntities.Remove(deathMessage.SendingEntity.BroadcasterId);
                    }
                }
            }
        }

        public void Shoot(Guid directedAtId, bool isRetaliation)
        {
            Accuracy += 1;
            environment.SendMessage(
                new DamageMessage(this, Damage, Accuracy, directedAtId, isRetaliation)
            );
        }

        public void Die()
        {
            Console.WriteLine($"{NpcName} says: Man I'm ded *dies*");
        }
    }
}
