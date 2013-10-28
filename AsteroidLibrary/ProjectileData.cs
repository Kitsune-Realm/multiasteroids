using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsteroidLibrary
{
    public class ProjectileData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float RotationAngle { get; set; }
        public TcpClient Owner { get; set; }

        public ProjectileData(float x, float y, float rotationangle)
        {
            this.X = x;
            this.Y = y;
            this.RotationAngle = rotationangle;
        }

        public ProjectileData(float x, float y, float rotationangle, TcpClient owner)
        {
            this.X = x;
            this.Y = y;
            this.RotationAngle = rotationangle;
            this.Owner = owner;
        }
    }
}
