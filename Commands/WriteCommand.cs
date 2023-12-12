using log4net.Util.TypeConverters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using tModPorter;

namespace WireWrite.Commands
{
    internal class WriteCommand
    {
        private static readonly int aOffsetX;
        private static readonly int aOffsetY;
        private static readonly int oOffset1X = 23;
        private static readonly int oOffset1Y = 23;
        private static readonly int oOffset2X = 1120;
        private static readonly int oOffset2Y = 23;

        private static readonly int oMaxLine = 256;
        private static uint XOR(uint data)
        {
            return data ^ (data << 1);
        }
        private static ushort XOR(ushort data)
        {
            return (ushort)(data ^ (data << 1));
        }
        private static void DataRAMWrite(int x, int y, string file)
        {
            Main.NewText("Unrealized Commands");
        }
        private static void DataROMWrite(int x, int y, string file)
        {
            Main.NewText("Unrealized Commands");
        }
        private static void InsRAMWrite(int x, int y, string file)
        {
            Main.NewText("Unrealized Commands");
        }
        private static void InsROMWrite(int x, int y, string file)
        {
            using BinaryReader binaryReader = new(File.Open(file, FileMode.Open));
            int dir = 1, line = 0, color = 0, cell = 0;
            int pX = x, pY = y;
            int x1, y1;
            ushort data = 0, xdata = 0;
            bool flag = false;
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                if (flag)
                {
                    xdata = (ushort)(XOR(data | (uint)(binaryReader.ReadUInt16() << 16)) >> 16);
                    flag = false;
                }
                else
                {
                    data = binaryReader.ReadUInt16();
                    xdata = XOR(data);
                    if ((data & 3) == 3)
                    {
                        flag = true;
                    }
                }

                x1 = pX + ((cell == 0) ? oOffset1X : oOffset2X) + dir * 15;
                y1 = pY + ((cell == 0) ? oOffset1Y : oOffset2Y);
                for (int i = 0; i < 16; i++)
                {
                    Tile tile;
                    switch (color)
                    {
                        case 0:
                            tile = Main.tile[x1, y1 + 1];
                            tile.RedWire = true;
                            tile = Main.tile[x1, y1 + 0];
                            tile.RedWire = (xdata & (1 << i)) == 0;
                            break;
                        case 1:
                            tile = Main.tile[x1, y1 + 1];
                            tile.BlueWire = true;
                            tile = Main.tile[x1, y1 + 0];
                            tile.BlueWire = (xdata & (1 << i)) == 0;
                            break;
                        case 2:
                            tile = Main.tile[x1, y1 + 2];
                            tile.GreenWire = true;
                            tile = Main.tile[x1, y1 + 3];
                            tile.GreenWire = (xdata & (1 << i)) == 0;
                            break;
                        case 3:
                            tile = Main.tile[x1, y1 + 2];
                            tile.YellowWire = true;
                            tile = Main.tile[x1, y1 + 3];
                            tile.YellowWire = (xdata & (1 << i)) == 0;
                            break;
                    }
                    x1 += dir == 1 ? -1 : 1;
                }

                cell++;
                if (cell >= 2)
                {
                    cell = 0;
                    color++;
                }
                if (color >= 4)
                {
                    color = 0;
                    line++;
                    pY += 3;
                }
                if (line >= oMaxLine)
                {
                    line = 0;
                    pY = y;
                    pX += 16 + dir;
                    dir = (dir == 1) ? 0 : 1;
                }
            }
            Main.NewText("Write complete");
        }
        public static void Action(string[] args)
        {
            string[] parts = args[2].Split(',');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            switch (args[0])
            {
                case "--ins":
                case "-i":
                    switch (args[1])
                    {
                        case "--rom":
                        case "-o":
                            InsROMWrite(x, y, args[3]);
                            break;
                        case "--ram":
                        case "-a":
                            InsRAMWrite(x, y, args[3]);
                            break;
                        default:
                            throw new UsageException("Commands not recognized");
                    }
                    break;
                case "--data":
                case "-d":

                    switch (args[1])
                    {
                        case "-rom":
                        case "-o":
                            DataROMWrite(x, y, args[3]);
                            break;
                        case "-ram":
                        case "-a":
                            DataRAMWrite(x, y, args[3]);
                            break;
                        default:
                            throw new UsageException("Commands not recognized");
                    }
                    break;
                default:
                    throw new UsageException("Options not recognized");
            }
        }
    }
    public class WriteCommandGame : ModCommand
    {
        public override CommandType Type => CommandType.World;

        public override string Command => "write";

        public override string Usage
            => "write [OPTIONS] [COMMAND] <COORDINATE> <FILENAME>" +
            "\nOptions:" +
            "\n -i,--ins     Write ins to memory" +
            "\n -d,--data    Write data to memory" +
            "\nCommands:" +
            "\n -a,--ram     Write to RAM" +
            "\n -o,--rom     Write to ROM" +
            "\n <COORDINATE> Coordinates of the upper left corner of the memory tile" +
            "\n <FILENAME>   Filename of the binary file";

        public override string Description
            => "Write binary data into memory";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            WriteCommand.Action(args);
        }
    }
    public class WriteCommandServer : ModCommand
    {
        public override CommandType Type => CommandType.Console;

        public override string Command => "write";

        public override string Description
            => "Write binary data into memory";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            WriteCommand.Action(args);
        }
    }
}
