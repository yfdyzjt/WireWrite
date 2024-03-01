using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireWrite.Terraria
{
    public partial class World
    {
        public void RunScripts(string dataFileName)
        {
            foreach (Sign s in Signs)
            {
                try
                {
                    using (var fileStream = new FileStream(dataFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (var binaryReader = new BinaryReader(fileStream))
                        {
                            if (s.Text.StartsWith("WIRE_WRITE"))
                            {
                                var script = new Script.Script(s);
                                Console.WriteLine($"Running script: {script.Title} ({script.X},{script.Y})");
                                using (Lua lua = new Lua())
                                {
                                    dynamic env = lua.CreateEnvironment();
                                    env.dochunk(script.Text, script.Title, "sign", script, "bin", binaryReader, "tiles", Tiles);
                                }
                            }
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
