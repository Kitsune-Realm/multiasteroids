using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary.Packets
{
    public class PacketPosition : Packet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rot { get; set; }

        public PacketPosition()
        {
            this.ID = (int)MessageType.Movement;
        }

        public override void Send(Socket socket)
        {
            List<byte> data = new List<byte>();

            data.Add((byte)this.ID);
            foreach (byte b in BitConverter.GetBytes(X))
                data.Add(b);
            foreach (byte b in BitConverter.GetBytes(Y))
                data.Add(b);
            foreach (byte b in BitConverter.GetBytes(Rot))
                data.Add(b);

            // finally add the total size of the array as the First index
            data.Insert(0, (byte)data.Count);
        }      
    }
}
