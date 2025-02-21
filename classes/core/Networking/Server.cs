using Lidgren.Network;
using System.Diagnostics;
using System.Threading;
using Tiled;
using Tiled.Networking.Shared;

public class TiledServer
{
    public NetServer server;
    public Thread serverThread;
    public bool running = true;

    int lastClientID = 0;
    public TiledServer()
    {
        NetPeerConfiguration config = new NetPeerConfiguration("tiled");
        config.BroadcastAddress = new System.Net.IPAddress(new byte[] { 192, 168, 0, 21 });
        config.Port = 12345;
        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        config.EnableMessageType(NetIncomingMessageType.Data);
        config.EnableMessageType(NetIncomingMessageType.StatusChanged);
        config.EnableMessageType(NetIncomingMessageType.DebugMessage);
        config.EnableMessageType(NetIncomingMessageType.WarningMessage);
        config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
        server = new NetServer(config);
        server.Start();
        Tiled.Program.GetGame().Exiting += TiledServer_Exiting;

        serverThread = new Thread(new ThreadStart(StartServer));
        serverThread.Start();
    }

    private void TiledServer_Exiting(object sender, System.EventArgs e)
    {
        running = false;
    }

    public void StartServer()
    {
        Debug.WriteLine("Server started");
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
                                PlayerIDPacket playerIDPacket = new PlayerIDPacket(lastClientID);

                                NetOutgoingMessage outMsg = server.CreateMessage();
                                outMsg.Write((byte)EPacketType.ReceivePlayerID);
                                playerIDPacket.PacketToNetOutgoingMessage(outMsg);

                                server.SendMessage(outMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                server.FlushSendQueue();
                                break;
                            case EPacketType.RequestClientSpawn:
                                break;
                        }
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        break;

                    case NetIncomingMessageType.DebugMessage:
                        Debug.WriteLine(msg.ReadString());
                        break;

                    case NetIncomingMessageType.WarningMessage:
                        break;

                    case NetIncomingMessageType.ErrorMessage:
                        break;

                }

                Debug.WriteLine(msg.Data + " / DEL: " + msg.MessageType.ToString());
            }
        }
        Debug.WriteLine("Server stopped");
    }
}