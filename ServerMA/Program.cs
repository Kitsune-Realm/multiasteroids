using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using AsteroidLibrary;

namespace ServerMA
{
    class Program
    {
        private static int port = 6000;
        private Dictionary<TcpClient, StarshipClientData> clients;
        private int clientId;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.run();            
        }

        private void run()
        {
            Console.WriteLine("Server for MultiAsteroids game");
            this.clients = new Dictionary<TcpClient, StarshipClientData>();
            this.clientId = 1;

            IPAddress ip;
            if (!IPAddress.TryParse("0.0.0.0", out ip))
                writeError("cannot parse this IP");

            TcpListener listener = new TcpListener(ip, port);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread thread = new Thread(() => handleClientThread(client));                
                thread.Start();                
            }
        }

        private void handleClientThread(object obj)
        {
            TcpClient client = obj as TcpClient;
            bool done = false;
            this.clients.Add(client, new StarshipClientData(clientId));
            Console.WriteLine("New Client accepted : " + ((IPEndPoint)client.Client.RemoteEndPoint).Address);           

            this.clientId++;            

            while (!done)
            {
                ReadMessage(client);
            }
        }

        private void ReadMessage(TcpClient client)
        {
            byte[] buffer = new byte[12];
            client.GetStream().Read(buffer, 0, buffer.Length);

            byte[] xAs = new byte[4];
            byte[] yAs = new byte[4];
            byte[] rot = new byte[4];

            for(int i=0; i<4; i++)      
                 xAs[i] = buffer[i];
            for (int i = 4; i < 8; i++)
                yAs[i % 4] = buffer[i];
            for (int i = 8; i < 12; i++)
                rot[i % 8] = buffer[i];

            clients[client].Update(FloatUnion.BytesToFloat(xAs), FloatUnion.BytesToFloat(yAs), FloatUnion.BytesToFloat(rot));

            writeMessage(client);
        }

        private void writeMessage(TcpClient client)
        {
            foreach (KeyValuePair<TcpClient, StarshipClientData> entry in clients)
            {
                if (!entry.Key.Equals(client))
                {
                    List<byte> data = new List<byte>();
                    data.Add((byte)entry.Value.ID);
                    foreach (byte b in FloatUnion.FloatToBytes(entry.Value.X))
                        data.Add(b);
                    foreach (byte b in FloatUnion.FloatToBytes(entry.Value.Y))
                        data.Add(b);
                    foreach (byte b in FloatUnion.FloatToBytes(entry.Value.Rotation))
                        data.Add(b);

                    client.GetStream().Write(data.ToArray(), 0, data.Count);
                    Console.WriteLine(string.Format("{0}, X:{1} Y:{2}", entry.Value.ID, entry.Value.X, entry.Value.Y));
                }
            }                
        }

        private void writeError(string description)
        {
            Console.WriteLine("ERROR: " + description);
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
