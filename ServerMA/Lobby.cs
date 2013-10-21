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
        public List<StarshipClientData> PlayersInLobby { get; set; }
        public TcpClient client { get; set; }
        public int LobbyID { get; set; }
        public bool allPlayersReady { get; set; }

        public Lobby(int id)
        {
            this.PlayersInLobby = new List<StarshipClientData>();
            this.LobbyID = id;            
            this.allPlayersReady = false;
        }

        public StarshipClientData GetHost()
        {
            if(PlayersInLobby.Count > 0)
                return PlayersInLobby[0];
            return null;
        }

        public StarshipClientData GetPlayerInLobby(int playerNumber)
        {
            foreach (StarshipClientData scd in PlayersInLobby)
            {
                if (scd.ID == playerNumber)
                    return scd;
            }
            return null;
        }

        public StarshipClientData[] GetOtherPlayersInLobby(int playerNumber)
        {
            List<StarshipClientData> localArray = new List<StarshipClientData>();
            foreach (StarshipClientData scd in PlayersInLobby)
            {
                if (scd.ID != playerNumber)
                    localArray.Add(scd);
            }
            return localArray.ToArray();
        }

        public bool CheckAllPlayersReady()
        {
            foreach (StarshipClientData scd in PlayersInLobby)
            {
                if (!scd.isReady)
                    return false;
            }
            return true;
        }
    }
}
