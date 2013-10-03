using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using AsteroidLibrary;

namespace MultiAsteroids
{
    class ClientComm
    {
        public int Port = 6000;
        public TcpClient client;

        public ClientComm()
        {
            client = new TcpClient("127.0.0.1", Port);
        }

        public void Send(float x, float y, float rotation)
        {
            List<byte> data = new List<byte>();          

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
            byte[] buffer = new byte[13];
            client.GetStream().Read(buffer, 0, buffer.Length);

            return buffer;
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
