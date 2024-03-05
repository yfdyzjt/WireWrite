using WireWrite.Terraria;

namespace WireWrite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string worldFileName;
            string dataFileName;

            World.GetFileName(args, out worldFileName, out dataFileName);

            StartLoadWorld:
            Console.WriteLine($"Start world load");
            var world = World.LoadWorld(worldFileName);
            Console.WriteLine($"Complete world load: {world.Title}");

            Console.WriteLine($"Start run scripts");
            if (!world.RunScripts(dataFileName)) if (Console.ReadKey().KeyChar == '\r') goto StartLoadWorld;
            Console.WriteLine("Complete run scripts");

            Console.WriteLine($"Start world save");
            World.SaveWorld(world, worldFileName);
            Console.WriteLine($"Complete world save: {world.Title}");
        }
    }
}
