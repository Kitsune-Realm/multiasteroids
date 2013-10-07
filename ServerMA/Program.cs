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
        //public delegate void OnClientAddedHandler(Object sender, ClientAddedMessage e);
        //public event OnClientAddedHandler OnClientAdded;

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
            Console.WriteLine(string.Format("New Client accepted : {0} (Player {1})", ((IPEndPoint)client.Client.RemoteEndPoint).Address, clientId));

            writeAddClientMessage(client);          
            this.clientId++;
            //ClientAddedMessage cam = new ClientAddedMessage();
            //cam.Send();           

            while (!done)
            {
                ReadMessage(client);
            }
        }

        private void ReadMessage(TcpClient client)
        {
            byte[] buffer = new byte[12];
            client.GetStream().Read(buffer, 0, buffer.Length);

            // create 1 large byte array containing all player data at once
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

            writeMovementMessage(client);
        }

        private void writeMovementMessage(TcpClient client)
        {
            foreach (KeyValuePair<TcpClient, StarshipClientData> entry in clients)
            {
                if (!entry.Key.Equals(client))
                {
                    List<byte> data = new List<byte>();
                    data.Add((int)MessageType.Movement);
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

        private void writeAddClientMessage(TcpClient client)
        {
            foreach (KeyValuePair<TcpClient, StarshipClientData> entry in clients)
            {
                if (!entry.Key.Equals(client))
                {
                    List<byte> data = new List<byte>();
                    data.Add((int)MessageType.AddedClient);
                    data.Add((byte)clientId);
                    entry.Key.GetStream().Write(data.ToArray(), 0, data.Count);
                }
                else
                {
                    foreach (KeyValuePair<TcpClient, StarshipClientData> entry2 in clients)
                    {
                        if (!entry2.Key.Equals(client))
                        {
                            List<byte> data2 = new List<byte>();
                            data2.Add((int)MessageType.AddedClient);
                            data2.Add((byte)entry2.Value.ID);
                            client.GetStream().Write(data2.ToArray(), 0, data2.Count);
                        }
                    }
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
