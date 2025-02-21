using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Numerics;
using System.Diagnostics;

namespace Tiled.Networking
{
    public class GameServer
    {
        private TcpListener listener;
        private Dictionary<int, ConnectedClient> clients = new Dictionary<int, ConnectedClient>();
        private CancellationTokenSource serverCancellation;
        private int lastClientId = 0;

        public class ConnectedClient
        {
            public WebSocket Socket { get; set; }
            public int ID { get; set; }
            public Vector2 Position { get; set; }
        }

        private class WebSocketFrameResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public GameServer(string ip, int port)
        {
            serverCancellation = new CancellationTokenSource();
            IPAddress ipAddress = IPAddress.Parse(ip);
            listener = new TcpListener(ipAddress, port);
        }

        public async Task StartServer()
        {
            try
            {
                listener.Start();
                Debug.WriteLine($"Server started on {listener.LocalEndpoint}");

                while (!serverCancellation.Token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = HandleNewClient(client); // Fire and forget
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleNewClient(TcpClient tcpClient)
        {
            try
            {
                NetworkStream stream = tcpClient.GetStream();

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                string key = request.Split(new[] { "Sec-WebSocket-Key: " }, StringSplitOptions.None)[1]
                                  .Split('\r')[0]
                                  .Trim();
                string acceptKey = Convert.ToBase64String(
                    System.Security.Cryptography.SHA1.Create()
                        .ComputeHash(Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

                string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                                "Upgrade: websocket\r\n" +
                                "Connection: Upgrade\r\n" +
                                "Sec-WebSocket-Accept: " + acceptKey + "\r\n\r\n";

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                var clientId = ++lastClientId;
                var client = new ConnectedClient
                {
                    ID = clientId,
                    Position = new Vector2(0, 0)
                };

                clients.Add(clientId, client);
                Debug.WriteLine($"New client connected. ID: {clientId}");

                await SendPacket(stream, new
                {
                    type = "player_id",
                    data = new { id = clientId }
                });

                await HandleWebSocketCommunication(stream, client);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling client: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private async Task HandleWebSocketCommunication(NetworkStream stream, ConnectedClient client)
        {
            byte[] buffer = new byte[4096];

            while (true)
            {
                try
                {
                    var frameResult = await ReadWebSocketFrame(stream, buffer);
                    if (!frameResult.Success)
                    {
                        Debug.WriteLine($"Client {client.ID}: WebSocket frame read failed");
                        break;
                    }

                    Debug.WriteLine($"Client {client.ID} sent: {frameResult.Message}");
                    await ProcessMessage(client, frameResult.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WebSocket error for client {client.ID}: {ex.Message}");
                    Debug.WriteLine(ex.StackTrace);
                    break;
                }
            }

            await HandleClientDisconnect(client);
        }

        private async Task<WebSocketFrameResult> ReadWebSocketFrame(NetworkStream stream, byte[] buffer)
        {
            // Read first 2 bytes (header)
            int bytesRead = await stream.ReadAsync(buffer, 0, 2);
            if (bytesRead != 2) return new WebSocketFrameResult { Success = false };

            bool fin = (buffer[0] & 0b10000000) != 0;
            bool mask = (buffer[1] & 0b10000000) != 0;
            int payloadLength = buffer[1] & 0b01111111;
            int maskingKeyOffset = 2;

            if (payloadLength == 126)
            {
                bytesRead = await stream.ReadAsync(buffer, 2, 2);
                if (bytesRead != 2) return new WebSocketFrameResult { Success = false };
                payloadLength = BitConverter.ToUInt16(new[] { buffer[3], buffer[2] }, 0);
                maskingKeyOffset = 4;
            }
            else if (payloadLength == 127)
            {
                Debug.WriteLine("Payload too large, rejecting frame");
                return new WebSocketFrameResult { Success = false };
            }

            bytesRead = await stream.ReadAsync(buffer, maskingKeyOffset, 4);
            if (bytesRead != 4) return new WebSocketFrameResult { Success = false };
            byte[] maskingKey = new byte[4];
            Array.Copy(buffer, maskingKeyOffset, maskingKey, 0, 4);

            byte[] payload = new byte[payloadLength];
            int totalRead = 0;
            while (totalRead < payloadLength)
            {
                bytesRead = await stream.ReadAsync(payload, totalRead, payloadLength - totalRead);
                if (bytesRead == 0) return new WebSocketFrameResult { Success = false };
                totalRead += bytesRead;
            }

            for (int i = 0; i < payloadLength; i++)
            {
                payload[i] = (byte)(payload[i] ^ maskingKey[i % 4]);
            }

            return new WebSocketFrameResult
            {
                Success = true,
                Message = Encoding.UTF8.GetString(payload)
            };
        }

        private async Task ProcessMessage(ConnectedClient client, string message)
        {
            try
            {
                var packet = JsonSerializer.Deserialize<JsonElement>(message);
                var type = packet.GetProperty("type").GetString();
                var data = packet.GetProperty("data");

                Debug.WriteLine($"Processing message type: {type} from client {client.ID}");

                switch (type)
                {
                    case "requestWorld":
                        
                        break;
                    default:
                        Debug.WriteLine($"Unknown message type: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing message from client {client.ID}: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private async Task HandleClientDisconnect(ConnectedClient client)
        {
            if (clients.ContainsKey(client.ID))
            {
                clients.Remove(client.ID);
                Debug.WriteLine($"Client {client.ID} disconnected");
            }
        }

        public void Stop()
        {
            Debug.WriteLine("Server stopping...");
            serverCancellation.Cancel();
            listener.Stop();
        }

        private async Task SendPacket(NetworkStream stream, object packet)
        {
            try
            {
                var json = JsonSerializer.Serialize(packet);
                byte[] payload = Encoding.UTF8.GetBytes(json);

                // Create WebSocket frame
                byte[] frame;
                if (payload.Length < 126)
                {
                    frame = new byte[2 + payload.Length];
                    frame[1] = (byte)payload.Length;
                }
                else if (payload.Length < 65536)
                {
                    frame = new byte[4 + payload.Length];
                    frame[1] = 126;
                    frame[2] = (byte)((payload.Length >> 8) & 255);
                    frame[3] = (byte)(payload.Length & 255);
                }
                else
                {
                    Debug.WriteLine($"Payload too large: {payload.Length} bytes");
                    return;
                }

                // Set FIN and Text frame bits
                frame[0] = 0b10000001; // FIN bit set, opcode = 1 (text)

                // Copy payload
                Buffer.BlockCopy(payload, 0, frame, frame.Length - payload.Length, payload.Length);

                await stream.WriteAsync(frame, 0, frame.Length);
                Debug.WriteLine($"Sent packet: {json}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending packet: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private async Task SendPacket(object packet, int excludeClientId = -1)
        {
            var disconnectedClients = new List<int>();

            foreach (var client in clients)
            {
                if (client.Key != excludeClientId)
                {
                    try
                    {
                        var tcpClient = (client.Value.Socket as NetworkStream);
                        if (tcpClient != null)
                        {
                            await SendPacket(tcpClient, packet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error broadcasting to client {client.Key}: {ex.Message}");
                        disconnectedClients.Add(client.Key);
                    }
                }
            }

            // Clean up disconnected clients
            foreach (var id in disconnectedClients)
            {
                clients.Remove(id);
                Debug.WriteLine($"Removed disconnected client {id}");
            }
        }

        // Add other methods as needed...
    }
}