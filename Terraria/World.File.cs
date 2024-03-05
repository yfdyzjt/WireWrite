using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WireWrite.Terraria
{
    public partial class World
    {
        public bool[] TileFrameImportant { get; set; }
        public static void GetFileName(string[] args, out string worldFileName, out string dataFileName)
        {
            bool flag;
            flag = true;
            while (true)
            {
                if (args.Length >= 1 && flag)
                {
                    flag = false;
                    worldFileName = SearchFile(args[0]);
                }
                else
                {
                    Console.Write("Input the world file name: ");
                    worldFileName = SearchFile(Console.ReadLine());
                }
                if (worldFileName != null)
                {
                    Console.WriteLine($"Open world file path: {worldFileName}");
                    break;
                }
                else
                {
                    Console.WriteLine("World file does not exist");
                }
            }

            flag = true;
            while (true)
            {
                if (args.Length >= 2 && flag)
                {
                    flag = false;
                    dataFileName = SearchFile(args[1]);
                }
                else
                {
                    Console.Write("Input the data file name: ");
                    dataFileName = SearchFile(Console.ReadLine());
                }
                if (dataFileName != null)
                {
                    Console.WriteLine($"Open data file path: {dataFileName}");
                    break;
                }
                else
                {
                    Console.WriteLine("Data file does not exist");
                }
            }
        }
        private static string? SearchFile(string file)
        {
            if (file == null)
            {
                return null;
            }
            if (File.Exists(file))
            {
                return file;
            }
            SetSearchPath();
            foreach (string path in WorldSearchPath)
            {
                if (File.Exists(Path.Combine(path, file)))
                {
                    return Path.Combine(path, file);
                }
            }
            return null;
        }
        private static void SetSearchPath()
        {
            WorldSearchPath.Clear();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WorldSearchPath.Add(@".\");
                WorldSearchPath.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds"));
                WorldSearchPath.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\tModLoader\Worlds"));
                WorldSearchPath.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\ModLoader\Worlds"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                WorldSearchPath.Add(@"./");
                WorldSearchPath.Add(@"~/.local/share/Terraria/Worlds/");
                WorldSearchPath.Add(@"~/.local/share/Terraria/tModLoader/Worlds/");
                WorldSearchPath.Add(@"~/.local/share/Terraria/ModLoader/Worlds/");
            }
        }
        public static void SaveWorld(World world, string filename)
        {
            try
            {
                if (filename == null)
                    return;
                using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var binaryWriter = new BinaryWriter(fileStream))
                    {
                        using (var binaryReader = new BinaryReader(fileStream))
                        {
                            bool[] tileFrameImportant;
                            int[] sectionPointers;

                            int offset;
                            byte[] data;

                            // reset the stream
                            binaryReader.BaseStream.Position = (long)0;
                            // read section pointers and tile frame data
                            if (!LoadSectionHeader(binaryReader, out tileFrameImportant, out sectionPointers, world))
                                throw new Exception("Invalid File Format Section");

                            binaryReader.BaseStream.Position = sectionPointers[2];
                            data = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length - sectionPointers[2]);

                            binaryWriter.BaseStream.Position = sectionPointers[1];
                            offset = SaveTiles(world.Tiles, (int)world.Version, world.TilesWide, world.TilesHigh, binaryWriter, tileFrameImportant) - sectionPointers[2];

                            binaryWriter.Write(data);

                            sectionPointers[2] += offset;
                            sectionPointers[3] += offset;
                            sectionPointers[4] += offset;
                            sectionPointers[5] += offset;
                            if (world.Version >= 140)
                            {
                                sectionPointers[6] += offset;
                            }
                            if (world.Version >= 170)
                            {
                                sectionPointers[7] += offset;
                            }
                            if (world.Version >= 189)
                            {
                                sectionPointers[8] += offset;
                            }
                            if (world.Version >= 210)
                            {
                                sectionPointers[9] += offset;
                            }
                            if (world.Version >= 220)
                            {
                                sectionPointers[10] += offset;
                            }
                            UpdateSectionPointers(world.Version, sectionPointers, binaryWriter);
                        }
                    }
                }
            }
            finally
            {
            }
        }
        public static int SaveTiles(Tile[][] tiles, int version, int maxX, int maxY, BinaryWriter bw, bool[] tileFrameImportant)
        {
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    Tile tile = tiles[x][y];

                    int dataIndex;
                    int headerIndex;

                    byte[] tileData = SerializeTileData(tile, version, tileFrameImportant, out dataIndex, out headerIndex);

                    // rle compression
                    byte header1 = tileData[headerIndex];

                    short rle = 0;
                    int nextY = y + 1;
                    int remainingY = maxY - y - 1;
                    while (remainingY > 0 && tile.Equals(tiles[x][nextY]) && tile.Type != 520 && tile.Type != 423)
                    {
                        rle = (short)(rle + 1);
                        remainingY--;
                        nextY++;
                    }

                    y = y + rle;

                    if (rle > 0)
                    {
                        // always write lower half
                        tileData[dataIndex++] = (byte)(rle & 0b_1111_1111); //255

                        if (rle <= 255)
                        {
                            // set bit[6] of header1 for byte size rle
                            header1 = (byte)(header1 | 0b_0100_0000); // 64
                        }
                        else
                        {
                            // set bit[7] of header1 for int16 size rle
                            header1 = (byte)(header1 | 0b_1000_0000); //128

                            // grab the upper half of the int16 and stick it in tiledata
                            tileData[dataIndex++] = (byte)((rle & 0b_1111_1111_0000_0000) >> 8); // 65280
                        }
                    }

                    tileData[headerIndex] = header1;
                    // end rle compression

                    bw.Write(tileData, headerIndex, dataIndex - headerIndex);
                }
            }

            return (int)bw.BaseStream.Position;
        }
        public static byte[] SerializeTileData(Tile tile, int version, bool[] tileFrameImportant, out int dataIndex, out int headerIndex)
        {
            int size = version switch
            {
                int v when v >= 269 => 16, // 1.4.4+
                int v when v > 222 => 15, // 1.4.0+
                _ => 13 // default
            };

            byte[] tileData = new byte[size];
            dataIndex = (version >= 269) ? 4 : 3; // 1.4.4+

            byte header4 = (byte)0;
            byte header3 = (byte)0;
            byte header2 = (byte)0;
            byte header1 = (byte)0;

            // tile data

            if (tile.IsActive && tile.Type != (int)TileType.IceByRod)
            {
                // activate bit[1]
                header1 |= 0b_0000_0010;

                // save tile type as byte or int16
                tileData[dataIndex++] = (byte)tile.Type; // low byte
                if (tile.Type > 255)
                {
                    // write high byte
                    tileData[dataIndex++] = (byte)(tile.Type >> 8);

                    // set header1 bit[5] for int16 tile type
                    header1 |= 0b_0010_0000;
                }

                if (tileFrameImportant[tile.Type])
                {
                    // pack UV coords
                    tileData[dataIndex++] = (byte)(tile.U & 0xFF); // low byte
                    tileData[dataIndex++] = (byte)((tile.U & 0xFF00) >> 8); // high byte
                    tileData[dataIndex++] = (byte)(tile.V & 0xFF); // low byte
                    tileData[dataIndex++] = (byte)((tile.V & 0xFF00) >> 8); // high byte
                }

                if (version < 269)
                {
                    if (tile.TileColor != 0 || tile.FullBrightBlock)
                    {

                        var color = tile.TileColor;

                        // downgraded illuminate coating to illuminate paint
                        // IF there is no other paint
                        if (color == 0 && tile.FullBrightBlock)
                        {
                            color = 31;
                        }

                        // set header3 bit[3] for tile color active
                        header3 |= 0b_0000_1000;
                        tileData[dataIndex++] = color;
                    }
                }
                else
                {
                    if (tile.TileColor != 0 && tile.TileColor != 31)
                    {
                        var color = tile.TileColor;

                        // set header3 bit[3] for tile color active
                        header3 |= 0b_0000_1000;
                        tileData[dataIndex++] = color;
                    }
                }
            }

            // wall data
            if (tile.Wall != 0)
            {
                // set header1 bit[2] for wall active
                header1 |= 0b_0000_0100;
                tileData[dataIndex++] = (byte)tile.Wall;

                // save tile wall color
                if (version < 269)
                {
                    if (tile.WallColor != 0 || tile.FullBrightWall)
                    {
                        var color = tile.WallColor;

                        // downgraded illuminate coating to illuminate paint
                        // IF there is no other paint
                        if (color == 0 && version < 269 && tile.FullBrightWall)
                        {
                            color = 31;
                        }

                        // set header3 bit[4] for wall color active
                        header3 |= 0b_0001_0000;
                        tileData[dataIndex++] = color;
                    }
                }
                else
                {
                    // for versions >= 269 upgrade illuminant paint to coating
                    if (tile.WallColor != 0 && tile.WallColor != 31)
                    {
                        var color = tile.WallColor;
                        // set header3 bit[4] for wall color active
                        header3 |= 0b_0001_0000;
                        tileData[dataIndex++] = color;
                    }
                }
            }

            // liquid data
            if (tile.LiquidAmount != 0 && tile.LiquidType != LiquidType.None)
            {
                if (version >= 269 && tile.LiquidType == LiquidType.Shimmer)
                {
                    // shimmer (v 1.4.4 +)
                    header3 = (byte)(header3 | (byte)0b_1000_0000);
                    header1 = (byte)(header1 | (byte)0b_0000_1000);
                }
                else if (tile.LiquidType == LiquidType.Lava)
                {
                    // lava
                    header1 = (byte)(header1 | (byte)0b_0001_0000);
                }
                else if (tile.LiquidType == LiquidType.Honey)
                {
                    // honey
                    header1 = (byte)(header1 | (byte)0b_0001_1000);
                }
                else
                {
                    // water
                    header1 = (byte)(header1 | (byte)0b_0000_1000);
                }

                tileData[dataIndex++] = tile.LiquidAmount;
            }

            // wire data
            if (tile.WireRed)
            {
                // red wire = header2 bit[1]
                header2 |= 0b_0000_0010;

            }
            if (tile.WireBlue)
            {
                // blue wire = header2 bit[2]
                header2 |= 0b_0000_0100;

            }
            if (tile.WireGreen)
            {
                // green wire = header2 bit[3]
                header2 |= 0b_0000_1000;
            }

            // brick style
            byte brickStyle = (byte)((byte)tile.BrickStyle << 4);

            // set bits[4,5,6] of header2
            header2 = (byte)(header2 | brickStyle);


            // actuator data
            if (tile.Actuator)
            {
                // set bit[1] of header3
                header3 |= 0b_0000_0010;
            }
            if (tile.InActive)
            {
                // set bit[2] of header3
                header3 |= 0b_0000_0100;
            }
            if (tile.WireYellow)
            {
                header3 |= 0b_0010_0000;
            }

            // wall high byte
            if (tile.Wall > 255 && version >= 222)
            {
                header3 |= 0b_0100_0000;
                tileData[dataIndex++] = (byte)(tile.Wall >> 8);
            }

            if (version >= 269)
            {
                // custom block lighting (v1.4.4+)
                if (tile.InvisibleBlock)
                {
                    header4 |= 0b_0000_0010;
                }
                if (tile.InvisibleWall)
                {
                    header4 |= 0b_0000_0100;
                }
                if (tile.FullBrightBlock || tile.TileColor == 31) // convert illuminant paint
                {
                    header4 |= 0b_0000_1000;
                }
                if (tile.FullBrightWall || tile.WallColor == 31) // convert illuminant paint
                {
                    header4 |= 0b_0001_0000;
                }

                // header 4 only used in 1.4.4+
                headerIndex = 3;
                if (header4 != 0)
                {
                    // set header4 active flag bit[0] of header3
                    header3 |= 0b_0000_0001;
                    tileData[headerIndex--] = header4;
                }
            }
            else
            {
                headerIndex = 2;
            }

            if (header3 != 0)
            {
                // set header3 active flag bit[0] of header2
                header2 |= 0b_0000_0001;
                tileData[headerIndex--] = header3;
            }
            if (header2 != 0)
            {
                // set header2 active flag bit[0] of header1
                header1 |= 0b_0000_0001;
                tileData[headerIndex--] = header2;
            }

            tileData[headerIndex] = header1;
            return tileData;
        }
        public static int UpdateSectionPointers(uint worldVersion, int[] sectionPointers, BinaryWriter bw)
        {
            bw.BaseStream.Position = 0;
            bw.Write((int)worldVersion);


            bw.BaseStream.Position = (worldVersion >= 140) ? 0x18L : 0x04;
            bw.Write((short)sectionPointers.Length);

            for (int i = 0; i < sectionPointers.Length; i++)
            {
                bw.Write(sectionPointers[i]);
            }

            return (int)bw.BaseStream.Position;
        }
        public static World LoadWorld(string fileName)
        {
            var world = new World();
            try
            {
                using (var fileSream = File.OpenRead(fileName))
                {
                    using (var binaryReader = new BinaryReader(fileSream))
                    {
                        world.Version = binaryReader.ReadUInt32();

                        bool[] tileFrameImportant;
                        int[] sectionPointers;

                        // reset the stream
                        binaryReader.BaseStream.Position = (long)0;

                        // read section pointers and tile frame data
                        if (!LoadSectionHeader(binaryReader, out tileFrameImportant, out sectionPointers, world))
                            throw new Exception("Invalid File Format Section");

                        world.TileFrameImportant = tileFrameImportant;

                        // we should be at the end of the first section
                        if (binaryReader.BaseStream.Position != sectionPointers[0])
                            throw new Exception("Unexpected Position: Invalid File Format Section");

                        // Load the flags
                        LoadHeaderFlags(binaryReader, world, sectionPointers[1]);
                        if (binaryReader.BaseStream.Position != sectionPointers[1])
                            throw new Exception("Unexpected Position: Invalid Header Flags");

                        world.Tiles = LoadTileData(binaryReader, world.TilesWide, world.TilesHigh, (int)world.Version, world.TileFrameImportant);

                        // replace chest load
                        binaryReader.BaseStream.Position = sectionPointers[3];

                        foreach (Sign sign in LoadSignData(binaryReader))
                        {
                            Tile tile = world.Tiles[sign.X][sign.Y];
                            if (tile.IsActive && Tile.IsSign(tile.Type))
                            {
                                world.Signs.Add(sign);
                            }
                        }
                        if (binaryReader.BaseStream.Position != sectionPointers[4])
                            throw new Exception("Unexpected Position: Invalid Sign Data");
                    }
                }

            }
            catch
            {
                throw;
            }
            return world;
        }
        public static bool LoadSectionHeader(BinaryReader r, out bool[] tileFrameImportant, out int[] sectionPointers, World w)
        {
            tileFrameImportant = null;
            sectionPointers = null;
            uint versionNumber = r.ReadUInt32();

            if (versionNumber >= 140) // 135
            {
                // check for chinese
                bool isChinese = (char)r.PeekChar() == 'x';

                string headerFormat = new string(r.ReadChars(7));
                FileType fileType = (FileType)r.ReadByte();

                if (fileType != FileType.World)
                {
                    throw new Exception($"Is not a supported file type: {fileType.ToString()}");
                }

                if (!isChinese && headerFormat != DesktopHeader)
                {
                    throw new Exception("Invalid desktop world header.");
                }

                if (isChinese && headerFormat != ChineseHeader)
                {
                    throw new Exception("Invalid chinese world header.");
                }

                // FileRevision
                r.ReadUInt32();

                r.ReadUInt64(); // load bitflags (currently only bit 1 isFavorite is used)
                // w.IsFavorite = ((flags & 1uL) == 1uL);
            }

            // read file section stream positions
            int sectionCount = r.ReadInt16();
            sectionPointers = new int[sectionCount];
            for (int i = 0; i < sectionCount; i++)
            {
                sectionPointers[i] = r.ReadInt32();
            }

            // Read tile frame importance from bit-packed data
            tileFrameImportant = ReadBitArray(r);

            return true;
        }
        public static void LoadHeaderFlags(BinaryReader r, World w, int expectedPosition)
        {
            w.Title = r.ReadString();

            if (w.Version >= 179)
            {
                if (w.Version == 179)
                    r.ReadInt32();
                else
                    r.ReadString();
                r.ReadUInt64();
            }
            if (w.Version >= 181)
            {
                r.ReadBytes(16);
            }
            r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            w.TilesHigh = r.ReadInt32();
            w.TilesWide = r.ReadInt32();

            // a little future proofing, read any "unknown" flags from the end of the list and save them. We will write these back after we write our "known" flags.
            if (r.BaseStream.Position < expectedPosition)
            {
                // UnknownData
                r.ReadBytes(expectedPosition - (int)r.BaseStream.Position);
            }
        }
        public static Tile[][] LoadTileData(BinaryReader r, int maxX, int maxY, int version, bool[] tileFrameImportant)
        {
            var tiles = new Tile[maxX][];
            int rle;
            for (int x = 0; x < maxX; x++)
            {
                tiles[x] = new Tile[maxY];
                for (int y = 0; y < maxY; y++)
                {
                    try
                    {
                        Tile tile = DeserializeTileData(r, tileFrameImportant, version, out rle);

                        tiles[x][y] = tile;

                        while (rle > 0)
                        {
                            y++;

                            if (y >= maxY)
                            {
                                break;
                                throw new Exception(
                                    $"Invalid Tile Data: RLE Compression outside of bounds [{x},{y}]");
                            }
                            tiles[x][y] = (Tile)tile.Clone();
                            rle--;
                        }
                    }
                    catch (Exception)
                    {
                        // forcing some recovery here

                        for (int x2 = 0; x2 < maxX; x2++)
                        {
                            for (int y2 = 0; y2 < maxY; y2++)
                            {
                                if (tiles[x2][y2] == null) tiles[x2][y2] = new Tile();
                            }
                        }
                        return tiles;
                    }
                }
            }

            return tiles;
        }
        public static IEnumerable<Sign> LoadSignData(BinaryReader r)
        {
            short totalSigns = r.ReadInt16();

            for (int i = 0; i < totalSigns; i++)
            {
                string text = r.ReadString();
                int x = r.ReadInt32();
                int y = r.ReadInt32();
                yield return new Sign(x, y, text);
            }
        }
        public static Tile DeserializeTileData(BinaryReader r, bool[] tileFrameImportant, int version, out int rle)
        {
            Tile tile = new Tile();

            int tileType = -1;
            byte header4 = 0;
            byte header3 = 0;
            byte header2 = 0;
            byte header1 = r.ReadByte();

            bool hasHeader2 = false;
            bool hasHeader3 = false;
            bool hasHeader4 = false;

            // check bit[0] to see if header2 has data
            if ((header1 & 0b_0000_0001) == 0b_0000_0001)
            {
                hasHeader2 = true;
                header2 = r.ReadByte();
            }

            // check bit[0] to see if header3 has data
            if (hasHeader2 && (header2 & 0b_0000_0001) == 0b_0000_0001)
            {
                hasHeader3 = true;
                header3 = r.ReadByte();
            }

            if (version >= 269) // 1.4.4+ 
            {
                // check bit[0] to see if header4 has data
                if (hasHeader3 && (header3 & 0b_0000_0001) == 0b_0000_0001)
                {
                    hasHeader4 = true;
                    header4 = r.ReadByte();
                }
            }

            // check bit[1] for active tile
            bool isActive = (header1 & 0b_0000_0010) == 0b_0000_0010;

            if (isActive)
            {
                tile.IsActive = isActive;
                // read tile type

                if ((header1 & 0b_0010_0000) != 0b_0010_0000) // check bit[5] to see if tile is byte or little endian int16
                {
                    // tile is byte
                    tileType = r.ReadByte();
                }
                else
                {
                    // tile is little endian int16
                    byte lowerByte = r.ReadByte();
                    tileType = r.ReadByte();
                    tileType = tileType << 8 | lowerByte;
                }
                tile.Type = (ushort)tileType; // convert type to ushort after bit operations

                // read frame UV coords
                if (!tileFrameImportant[tileType])
                {
                    tile.U = 0;//-1;
                    tile.V = 0;//-1;
                }
                else
                {
                    // read UV coords
                    tile.U = r.ReadInt16();
                    tile.V = r.ReadInt16();

                    // reset timers
                    if (tile.Type == (int)TileType.Timer)
                    {
                        tile.V = 0;
                    }
                }

                // check header3 bit[3] for tile color
                if ((header3 & 0b_0000_1000) == 0b_0000_1000)
                {
                    tile.TileColor = r.ReadByte();
                }
            }

            // Read Walls
            if ((header1 & 0b_0000_0100) == 0b_0000_0100) // check bit[3] bit for active wall
            {
                tile.Wall = r.ReadByte();


                // check bit[4] of header3 to see if there is a wall color
                if ((header3 & 0b_0001_0000) == 0b_0001_0000)
                {
                    tile.WallColor = r.ReadByte();
                }
            }

            // check for liquids, grab the bit[3] and bit[4], shift them to the 0 and 1 bits
            byte liquidType = (byte)((header1 & 0b_0001_1000) >> 3);
            if (liquidType != 0)
            {
                tile.LiquidAmount = r.ReadByte();
                tile.LiquidType = (LiquidType)liquidType; // water, lava, honey

                // shimmer (v 1.4.4 +)
                if (version >= 269 && (header3 & 0b_1000_0000) == 0b_1000_0000)
                {
                    tile.LiquidType = LiquidType.Shimmer;
                }
            }

            // check if we have data in header2 other than just telling us we have header3
            if (header2 > 1)
            {
                // check bit[1] for red wire
                if ((header2 & 0b_0000_0010) == 0b_0000_0010)
                {
                    tile.WireRed = true;
                }
                // check bit[2] for blue wire
                if ((header2 & 0b_0000_0100) == 0b_0000_0100)
                {
                    tile.WireBlue = true;
                }
                // check bit[3] for green wire
                if ((header2 & 0b_0000_1000) == 0b_0000_1000)
                {
                    tile.WireGreen = true;
                }

                // grab bits[4, 5, 6] and shift 4 places to 0,1,2. This byte is our brick style
                byte brickStyle = (byte)((header2 & 0b_0111_0000) >> 4);
                // if (brickStyle != 0 && TileProperties.Count > tile.Type && TileProperties[tile.Type].HasSlopes)
                if (brickStyle != 0 && 693 > tile.Type)
                {
                    tile.BrickStyle = (BrickStyle)brickStyle;
                }
            }

            // check if we have data in header3 to process
            if (header3 > 1)
            {
                // check bit[1] for actuator
                if ((header3 & 0b_0000_0010) == 0b_0000_0010)
                {
                    tile.Actuator = true;
                }

                // check bit[2] for inactive due to actuator
                if ((header3 & 0b_0000_0100) == 0b_0000_0100)
                {
                    tile.InActive = true;
                }

                if ((header3 & 0b_0010_0000) == 0b_0010_0000)
                {
                    tile.WireYellow = true;
                }

                if (version >= 222)
                {
                    if ((header3 & 0b_0100_0000) == 0b_0100_0000)
                    {
                        tile.Wall = (ushort)(r.ReadByte() << 8 | tile.Wall);

                    }
                }
            }

            if (version >= 269 && header4 > (byte)1)
            {
                if ((header4 & 0b_0000_0010) == 0b_0000_0010)
                {
                    tile.InvisibleBlock = true;
                }
                if ((header4 & 0b_0000_0100) == 0b_0000_0100)
                {
                    tile.InvisibleWall = true;
                }
                if ((header4 & 0b_0000_1000) == 0b_0000_1000)
                {
                    tile.FullBrightBlock = true;
                }
                if ((header4 & 0b_0001_0000) == 0b_0001_0000)
                {
                    tile.FullBrightWall = true;
                }
            }

            // get bit[6,7] shift to 0,1 for RLE encoding type
            // 0 = no RLE compression
            // 1 = byte RLE counter
            // 2 = int16 RLE counter
            // 3 = not implemented, assume int16
            byte rleStorageType = (byte)((header1 & 192) >> 6);

            rle = rleStorageType switch
            {
                0 => (int)0,
                1 => (int)r.ReadByte(),
                _ => (int)r.ReadInt16()
            };

            return tile;
        }
        public static bool[] ReadBitArray(BinaryReader reader)
        {
            // get the number of bits
            int length = reader.ReadInt16();

            // read the bit data
            var booleans = new bool[length];
            byte data = 0;
            byte bitMask = 128;
            for (int i = 0; i < length; i++)
            {
                // If we read the last bit mask (B1000000 = 0x80 = 128), read the next byte from the stream and start the mask over.
                // Otherwise, keep incrementing the mask to get the next bit.
                if (bitMask != 128)
                {
                    bitMask = (byte)(bitMask << 1);
                }
                else
                {
                    data = reader.ReadByte();
                    bitMask = 1;
                }

                // Check the mask, if it is set then set the current boolean to true
                if ((data & bitMask) == bitMask)
                {
                    booleans[i] = true;
                }
            }

            return booleans;
        }

        static List<string> WorldSearchPath = new();

        public const string DesktopHeader = "relogic";
        public const string ChineseHeader = "xindong";
    }
}
