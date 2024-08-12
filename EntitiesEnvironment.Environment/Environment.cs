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
            Task.Run(BroadcastMessages);
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

        public void SendMessage(IMessage message)
        {
            lock (_messages)
            {
                _messages.Add(message);
                if (message is DeathMessage deathMessage)
                {
                    var deadNpc = Npcs.FirstOrDefault(x =>
                        x.BroadcasterId == deathMessage.BroadcasterId
                    );
                    if (deadNpc != null)
                    {
                        Npcs.Remove(deadNpc);
                        PublishMessage(deathMessage);
                        _messages.Remove(message);
                    }
                }
                else if (Npcs.Select(x => x.BroadcasterId).Contains(message.BroadcasterId))
                {
                    var dmgMessage = message as DamageMessage;
                    var npcShooting = Npcs.FirstOrDefault(x =>
                        x.BroadcasterId == dmgMessage!.BroadcasterId
                    );
                    var npcGettingHit = Npcs.FirstOrDefault(x =>
                        x.BroadcasterId == dmgMessage!.DirectedAtId
                    );

                    if (npcGettingHit == null || npcShooting == null)
                    {
                        _messages.Remove(message);
                        return;
                    }
                }
            }
        }

        private void BroadcastMessages()
        {
            int count = 0;
            while (count < 4)
            {
                lock (_messages)
                {
                    Console.WriteLine("Sending messages....");
                    if (_messages.Count == 0)
                        count++;
                    foreach (var message in _messages)
                    {
                        PublishMessage(message);
                    }
                    _messages.Clear();
                    Console.WriteLine(".....SENT......");
                }
                Thread.Sleep(1000);
            }
        }

        private void PublishMessage(IMessage message)
        {

            if (message is DeathMessage deathMessage)
            {
                var deadNpc = Npcs.FirstOrDefault(x =>
                    x.BroadcasterId == deathMessage.BroadcasterId
                );
                if (deadNpc != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(deathMessage.Message);
                }
            }
            else if (Npcs.Select(x => x.BroadcasterId).Contains(message.BroadcasterId))
            {
                var dmgMessage = message as DamageMessage;
                var npcShooting = Npcs.FirstOrDefault(x =>
                    x.BroadcasterId == dmgMessage!.BroadcasterId
                );
                var npcGettingHit = Npcs.FirstOrDefault(x =>
                    x.BroadcasterId == dmgMessage!.DirectedAtId
                );

                Console.ForegroundColor = npcShooting!.Color;
                Console.WriteLine(
                    $"Entity {dmgMessage!.BroadcasterName} is shooting for {dmgMessage.Damage}dmg and with {dmgMessage.Accuracy * 10}% of hitting! BANG! {(dmgMessage.Message.Contains("CHAOS") ? "CHAOS!!!!!" : "Retaliation shot :(")}"
                );
            }
            Parallel.ForEach(
                _observers,
                item =>
                {
                    Task.Run(() => item.OnNext(message));
                }
            );
        }
    }

    public sealed class Unsubscriber<T> : IDisposable
    {
        private readonly ISet<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        internal Unsubscriber(ISet<IObserver<T>> observers, IObserver<T> observer) =>
            (_observers, _observer) = (observers, observer);

        public void Dispose() => _observers.Remove(_observer);
    }
}
