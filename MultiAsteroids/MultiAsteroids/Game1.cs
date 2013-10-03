using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AsteroidLibrary;

namespace MultiAsteroids
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState state;
        List<StarshipClientData> otherPlayers = new List<StarshipClientData>();

        SpriteFont font;
        
        Starship player1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            font = Content.Load<SpriteFont>("displayFont");

            LoadGame();

            base.Initialize();
        }

        void player1_PositionChanged(object sender, EventArgs e)
        {
            player1.UpdatePosition();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void LoadGame()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player1 = new Starship("Chel Grett", this.Content);            
            player1.PositionChanged += new EventHandler(player1_PositionChanged);
            otherPlayers.Add(new StarshipClientData(2));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            player1.MovementReset();
            determineKeyboardInput();
            player1.MovementUpdate();

            player1.clientComm.Send(player1.X, player1.Y, player1.RotationAngle);           
            
            byte[] buffer = player1.clientComm.Read();
            if (buffer != null)
            {
                byte[] xAs = new byte[4];
                byte[] yAs = new byte[4];
                byte[] rot = new byte[4];

                for (int i = 1; i < 5; i++)
                    xAs[(i - 1)] = buffer[i];
                for (int i = 5; i < 9; i++)
                    yAs[i % 5] = buffer[i];
                for (int i = 9; i < 13; i++)
                    rot[i % 9] = buffer[i];

                otherPlayers[0].Update(FloatUnion.BytesToFloat(xAs), FloatUnion.BytesToFloat(yAs), FloatUnion.BytesToFloat(rot));
            }
            updateProjectiles(gameTime);

            //Console.WriteLine("X:{0} Y:{1} R:{2}", player1.X, player1.Y, player1.RotationAngle);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            foreach (Projectile projectile in player1.projectiles)
            {
                if (projectile.IsAlive)
                {
                    spriteBatch.Draw(projectile.Texture, new Rectangle((int)projectile.Position.X, (int)projectile.Position.Y, projectile.SpriteWidth, projectile.SpriteHeight), projectile.SpriteRectangle, Color.White, projectile.RotationAngle, projectile.Origin, SpriteEffects.None, 0f);
                }
            }

            drawStatistics();

            spriteBatch.Draw(player1.ShipTexture, player1.Position, null, Color.White, player1.RotationAngle, player1.Origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void determineKeyboardInput()
        {
            state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Space))
                fireProjectile();
            // Up, Left and Space causes bug, Space wont work then...
            if (state.IsKeyDown(Keys.W))
                player1.MoveForward = true;
            if (state.IsKeyDown(Keys.S))
                player1.MoveBackward = true;
            if (state.IsKeyDown(Keys.A))
                player1.MoveLeft = true;
            if (state.IsKeyDown(Keys.D))
                player1.MoveRight = true;                            
        }

        private void updateProjectiles(GameTime gameTime)
        {
            foreach (Projectile projectile in player1.projectiles)
            {
                if (projectile.IsAlive)
                {
                    projectile.Timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (projectile.Timer >= projectile.Interval)
                    {
                        projectile.Timer = 0;
                        projectile.CurrentFrame++;
                        projectile.CurrentFrame %= projectile.AmountImages;
                    }
                    projectile.SpriteRectangle.X = projectile.CurrentFrame * projectile.SpriteWidth;

                    float x = (float)(Math.Sin(projectile.RotationAngle) * projectile.Velocity);
                    float y = (float)(Math.Cos(projectile.RotationAngle) * projectile.Velocity);
                    projectile.UpdatePosition(x, y);
                    //Console.WriteLine("X:{0} Y:{1} R:{2}", projectile.Position.X, projectile.Position.Y, projectile.RotationAngle);

                    if (projectile.Position.X < 0 || projectile.Position.X > GraphicsDevice.Viewport.Width || projectile.Position.Y < 0 || projectile.Position.Y > GraphicsDevice.Viewport.Height)
                        projectile.IsAlive = false;
                }
            }
        }

        private void fireProjectile()
        {
            foreach (Projectile projectile in player1.projectiles)
            {
                if (!projectile.IsAlive)
                {
                    projectile.soundEffect.Play();
                    projectile.IsAlive = true;
                    projectile.RotationAngle = player1.RotationAngle;
                    projectile.Position = player1.Position;
                    break;
                }                    
            }
        }

        private void drawStatistics()
        {
            spriteBatch.DrawString(font, player1.Name, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "X: " + player1.X + " Y: "+ player1.Y, new Vector2(0, 11), Color.White);
            spriteBatch.DrawString(font, "Angle: " + player1.RotationAngle, new Vector2(0, 22), Color.White);

            spriteBatch.DrawString(font, "Player 2", new Vector2(0, 55), Color.White);
            spriteBatch.DrawString(font, "X: " + otherPlayers[0].X + " Y: " + otherPlayers[0].Y, new Vector2(0, 66), Color.White);
            spriteBatch.DrawString(font, "Angle: " + otherPlayers[0].Rotation, new Vector2(0, 77), Color.White); 
        }
    }
}
