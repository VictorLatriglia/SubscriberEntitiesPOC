using Newtonsoft.Json;

namespace EntitiesEnvironment.Environment
{
    public class Environment : IEnvironment
    {
        private readonly HashSet<IObserver<IMessage>> _observers = [];

        private readonly Queue<IMessage> _messages = [];
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);

        public HashSet<INpcEntity> Npcs { get; }

        public Environment()
        {
            Npcs = [];
            Task.Run(BroadcastMessages);
        }

        public IDisposable RegisterNpc(IObserver<IMessage> observer, INpcEntity npc)
        {
            // Check whether observer is already registered. If not, add it.
            Npcs.Add(copyOf(npc));
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
                _messages.Enqueue(message);
                _waitHandle.Set(); // Signal the thread to wake up and process
                _waitHandle.Reset(); // Reset after signaling
            }
        }

        private void BroadcastMessages()
        {
            while (true)
            {
                List<DamageMessage> shotsFired;
                lock (_messages)
                {
                    shotsFired = _messages.OfType<DamageMessage>().ToList();
                    _messages.Clear(); // Clear the messages after copying them
                }

                var npcsIds = Npcs.Select(x => x.BroadcasterId).ToHashSet(); // Use a HashSet for faster lookup
                foreach (
                    var barrage in shotsFired
                        .Where(x =>
                            npcsIds.Contains(x.SendingEntity.BroadcasterId)
                            || npcsIds.Contains(x.DirectedAtId)
                        )
                        .GroupBy(x => x.DirectedAtId)
                )
                {
                    ProcessShotFired(barrage); // This is now outside the lock
                }

                _waitHandle.WaitOne(1000);
            }
        }

        private void ProcessShotFired(IGrouping<Guid, DamageMessage> barrage)
        {
            var npcGettingShot = Npcs.FirstOrDefault(x =>
                x.BroadcasterId == barrage.First().DirectedAtId
            );
            var npcShooting = Npcs.FirstOrDefault(x =>
                x.BroadcasterId == barrage.First().SendingEntity.BroadcasterId
            );
            if (IsDmgValid(npcShooting, npcGettingShot))
            {
                foreach (var shot in barrage)
                {
                    var probabilityOfHit = 2;
                    Console.ForegroundColor = npcGettingShot.Color;
                    if (shot.Accuracy > probabilityOfHit)
                    {
                        ValidShot(shot, npcGettingShot);
                        if (npcGettingShot.Health <= 0)
                        {
                            NpcDead(npcGettingShot);
                        }
                    }
                }
            }
        }

        private static bool IsDmgValid(INpcEntity shootingNpc, INpcEntity npcGettingShot)
        {
            if (npcGettingShot == null || shootingNpc == null)
                return false;
            if (!npcGettingShot!.IsAlive)
                return false;
            if (!shootingNpc!.IsAlive)
                return false;
            return true;
        }

        private void ValidShot(DamageMessage shot, INpcEntity npcGettingShot)
        {
            shot.HitsTarget = true;
            npcGettingShot.Health -= shot.Damage;
            Console.ForegroundColor = shot.SendingEntity!.Color;
            Console.WriteLine(
                $"Entity {shot.SendingEntity.NpcName} is shooting at {npcGettingShot.NpcName} for {shot.Damage}dmg and with {shot.Accuracy * 10}% of hitting! BANG! {(shot.Message.Contains("CHAOS") ? "CHAOS!!!!!" : "Retaliation shot :(")}"
            );
            PublishMessage(shot);
        }

        private void NpcDead(INpcEntity npcGettingShot)
        {
            var deathMessage = new DeathMessage(npcGettingShot);
            PublishMessage(deathMessage);
            Npcs.Remove(npcGettingShot);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(deathMessage.Message);
        }

        private void PublishMessage(IMessage message)
        {
            Parallel.ForEach(
                _observers,
                item =>
                {
                    Task.Run(() => item.OnNext(message));
                }
            );
        }

        public INpcEntity copyOf(INpcEntity entity)
        {
            string data = JsonConvert.SerializeObject(entity);
            TrackedEntity copy = JsonConvert.DeserializeObject<TrackedEntity>(data)!;
            return copy;
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
