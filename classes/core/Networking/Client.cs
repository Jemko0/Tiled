using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;

namespace Tiled.Networking
{
    public class TiledClient
    {
        private ClientWebSocket ws;
        public string SV_URI = "ws://localhost:17777";
        private bool isRunning = true;

        public int PlayerID { get; private set; }
        
        // Event for message logging
        public event Action<string> OnLogMessage;

        private class Packet
        {
            public string type { get; set; }
            public PacketData data { get; set; }
        }

        private class PacketData
        {
            public int id { get; set; }
        }

        public TiledClient()
        {
            // Set up default log handler if none provided
            OnLogMessage += (msg) => Debug.WriteLine(msg);

            
        }

        public void Run()
        {
            // Start client on the main thread
            Task.Run(Client);
        }

        private async Task Client()
        {
            ws = new ClientWebSocket();
            Uri serverUri = new Uri(SV_URI);

            try
            {
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                Log("Connected to the server");
                await HandleMessages();
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke(message);
        }

        private async Task HandleMessages()
        {
            byte[] buffer = new byte[1024];

            while (isRunning && ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

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
                    break;
                }
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var packet = System.Text.Json.JsonSerializer.Deserialize<Packet>(message);

                switch (packet.type)
                {
                    case "player_id":
                        PlayerID = packet.data.id;
                        Log($"Received player ID: {PlayerID}");

                        SendPacket("spawn", new { id = PlayerID });

                        break;

                    case "spawn":
                        Log("player with ID: " + packet.data.id + " wants to spawn on Client");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"Error processing message: {ex.Message}");
            }
        }

        public async Task SendPacket(string type, object data)
        {
            var packet = new
            {
                type = type,
                data = data
            };

            string jsonPacket = System.Text.Json.JsonSerializer.Serialize(packet);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonPacket);

            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
    }
}
