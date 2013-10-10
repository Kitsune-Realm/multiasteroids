using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

namespace MultiAsteroids
{
    class MenuItem
    {
        public Texture2D Texture;
        public Texture2D Texture_on;
        public Texture2D Texture_off;
        public Vector2 position { get; set; }

        public MenuItem(ContentManager content, string texture)
        {
            this.Texture = content.Load<Texture2D>(texture);
            this.position = new Vector2(0, 0);
        }

        public MenuItem(ContentManager content, string texture_on, string texture_off)
        {
            this.Texture_on = content.Load<Texture2D>(texture_on);
            this.Texture_off = content.Load<Texture2D>(texture_off);
            this.position = new Vector2(0, 0);
        }

        public MenuItem(ContentManager content, string texture, Vector2 position)
        {
            this.Texture = content.Load<Texture2D>(texture);
            this.position = position;
        }
    }
}
