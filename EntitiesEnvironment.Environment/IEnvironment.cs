namespace EntitiesEnvironment.Environment
{
    public interface IEnvironment : IObservable<IMessage>
    {
        HashSet<INpcEntity> Npcs { get; }
        void BroadcastMessage(IMessage message);
        IDisposable RegisterNpc(IObserver<IMessage> observer, INpcEntity npc);
    }
}
