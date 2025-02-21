using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Tiled.Networking.Shared
{
    public enum EPacketType
    {
        RequestPlayerID,
        RequestClientSpawn,
        ReceivePlayerID,
        SpawnClient,
    }

    public interface IPacket
    {
        void PacketToNetOutgoingMessage(NetOutgoingMessage msg);
        void PacketToNetIncomingMessage(NetIncomingMessage msg);
    }

    public abstract class Packet : IPacket
    {
        public abstract void PacketToNetIncomingMessage(NetIncomingMessage msg);

        public abstract void PacketToNetOutgoingMessage(NetOutgoingMessage msg);
    }

    public class PlayerIDPacket : Packet
    {
        public PlayerIDPacket(int playerID)
        {
            this.playerID = playerID;
        }
        public int playerID { get; set; }
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            playerID = msg.ReadInt32();
        }
        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(playerID);
        }
    }
}