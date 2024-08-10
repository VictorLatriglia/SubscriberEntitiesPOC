// See https://aka.ms/new-console-template for more information
using EntitiesEnvironment.Entities;
using EntitiesEnvironment.Environment;
using Soenneker.Utils.AutoBogus;

var environment = new EntitiesEnvironment.Environment.Environment();
Console.BackgroundColor = ConsoleColor.Gray;
var npcs = new List<NpcEntity>();
var fakeNames = AutoFaker.GenerateStatic<string>(3);
var availableColors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
for (int i = 0; i < fakeNames.Count; i++)
{
    npcs.Add(new NpcEntity(environment, Faction.Ally, fakeNames[i], availableColors[i + 1]));
}

npcs[0].Shoot(npcs[^1].BroadcasterId);

Console.ForegroundColor = ConsoleColor.Black;
var aliveNpcs = environment.Npcs;
if (aliveNpcs.Count == 1)
{
    var victor = aliveNpcs.First()!;
    Console.WriteLine($"We have a winner: {victor.NpcName}, with {victor.Health}hp remaining, nice");
}
else
{
    Console.WriteLine("Press enter to shake the status quo hehe");

    Console.ReadLine();
    Console.WriteLine("Creating chaos!!!!");
    //If they aint gonna kill themselves, I´ll force them to do so
    aliveNpcs.First().Shoot(aliveNpcs.Last().BroadcasterId);

}

