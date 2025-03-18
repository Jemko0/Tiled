using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Gameplay.Entities.Projectiles;
using Tiled.Gameplay.Items;
using Tiled.Inventory;

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
        /// Dict of registered net containers
        /// </summary>
        public static Dictionary<uint, Container> netContainers = new Dictionary<uint, Container>();

        /// <summary>
        /// reads from the packet and spawns a new entity, client and server run this seperately
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="id"></param>
        public static Entity SpawnEntityShared(SpawnEntityPacket packet)
        {
            switch(packet.spawnType)
            {
                default:
                    return null;

                case ENetEntitySpawnType.Item:
                    EItem newItem = EItem.CreateItem(packet.itemType);
                    newItem.netID = packet.entityID;
                    netEntitites.Add(packet.entityID, newItem);
                    newItem.position = packet.position;
                    newItem.velocity = packet.velocity;
                    return newItem;

                case ENetEntitySpawnType.Entity:
                    Entity newEntity = new Entity();
                    newEntity.netID = packet.entityID;
                    netEntitites.Add(packet.entityID, newEntity);
                    newEntity.position = packet.position;
                    newEntity.velocity = packet.velocity;
                    return newEntity;

                case ENetEntitySpawnType.Projectile:
                    EProjectile newProjectile = EProjectile.CreateProjectile(packet.projectileType);
                    newProjectile.netID = packet.entityID;
                    netEntitites.Add(packet.entityID, newProjectile);
                    newProjectile.position = packet.position;
                    newProjectile.velocity = packet.velocity;
                    return newProjectile;
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
        //RequestServerInfo,
        RequestWorld,
        RequestWorldChanges,
        RequestActiveEntities,
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
        //ReceiveServerInfo,
        ReceiveWorld,
        ReceiveWorldChanges,
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
        ReceiveWorldChunk,
        ReceiveWorldComplete,
        ReceiveDamage,
        ReceiveClientContainer,

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

    public class WorldChangesPacket : Packet
    {
        public int length;
        public NetWorldChange[] changes;
        public override void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            length = msg.ReadInt32();
            changes = new NetWorldChange[length];

            for (int i = 0; i < length; i++)
            {
                changes[i].x = msg.ReadInt32();
                changes[i].y = msg.ReadInt32();
                changes[i].type = (ETileType)msg.ReadByte();
            }
           
        }

        public override void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(length);
            for (int i = 0; i < changes.Length; i++)
            {
                msg.Write(changes[i].x);
                msg.Write(changes[i].y);
                msg.Write((byte)changes[i].type);
            }
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
        public uint containerID;
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
            containerID = msg.ReadUInt32();
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
            msg.Write(containerID);
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

    public class WorldChunkPacket : IPacket
    {
        public int chunkX;
        public int chunkY;
        public int chunkSize;
        public int totalChunks;
        public ETileType[,] tiles;

        public void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(chunkX);
            msg.Write(chunkY);
            msg.Write(chunkSize);
            msg.Write(totalChunks);

            // Send tile data
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    msg.Write((byte)tiles[x, y]);
                }
            }
        }

        public void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            chunkX = msg.ReadInt32();
            chunkY = msg.ReadInt32();
            chunkSize = msg.ReadInt32();
            totalChunks = msg.ReadInt32();

            tiles = new ETileType[chunkSize, chunkSize];
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    tiles[x, y] = (ETileType)msg.ReadByte();
                }
            }
        }
    }

    public class DamagePacket : IPacket
    {
        public uint damage;
        public int toID;
        public bool isPlayer;
        public void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            damage = msg.ReadUInt32();
            toID = msg.ReadInt32();
            isPlayer = msg.ReadBoolean();
        }

        public void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(damage);
            msg.Write(toID);
            msg.Write(isPlayer);
        }
    }

    public class InvMoveSlotPacket : IPacket
    {
        public int fromID;
        public int toID;

        public void PacketToNetIncomingMessage(NetIncomingMessage msg)
        {
            fromID = msg.ReadInt32();
            toID = msg.ReadInt32();
        }

        public void PacketToNetOutgoingMessage(NetOutgoingMessage msg)
        {
            msg.Write(fromID);
            msg.Write(toID);
        }
    }
}