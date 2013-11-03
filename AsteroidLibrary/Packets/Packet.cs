using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary.Packets
{
    public abstract class Packet
    {
        public int ID { get; set; }
        protected List<byte> data;
        public abstract void Send(Socket socket);        
    }
}
