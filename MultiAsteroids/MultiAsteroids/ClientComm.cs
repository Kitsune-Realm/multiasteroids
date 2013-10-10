using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using AsteroidLibrary;
using System.IO;

namespace MultiAsteroids
{
    class ClientComm
    {
        public int Port = 6000;
        public TcpClient client;
        public bool isListening { get; set; }

        public ClientComm()
        {
            this.isListening = false;
        }

        public void StartListening()
        {
            client = new TcpClient("127.0.0.1", Port);
            client.ReceiveTimeout = 10;        
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

        public byte[] Read()
        {
            byte[] buffer = new byte[client.ReceiveBufferSize]; // These buffers are humongeously huge... noob
            List<byte> data = new List<byte>();
            client.GetStream().Read(buffer, 0, client.ReceiveBufferSize);
            switch ((int)buffer[0])
            {
                case (int)MessageType.PlayerReadyStatus:
                    for (int i = 0; i < 2; i++ )
                    {
                        if (buffer[i] > 0)
                            data.Add(buffer[i]);
                        else
                            break;
                    }
                    return data.ToArray();
            }
            return null;        
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
