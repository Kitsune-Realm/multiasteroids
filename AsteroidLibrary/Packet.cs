using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary
{
    public abstract class Packet
    {
        public int ID { get; set; }

        public abstract void Send(Socket socket);
        public abstract byte[] convertToBytes();
    }
}
