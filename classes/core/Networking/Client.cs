using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using Tiled.Gameplay;
using Tiled.DataStructures;
using System.Text.Json;

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
                    OnException?.Invoke(ex.Message);
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
