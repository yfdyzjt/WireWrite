using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireWrite.Script;
using static System.Net.Mime.MediaTypeNames;

namespace WireWrite.Terraria
{
    public partial class World
    {
        public uint Version;
        public Tile[][] Tiles;
        public string Title { get; set; }
        public int TilesWide { get; set; }
        public int TilesHigh { get; set; }
    }
}
