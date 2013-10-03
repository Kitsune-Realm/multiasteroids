using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMA
{
    class StarshipClientData
    {
        public float x { get; set; }
        public float y { get; set; }
        public float rotation { get; set; }
        public int id { get; set; }

        public StarshipClientData(int id)
        {
            this.id = id;
        }

        public void Update(float x, float y, float rotation)
        {
            this.x = x;
            this.y = y;
            this.rotation = rotation;
        }
    }
}
