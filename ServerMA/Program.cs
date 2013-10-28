using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using AsteroidLibrary;
using System.IO;
using System.Collections;

namespace ServerMA
{
    class Program
    {
        //public delegate void OnClientAddedHandler(Object sender, ClientAddedMessage e);
        //public event OnClientAddedHandler OnClientAdded;

        private static int port = 5938; // movement
        private List<TcpClient> clients;
        private int clientId;
        private int lobbyId;
        private Lobby lobby;    

        static void Main(string[] args)
        {
            Program program = new Program();
            program.run();            
        }

        private void run()
        {
            Console.WriteLine("Server for MultiAsteroids game");
            this.clients = new List<TcpClient>();
            this.clientId = 1;
            this.lobbyId = 1;
            lobby = new Lobby(lobbyId);
            Console.WriteLine(string.Format("Lobby #{0} is open now", this.lobbyId));


            IPAddress ip;
            if (!IPAddress.TryParse("0.0.0.0", out ip))
                Console.WriteLine("cannot parse this IP");

            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                addToLog(client);
                Thread thread = new Thread(() => handleClientThread(client));
                thread.Name = "handling client";
                thread.Start();
            }
        }

        private void addToLog(TcpClient client)
        {
            using (FileStream fileStream = new FileStream("log.txt", FileMode.OpenOrCreate, FileAccess.Write))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
                streamWriter.WriteLine(string.Format("<{0}> - Client {1}", DateTime.Now, client.ToString()));
        }

        private void handleClientThread(object obj)
        {
            TcpClient client = obj as TcpClient;
            StarshipClientData clientData = new StarshipClientData(clientId);
            bool running = true;
            string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            updateLobby(clientData, clientIp);            
            this.clients.Add(client);                

            writeAddClientMessage(client);
            Console.WriteLine("Player {0} has been added!", clientId);
            this.clientId++; 

            while (!lobby.allPlayersReady)
            {                
                writeClientReadyStatus(client);
                readClientReadyStatus(client);
            }
            writeClientReadyStatus(client); // write the status one more time to let the game know to switch to gameState.Playing

            while (running)
            {    
                try
                {
                    writeMovementMessage(client);
                    readIncomingMessage(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("ERROR({0}): {1}", clientIp, ex.Message));
                    running = false;
                }
            }
        }

        private void handleProjectilesThread(object obj)
        {
            Console.WriteLine("THREAD - Listening for Projectiles");
            TcpClient client = obj as TcpClient;
            bool running = true;
            while (running)
            {
                byte[] buffer = new byte[256]; // (lobby.PlayersInLobby.Count * 13) + 1
                client.GetStream().Read(buffer, 0, buffer.Length);
                Console.WriteLine("Projectile heard");
                switch ((int)buffer[0])
                {
                    case (int)MessageType.PlayerFired:
                        byte[] xProj = new byte[4];
                        byte[] yProj = new byte[4];
                        byte[] rProj = new byte[4];
                        int playerNumber = buffer[1];
                        int projectileType = buffer[2];

                        for (int c = 0; c < 4; c++)
                            xProj[c] = buffer[c + 3];
                        for (int c = 0; c < 4; c++)
                            yProj[c] = buffer[c + 7];
                        for (int c = 0; c < 4; c++)
                            rProj[c] = buffer[c + 11];

                        Console.WriteLine("Player {0} fired a {1} projectile at X: {2}, Y: {3}, R: {4}", playerNumber, ((ProjectileType)projectileType).ToString(),
                            FloatUnion.BytesToFloat(xProj), FloatUnion.BytesToFloat(yProj), FloatUnion.BytesToFloat(rProj));
                        break;
                }

                foreach (TcpClient c in clients)
                {
                    if (c != client)
                    {
                        Console.WriteLine("Sending Projectile data to other player");
                        buffer[0] = (byte)MessageType.ServerSendsFired;
                        c.GetStream().Flush();
                        c.GetStream().Write(buffer, 0, 15);
                        // Client can never recieve this for some reason
                    }
                }
            }
            Console.WriteLine("Thread complete");
        }

