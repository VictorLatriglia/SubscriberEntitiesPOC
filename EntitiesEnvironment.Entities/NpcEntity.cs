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

        public double Health { get; private set; }

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
            _cancellation = environment.RegisterNpc(this, this);
            BroadcasterId = Guid.NewGuid();
            NpcName = npcName;
            Color = color;
            Faction = faction;
            Accuracy = rng.Next(1, 10);
            AnimosityLevel = rng.Next(1, 10);
            probabilityToRetaliate = rng.Next(1, 10);
            Damage = rng.Next(1, 10);
            Health = 10;
        }

        public void OnCompleted() => messages.Clear();

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(IMessage value)
        {
            if (value.BroadcasterId != BroadcasterId)
            {
                bool shotsFired = false;
                KnownEntities.Add(value.BroadcasterId);
                if (value is DamageMessage dmgMessage && dmgMessage.DirectedAtId == BroadcasterId)
                {
                    var probabilityOfHit = 5;
                    AnimosityLevel += 1;
                    var probabilityOfRetaliation = 3;
                    Console.ForegroundColor = Color;
                    if (dmgMessage.Accuracy > probabilityOfHit)
                    {
                        this.Health -= dmgMessage.Damage;
                        Console.WriteLine(
                            $"Entity {NpcName} got shot!! their new health is {Health}"
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Entity {NpcName} got shot!! but they missed lmao");
                    }
                    if (!IsAlive)
                    {
                        Die(); //Lol, rip
                        _cancellation!.Dispose();
                        return;
                    }
                    else if (probabilityToRetaliate + AnimosityLevel > probabilityOfRetaliation)
                    {
                        shotsFired = true;
                        Shoot(dmgMessage.BroadcasterId, true);
                    }
                }
                else if (value is DeathMessage deathMessage)
                {
                    KnownEntities.Remove(deathMessage.BroadcasterId);
                }
                var peace = 4;
                if (AnimosityLevel > peace && KnownEntities.Count > 0 && !shotsFired)
                {
                    var randomPerson = rng.Next(0, KnownEntities.Count);
                    Shoot(KnownEntities.ElementAt(randomPerson), false);
                }
            }
        }

        public void Shoot(Guid directedAtId, bool isRetaliation)
        {
            Accuracy += 1;
            environment.SendMessage(
                new DamageMessage(
                    BroadcasterId,
                    NpcName,
                    Damage,
                    Accuracy,
                    directedAtId,
                    isRetaliation
                )
            );
        }

        public void Die()
        {
            environment.SendMessage(new DeathMessage(BroadcasterId, NpcName));
        }

        public void Speak(string message)
        {
            environment.SendMessage(new NpcMessage(message, NpcName, BroadcasterId));
        }
    }
}
