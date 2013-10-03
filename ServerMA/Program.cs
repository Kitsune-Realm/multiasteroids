using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace ServerMA
{
    class Program
    {
        private static int port = 6000;
        private List<StarshipClient> clients;
        private int clientId;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.run();
            
        }

        private void run()
        {
            Console.WriteLine("Server for MultiAsteroids game");
            this.clients = new List<StarshipClient>();
            this.clientId = 1;

            IPAddress ip;
            if (!IPAddress.TryParse("0.0.0.0", out ip))
                writeError("cannot parse this IP");


            byte[] test = new byte[2];
            test[0] = 8;
            test[1] = 50;
            decodeBytes(test);

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
            Console.WriteLine("New Client accepted : " + ((IPEndPoint)client.Client.RemoteEndPoint).Address);
            //string[] clientData = ReadMessage(client);
            this.clients.Add(new StarshipClient(clientId));
            this.clientId++;
            
            while (!done)
            {

            }
        }

        private void writeError(string description)
        {
            Console.WriteLine("ERROR: " + description);
            Console.ReadKey();
            Environment.Exit(0);
        }

        private void ReadMessage(TcpClient client)
        {
            byte[] buffer = new byte[256];
            int totalRead = 0;

            do
            {
                int read = client.GetStream().Read(buffer, totalRead, buffer.Length - totalRead);
                totalRead += read;
            } while (client.GetStream().DataAvailable);

            //return decodeBytes(buffer);
        }

        private void decodeBytes(byte[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                Console.WriteLine("byte " + i + ": " + Convert.ToString(input[i], 2));
            }            
        }
    }
}