        private void updateLobby(StarshipClientData clientData, string clientIp)
        {
            if (lobby.PlayersInLobby.Count == 0) //first in lobby            
            {
                lobby.PlayersInLobby.Add(clientData);                
                Console.WriteLine("Player {0} ({1}) entered lobby {2} as Host", clientData.ID, clientIp, lobbyId);
            }
            else if (lobby.PlayersInLobby.Count < 4)
            {
                lobby.PlayersInLobby.Add(clientData);
                Console.WriteLine("Player {0} ({1}) entered lobby {2} at spot {3}", clientData.ID, clientIp, lobbyId, (lobby.PlayersInLobby.Count - 1));
            }
            else
                Console.WriteLine("ERROR({0}): cannot enter lobby", clientIp);            
        }

        private void readIncomingMessage(TcpClient client)
        {
            byte[] buffer = new byte[256]; // (lobby.PlayersInLobby.Count * 13) + 1
            client.GetStream().Read(buffer, 0, buffer.Length);            
            switch ((int)buffer[0])
            {
                case (int)MessageType.Movement:
                    for (int i = 1; i < lobby.PlayersInLobby.Count; i += 13) // per client
                    {                        
                        byte[] xAs = new byte[4];
                        byte[] yAs = new byte[4];
                        byte[] rot = new byte[4];
                        int player = (int)buffer[i];
                        for (int c = 0; c < 4; c++)
                            xAs[c] = buffer[(i + 1) + c];
                        for (int c = 0; c < 4; c++)
                            yAs[c] = buffer[(i + 5) + c];
                        for (int c = 0; c < 4; c++)
                            rot[c] = buffer[(i + 9) + c];
                        lobby.GetPlayerInLobby(player).Update(FloatUnion.BytesToFloat(xAs), FloatUnion.BytesToFloat(yAs), FloatUnion.BytesToFloat(rot));
                        Console.WriteLine("({0}) - X:{1} Y:{2} R:{3}", player, FloatUnion.BytesToFloat(xAs), FloatUnion.BytesToFloat(yAs), FloatUnion.BytesToFloat(rot));                                                
                    }
                    break; 
                case (int)MessageType.PlayerFired:
                    {
                        Console.WriteLine("player shot");
                    }
                    break;
            }            
        }

        private void writeMovementMessage(TcpClient client)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.Movement); // Message type byte
            foreach (StarshipClientData scd in lobby.PlayersInLobby)
            {   
                data.Add((byte)scd.ID); // Player number byte
                foreach (byte b in FloatUnion.FloatToBytes(scd.X))
                    data.Add(b);
                foreach (byte b in FloatUnion.FloatToBytes(scd.Y))
                    data.Add(b);
                foreach (byte b in FloatUnion.FloatToBytes(scd.Rotation))
                    data.Add(b);             
            }
            data.Add((int)MessageType.EndOfMessage);
            client.GetStream().Write(data.ToArray(), 0, data.Count);
            //client.GetStream().Flush();
        }

        // change to delegates?
        private void writeAddClientMessage(TcpClient client)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.AddedClient);
            data.Add((byte)clientId);
            client.GetStream().Write(data.ToArray(), 0, data.Count);
            client.GetStream().Flush();
        }

        private void writeClientReadyStatus(TcpClient client)
        {
            List<byte> data = new List<byte>();
            data.Add((int)MessageType.PlayerReadyStatus);
            data.Add((byte)(lobby.PlayersInLobby.Count));
            foreach (StarshipClientData scd in lobby.PlayersInLobby)
                data.Add((scd.isReady) ? (byte)1 : (byte)0);            
            client.GetStream().Write(data.ToArray(), 0, data.Count);
            client.GetStream().Flush();


            //foreach (byte b in data)
            //    Console.Write(b.ToString());
            //Console.WriteLine(" - ({0})", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

        }

        private void readClientReadyStatus(TcpClient client)
        {
            try
            {
                // ID, Ready, PlayerNum
                byte[] buffer = new byte[client.ReceiveBufferSize];
                List<byte> data = new List<byte>();
                client.GetStream().Read(buffer, 0, 3);
                switch ((int)buffer[0])
                {
                    case (int)MessageType.PlayerReadyStatus:                                          
                        lobby.GetPlayerInLobby(buffer[2]).isReady = (buffer[1] == 1); // true or false
                        lobby.allPlayersReady = lobby.CheckAllPlayersReady();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
    }
}
