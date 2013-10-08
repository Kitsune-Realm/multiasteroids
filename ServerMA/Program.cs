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
            this.clients.Add(client, new StarshipClientData(clientId));
            Console.WriteLine(string.Format("New Client accepted : {0} (Player {1})", ((IPEndPoint)client.Client.RemoteEndPoint).Address, clientId));
            bool running = true;
            writeAddClientMessage(client);
            this.clientId++;   

            while (running)
            {
                readMessage(client);
                writeMovementMessage(client);
            }
        }

        private void readMessage(TcpClient client)
        {
            byte[] buffer = new byte[(clients.Count*14) + 1];
            client.GetStream().Read(buffer, 0, buffer.Length);

            switch((int)buffer[0])
            {
                case (int)MessageType.Movement:
                    for (int i = 1; i < buffer.Length; i+=14) // per client
                    {
                        byte[] xAs = new byte[4];
                        byte[] yAs = new byte[4];
                        byte[] rot = new byte[4];
                        int player = (int)buffer[i];
                        for(int c = 0; c<4; c++)
                            xAs[c] = buffer[(i + 1) + c];
                        for (int c = 0; c < 4; c++)
                            yAs[c] = buffer[(i + 5) + c];
                        for (int c = 0; c < 4; c++)
                            rot[c] = buffer[(i + 9) + c];

                        foreach (KeyValuePair<TcpClient, StarshipClientData> entry in clients)
                        {
                            if (entry.Value.ID == player)
                            {
                                entry.Value.Update(FloatUnion.BytesToFloat(xAs), FloatUnion.BytesToFloat(yAs), FloatUnion.BytesToFloat(rot));                                
                            }
                            break;
                        }
                    }
                    break;
            }
        }

        private void writeMovementMessage(TcpClient client)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.Movement); // Message type byte
            foreach (KeyValuePair<TcpClient, StarshipClientData> entry in clients)
            {   
                data.Add((byte)entry.Value.ID); // Player number byte
                foreach (byte b in FloatUnion.FloatToBytes(entry.Value.X))
                    data.Add(b);
                foreach (byte b in FloatUnion.FloatToBytes(entry.Value.Y))
                    data.Add(b);
                foreach (byte b in FloatUnion.FloatToBytes(entry.Value.Rotation))
                    data.Add(b);             
            }
            client.GetStream().Write(data.ToArray(), 0, data.Count);
        }

        private void writeAddClientMessage(TcpClient client)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.AddedClient);
            data.Add((byte)clientId);
            client.GetStream().Write(data.ToArray(), 0, data.Count);
        }
            //foreach (KeyValuePair<TcpClient, StarshipClientData> entry in clients)
            //{
            //    if (!entry.Key.Equals(client))
            //    {
            //        List<byte> data = new List<byte>();
            //        data.Add((int)MessageType.AddedClient);
            //        data.Add((byte)clientId);
            //        entry.Key.GetStream().Write(data.ToArray(), 0, data.Count);
            //    }
            //    else
            //    {
            //        foreach (KeyValuePair<TcpClient, StarshipClientData> entry2 in clients)
            //        {
            //            if (!entry2.Key.Equals(client))
            //            {
            //                List<byte> data2 = new List<byte>();
            //                data2.Add((int)MessageType.AddedClient);
            //                data2.Add((byte)entry2.Value.ID);
            //                client.GetStream().Write(data2.ToArray(), 0, data2.Count);
            //            }
            //        }
            //    }
            //}
        


        private void writeError(string description)
        {
            Console.WriteLine("ERROR: " + description);
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
