using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

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

        public void Send(float x, float y)
        {
            List<byte> data = new List<byte>();

            //foreach (byte b in BitConverter.GetBytes(x))            
            //    data.Add(b);
            //foreach (byte b in BitConverter.GetBytes(y))
            //    data.Add(b);


            this.client.GetStream().Write(data.ToArray(), 0, data.Count);
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
