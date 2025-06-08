using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Tiled;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Inventory;
using Tiled.Networking.Shared;
using System.Text.Json;

namespace Tiled
{

    public class TiledClient
    {
        public NetClient client;
        public int localPlayerID = -1;
        public Thread clientThread;
        public bool running = true;

        public delegate void ClientJoinResult(bool obj);
        public delegate void ClientException(Exception e);
        public event ClientJoinResult clientJoined;
        public event ClientException clientException;

        public TiledClient()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("tiled");
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            client = new NetClient(config);
            client.Start();
            Program.GetGame().Exiting += TiledClient_Exiting;
            Debug.WriteLine("Client started");
        }

        public void ConnectToServer(byte[] ip, int port)
        {
            try
            {
                Debug.WriteLine("Connecting to IPEndPoint: " + string.Join('.', ip) + ":" + port);
                client.Connect(new System.Net.IPEndPoint(new System.Net.IPAddress(ip), port));
                clientThread = new Thread(new ThreadStart(StartClient));
                clientThread.Name = "Client Thread";
                clientThread.Start();
                return;
            }
            catch (Exception e)
            {
                clientException?.Invoke(e);
            }
            public int id { get; set; }
            public int tickrate { get; set; }
            public int seed { get; set; }
            public int maxTilesX { get; set; }
            public int maxTilesY { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public byte tileType { get; set; }
            public float velX { get; set; }
            public float velY { get; set; }

            public object[] objectArray { get; set; }
        }

        public void externClientInvokeException(Exception e)
        {
            clientException?.Invoke(e);
        }

        private void TiledClient_Exiting(object sender, EventArgs e)
        {
            running = false;
        }

        public enum WorldLoadState
        {
            NotStarted,
            GeneratingWorld,
            RequestingChanges,
            RequestingEntities,
            Complete
        }

        public WorldLoadState loadState = WorldLoadState.NotStarted;

        public void StartClient()
        {
            Thread.Sleep(1000);
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.RequestPlayerID);
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();

            System.Timers.Timer clientTickTimer = new System.Timers.Timer(41);
            clientTickTimer.Elapsed += ClientTick;

            while (running)
            {
                NetIncomingMessage inc;
                while ((inc = client.ReadMessage()) != null)
                {
                    switch (inc.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                            inc.SenderConnection.Approve();
                            break;
                        case NetIncomingMessageType.Data:
                            switch ((EPacketType)inc.ReadByte())
                            {
                                case EPacketType.ReceivePlayerID:
                                    Main.netMode = ENetMode.Client;
                                    Main.inTitle = false;

                                    IDPacket playerIDPacket = new IDPacket(0);
                                    playerIDPacket.PacketToNetIncomingMessage(inc);
    
                                    localPlayerID = playerIDPacket.ID;
    
                                    Debug.WriteLine("Player ID: " + localPlayerID);

                                    //request World
                                    ClientWorldRequestPacket worldRequestPacket = new ClientWorldRequestPacket(localPlayerID);
                                    NetOutgoingMessage worldRequest = client.CreateMessage();
                                    worldRequest.Write((byte)EPacketType.RequestWorld);
                                    worldRequestPacket.PacketToNetOutgoingMessage(worldRequest);
                                    client.SendMessage(worldRequest, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.ReceiveWorld:
                                    WorldPacket worldPacket = new WorldPacket();
                                    worldPacket.PacketToNetIncomingMessage(inc);
                                    World.maxTilesX = worldPacket.maxTilesX;
                                    World.maxTilesY = worldPacket.maxTilesY;
                                    Program.GetGame().world.seed = worldPacket.seed;
                                    Program.GetGame().world.StartWorldGeneration();

                                    loadState = WorldLoadState.GeneratingWorld;

                                    // Wait for world generation to complete locally before rendering
                                    Thread worldGenWaitThread = new Thread(() => {
                                        while (World.isGenerating)
                                        {
                                            Thread.Sleep(100);
                                        }
                                        World.renderWorld = true;
                                    });
                                    worldGenWaitThread.Start();
                                    break;

                                case EPacketType.ReceiveWorldComplete:
                                    loadState = WorldLoadState.RequestingEntities;

                                    NetOutgoingMessage entityRequest = client.CreateMessage();
                                    entityRequest.Write((byte)EPacketType.RequestActiveEntities);
                                    client.SendMessage(entityRequest, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.ReceiveSpawnClient:
                                    ClientSpawnPacket spawnPacket = new ClientSpawnPacket();
                                    spawnPacket.PacketToNetIncomingMessage(inc);
                                    
                                    EPlayer newPlayer = Entity.NewEntity<EPlayer>();
                                    newPlayer.Initialize(EEntityType.Player);
                                    newPlayer.clientID = spawnPacket.playerID;
                                    newPlayer.position = spawnPacket.position;

                                    if(NetShared.clientIDToPlayer.ContainsKey(newPlayer.clientID))
                                    {
                                        NetShared.clientIDToPlayer[spawnPacket.playerID] = newPlayer;
                                    }
                                    else
                                    {
                                        NetShared.clientIDToPlayer.Add(spawnPacket.playerID, newPlayer);
                                    }
                                    

                                    if (localPlayerID == spawnPacket.playerID)
                                    {
                                        Program.GetGame().localPlayerController.Possess(newPlayer);

                                        //if we spawned, request other clients
                                        IDPacket requestOthersPacket = new IDPacket(localPlayerID);
                                        NetOutgoingMessage requestOthers = client.CreateMessage();

                                        requestOthers.Write((byte)EPacketType.RequestOtherClients);
                                        requestOthersPacket.PacketToNetOutgoingMessage(requestOthers);
                                        client.SendMessage(requestOthers, NetDeliveryMethod.ReliableOrdered);

                                        clientTickTimer.Start();
                                        loadState = WorldLoadState.Complete;
                                        clientJoined?.Invoke(true);
                                    }
                                    break;

                                case EPacketType.ReceiveClientDisconnected:
                                    ClientDisconnectedPacket disconnectedPacket = new ClientDisconnectedPacket();
                                    disconnectedPacket.PacketToNetIncomingMessage(inc);

                                    if (NetShared.clientIDToPlayer.ContainsKey(disconnectedPacket.disconnectedPlayerID))
                                    {
                                        NetShared.clientIDToPlayer[disconnectedPacket.disconnectedPlayerID].Destroy();
                                        NetShared.clientIDToPlayer.Remove(disconnectedPacket.disconnectedPlayerID);
                                    }
                                    break;

                                case EPacketType.ReceiveWorldUpdate:
                                    Debug.WriteLine("received world update");
                                    WorldUpdatePacket worldUpdatePacket = new WorldUpdatePacket();
                                    worldUpdatePacket.PacketToNetIncomingMessage(inc);
                                    Program.GetGame().world.worldTime = worldUpdatePacket.time;
                                    break;

                                case EPacketType.ReceiveClientUpdate:
                                    ClientUpdatePacket clientUpdatePacket = new ClientUpdatePacket();
                                    clientUpdatePacket.PacketToNetIncomingMessage(inc);

                                    if (NetShared.clientIDToPlayer.ContainsKey(clientUpdatePacket.playerID))
                                    {
                                        NetShared.clientIDToPlayer[clientUpdatePacket.playerID].position = clientUpdatePacket.position;
                                        NetShared.clientIDToPlayer[clientUpdatePacket.playerID].velocity = clientUpdatePacket.velocity;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Client ID not found");
                                    }
                                    break;


                                case EPacketType.ReceiveTileChange:
                                    TileChangePacket change = new TileChangePacket();
                                    change.PacketToNetIncomingMessage(inc);

                                    World.SetTile(change.x, change.y, change.tileType);
                                    break;

                                case EPacketType.ReceiveSpawnEntity:
                                    SpawnEntityPacket spawnEntityPacket = new SpawnEntityPacket();
                                    spawnEntityPacket.PacketToNetIncomingMessage(inc);

                                    //spawn entity locally/client
                                    NetShared.SpawnEntityShared(spawnEntityPacket);
                                    break;

                                case EPacketType.ReceiveServerUpdateEntity:
                                    EntityUpdatePacket entityUpdatePacket = new EntityUpdatePacket();
                                    entityUpdatePacket.PacketToNetIncomingMessage(inc);

                                    if(NetShared.netEntitites.ContainsKey(entityUpdatePacket.entityID))
                                    {
                                        NetShared.netEntitites[entityUpdatePacket.entityID].position = entityUpdatePacket.position;
                                        NetShared.netEntitites[entityUpdatePacket.entityID].velocity = entityUpdatePacket.velocity;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("client id was invalid");
                                    }
                                    
                                    break;

                                case EPacketType.ReceiveDestroyEntity:
                                    IDPacket idPacket = new IDPacket(-1);
                                    idPacket.PacketToNetIncomingMessage(inc);
                                    
                                    NetShared.netEntitites[idPacket.ID].LocalDestroy();
                                    break;

                                case EPacketType.ReceiveWorldChanges:
                                    WorldChangesPacket worldChangesPacket = new WorldChangesPacket();
                                    worldChangesPacket.PacketToNetIncomingMessage(inc);

                                    for (int i = 0; i < worldChangesPacket.length; i++)
                                    {
                                        NetWorldChange c = worldChangesPacket.changes[i];
                                        World.SetTile(c.x, c.y, (ETileType)c.type);
                                    }

                                    break;

                                case EPacketType.ReceiveInventory:
                                    InventoryPacket inventoryPacket = new InventoryPacket();
                                    inventoryPacket.PacketToNetIncomingMessage(inc);

                                    Container inv = new Container(inventoryPacket.size);
                                    inv.items = inventoryPacket.items;

                                    ((EPlayer)(Program.GetGame().localPlayerController.controlledEntity)).inventory = inv;
                                    ((EPlayer)(Program.GetGame().localPlayerController.controlledEntity)).healthComponent = new Gameplay.Components.HealthComponent(100, 100, 0);
                                    ((EPlayer)(Program.GetGame().localPlayerController.controlledEntity)).ClientInventoryReceived();


                                    break;

                                case EPacketType.ReceiveInventoryChange:
                                    InventoryPacket newInventory = new InventoryPacket();
                                    newInventory.PacketToNetIncomingMessage(inc);

                                    ((EPlayer)(Program.GetGame().localPlayerController.controlledEntity)).inventory.items = newInventory.items;
                                    break;

                                case EPacketType.ReceiveEntities:
                                    ActiveEntityPacket active = new ActiveEntityPacket();
                                    active.PacketToNetIncomingMessage(inc);

                                    for(int i = 0; i < active.arrayLength; i++)
                                    {
                                        SpawnEntityPacket newEntity = new SpawnEntityPacket();
                                        newEntity.entityID = active.entities[i].netID;
                                        newEntity.spawnType = active.entities[i].spawnType;
                                        newEntity.entityType = active.entities[i].type;
                                        newEntity.itemType = active.entities[i].itemType;
                                        newEntity.projectileType = active.entities[i].projectileType;
                                        newEntity.position = active.entities[i].position;
                                        newEntity.velocity = new(0, 0);

                                        NetShared.SpawnEntityShared(newEntity);
                                    }

                                    NetOutgoingMessage spawnRequest = client.CreateMessage();
                                    spawnRequest.Write((byte)EPacketType.RequestClientSpawn);
                                    spawnRequest.Write(localPlayerID);
                                    client.SendMessage(spawnRequest, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.ReceiveWorldChunk:
                                    WorldChunkPacket chunkPacket = new WorldChunkPacket();
                                    chunkPacket.PacketToNetIncomingMessage(inc);

                                    // Process this chunk
                                    int baseX = chunkPacket.chunkX * chunkPacket.chunkSize;
                                    int baseY = chunkPacket.chunkY * chunkPacket.chunkSize;

                                    for (int x = 0; x < chunkPacket.chunkSize; x++)
                                    {
                                        for (int y = 0; y < chunkPacket.chunkSize; y++)
                                        {
                                            int worldX = baseX + x;
                                            int worldY = baseY + y;

                                            if (worldX < World.maxTilesX && worldY < World.maxTilesY)
                                            {
                                                World.SetTile(worldX, worldY, chunkPacket.tiles[x, y]);
                                            }
                                        }
                                    }

                                    loadState = WorldLoadState.RequestingChanges;
                                    break;

                                case EPacketType.ReceiveDamage:
                                    DamagePacket damagePacket = new DamagePacket();
                                    damagePacket.PacketToNetIncomingMessage(inc);

                                    if (damagePacket.isPlayer)
                                    {
                                        EPlayer dmgPlayer = NetShared.clientIDToPlayer[damagePacket.toID];
                                        dmgPlayer.healthComponent.DoDamage(damagePacket.damage, damagePacket.toID);
                                    }
                                    else
                                    {
                                        Entity dmgEntity = NetShared.netEntitites[damagePacket.toID];
                                        dmgEntity.healthComponent.DoDamage(damagePacket.damage, damagePacket.toID);
                                    }
                                    break;
                            }
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            break;
                        case NetIncomingMessageType.DebugMessage:
                            Debug.WriteLine(inc.ReadString());
                            break;
                        case NetIncomingMessageType.WarningMessage:
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            break;
                    }
                }
            }
            Debug.WriteLine("Server stopped");
        }

        private void ClientTick(object sender, ElapsedEventArgs e)
        {
            if(localPlayerID != -1)
            {
                ClientUpdatePacket clientUpdatePacket = new ClientUpdatePacket();
                clientUpdatePacket.playerID = localPlayerID;
                clientUpdatePacket.position = Program.GetGame().localPlayerController.controlledEntity.position;
                clientUpdatePacket.velocity = Program.GetGame().localPlayerController.controlledEntity.velocity;

                NetOutgoingMessage clientUpdateMsg = client.CreateMessage();

                clientUpdateMsg.Write((byte)EPacketType.ReceiveClientUpdate);
                clientUpdatePacket.PacketToNetOutgoingMessage(clientUpdateMsg);
                client.SendMessage(clientUpdateMsg, NetDeliveryMethod.Unreliable);
            try
            {
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                Log("Connected to the server");
                await HandleMessages();
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                OnException?.Invoke(ex.Message);
            }
        }

        public void SendTileSquare(int x, int y, ETileType type)
        {
            TileChangePacket tileChangePacket = new TileChangePacket();
            tileChangePacket.x = x;
            tileChangePacket.y = y;
            tileChangePacket.tileType = type;

            NetOutgoingMessage msg = client.CreateMessage();

            msg.Write((byte)EPacketType.RequestTileChange);
            tileChangePacket.PacketToNetOutgoingMessage(msg);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void ClientRequestSpawnEntity(ENetEntitySpawnType type, EEntityType entityType, EItemType itemType, Vector2 position, Vector2 velocity)
        {
            SpawnEntityPacket request = new SpawnEntityPacket();
            request.spawnType = type;
            request.entityType = entityType;
            request.itemType = itemType;
            request.spawnType = type;
            request.position = position;
            request.velocity = velocity;

            NetOutgoingMessage requestMsg = client.CreateMessage();
            requestMsg.Write((byte)EPacketType.RequestSpawnEntity);
            request.PacketToNetOutgoingMessage(requestMsg);

            client.SendMessage(requestMsg, NetDeliveryMethod.ReliableOrdered);
        }

        public void ClientRequestDestroyEntity(int id)
        {
            IDPacket idPacket = new IDPacket(id);
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.RequestDestroyEntity);
            idPacket.PacketToNetOutgoingMessage(msg);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessMessage(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error while receiving: {ex.Message}");
                    OnException?.Invoke(ex.Message);
                    break;
                }
            }
        }

        public void RequestInventory()
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.RequestInventory);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void RequestItemPickup()
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.RequestItemPickup);

            client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestItemSwing(Point onTile)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.RequestItemSwing);
            msg.Write(onTile.X);
            msg.Write(onTile.Y);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void SetSelectedSlot(int slot)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.ReceiveSelectedSlotChange);
            msg.Write(slot);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
                switch (packet.type)
                {
                    case "player_id":
                        PlayerID = packet.data.id;

                        if(PlayerID == -1)
                        {
                            OnJoinResult?.Invoke(false);
                            return;
                        }

                        OnJoinResult?.Invoke(true);
                        Log($"Received player ID: {PlayerID}");

                        SendPacket("requestWorld", null);
                        
                        break;

                    case "world":
                        Main.SERVER_TICKRATE = packet.data.tickrate;

                        var seed = packet.data.seed;
                        
                        Log("Received world seed: " + seed);

                        Program.GetGame().world.seed = seed;
                        
                        World.maxTilesX = packet.data.maxTilesX;
                        World.maxTilesY = packet.data.maxTilesY;

                        Program.GetGame().world.StartWorldGeneration();

                        //when we have world, try spawning player
                        SendPacket("requestWorldChanges", new { id = PlayerID });
                        SendPacket("spawnNewClient", new { id = PlayerID });
                        break;

                    case "worldTime":
                        var worldTime = packet.data.x;
                        var worldTimeSpeed = packet.data.y;
                        Program.GetGame().world.worldTime = worldTime;
                        Program.GetGame().world.timeSpeed = worldTimeSpeed;
                        break;

                    case "worldChanges":
                        object[] changes = packet.data.objectArray;

                        for (int i = 0; i < changes.Length; i++)
                        {
                            var change = (JsonElement)changes[i];
                            World.SetTile(change.GetProperty("x").GetInt32(), change.GetProperty("y").GetInt32(), (ETileType)change.GetProperty("tileType").GetByte(), true);
                        }

                        break;

                    case "spawnNewClient":
                        Log("player with ID: " + packet.data.id + " wants to spawn on Client");
                        var p = Entity.NewEntity<EPlayer>();
                        p.position.X = packet.data.x;
                        p.position.Y = packet.data.y;

                        World.renderWorld = true;

                        if (packet.data.id == PlayerID)
                        {
                            Program.GetGame().localPlayerController.Possess(p);
                            Program.GetGame().localPlayerController.StartMultiplayerUpdate();
                        }
                        SendPacket("requestOthers", new { id = PlayerID });
                        break;

                    case "spawnOthers":
                        break;

                    case "otherPlayerUpdate":

                        if(!Main.cl_playerDictionary.ContainsKey(packet.data.id))
                        {
                            EPlayer newPlayer = Entity.NewEntity<EPlayer>();
                            newPlayer.clientID = packet.data.id;
                            newPlayer.position.X = packet.data.x;
                            newPlayer.position.Y = packet.data.y;
                            newPlayer.velocity.X = packet.data.velX;
                            newPlayer.velocity.Y = packet.data.velY;
                            Main.cl_playerDictionary.Add(packet.data.id, newPlayer);
                            return;
                        }

                        Main.cl_playerDictionary[packet.data.id].position.X = packet.data.x;
                        Main.cl_playerDictionary[packet.data.id].position.Y = packet.data.y;

                        Main.cl_playerDictionary[packet.data.id].velocity.X = packet.data.velX;
                        Main.cl_playerDictionary[packet.data.id].velocity.Y = packet.data.velY;
                        break;

                    case "newTile":
                        World.SetTile(packet.data.maxTilesX, packet.data.maxTilesY, (ETileType)packet.data.tileType, true);
                        break;

                    case "clientDisconnect":
                        if (Main.cl_playerDictionary.ContainsKey(packet.data.id))
                        {
                            Main.cl_playerDictionary[packet.data.id].Destroy();
                            Main.cl_playerDictionary.Remove(packet.data.id);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"Error processing message: {ex.Message}");
                OnException?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// isPlayer flag will always be true since a client should only send damage events from themselves
        /// </summary>
        /// <param name="d"></param>
        /// <param name="netID"></param>
        public void SendDamage(uint d, int netID)
        {
            DamagePacket p = new DamagePacket();
            p.damage = d;
            p.toID = netID;
            p.isPlayer = true;

            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)EPacketType.ReceiveDamage);
            p.PacketToNetOutgoingMessage(msg);
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendInventoryMove(int from, int to)
        {
            InvMoveSlotPacket p = new InvMoveSlotPacket();
            p.fromID = from;
            p.toID = to;
            NetOutgoingMessage msg = client.CreateMessage();
            p.PacketToNetOutgoingMessage(msg);
            client.SendMessage(msg, NetDeliveryMethod.ReliableSequenced);
        }

        public void SendContainerState(Container c)
        {
            InventoryPacket i = new InventoryPacket();
            i.items = c.items;
            i.size = c.items.Length;

            NetOutgoingMessage m = client.CreateMessage();
            m.Write((byte)EPacketType.ReceiveClientContainer);
            i.PacketToNetOutgoingMessage(m);
            
            client.SendMessage(m, NetDeliveryMethod.ReliableSequenced);
        }
    }
}