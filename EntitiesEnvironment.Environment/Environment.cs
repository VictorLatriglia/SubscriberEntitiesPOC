namespace EntitiesEnvironment.Environment
{
    public class Environment : IEnvironment
    {
        private readonly HashSet<IObserver<IMessage>> _observers = [];

        private readonly HashSet<IMessage> _messages = [];


        public HashSet<INpcEntity> Npcs { get; }

        public Environment()
        {
            Npcs = [];
        }

        public IDisposable RegisterNpc(IObserver<IMessage> observer, INpcEntity npc)
        {
            // Check whether observer is already registered. If not, add it.
            Npcs.Add(npc);
            return Subscribe(observer);
        }

        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            // Check whether observer is already registered. If not, add it.
            if (_observers.Add(observer))
            {
                // Provide observer with existing data.
                foreach (var item in _messages)
                {
                    observer.OnNext(item);
                }
            }

            return new Unsubscriber<IMessage>(_observers, observer);
        }

        public void BroadcastMessage(IMessage message)
        {
            lock (_messages)
            {
                Thread.Sleep(10);
                _messages.Add(message);
                if (message is DeathMessage deathMessage)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(deathMessage.Message);
                    var deadNpc = Npcs.First(x => x.BroadcasterId == deathMessage.BroadcasterId);
                    Npcs.Remove(deadNpc);
                }
                else if (Npcs.Select(x => x.BroadcasterId).Contains(message.BroadcasterId))
                {
                    var dmgMessage = message as DamageMessage;
                    var npcShooting = Npcs.FirstOrDefault(x => x.BroadcasterId == dmgMessage!.BroadcasterId);
                    var npcGettingHit = Npcs.FirstOrDefault(x => x.BroadcasterId == dmgMessage!.DirectedAtId);

                    if (npcGettingHit == null || npcShooting == null)
                    {
                        RemoveMessage(message);
                        return;
                    }

                    Console.ForegroundColor = npcShooting!.Color;
                    Console.WriteLine($"Entity {dmgMessage!.BroadcasterName} is shooting at {npcGettingHit!.NpcName} for {dmgMessage.Damage}dmg and with {dmgMessage.Accuracy * 10}% of hitting! BANG!");
                    foreach (var item in _observers)
                    {
                        item.OnNext(message);
                    }
                }

                RemoveMessage(message);

                void RemoveMessage(IMessage message)
                {
                    _messages.Remove(message);
                }
            }

        }
    }
    public sealed class Unsubscriber<T> : IDisposable
    {
        private readonly ISet<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        internal Unsubscriber(
            ISet<IObserver<T>> observers,
            IObserver<T> observer) => (_observers, _observer) = (observers, observer);

        public void Dispose() => _observers.Remove(_observer);
    }
}
