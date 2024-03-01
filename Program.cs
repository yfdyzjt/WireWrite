using WireWrite.Terraria;

namespace WireWrite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string worldFileName;
            string dataFileName;

            Terraria.World.GetFileName(args, out worldFileName, out dataFileName);

            var world = Terraria.World.LoadWorld(worldFileName);
            Console.WriteLine($"Complete world load: {world.Title}");

            world.RunScripts(dataFileName);
            Console.WriteLine("Complete run scripts");

            Terraria.World.SaveWorld(world, worldFileName);
            Console.WriteLine($"Complete world save: {world.Title}");
        }
    }
}
