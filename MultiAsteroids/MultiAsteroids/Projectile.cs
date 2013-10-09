using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MultiAsteroids
{
    class Projectile
    {
        public Vector2 Position { get; set; }
        public bool IsAlive { get; set; }
        public int Velocity { get; set; }
        public float RotationAngle { get; set; }
        public Vector2 Origin { get; set; }
        public Texture2D Texture { get; set; }
        public SoundEffect soundEffect { get; set; }

        public float Timer = 0;
        public float Interval = 40f;
        public int CurrentFrame = 0;
        public int SpriteWidth;
        public int SpriteHeight;
        public int AmountImages = 6;
        public Rectangle SpriteRectangle;

        public Projectile(ContentManager content)
        {
            this.IsAlive = false;          
            this.Position = new Vector2(50, 50);
            this.Velocity = 10;
            this.Texture = content.Load<Texture2D>("green_projectile_sheet");
            this.Origin = new Vector2((this.Texture.Width / AmountImages)/2, this.Texture.Height / 2);

            this.SpriteWidth = Texture.Width / AmountImages;
            this.SpriteHeight = Texture.Height;
            this.SpriteRectangle = new Rectangle(CurrentFrame * SpriteWidth, 0, SpriteWidth, SpriteHeight);

            this.soundEffect = content.Load<SoundEffect>("sounds/projectile_1_fire");
        }

        public void UpdatePosition(float x, float y)
        {
            this.Position = new Vector2(this.Position.X + x, this.Position.Y - y);
        }
    }
}
