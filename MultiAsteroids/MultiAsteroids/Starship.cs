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
    class Starship
    {
        private float x;
        public float X { get { return this.x; } set { this.x = value; PositionChanged(this, new EventArgs()); } }
        private float y;
        public float Y { get { return this.y; } set { this.y = value; PositionChanged(this, new EventArgs()); } }
        public string Name { get; private set; }
        public Vector2 Position { get; set; }
        public Texture2D ShipTexture { get; set; }
        public bool MoveForward { get; set; }
        public bool MoveBackward { get; set; }
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }
        public float RotationAngle { get; set; }
        public Vector2 Origin { get; set; }
        public float Velocity { get; set; }
        public float BackVelocity { get; set; }

        public event EventHandler PositionChanged;

        public Starship(string name)
        {
            this.x = 0;
            this.y = 0;
            this.Name = name;
            this.Position = new Vector2(0,0);
            this.Origin = new Vector2(0,0);
            this.RotationAngle = 0;
            this.Velocity = 5;
            this.BackVelocity = 2;
            this.PositionChanged += new EventHandler(Starship_PositionChanged);
        }

        void Starship_PositionChanged(object sender, EventArgs e)
        {
            UpdatePosition(); // looks lumpy cannot put above in constructor???
        }


        public void UpdatePosition()
        {
            this.Position = new Vector2(this.X, this.Y);
        }

        public void UpdatePosition(float x, float y)
        {
            this.Position = new Vector2(x, y);
        }

        public void MovementUpdate()
        {
            if (MoveForward)
                moveForward();
            if (MoveBackward)
                moveBackward();
            if (MoveLeft)
                this.RotationAngle -= 0.06f;
            if (MoveRight)
                this.RotationAngle += 0.06f;

            if (this.RotationAngle < 0)
                this.RotationAngle = (float)((2 * Math.PI) + this.RotationAngle);
            this.RotationAngle = (float)(this.RotationAngle % (2*Math.PI));
        }

        private void moveForward()
        {
                this.X += (float)(Math.Sin(RotationAngle) * this.Velocity);
                this.Y -= (float)(Math.Cos(RotationAngle) * this.Velocity);         
        }

        private void moveBackward()
        {
            this.X -= (float)(Math.Sin(RotationAngle) * this.BackVelocity);
            this.Y += (float)(Math.Cos(RotationAngle) * this.BackVelocity);
        }

        public void MovementReset()
        {
            MoveForward = MoveBackward = MoveLeft = MoveRight = false;
        }
    }
}
