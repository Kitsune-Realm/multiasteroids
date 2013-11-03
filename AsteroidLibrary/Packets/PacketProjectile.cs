using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary.Packets
{
    public class PacketProjectile : Packet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rot { get; set; }

        public PacketProjectile(float x, float y, float rotation)
        {
            this.ID = (int)MessageType.Movement;
            this.X = x;
            this.Y = y;
            this.Rot = rotation;
            this.data = new List<byte>();
        }

        public override void Send(Socket socket)
        {
            data.Clear();
            data.Add((byte)this.ID);
            foreach (byte b in BitConverter.GetBytes(X))
                data.Add(b);
            foreach (byte b in BitConverter.GetBytes(Y))
                data.Add(b);
            foreach (byte b in BitConverter.GetBytes(Rot))
                data.Add(b);

            // finally add the total size of the array as the First index
            data.Insert(0, (byte)data.Count);

            socket.Send(data.ToArray());
        }      
    }
}
