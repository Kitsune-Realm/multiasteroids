using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidLibrary
{
    public class StarshipClientData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public int ID { get; set; }
      

        public StarshipClientData(int id)
        {
            this.ID = id;
        }

        public void Update(float x, float y, float rotation)
        {
            this.X = x;
            this.Y = y;
            this.Rotation = rotation;
        }
    }
}
