using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary.Packets
{
    public class PacketReady : Packet
    {
        public bool Status { get; set; }
        public int PlayerNumber { get; set; }
        public string IsReady { get {return (this.Status) ? "Ready!" : "Not ready";} }

        public PacketReady(bool status, int playerNumber)
        {
            this.Status = status;
            this.PlayerNumber = playerNumber;
            this.data = new List<byte>();
        }

        public override void Send(Socket socket)
        {
            data.Clear();
            data.Add((byte)MessageType.PlayerReadyStatus);
            data.Add((byte)PlayerNumber);
            data.Add((byte)(Status ? 1 : 0));

            // finally add the total size of the array as the First index
            data.Insert(0, (byte)data.Count);

            socket.Send(data.ToArray());
        }

        public static void Parse(byte[] data, out PacketReady pr)
        {
            bool status = (data[3] == 1);
            pr = new PacketReady(status, (int)data[2]);
        }
    }
}
