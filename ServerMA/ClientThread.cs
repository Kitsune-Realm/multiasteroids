using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using AsteroidLibrary;
using AsteroidLibrary.Packets;

namespace ServerMA
{
    class ClientThread
    {
        private Socket socket;
        byte[] buffer = new byte[256];


        public ClientThread(Socket newSocket)
        {
            this.socket = newSocket;
            Thread thread = new Thread(new ThreadStart(run));
            thread.Start();

        }
        public void run()
        {
            bool running = true;
            int read = 0;
            while (running)
            {
                read = socket.Receive(buffer);
                if (buffer[1] == (int)MessageType.PlayerReadyStatus)
                {
                    PacketReady pr;
                    PacketReady.Parse(buffer, out pr);
                    Console.WriteLine("<{0}> - Player {1} is {2}", socket.RemoteEndPoint.ToString(), pr.PlayerNumber, pr.IsReady);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
