using Lidgren.Network;
using System.Diagnostics;
using System.Threading;
using Tiled.Networking.Shared;

public class TiledClient
{
    public NetClient client;
    public int localPlayerID;
    public Thread clientThread;
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
        Debug.WriteLine("Client started");

        clientThread = new Thread(new ThreadStart(StartClient));
        clientThread.Start();
    }

    public void StartClient()
    {
        Thread.Sleep(1000);
        NetOutgoingMessage msg = client.CreateMessage();
        msg.Write((byte)EPacketType.RequestPlayerID);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        client.FlushSendQueue();

        while (true)
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
                            case EPacketType.RequestPlayerID:
                                break;

                            case EPacketType.ReceivePlayerID:
                                PlayerIDPacket playerIDPacket = new PlayerIDPacket(0);
                                playerIDPacket.PacketToNetIncomingMessage(inc);

                                localPlayerID = playerIDPacket.playerID;

                                Debug.WriteLine("Player ID: " + localPlayerID);
                                
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
    }


}