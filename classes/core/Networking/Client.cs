using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using Tiled.Gameplay;

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

        public event Action<string> OnException;
        public event Action<bool> OnJoinResult;
        private class Packet
        {
            public string type { get; set; }
            public PacketData data { get; set; }
        }

        private class PacketData
        {
            public int id { get; set; }
            public int seed { get; set; }
            public int time { get; set; }
            public int maxTilesX { get; set; }
            public int maxTilesY { get; set; }
            public float x { get; set; }
            public float y { get; set; }
        }

        public TiledClient()
        {
            // Set up default log handler if none provided
            Main.isClient = true;
            OnLogMessage += (msg) => Debug.WriteLine(msg);
        }

        public void Run()
        {
            // Start client on the main thread
            Task.Run(Client);
        }

        public void DestroySocket()
        {
            Main.isClient = false;
            isRunning = false;
            ws?.Abort();
            ws?.Dispose();
        }

        private async Task Client()
        {
            ws = new ClientWebSocket();
            Uri serverUri;
            try
            {
               serverUri = new Uri(SV_URI);
            }
            catch (Exception ex)
            {
                Log($"URI_Error: {ex.Message}");
                OnException?.Invoke(ex.Message);
                return;
            }


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
                Log($"Received packet: {message}");

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
                        var seed = packet.data.seed;
                        
                        Log("Received world seed: " + seed);

                        Program.GetGame().world.seed = seed;
                        
                        World.maxTilesX = packet.data.maxTilesX;
                        World.maxTilesY = packet.data.maxTilesY;

                        Program.GetGame().world.StartWorldGeneration();

                        //when we have world, try spawning player
                        SendPacket("spawn", new { id = PlayerID });
                        break;

                    case "worldTime":
                        var worldTime = packet.data.time;
                        Program.GetGame().world.worldTime = worldTime;
                        break;

                    case "spawn":
                        Log("player with ID: " + packet.data.id + " wants to spawn on Client");
                        var p = Entity.NewEntity<EPlayer>();
                        p.position.X = packet.data.x;
                        p.position.Y = packet.data.y;

                        World.renderWorld = true;

                        if (packet.data.id == PlayerID)
                        {
                            Program.GetGame().localPlayerController.Possess(p);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"Error processing message: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a packet to the SERVER (from client)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
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
