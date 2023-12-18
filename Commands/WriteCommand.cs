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
        private static readonly int iPos1X = 22;
        private static readonly int iPos1Y = 22;
        private static readonly int iPos2X = 1119;
        private static readonly int iPos2Y = 22;
        private static readonly int iMaxLine = 256;
        private static readonly int iMaxRow = 64;

        private static readonly int dPosX = 22;
        private static readonly int dPosY = 23;
        private static readonly int dMaxLine = 256;
        private static readonly int dMaxRow = 64;
        private static uint XOR(uint data)
        {
            return data ^ (data << 1);
        }
        private static ushort XOR(ushort data)
        {
            return (ushort)(data ^ (data << 1));
        }
        private static void DataROMWrite(int x, int y, string file)
        {
            using BinaryReader binaryReader = new(File.Open(file, FileMode.Open));
            int dir = 1, line = 0, row = 0, color = 0;
            int pX = x, pY = y;
            int x1, y1;
            uint xdata = 0;

            binaryReader.BaseStream.Position = 0x100000;
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                xdata = XOR(binaryReader.ReadUInt32());

                x1 = pX + dPosX + dir * 31;
                y1 = pY + dPosY;
                for (int i = 0; i < 32; i++)
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

                row++;
                pX += 32 + dir;
                dir = (dir == 1) ? 0 : 1;
                if (row >= dMaxRow)
                {
                    row = 0;
                    color++;
                    pX = x;
                }
                if (color >= 4)
                {
                    color = 0;
                    line++;
                    pY += 3;
                }
                if (line >= dMaxLine)
                {
                    break;
                }
            }
            Main.NewText("Write data memory complete");
        }
        private static void InsROMWrite(int x, int y, string file)
        {
            using BinaryReader binaryReader = new(File.Open(file, FileMode.Open));
            int dir = 1, line = 0, row = 0, color = 0, cell = 0;
            int pX = x, pY = y;
            int x1, y1;
            ushort data = 0, xdata = 0;
            bool flag = false;

            binaryReader.BaseStream.Position = 0;
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

                x1 = pX + ((cell == 0) ? iPos1X : iPos2X) + dir * 15;
                y1 = pY + ((cell == 0) ? iPos1Y : iPos2Y);
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
                if (line >= iMaxLine)
                {
                    line = 0;
                    row++;
                    pY = y;
                    pX += 16 + dir;
                    dir = (dir == 1) ? 0 : 1;
                }
                if (row >= iMaxRow)
                {
                    break;
                }
            }
            Main.NewText("Write instruction memory complete");
        }
        public static void Action(string[] args)
        {
            string[] inspos = args[1].Split(',');
            InsROMWrite(int.Parse(inspos[0]), int.Parse(inspos[1]), args[0]);
            string[] datapos = args[2].Split(',');
            DataROMWrite(int.Parse(datapos[0]), int.Parse(datapos[1]), args[0]);
        }
    }
    public class WriteCommandGame : ModCommand
    {
        public override CommandType Type => CommandType.World;

        public override string Command => "write";

        public override string Usage
            => "write <FILENAME> <INS_POSITION> <DATA_POSITION>" +
            "\n <FILENAME>        Filename of the binary file" +
            "\n <INS_POSITION>    Tile position in the upper left corner of the instruction memory" +
            "\n <DATA_POSITION>   Tile position in the upper left corner of the data memory";


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
