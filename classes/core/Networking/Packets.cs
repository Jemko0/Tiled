using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Tiled.Gameplay;

namespace Tiled.Networking.Shared
{
    public static class NetShared
    {
        public static Dictionary<int, EPlayer> clientIDPairs = new Dictionary<int, EPlayer>();
    }

    public enum EPacketType
    {
        //one time send
        RequestPlayerID,
        RequestServerInfo,
        RequestWorld,
        RequestClientSpawn,
        RequestOtherClients,

        //one time receive
        ReceivePlayerID,
        ReceiveServerInfo,
        ReceiveWorld,
        ReceiveSpawnClient,
        ReceiveOtherClients,
        ReceiveClientDisconnected,

        //ticking receive
        ReceiveWorldUpdate,
        ReceiveClientUpdate,
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

    public class ClientSpawnPacket : Packet
    {
        public ClientSpawnPacket()
        {
        }

        public int playerID { get; set; }
        public Vector2 position { get; set; }
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            playerID = msg.ReadInt32();
            position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
        }
        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(playerID);
            msg.Write(position.X);
            msg.Write(position.Y);
        }
    }

    public class ClientSpawnRequestPacket : Packet
    {
        int requestClientID;
        public ClientSpawnRequestPacket(int requestorID)
        {
            requestClientID = requestorID;
        }

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            requestClientID = msg.ReadInt32();
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(requestClientID);
        }
    }
    public class ClientWorldRequestPacket : Packet
    {
        int requestClientID;
        public ClientWorldRequestPacket(int requestorID)
        {
            requestClientID = requestorID;
        }
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            requestClientID = msg.ReadInt32();
        }
        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(requestClientID);
        }
    }

    public class WorldPacket : Packet
    {
        public int seed;
        public int maxTilesX;
        public int maxTilesY;

        public WorldPacket(int seed, int maxX, int maxY)
        {
            this.seed = seed;
            maxTilesX = maxX;
            maxTilesY = maxY;
        }

        public WorldPacket()
        {
        }

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            seed = msg.ReadInt32();
            maxTilesX = msg.ReadInt32();
            maxTilesY = msg.ReadInt32();
        }
        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(seed);
            msg.Write(maxTilesX);
            msg.Write(maxTilesY);
        }
    }

    public class WorldUpdatePacket : Packet
    {
        public float time;

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            time = msg.ReadFloat();
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(time);
        }
    }

    public class ClientUpdatePacket : Packet
    {
        public int playerID;
        public Vector2 position;
        public Vector2 velocity;

        public ClientUpdatePacket(int playerID, Vector2 position, Vector2 velocity)
        {
            this.playerID = playerID;
            this.position = position;
            this.velocity = velocity;
        }

        public ClientUpdatePacket()
        {
        }

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            playerID = msg.ReadInt32();
            position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(playerID);
            msg.Write(position.X);
            msg.Write(position.Y);
            msg.Write(velocity.X);
            msg.Write(velocity.Y);
        }
    }
    public class ClientDisconnectedPacket : Packet
    {
        public int disconnectedPlayerID;

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            disconnectedPlayerID = msg.ReadInt32();
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(disconnectedPlayerID);
        }
    }
}