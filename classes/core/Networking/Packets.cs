using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Gameplay.Entities.Projectiles;
using Tiled.Gameplay.Items;

namespace Tiled.Networking.Shared
{
    /// <summary>
    /// dataStructures that server and client share (doesnt mean that the values are the same!)
    /// </summary>
    public static class NetShared
    {
        /// <summary>
        /// PlayerID -> PlayerEntity, good for doing stuff to certain players
        /// </summary>
        public static Dictionary<int, EPlayer> clientIDToPlayer = new Dictionary<int, EPlayer>();

        /// <summary>
        /// Array of entities [NET SYNCED]
        /// </summary>
        public static Dictionary<int, Entity> netEntitites = new Dictionary<int, Entity>();

        /// <summary>
        /// reads from the packet and spawns a new entity, client and server run this seperately
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="id"></param>
        public static void SpawnEntityShared(SpawnEntityPacket packet)
        {
            switch(packet.spawnType)
            {
                case ENetEntitySpawnType.Item:
                    EItem newItem = EItem.CreateItem(packet.itemType);
                    newItem.netID = packet.entityID;
                    netEntitites.Add(packet.entityID, newItem);
                    newItem.position = packet.position;
                    newItem.velocity = packet.velocity;
                    break;

                case ENetEntitySpawnType.Entity:
                    Entity newEntity = new Entity();
                    newEntity.netID = packet.entityID;
                    netEntitites.Add(packet.entityID, newEntity);
                    newEntity.position = packet.position;
                    newEntity.velocity = packet.velocity;
                    break;

                case ENetEntitySpawnType.Projectile:
                    EProjectile newProjectile = EProjectile.CreateProjectile(packet.projectileType);
                    newProjectile.netID = packet.entityID;
                    netEntitites.Add(packet.entityID, newProjectile);
                    newProjectile.position = packet.position;
                    newProjectile.velocity = packet.velocity;
                    break;
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
        RequestInventory,
        RequestItemPickup,
        RequestItemSwing,

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
        ReceiveInventory,
        ReceiveInventoryChange,
        ReceiveTileBreak,
        ReceiveSelectedSlotChange,
        ReceiveEntities,

        //ticking receive
        ReceiveWorldUpdate,
        ReceiveClientUpdate,
    }


    public enum ENetEntitySpawnType
    {
        Entity,
        Item,
        Projectile
    }

    public interface IPacket
    {
        void PacketToNetOutgoingMessage(NetOutgoingMessage msg);
        void PacketToNetIncomingMessage(NetIncomingMessage msg);
    }

    public abstract class Packet : IPacket
    {
        /// <summary>
        /// Reads the message and sets the member variables
        /// </summary>
        /// <param name="msg"></param>
        public abstract void PacketToNetIncomingMessage(NetIncomingMessage msg);

        /// <summary>
        /// Reads member variables and writes them to the message
        /// </summary>
        /// <param name="msg"></param>

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

        public ENetEntitySpawnType spawnType;

        /// <summary>
        /// SERVER ASSIGNED ENTITY ID
        /// </summary>
        public int entityID;
        public EEntityType entityType;
        public EItemType itemType;
        public EProjectileType projectileType;
        
        public Vector2 position;
        public Vector2 velocity;

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            spawnType = (ENetEntitySpawnType)msg.ReadByte();
            entityID = msg.ReadInt32();

            switch(spawnType)
            {
                case ENetEntitySpawnType.Item:
                    itemType = (EItemType)msg.ReadByte();
                    break;

                case ENetEntitySpawnType.Entity:
                    entityType = (EEntityType)msg.ReadByte();
                    break;

                case ENetEntitySpawnType.Projectile:
                    projectileType = (EProjectileType)msg.ReadByte();
                    break;
            }

            position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write((byte)spawnType);
            msg.Write(entityID);

            switch (spawnType)
            {
                case ENetEntitySpawnType.Item:
                    msg.Write((byte)itemType);
                    break;

                case ENetEntitySpawnType.Entity:
                    msg.Write((byte)entityType);
                    break;

                case ENetEntitySpawnType.Projectile:
                    msg.Write((byte)projectileType);
                    break;
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

    public class InventoryPacket : Packet
    {
        public int size;
        public ContainerItem[] items;


        public InventoryPacket()
        {

        }

        public InventoryPacket(int size, ContainerItem[] items)
        {
            this.items = items;
            this.size = size;
        }

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            size = msg.ReadInt32();
            items = new ContainerItem[size];

            for (int i = 0; i < size; i++)
            {
                items[i].type = (EItemType)msg.ReadByte();
                items[i].stack = msg.ReadUInt16();
            }
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(size);
            foreach (var item in items)
            {
                msg.Write((byte)item.type);
                msg.Write(item.stack);
            }
        }
    }

    public class ActiveEntityPacket : Packet
    {
        public int arrayLength;
        public NetEntity[] entities;

        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            arrayLength = msg.ReadInt32();
            entities = new NetEntity[arrayLength];

            for (int i = 0; i < arrayLength; i++)
            {
                entities[i].netID = msg.ReadInt32();
                entities[i].spawnType = (ENetEntitySpawnType)msg.ReadByte();
                entities[i].itemType = (EItemType)msg.ReadByte();
                entities[i].type = (EEntityType)msg.ReadByte();
                entities[i].projectileType = (EProjectileType)msg.ReadByte();
                entities[i].position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            }
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(entities.Length);

            for (int i = 0; i < entities.Length; i++)
            {
                msg.Write(entities[i].netID);
                msg.Write((byte)entities[i].spawnType);
                msg.Write((byte)entities[i].itemType);
                msg.Write((byte)entities[i].type);
                msg.Write((byte)entities[i].projectileType);
                msg.Write(entities[i].position.X);
                msg.Write(entities[i].position.Y);
            }
        }
    }
}