using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Tiled;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Networking.Shared;

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
            Debug.WriteLine("Connecting to IPEndPoint: " + string.Join('.', ip) + ":" + port);
            client.Connect(new System.Net.IPEndPoint(new System.Net.IPAddress(ip), port));

            clientThread = new Thread(new ThreadStart(StartClient));
            clientThread.Name = "Client Thread";
            clientThread.Start();
        }

        private void TiledClient_Exiting(object sender, EventArgs e)
        {
            running = false;
        }

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

                                    clientJoined?.Invoke(true);

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
                                    World.renderWorld = true;
                                    
                                    //request world changes
                                    NetOutgoingMessage changeRequest = client.CreateMessage();
                                    changeRequest.Write((byte)EPacketType.RequestWorldChanges);
                                    client.SendMessage(changeRequest, NetDeliveryMethod.ReliableOrdered);

                                    //request spawn
                                    NetOutgoingMessage spawnRequest = client.CreateMessage();
                                    spawnRequest.Write((byte)EPacketType.RequestClientSpawn);
                                    spawnRequest.Write(localPlayerID);
                                    client.SendMessage(spawnRequest, NetDeliveryMethod.ReliableOrdered);

                                    break;
    
                                case EPacketType.ReceiveSpawnClient:

                                    ClientSpawnPacket spawnPacket = new ClientSpawnPacket();
                                    spawnPacket.PacketToNetIncomingMessage(inc);
    
                                    EPlayer newPlayer = Entity.NewEntity<EPlayer>();
                                    newPlayer.clientID = spawnPacket.playerID;
                                    newPlayer.position = spawnPacket.position;

                                    NetShared.clientIDPairs.Add(spawnPacket.playerID, newPlayer);

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
                                    }
                                    break;

                                case EPacketType.ReceiveClientDisconnected:
                                    ClientDisconnectedPacket disconnectedPacket = new ClientDisconnectedPacket();
                                    disconnectedPacket.PacketToNetIncomingMessage(inc);

                                    if (NetShared.clientIDPairs.ContainsKey(disconnectedPacket.disconnectedPlayerID))
                                    {
                                        NetShared.clientIDPairs[disconnectedPacket.disconnectedPlayerID].Destroy();
                                        NetShared.clientIDPairs.Remove(disconnectedPacket.disconnectedPlayerID);
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

                                    if (NetShared.clientIDPairs.ContainsKey(clientUpdatePacket.playerID))
                                    {
                                        NetShared.clientIDPairs[clientUpdatePacket.playerID].position = clientUpdatePacket.position;
                                        NetShared.clientIDPairs[clientUpdatePacket.playerID].velocity = clientUpdatePacket.velocity;
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

                                    NetShared.netEntitites[entityUpdatePacket.entityID].position = entityUpdatePacket.position;
                                    NetShared.netEntitites[entityUpdatePacket.entityID].velocity = entityUpdatePacket.velocity;
                                    break;

                                case EPacketType.ReceiveDestroyEntity:
                                    IDPacket idPacket = new IDPacket(-1);
                                    idPacket.PacketToNetIncomingMessage(inc);
                                    
                                    NetShared.netEntitites[idPacket.ID].LocalDestroy();
                                    break;

                                case EPacketType.ReceiveWorldChange:
                                    WorldChangesPacket worldChangesPacket = new WorldChangesPacket();
                                    worldChangesPacket.PacketToNetIncomingMessage(inc);

                                    World.SetTile(worldChangesPacket.x, worldChangesPacket.y, (ETileType)worldChangesPacket.type);
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

        public void ClientRequestSpawnEntity(bool isItem, EEntityType entityType, EItemType itemType, Vector2 position, Vector2 velocity)
        {
            SpawnEntityPacket request = new SpawnEntityPacket();
            request.isItem = isItem;
            request.entityType = entityType;
            request.itemType = itemType;
            request.isItem = isItem;
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
        }
    }
}
