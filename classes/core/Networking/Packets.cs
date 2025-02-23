using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Gameplay.Items;

namespace Tiled.Networking.Shared
{
    /// <summary>
    /// data that server and client share (doesnt mean that the values are the same!)
    /// </summary>
    public static class NetShared
    {
        /// <summary>
        /// PlayerID -> PlayerEntity, good for doing stuff to certain players
        /// </summary>
        public static Dictionary<int, EPlayer> clientIDPairs = new Dictionary<int, EPlayer>();

        /// <summary>
        /// net synced array of entities
        /// </summary>
        public static Dictionary<int, Entity> netEntitites = new Dictionary<int, Entity>();

        /// <summary>
        /// reads from the packet and spawns a new entity, client and server run this seperately
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="id"></param>
        public static void SpawnEntityShared(SpawnEntityPacket packet)
        {
            if (packet.isItem)
            {
                EItem newItem = EItem.CreateItem(packet.itemType);
                newItem.netID = packet.entityID;
                netEntitites.Add(packet.entityID, newItem);
                newItem.position = packet.position;
                newItem.velocity = packet.velocity;
            }
            else
            {
                Entity newEntity = new Entity();
                newEntity.netID = packet.entityID;
                netEntitites.Add(packet.entityID, newEntity);
                newEntity.position = packet.position;
                newEntity.velocity = packet.velocity;
            }
        }

        public static void DestroyEntity(int netID)
        {
            netEntitites[netID].LocalDestroy();
            netEntitites.Remove(netID);
        }
    }

    public enum EPacketType
    {
        //one time send
        RequestPlayerID,
        RequestServerInfo,
        RequestWorld,
        RequestWorldChanges,
        RequestClientSpawn,
        RequestOtherClients,
        RequestTileChange,
        RequestSpawnEntity,
        RequestDestroyEntity,

        //one time receive
        ReceivePlayerID,
        ReceiveServerInfo,
        ReceiveWorld,
        ReceiveWorldChange,
        ReceiveSpawnClient,
        ReceiveOtherClients,
        ReceiveClientDisconnected,
        ReceiveTileChange,
        ReceiveSpawnEntity,
        ReceiveServerUpdateEntity,
        ReceiveDestroyEntity,

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

    public class IDPacket : Packet
    {
        public IDPacket(int ID)
        {
            this.ID = ID;
        }
        public int ID { get; set; }
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            ID = msg.ReadInt32();
        }
        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(ID);
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

        public WorldPacket(int seed, int maxX, int maxY, List<NetWorldChange> changes)
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

    public class WorldChangesPacket : Packet
    {
        public int x;
        public int y;
        public byte type;
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            x = msg.ReadInt32();
            y = msg.ReadInt32();
            type = msg.ReadByte();
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(x);
            msg.Write(y);
            msg.Write(type);
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

    public class TileChangePacket : Packet
    {
        public ETileType tileType;
        public int x;
        public int y;

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            tileType = (ETileType)msg.ReadByte();
            x = msg.ReadInt32();
            y = msg.ReadInt32();
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write((byte)tileType);
            msg.Write(x);
            msg.Write(y);
        }
    }

    public class SpawnEntityPacket : Packet
    {
        //first bit corresponds to if this is an item

        public bool isItem;

        /// <summary>
        /// SERVER ASSIGNED ENTITY ID
        /// </summary>
        public int entityID;
        public EEntityType entityType;
        public EItemType itemType;
        
        public Vector2 position;
        public Vector2 velocity;

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            isItem = msg.ReadBoolean();
            entityID = msg.ReadInt32();

            if(isItem)
            {
                itemType = (EItemType)msg.ReadByte();
            }
            else
            {
                entityType = (EEntityType)msg.ReadByte();
            }

            position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(isItem);
            msg.Write(entityID);

            if (isItem)
            {
                msg.Write((byte)itemType);
            }
            else
            {
                msg.Write((byte)entityType);
            }

            msg.Write(position.X);
            msg.Write(position.Y);
            msg.Write(velocity.X);
            msg.Write(velocity.Y);
        }
    }

    public class EntityUpdatePacket : Packet
    {
        public int entityID;
        public Vector2 position;
        public Vector2 velocity;
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            entityID = msg.ReadInt32();
            position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(entityID);
            msg.Write(position.X);
            msg.Write(position.Y);
            msg.Write(velocity.X);
            msg.Write(velocity.Y);
        }
    }
}