using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using Tiled.Gameplay;
using System.Net.Http.Headers;

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

                        SendPacket("requestWorld", new {id = PlayerID});
                        
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
                        SendPacket("spawnNewClient", new { id = PlayerID });
                        break;

                    case "worldTime":
                        var worldTime = packet.data.x;
                        var worldTimeSpeed = packet.data.y;
                        Program.GetGame().world.worldTime = worldTime;
                        Program.GetGame().world.timeSpeed = worldTimeSpeed;
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
                        //Log("spawning other players on client");
                        /*EPlayer newPlayer = Entity.NewEntity<EPlayer>();
                        newPlayer.clientID = packet.data.id;
                        newPlayer.position.X = packet.data.x;
                        Main.cl_playerDictionary.Add(packet.data.id, newPlayer); newPlayer.position.Y = packet.data.y;*/

                        if(packet.data.objectArray == null || packet.data.objectArray.Length < 1)
                        {
                            Debug.WriteLine("object array null");
                            return;
                        }
                        Debug.WriteLine("OBJECT ARRAY: " + string.Join('\n', packet.data.objectArray));

                        //Log(packet.data.objectArray[]);
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
            byte[] payloadData = System.Text.Encoding.UTF8.GetBytes(jsonPacket);

            // Create WebSocket frame
            byte[] frame;
            byte[] length;

            if (payloadData.Length < 126)
            {
                frame = new byte[6 + payloadData.Length]; // 2 bytes header + 4 bytes mask + payload
                frame[1] = (byte)(0x80 | payloadData.Length); // Set masked bit and length
            }
            else if (payloadData.Length < 65536)
            {
                frame = new byte[8 + payloadData.Length]; // 2 bytes header + 2 bytes extended length + 4 bytes mask + payload
                frame[1] = (byte)(0x80 | 126); // Set masked bit and 126 to indicate 2-byte length
                frame[2] = (byte)((payloadData.Length >> 8) & 255);
                frame[3] = (byte)(payloadData.Length & 255);
            }
            else
            {
                throw new Exception("Payload too large");
            }

            // Set FIN and Text frame bits
            frame[0] = 0x81; // 1000 0001: FIN bit set, opcode = 1 (text)

            // Generate random mask
            byte[] mask = new byte[4];
            Random random = new Random();
            random.NextBytes(mask);

            // Copy mask bytes
            int maskOffset = frame.Length - (4 + payloadData.Length);
            Buffer.BlockCopy(mask, 0, frame, maskOffset, 4);

            // Mask and copy payload
            for (int i = 0; i < payloadData.Length; i++)
            {
                frame[maskOffset + 4 + i] = (byte)(payloadData[i] ^ mask[i % 4]);
            }

            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(
                    new ArraySegment<byte>(frame),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None
                );
            }
        }
    }
}
