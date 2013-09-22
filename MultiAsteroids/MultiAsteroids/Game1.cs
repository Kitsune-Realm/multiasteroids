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

namespace MultiAsteroids
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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
            player1 = new Starship("USS Avenger");
            player1.PositionChanged += new EventHandler(player1_PositionChanged);


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
            player1.ShipTexture = Content.Load<Texture2D>("ship_texture_breen");
            player1.Origin = new Vector2(player1.ShipTexture.Width / 2, player1.ShipTexture.Height / 2);
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
            determineKeyboardMovement();
            player1.MovementUpdate();

            Console.WriteLine("X:{0} Y:{1} R:{2}", player1.X, player1.Y, player1.RotationAngle);

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

            spriteBatch.DrawString(font, player1.Name, new Vector2(0,0), Color.White);            
            spriteBatch.Draw(player1.ShipTexture, player1.Position, null, Color.White, player1.RotationAngle, player1.Origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void determineKeyboardMovement()
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Up) && !state.IsKeyDown(Keys.Down))
                player1.MoveForward = true;
            if (state.IsKeyDown(Keys.Down) && !state.IsKeyDown(Keys.Up))
                player1.MoveBackward = true;
            if (state.IsKeyDown(Keys.Left) && !state.IsKeyDown(Keys.Right))
                player1.MoveLeft = true;
            if (state.IsKeyDown(Keys.Right) && !state.IsKeyDown(Keys.Left))
                player1.MoveRight = true;            
        }
    }
}
