using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled.Networking
{
    public class Packet
    {
        public string type { get; set; }
        public PacketData data { get; set; }
    }

    public class PacketData
    {
        public int id { get; set; }
        public int tickrate { get; set; }
        public int seed { get; set; }
        public int maxTilesX { get; set; }
        public int maxTilesY { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float velX { get; set; }
        public float velY { get; set; }

        public object[] objectArray { get; set; }
    }
}
