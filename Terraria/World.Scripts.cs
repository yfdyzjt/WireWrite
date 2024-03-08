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
        public bool RunScripts(string dataFileName)
        {
            try
            {
                using (var fileStream = new FileStream(dataFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var binaryReader = new BinaryReader(fileStream))
                    {
                        foreach (Sign s in Signs)
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
            }
            catch (LuaParseException e)
            {
                Console.WriteLine($"Lua script have a syntactical exception.\r\n" +
                    $"Message: {e.Message}\r\n" +
                    $"Line: {e.Line}");
                Console.WriteLine("Press enter to reload the world.");
                return false;
            }
            catch (LuaRuntimeException e)
            {
                Console.WriteLine($"Lua script have a runtime exception.\r\n" +
                    $"Message: {e.Message}\r\n" +
                    $"Line: {e.Line}");
                Console.WriteLine("Press enter to reload the world.");
                return false;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Lua script have a file exception.\r\n" +
                    $"Message: {e.Message}\r\n");
                Console.WriteLine("Press enter to reload the world.");
                return false;
            }
            catch
            {
                throw;
            }
            return true;
        }
    }
}
