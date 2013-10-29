using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary
{
    public class PacketPosition : Packet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rot { get; set; }

        public override void Send(Socket socket)
        {            
        }
        public override byte[] convertToBytes()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)14); // size of this packet

            data.Add((byte)this.ID);
            foreach (byte b in BitConverter.GetBytes(X))
                data.Add(b);
            foreach (byte b in BitConverter.GetBytes(Y))
                data.Add(b);
            foreach (byte b in BitConverter.GetBytes(Rot))
                data.Add(b);

            return data.ToArray();
        }       
    }
}
