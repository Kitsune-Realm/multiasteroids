using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMA
{
    class StarshipClient
    {
        public int x { get; set; }
        public int y { get; set; }
        public int id { get; set; }

        public StarshipClient(int id)
        {
            this.id = id;
        }
    }
}
