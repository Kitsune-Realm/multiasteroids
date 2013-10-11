using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidLibrary;
using System.Net.Sockets;

namespace ServerMA
{
    class Lobby
    {
        public List<StarshipClientData> OtherPlayersInLobby { get; set; }
        public TcpClient client { get; set; }
        public int LobbyID { get; set; }
        public StarshipClientData Host { get; set; }
        public bool gameStarted { get; set; }
        public List<bool> playersReady;

        public Lobby(int id)
        {
            this.OtherPlayersInLobby = new List<StarshipClientData>();
            this.LobbyID = id;            
            this.gameStarted = false;
            this.playersReady = new List<bool>();
        }

        public int allPlayersReady()
        {            
            foreach (bool b in playersReady)
                if (!b)
                    return 0;
            return 1;
        }
    }
}
