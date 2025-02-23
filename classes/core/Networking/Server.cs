using Lidgren.Network;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.Networking.Shared;

namespace Tiled
{

    public class TiledServer
    {
        public NetServer server;
        public Thread serverThread;
        public bool running = true;

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
            Debug.WriteLine("Server started");

            // Generate world
            Program.GetGame().world = new World();
            Program.GetGame().world.worldTime = 8;
            Program.GetGame().world.seed = 35341;
            World.maxTilesX = 1000;
            World.maxTilesY = 600;
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
                                    clientSpawnPacket.position = new Vector2(128, 64);
                                    NetOutgoingMessage spawnOutMsg = server.CreateMessage();
                                    
                                    spawnOutMsg.Write((byte)EPacketType.ReceiveSpawnClient);
                                    clientSpawnPacket.PacketToNetOutgoingMessage(spawnOutMsg);

                                    EPlayer newClient = Entity.NewEntity<EPlayer>();
                                    newClient.clientID = clientSpawnPacket.playerID;
                                    newClient.position = clientSpawnPacket.position;

                                    NetShared.clientIDPairs.Add(clientSpawnPacket.playerID, newClient);

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
                                    break;

                                case EPacketType.RequestWorldChanges:
                                    Thread.Sleep(100);
                                    foreach (NetWorldChange c in worldChanges)
                                    {
                                        WorldChangesPacket worldChangesPacket = new WorldChangesPacket();
                                        worldChangesPacket.x = c.x;
                                        worldChangesPacket.y = c.y;
                                        worldChangesPacket.type = (byte)c.type;

                                        NetOutgoingMessage outChange = server.CreateMessage();

                                        outChange.Write((byte)EPacketType.ReceiveWorldChange);
                                        worldChangesPacket.PacketToNetOutgoingMessage(outChange);

                                        server.SendMessage(outChange, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    }
                                    break;

                                case EPacketType.ReceiveClientUpdate:

                                    ClientUpdatePacket clientUpdatePacket = new ClientUpdatePacket();
                                    clientUpdatePacket.PacketToNetIncomingMessage(msg);

                                    if(NetShared.clientIDPairs.ContainsKey(clientUpdatePacket.playerID))
                                    {
                                        NetShared.clientIDPairs[clientUpdatePacket.playerID].position = clientUpdatePacket.position;
                                        NetShared.clientIDPairs[clientUpdatePacket.playerID].velocity = clientUpdatePacket.velocity;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Client ID not found: " + clientUpdatePacket.playerID);
                                        break;
                                    }
                                    
                                    ClientUpdatePacket multicastUpdatePacket = new ClientUpdatePacket();
                                    multicastUpdatePacket.playerID = clientUpdatePacket.playerID;
                                    multicastUpdatePacket.position = NetShared.clientIDPairs[clientUpdatePacket.playerID].position;
                                    multicastUpdatePacket.velocity = NetShared.clientIDPairs[clientUpdatePacket.playerID].velocity;

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

                                    foreach (KeyValuePair<int, EPlayer> pair in NetShared.clientIDPairs)
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

                                    NetOutgoingMessage tileOut = server.CreateMessage();

                                    //set tile for server locally
                                    World.SetTile(change.x, change.y, change.tileType, true);
                                    worldChanges.Add(new NetWorldChange(change.x, change.y, change.tileType));

                                    //send change to everyone else
                                    tileOut.Write((byte)EPacketType.ReceiveTileChange);
                                    change.PacketToNetOutgoingMessage(tileOut);
                                    server.SendToAll(tileOut, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case EPacketType.RequestSpawnEntity:
                                    SpawnEntityPacket newEntityPacket = new SpawnEntityPacket();
                                    newEntityPacket.PacketToNetIncomingMessage(msg);

                                    lastEntityID++;
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
                                    IDPacket idPacket = new IDPacket(-999);
                                    idPacket.PacketToNetIncomingMessage(msg);

                                    NetShared.netEntitites[idPacket.ID].LocalDestroy();

                                    NetOutgoingMessage entityDestroyMsg = server.CreateMessage();
                                    entityDestroyMsg.Write((byte)EPacketType.ReceiveDestroyEntity);
                                    idPacket.PacketToNetOutgoingMessage(entityDestroyMsg);

                                    server.SendToAll(entityDestroyMsg, NetDeliveryMethod.ReliableOrdered);
                                    break;
                            }
                            break;
    
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            string reason = msg.ReadString();
                            Debug.WriteLine("Status changed: " + status + " (" + reason + ")");

                            if (status == NetConnectionStatus.Disconnected && reason.Contains("timed out"))
                            {
                                int disconnectedClientID = socketToClientID[msg.SenderConnection];

                                if(NetShared.clientIDPairs.ContainsKey(disconnectedClientID))
                                {
                                    NetShared.clientIDPairs[disconnectedClientID].Destroy();
                                    NetShared.clientIDPairs.Remove(disconnectedClientID);
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
            Debug.WriteLine("Server stopped.");
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
        }

        private void SendEntityUpdates()
        {
           foreach(int ID in NetShared.netEntitites.Keys)
           {
                EntityUpdatePacket entityUpdatePacket = new EntityUpdatePacket();
                entityUpdatePacket.entityID = ID;
                entityUpdatePacket.position = NetShared.netEntitites[ID].position;
                entityUpdatePacket.velocity = NetShared.netEntitites[ID].velocity;

                NetOutgoingMessage entityUpdateMsg = server.CreateMessage();
                entityUpdateMsg.Write((byte)EPacketType.ReceiveServerUpdateEntity);
                entityUpdatePacket.PacketToNetOutgoingMessage(entityUpdateMsg);

                server.SendToAll(entityUpdateMsg, NetDeliveryMethod.Unreliable);
           }
        }

        public void ServerSpawnEntity(bool isItem, EEntityType entityType, EItemType itemType, Vector2 position, Vector2 velocity)
        {
            SpawnEntityPacket request = new SpawnEntityPacket();
            request.isItem = isItem;
            request.entityType = entityType;
            request.itemType = itemType;
            request.isItem = isItem;
            request.position = position;
            request.velocity = velocity;

            NetOutgoingMessage selfRequest = server.CreateMessage();
            selfRequest.Write((byte)EPacketType.RequestSpawnEntity);
            request.PacketToNetOutgoingMessage(selfRequest);

            server.SendUnconnectedToSelf(selfRequest);
        }

        public void ServerDestroyEntity(int id)
        {
            IDPacket idPacket = new IDPacket(id);
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((byte)EPacketType.RequestDestroyEntity);
            idPacket.PacketToNetOutgoingMessage(msg);

            server.SendUnconnectedToSelf(msg);
        }
    }
}