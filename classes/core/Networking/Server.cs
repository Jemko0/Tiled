using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Timers;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Gameplay.Entities.Projectiles;
using Tiled.Gameplay.Items;
using Tiled.Networking.Shared;

namespace Tiled
{

    public class TiledServer
    {
        public NetServer server;
        public Thread serverThread;
        public bool running = true;

        public delegate void ServerLog(string msg);
        public event ServerLog onServerLog;

        int lastClientID = 0;
        int lastEntityID = int.MinValue;
        public TiledServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("tiled");

            config.Port = 7777;

            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.ConnectionTimeout = 5.0f;
            config.PingInterval = 3.0f;
            server = new NetServer(config);
            server.Start();
            Program.GetGame().Exiting += TiledServer_Exiting;

            Main.netMode = ENetMode.Server;
            Main.inTitle = false;
            serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.Name = "Server Thread";
            serverThread.Start();
        }

        public static Dictionary<NetConnection, int> socketToClientID = new Dictionary<NetConnection, int>();
        public static List<NetWorldChange> worldChanges = new List<NetWorldChange>();

        private void TiledServer_Exiting(object sender, System.EventArgs e)
        {
            running = false;
        }
    
        public void StartServer()
        {
            onServerLog.Invoke("Server Started");

            // Generate world
            Program.GetGame().world = new World();
            Program.GetGame().world.worldTime = 4;
            Program.GetGame().world.seed = 85938471;
            World.maxTilesX = 1200;
            World.maxTilesY = 800;
            Program.GetGame().world.StartWorldGeneration();
            
            while(World.isGenerating)
            {

            }

            System.Timers.Timer timer = new System.Timers.Timer(41);
            timer.Elapsed += ServerTick;
            timer.Start();

            while (running)
            {
                NetIncomingMessage msg;
                while ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                            msg.SenderConnection.Approve();
                            break;
    
                        case NetIncomingMessageType.Data:
    
                            byte type = msg.ReadByte();
    
                            switch ((EPacketType)type)
                            {
                                case EPacketType.RequestPlayerID:
                                    lastClientID++;
                                    IDPacket playerIDPacket = new IDPacket(lastClientID);

                                    socketToClientID.Add(msg.SenderConnection, lastClientID);

                                    NetOutgoingMessage outMsg = server.CreateMessage();

                                    outMsg.Write((byte)EPacketType.ReceivePlayerID);
                                    playerIDPacket.PacketToNetOutgoingMessage(outMsg);
    
                                    server.SendMessage(outMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    server.FlushSendQueue();
                                    break;
    
                                case EPacketType.RequestClientSpawn:
                                    ClientSpawnPacket clientSpawnPacket = new ClientSpawnPacket();
                                    clientSpawnPacket.playerID = lastClientID;
                                    clientSpawnPacket.position = new Vector2(1024, 64);
                                    NetOutgoingMessage spawnOutMsg = server.CreateMessage();
                                    
                                    spawnOutMsg.Write((byte)EPacketType.ReceiveSpawnClient);
                                    clientSpawnPacket.PacketToNetOutgoingMessage(spawnOutMsg);

                                    EPlayer newClient = Entity.NewEntity<EPlayer>();
                                    newClient.Initialize(EEntityType.Player);
                                    newClient.clientID = clientSpawnPacket.playerID;
                                    //newClient.netID = clientSpawnPacket.playerID;
                                    newClient.position = clientSpawnPacket.position;

                                    NetShared.clientIDToPlayer.Add(clientSpawnPacket.playerID, newClient);

                                    server.SendToAll(spawnOutMsg, NetDeliveryMethod.ReliableOrdered);
                                    server.FlushSendQueue();
                                    break;

                                case EPacketType.RequestWorld:
                                    WorldPacket worldPacket = new WorldPacket();
                                    worldPacket.maxTilesX = World.maxTilesX;
                                    worldPacket.maxTilesY = World.maxTilesY;
                                    worldPacket.seed = Program.GetGame().world.seed;

                                    NetOutgoingMessage worldOutMsg = server.CreateMessage();
                                    worldOutMsg.Write((byte)EPacketType.ReceiveWorld);
                                    worldPacket.PacketToNetOutgoingMessage(worldOutMsg);

                                    server.SendMessage(worldOutMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    server.FlushSendQueue();

                                    // Wait for world generation to complete before sending chunks
                                    while (World.isGenerating)
                                    {
                                        Thread.Sleep(100);
                                    }

                                    // Send world in chunks
                                    SendWorldChunks(msg.SenderConnection);
                                    break;

                                case EPacketType.RequestWorldChanges:
                                    WorldChangesPacket changesPacket = new WorldChangesPacket();

                                    changesPacket.length = worldChanges.Count;
                                    changesPacket.changes = new NetWorldChange[worldChanges.Count];

                                    for (int i = 0; i < changesPacket.length; i++)
                                    {
                                        changesPacket.changes[i] = worldChanges[i];
                                    }

                                    NetOutgoingMessage changesOutMsg = server.CreateMessage();

                                    changesOutMsg.Write((byte)EPacketType.ReceiveWorldChanges);
                                    
                                    changesPacket.PacketToNetOutgoingMessage(changesOutMsg);
                                    server.SendMessage(changesOutMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    server.FlushSendQueue();
                                    break;

                                case EPacketType.RequestActiveEntities:
                                    //SEND CURRENTLY ACTIVE ENTITIES TO NEW CLIENT
                                    ActiveEntityPacket entityData = new ActiveEntityPacket();
                                    entityData.arrayLength = NetShared.netEntitites.Count;
                                    entityData.entities = new NetEntity[entityData.arrayLength];

                                    for (int i = 0; i < entityData.arrayLength; i++)
                                    {
                                        NetEntity current = new NetEntity();
                                        current.netID = NetShared.netEntitites.ElementAt(i).Value.netID;

                                        if (NetShared.netEntitites.ElementAt(i).Value is EItem)
                                        {
                                            current.spawnType = ENetEntitySpawnType.Item;
                                        }
                                        else if (NetShared.netEntitites.ElementAt(i).Value is EProjectile)
                                        {
                                            current.spawnType = ENetEntitySpawnType.Projectile;
                                        }

                                        switch (current.spawnType)
                                        {
                                            case ENetEntitySpawnType.Item:
                                                current.itemType = (NetShared.netEntitites.ElementAt(i).Value as EItem).type;
                                                break;

                                            case ENetEntitySpawnType.Entity:
                                                current.type = NetShared.netEntitites.ElementAt(i).Value.entityType;
                                                break;

                                            case ENetEntitySpawnType.Projectile:
                                                current.projectileType = (NetShared.netEntitites.ElementAt(i).Value as EProjectile).type;
                                                break;
                                        }

                                        current.position = NetShared.netEntitites.ElementAt(i).Value.position;
                                        entityData.entities[i] = current;
                                    }

                                    NetOutgoingMessage entitiesMsg = server.CreateMessage();
                                    entitiesMsg.Write((byte)EPacketType.ReceiveEntities);
                                    entityData.PacketToNetOutgoingMessage(entitiesMsg);
                                    server.SendMessage(entitiesMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    server.FlushSendQueue();
                                    break;



                                case EPacketType.ReceiveClientUpdate:
                                    ClientUpdatePacket clientUpdatePacket = new ClientUpdatePacket();
                                    clientUpdatePacket.PacketToNetIncomingMessage(msg);

                                    if(NetShared.clientIDToPlayer.ContainsKey(clientUpdatePacket.playerID))
                                    {
                                        NetShared.clientIDToPlayer[clientUpdatePacket.playerID].position = clientUpdatePacket.position;
                                        NetShared.clientIDToPlayer[clientUpdatePacket.playerID].velocity = clientUpdatePacket.velocity;
                                    }
                                    else
                                    {
                                        onServerLog.Invoke("Client ID not found: " + clientUpdatePacket.playerID);
                                        break;
                                    }
                                    
                                    ClientUpdatePacket multicastUpdatePacket = new ClientUpdatePacket();
                                    multicastUpdatePacket.playerID = clientUpdatePacket.playerID;
                                    multicastUpdatePacket.position = NetShared.clientIDToPlayer[clientUpdatePacket.playerID].position;
                                    multicastUpdatePacket.velocity = NetShared.clientIDToPlayer[clientUpdatePacket.playerID].velocity;

                                    NetOutgoingMessage multicastUpdateMsg = server.CreateMessage();
                                    multicastUpdateMsg.Write((byte)EPacketType.ReceiveClientUpdate);
                                    multicastUpdatePacket.PacketToNetOutgoingMessage(multicastUpdateMsg);

                                    server.SendToAll(multicastUpdateMsg, msg.SenderConnection, NetDeliveryMethod.UnreliableSequenced, 0);
                                    server.FlushSendQueue();
                                    break;

                                case EPacketType.RequestOtherClients:
                                    IDPacket requestOthersPacket = new IDPacket(-1);
                                    requestOthersPacket.PacketToNetIncomingMessage(msg);

                                    int requestID = requestOthersPacket.ID;

                                    foreach (KeyValuePair<int, EPlayer> pair in NetShared.clientIDToPlayer)
                                    {
                                        if (pair.Key != requestID)
                                        {
                                            ClientSpawnPacket otherClientPacket = new ClientSpawnPacket();
                                            otherClientPacket.playerID = pair.Key;
                                            otherClientPacket.position = pair.Value.position;
                                            NetOutgoingMessage otherClientMsg = server.CreateMessage();

                                            otherClientMsg.Write((byte)EPacketType.ReceiveSpawnClient);
                                            otherClientPacket.PacketToNetOutgoingMessage(otherClientMsg);

                                            server.SendMessage(otherClientMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                        }
                                    }
                                    server.FlushSendQueue();
                                    break;

                                case EPacketType.RequestTileChange:
                                    TileChangePacket change = new TileChangePacket();
                                    change.PacketToNetIncomingMessage(msg);

                                    ETileType previousTile = World.tiles[change.x, change.y];

                                    
                                    //send tile
                                    SendTileSquare(change);
                                    break;

                                case EPacketType.RequestSpawnEntity:
                                    SpawnEntityPacket newEntityPacket = new SpawnEntityPacket();
                                    newEntityPacket.PacketToNetIncomingMessage(msg);

                                    lastEntityID++;
                                    if(lastEntityID == -1)
                                    {
                                        lastEntityID++;
                                    }

                                    newEntityPacket.entityID = lastEntityID;

                                    //spawn entity locally/server
                                    NetShared.SpawnEntityShared(newEntityPacket);

                                    //spawn for everyone else
                                    NetOutgoingMessage newEntityMsg = server.CreateMessage();

                                    newEntityMsg.Write((byte)EPacketType.ReceiveSpawnEntity);
                                    newEntityPacket.PacketToNetOutgoingMessage(newEntityMsg);

                                    server.SendToAll(newEntityMsg, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.RequestDestroyEntity:
                                    IDPacket idPacket = new IDPacket(-1);
                                    idPacket.PacketToNetIncomingMessage(msg);

                                    NetShared.netEntitites[idPacket.ID].LocalDestroy();

                                    NetOutgoingMessage entityDestroyMsg = server.CreateMessage();
                                    entityDestroyMsg.Write((byte)EPacketType.ReceiveDestroyEntity);
                                    idPacket.PacketToNetOutgoingMessage(entityDestroyMsg);

                                    server.SendToAll(entityDestroyMsg, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.RequestInventory:
                                    int requestInventoryID = socketToClientID[msg.SenderConnection];

                                    int containerSize = 10;
                                    NetShared.clientIDToPlayer[requestInventoryID].inventory = new Inventory.Container(containerSize);
                                    NetShared.clientIDToPlayer[requestInventoryID].inventory.SetItem(0, new ContainerItem(EItemType.BasePickaxe, 1));
                                    NetShared.clientIDToPlayer[requestInventoryID].inventory.SetItem(1, new ContainerItem(EItemType.BaseAxe, 1));
                                    NetShared.clientIDToPlayer[requestInventoryID].inventory.SetItem(2, new ContainerItem(EItemType.Torch, 16));
                                    NetShared.clientIDToPlayer[requestInventoryID].inventory.SetItem(3, new ContainerItem(EItemType.Bomb, 5));

                                    InventoryPacket inventoryPacket = new InventoryPacket();
                                    inventoryPacket.size = containerSize;
                                    inventoryPacket.items = NetShared.clientIDToPlayer[requestInventoryID].inventory.items;

                                    NetOutgoingMessage invMsg = server.CreateMessage();
                                    invMsg.Write((byte)EPacketType.ReceiveInventory);
                                    inventoryPacket.PacketToNetOutgoingMessage(invMsg);

                                    server.SendMessage(invMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.RequestItemPickup:
                                    int pickupClientID = socketToClientID[msg.SenderConnection];
                                    EItem? collidingEntity = NetShared.clientIDToPlayer[pickupClientID].collision.GetCollidingEntity() as EItem;

                                    if(collidingEntity != null)
                                    {
                                        NetShared.clientIDToPlayer[pickupClientID].inventory.RepAdd(pickupClientID, new ContainerItem(collidingEntity.type, collidingEntity.count));
                                        ServerDestroyEntity(collidingEntity.netID);

                                        //let clent know that we updated his inventory fr
                                        InventoryPacket invChangePacket = new InventoryPacket();
                                        invChangePacket.size = NetShared.clientIDToPlayer[pickupClientID].inventory.items.Length;
                                        invChangePacket.items = NetShared.clientIDToPlayer[pickupClientID].inventory.items;

                                        NetOutgoingMessage newInventoryMsg = server.CreateMessage();
                                        newInventoryMsg.Write((byte)EPacketType.ReceiveInventoryChange);
                                        invChangePacket.PacketToNetOutgoingMessage(newInventoryMsg);

                                        server.SendMessage(newInventoryMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    }
                                    break;

                                case EPacketType.RequestItemSwing:
                                    int swingClientID = socketToClientID[msg.SenderConnection];
                                    Point tile = new Point(msg.ReadInt32(), msg.ReadInt32());
                                    EPlayer client = NetShared.clientIDToPlayer[swingClientID];
                                    
                                    client.SwingItem(client.selectedSlot, tile);
                                    break;

                                case EPacketType.ReceiveSelectedSlotChange:
                                    NetShared.clientIDToPlayer[socketToClientID[msg.SenderConnection]].selectedSlot = msg.ReadInt32();
                                    break;

                            }
                            break;
    
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            string reason = msg.ReadString();
                            onServerLog.Invoke("Status changed: " + status + " (" + reason + ")");

                            if (status == NetConnectionStatus.Disconnected && reason.Contains("timed out"))
                            {
                                int disconnectedClientID = socketToClientID[msg.SenderConnection];

                                if(NetShared.clientIDToPlayer.ContainsKey(disconnectedClientID))
                                {
                                    NetShared.clientIDToPlayer[disconnectedClientID].Destroy();
                                    NetShared.clientIDToPlayer.Remove(disconnectedClientID);
                                }
                                
                                socketToClientID.Remove(msg.SenderConnection);

                                ClientDisconnectedPacket disconnectedPacket = new ClientDisconnectedPacket();
                                disconnectedPacket.disconnectedPlayerID = disconnectedClientID;

                                NetOutgoingMessage disconnectOut = server.CreateMessage();
                                disconnectOut.Write((byte)EPacketType.ReceiveClientDisconnected);
                                disconnectedPacket.PacketToNetOutgoingMessage(disconnectOut);

                                server.SendToAll(disconnectOut, NetDeliveryMethod.ReliableOrdered);
                                server.FlushSendQueue();
                            }
                            break;
    
                        case NetIncomingMessageType.DebugMessage:
                            //Debug.WriteLine(msg.ReadString());
                            break;
    
                        case NetIncomingMessageType.WarningMessage:
                            break;
    
                        case NetIncomingMessageType.ErrorMessage:
                            break;
    
                    }
                }
            }
            onServerLog.Invoke("Server stopped.");
        }

        private void ServerTick(object sender, ElapsedEventArgs e)
        {
            SendWorldUpdate();
            SendEntityUpdates();
            server.FlushSendQueue();
        }

        private void SendWorldUpdate()
        {
            WorldUpdatePacket worldUpdatePacket = new WorldUpdatePacket();
            worldUpdatePacket.time = Program.GetGame().world.worldTime;

            NetOutgoingMessage worldUpdateMsg = server.CreateMessage();
            worldUpdateMsg.Write((byte)EPacketType.ReceiveWorldUpdate);
            worldUpdatePacket.PacketToNetOutgoingMessage(worldUpdateMsg);
            server.SendToAll(worldUpdateMsg, NetDeliveryMethod.Unreliable);
            server.FlushSendQueue();
        }

        private void SendEntityUpdates()
        {
           var copy = NetShared.netEntitites;

            //CAUSES AN EXCEPTION SOMETIMES; DEPERECATED
           /*foreach (int ID in copy.Keys)
           {
                EntityUpdatePacket entityUpdatePacket = new EntityUpdatePacket();
                
                entityUpdatePacket.entityID = ID;
                entityUpdatePacket.position = copy[ID].position;
                entityUpdatePacket.velocity = copy[ID].velocity;

                NetOutgoingMessage entityUpdateMsg = server.CreateMessage();
                entityUpdateMsg.Write((byte)EPacketType.ReceiveServerUpdateEntity);
                entityUpdatePacket.PacketToNetOutgoingMessage(entityUpdateMsg);

                server.SendToAll(entityUpdateMsg, NetDeliveryMethod.Unreliable);
                server.FlushSendQueue();
           }*/

           for (int ID = 0; ID < copy.Count; ID++)
           {
                EntityUpdatePacket entityUpdatePacket = new EntityUpdatePacket();

                entityUpdatePacket.entityID = copy.ElementAt(ID).Key;
                entityUpdatePacket.position = copy.ElementAt(ID).Value.position;
                entityUpdatePacket.velocity = copy.ElementAt(ID).Value.velocity;

                NetOutgoingMessage entityUpdateMsg = server.CreateMessage();
                entityUpdateMsg.Write((byte)EPacketType.ReceiveServerUpdateEntity);
                entityUpdatePacket.PacketToNetOutgoingMessage(entityUpdateMsg);

                server.SendToAll(entityUpdateMsg, NetDeliveryMethod.Unreliable);
                server.FlushSendQueue();
            }
        }

        public void ServerSpawnEntity(ENetEntitySpawnType type, EEntityType entityType, EItemType itemType, EProjectileType projectileType, Vector2 position, Vector2 velocity)
        {
            SpawnEntityPacket request = new SpawnEntityPacket();
            request.spawnType = type;
            request.entityType = entityType;
            request.itemType = itemType;
            request.spawnType = type;
            request.projectileType = projectileType;
            request.position = position;
            request.velocity = velocity;

            NetOutgoingMessage msg = server.CreateMessage();

            lastEntityID++;

            if (lastEntityID == -1)
            {
                lastEntityID++;
            }

            request.entityID = lastEntityID;

            //spawn entity locally/server
            NetShared.SpawnEntityShared(request);

            //spawn for everyone else
            NetOutgoingMessage newEntityMsg = server.CreateMessage();

            newEntityMsg.Write((byte)EPacketType.ReceiveSpawnEntity);
            request.PacketToNetOutgoingMessage(msg);
            request.PacketToNetOutgoingMessage(newEntityMsg);

            server.SendToAll(newEntityMsg, NetDeliveryMethod.ReliableOrdered);
            server.FlushSendQueue();
        }

        public void ServerDestroyEntity(int id)
        {
            IDPacket idPacket = new IDPacket(id);

            if(id == -1)
            {
                onServerLog.Invoke("tried to destroy entity with ID = -1, ignoring.");
                return;
            }

            NetShared.netEntitites[idPacket.ID].LocalDestroy();
            NetShared.netEntitites.Remove(idPacket.ID);
            NetOutgoingMessage entityDestroyMsg = server.CreateMessage();
            entityDestroyMsg.Write((byte)EPacketType.ReceiveDestroyEntity);
            idPacket.PacketToNetOutgoingMessage(entityDestroyMsg);

            server.SendToAll(entityDestroyMsg, NetDeliveryMethod.ReliableOrdered);
            server.FlushSendQueue();
        }

        public void SendInventoryToClient(int id, ContainerItem[] items)
        {
            InventoryPacket inv = new InventoryPacket(items.Length, items);

            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write((byte)EPacketType.ReceiveInventoryChange);
            inv.PacketToNetOutgoingMessage(outMsg);

            NetConnection client = GetClientByID(id);
            server.SendMessage(outMsg, client, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendTileSquare(TileChangePacket change)
        {
            NetOutgoingMessage tileOut = server.CreateMessage();

            World.SetTile(change.x, change.y, change.tileType, true);
            MarkTileChange(new NetWorldChange(change.x, change.y, change.tileType));

            //send change to everyone
            tileOut.Write((byte)EPacketType.ReceiveTileChange);
            change.PacketToNetOutgoingMessage(tileOut);
            server.SendToAll(tileOut, NetDeliveryMethod.ReliableOrdered);
        }

        public static NetConnection GetClientByID(int id)
        {
            for(int i = 0; i < socketToClientID.Count; i++)
            {
                if(socketToClientID.ElementAt(i).Value == id)
                {
                    return socketToClientID.ElementAt(i).Key;
                }
            }
            return null;
        }

        public void MarkTileChange(int x, int y, ETileType newType)
        {
            int existingIndex = -1;
            for(int i = 0; i < worldChanges.Count; i++)
            {
                if(worldChanges[i].x  == x && worldChanges[i].y == y)
                {
                    existingIndex = i;
                    break;
                }
            }

            if(existingIndex != -1)
            {
                worldChanges[existingIndex] = new NetWorldChange(x, y, newType);
            }
            else
            {
                worldChanges.Add(new NetWorldChange(x, y, newType));
            }
        }

        public void MarkTileChange(NetWorldChange change)
        {
            MarkTileChange(change.x, change.y, change.type);
        }

        public void SendWorldChunks(NetConnection connection, int chunkSize = 100)
        {
            // Calculate number of chunks
            int chunksX = (int)Math.Ceiling((double)World.maxTilesX / chunkSize);
            int chunksY = (int)Math.Ceiling((double)World.maxTilesY / chunkSize);
            int totalChunks = chunksX * chunksY;

            for (int chunkX = 0; chunkX < chunksX; chunkX++)
            {
                for (int chunkY = 0; chunkY < chunksY; chunkY++)
                {
                    WorldChunkPacket chunkPacket = new WorldChunkPacket();
                    chunkPacket.chunkX = chunkX;
                    chunkPacket.chunkY = chunkY;
                    chunkPacket.totalChunks = totalChunks;
                    chunkPacket.chunkSize = chunkSize;

                    // Fill chunk with tile data
                    chunkPacket.tiles = new ETileType[chunkSize, chunkSize];
                    for (int x = 0; x < chunkSize; x++)
                    {
                        for (int y = 0; y < chunkSize; y++)
                        {
                            int worldX = chunkX * chunkSize + x;
                            int worldY = chunkY * chunkSize + y;

                            if (worldX < World.maxTilesX && worldY < World.maxTilesY)
                            {
                                chunkPacket.tiles[x, y] = World.tiles[worldX, worldY];
                            }
                        }
                    }

                    NetOutgoingMessage chunkMsg = server.CreateMessage();
                    chunkMsg.Write((byte)EPacketType.ReceiveWorldChunk);
                    chunkPacket.PacketToNetOutgoingMessage(chunkMsg);

                    server.SendMessage(chunkMsg, connection, NetDeliveryMethod.ReliableOrdered);

                    // Small delay to prevent overwhelming the network
                    System.Threading.Thread.Sleep(10);
                }
            }

            // Send completion message
            NetOutgoingMessage completeMsg = server.CreateMessage();
            completeMsg.Write((byte)EPacketType.ReceiveWorldComplete);
            server.SendMessage(completeMsg, connection, NetDeliveryMethod.ReliableOrdered);
        }
    }
}