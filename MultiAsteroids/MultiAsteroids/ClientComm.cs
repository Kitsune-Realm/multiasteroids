using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using AsteroidLibrary;
using AsteroidLibrary.Packets;
using System.IO;

namespace MultiAsteroids
{

    class ClientComm
    {
        public int Port = 5938;
        public TcpClient client;
        public Socket socket;
        public int amountPlayers { get; set; }

        public ClientComm(Game1 game)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            socket.Connect("127.0.0.1", 1234);
            game.PlayerFired += new PlayerFiredHandler(game_PlayerFired);
        }        

        public void StartListening()
        {
            //client = new TcpClient("127.0.0.1", Port);
            //client.ReceiveTimeout = 10;
            //client.SendTimeout = 10;     
        }

        public void Send(Packet packet)
        {
            packet.Send(this.socket);
        }

        public void Send(int playerNumber, float x, float y, float rotation)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.Movement);
            data.Add((byte)playerNumber);
            foreach (byte b in FloatUnion.FloatToBytes(x))
                data.Add(b);
            foreach (byte b in FloatUnion.FloatToBytes(y))
                data.Add(b);
            foreach (byte b in FloatUnion.FloatToBytes(rotation))
                data.Add(b);

            this.client.GetStream().Write(data.ToArray(), 0, data.Count);
        }

        void game_PlayerFired(int playerNum, Projectile projectile)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.PlayerFired);
            data.Add((byte)playerNum);
            data.Add((byte)projectile.Type);
            foreach (byte b in FloatUnion.FloatToBytes(projectile.Position.X))
                data.Add(b);
            foreach (byte b in FloatUnion.FloatToBytes(projectile.Position.Y))
                data.Add(b);
            foreach (byte b in FloatUnion.FloatToBytes(projectile.RotationAngle))
                data.Add(b);

            this.client.GetStream().Write(data.ToArray(), 0, data.Count);
        }

        public byte[] Read()
        {            
            byte[] buffer = new byte[client.ReceiveBufferSize]; // These buffers are humongeously huge... noob
            List<byte> data = new List<byte>();
            int bytesRead = 0;
            try
            {                
                int totalBytesRead = client.GetStream().Read(buffer, 0, buffer.Length);

                switch ((int)buffer[bytesRead])
                {
                    // ID, AmountPlayers, P1, P2, P3, P4
                    case (int)MessageType.PlayerReadyStatus:
                        for (int i = 0 + bytesRead; i < 6 + bytesRead; i++)
                            data.Add(buffer[i]);
                        bytesRead += 6;
                        break;
                    case (int)MessageType.Movement:
                        for (int i = 0 + bytesRead; i < 1 + (13 * amountPlayers) + bytesRead; i++)
                            data.Add(buffer[i]);
                        bytesRead += 2 + (13 * amountPlayers);
                        break;
                    case (int)MessageType.ServerSendsFired:
                        for (int i = 0 + bytesRead; i < 15 + bytesRead; i++)
                            data.Add(buffer[i]);
                        bytesRead += 15;
                        break;
                }
                
            }
            catch { }
            return data.ToArray();
        }       

        [Obsolete]
        private string generateIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
    }
}
