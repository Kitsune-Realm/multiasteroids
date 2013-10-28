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
using System.Collections;

namespace MultiAsteroids
{
    public delegate void PlayerFiredHandler(int playerNum, Projectile projectile);

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState newState;
        KeyboardState oldState;
        List<StarshipClientData> otherPlayers = new List<StarshipClientData>();
        private GameState gameState;

        private MenuItem menu_start;
        private MenuItem menu_exit;
        private MenuItem lobby_ready;
        private SelectCursor selectCursor;
        
        SpriteFont font;
        
        Starship player;
        private List<Projectile> otherProjectiles = new List<Projectile>();
        public event PlayerFiredHandler PlayerFired;

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
            font = Content.Load<SpriteFont>("displayFont");            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.gameState = GameState.Loading;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.selectCursor = new SelectCursor(Content);
            this.selectCursor.menuContent = new string[] { "start", "exit"};

            this.menu_start = new MenuItem(Content, "menu_items/menu_start_on", "menu_items/menu_start_off");
            this.menu_start.position = new Vector2(((this.GraphicsDevice.Viewport.Width / 2) - this.menu_start.Texture_on.Width / 2), this.GraphicsDevice.Viewport.Height / 2);
            this.menu_exit = new MenuItem(Content, "menu_items/menu_exit_on", "menu_items/menu_exit_off");
            this.menu_exit.position = new Vector2(((this.GraphicsDevice.Viewport.Width / 2) - this.menu_exit.Texture_on.Width / 2), (this.GraphicsDevice.Viewport.Height / 2)+40);
            this.lobby_ready = new MenuItem(Content, "menu_items/lobby_ready_on", "menu_items/lobby_ready_off");
            this.lobby_ready.position = new Vector2(((this.GraphicsDevice.Viewport.Width / 2) - this.lobby_ready.Texture_on.Width / 2), this.GraphicsDevice.Viewport.Height / 2);
            spriteBatch = new SpriteBatch(GraphicsDevice);            
            player = new Starship(this.Content, this);

            this.gameState = GameState.StartMenu;            
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

            if (this.gameState == GameState.Playing)
            {
                player.MovementReset();
                determineKeyboardInput();
                player.MovementUpdate();
                                
                byte[] read = player.clientComm.Read();                

                switch(read[0])
                {
                    case (int)MessageType.Movement:                
                        foreach (StarshipClientData scd in otherPlayers)
                        {
                            for (int i = 0; i <= otherPlayers.Count; i++)
                            {
                                if (read[1 + (13 * i)] == scd.ID)
                                {
                                    scd.X = FloatUnion.BytesToFloat(read, 2 + (13 * i), 5 + (13 * i));
                                    scd.Y = FloatUnion.BytesToFloat(read, 6 + (13 * i), 9 + (13 * i));
                                    scd.Rotation = FloatUnion.BytesToFloat(read, 10 + (13 * i), 13 + (13 * i));
                                }
                            }
                        }
                        break;
                    case (int)MessageType.ServerSendsFired:
                        float X = FloatUnion.BytesToFloat(read, 3, 6);
                        float Y = FloatUnion.BytesToFloat(read, 7, 10);
                        float Rot = FloatUnion.BytesToFloat(read, 11, 14);
                        Projectile proj = new Projectile(this.Content, read[1], X, Y, Rot);
                         otherProjectiles.Add(proj);
                        break;
                }
                player.Transmit();

                byte[] projectilesRead = player.clientComm.ReadProjectiles();
                if (projectilesRead != null)
                    Console.WriteLine("test");

                
                updateProjectiles(gameTime);
            }
            else if (this.gameState == GameState.StartMenu)
            {
                determineKeyboardInput();
            }
            else if (this.gameState == GameState.Lobby)
            {
                determineKeyboardInput();
                if (!player.clientComm.isListening)                
                    player.clientComm.StartListening();
                if(player.PlayerNumber <= 0)
                    player.AssignPlayerNumber();                
                
                byte[] readyData = new byte[3];
                readyData[0] = (int)MessageType.PlayerReadyStatus;
                readyData[1] = (byte)player.getReadyStatus();
                readyData[2] = (byte)player.PlayerNumber;

                player.clientComm.client.GetStream().Write(readyData,0,3);
                byte[] read = player.clientComm.Read();
                if (read[0] == (int)MessageType.PlayerReadyStatus)
                {
                    bool ready = true;
                    for (int i = 2; i < read[1]+2; i++)
                    {
                        if (read[i] != 1)
                            ready = false;
                    }
                    if (ready)
                    {
                        for (int i = 1; i <= read[1]; i++)
                        {
                            if (i != this.player.PlayerNumber)
                                this.otherPlayers.Add(new StarshipClientData(i));
                        }
                        this.player.clientComm.amountPlayers = otherPlayers.Count + 1;
                        if (player.PlayerNumber == 1)
                            player.UpdatePosition(111f, 111f, 2f);
                        if (player.PlayerNumber == 2)
                            player.UpdatePosition(374f, 246f, 4f);
                        this.gameState = GameState.Playing;
                    }
                    
                }                 
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {     
            spriteBatch.Begin();                    

            if (this.gameState == GameState.Playing)
            {
                GraphicsDevice.Clear(Color.Black);
                foreach (Projectile projectile in player.projectiles)
                {
                    if (projectile.IsAlive)
                    {
                        spriteBatch.Draw(projectile.Texture, new Rectangle((int)projectile.Position.X, (int)projectile.Position.Y, projectile.SpriteWidth, projectile.SpriteHeight), projectile.SpriteRectangle, Color.White, projectile.RotationAngle, projectile.Origin, SpriteEffects.None, 0f);
                    }
                }
                drawStatistics();
                // draw the ship of the player
                spriteBatch.Draw(player.ShipTexture, player.Position, null, Color.White, player.RotationAngle, player.Origin, 1.0f, SpriteEffects.None, 0f);
                // draw other ships
                foreach (StarshipClientData scd in otherPlayers)
                {
                    spriteBatch.Draw(player.ShipTexture, new Vector2(scd.X, scd.Y), null, Color.White, scd.Rotation, player.Origin, 1.0f, SpriteEffects.None, 0f);
                }
            }
            else if (this.gameState == GameState.StartMenu)
            {
                GraphicsDevice.Clear(Color.Black);
                drawStartMenu();
            }

            else if (this.gameState == GameState.Lobby)
            {
                GraphicsDevice.Clear(Color.Black);
                //spriteBatch.DrawString(font, string.Format("{0}, {1}, {2}, {3} ", pR[0], pR[1], pR[2], pR[3]), new Vector2(0, 0), Color.White);
                drawLobbyMenu();
            }

            spriteBatch.End(); 
            base.Draw(gameTime);
        }

        private void determineKeyboardInput()
        {
            newState = Keyboard.GetState();
            if (this.gameState == GameState.Playing)
            {
                if (newState.IsKeyDown(Keys.Space))
                    if (!oldState.IsKeyDown(Keys.Space))
                    {
                        fireProjectile();                        
                    }
                // Up, Left and Space causes bug, Space wont work then...
                if (newState.IsKeyDown(Keys.W))
                    player.MoveForward = true;
                if (newState.IsKeyDown(Keys.S))
                    player.MoveBackward = true;
                if (newState.IsKeyDown(Keys.A))
                    player.MoveLeft = true;
                if (newState.IsKeyDown(Keys.D))
                    player.MoveRight = true;
            }
            else if (this.gameState == GameState.StartMenu)
            {
                if (newState.IsKeyDown(Keys.W))
                    if (!oldState.IsKeyDown(Keys.W))
                        selectCursor.updateMenu(-1);
                if (newState.IsKeyDown(Keys.S))
                    if (!oldState.IsKeyDown(Keys.S))
                        selectCursor.updateMenu(1);
                if (newState.IsKeyDown(Keys.Enter))
                    if (!oldState.IsKeyDown(Keys.Enter))
                        if (selectCursor.menuIndex == 0)
                        {
                            this.gameState = GameState.Lobby;
                            selectCursor.sfxClick.Play();
                        }
                        else
                            Environment.Exit(0);
            }
            else if (this.gameState == GameState.Lobby)
            {
                if (newState.IsKeyDown(Keys.Enter))
                    if (!oldState.IsKeyDown(Keys.Enter))
                    {
                        if (player.isReady)
                        {
                            player.isReady = false;
                            selectCursor.sfxSelect.Play();
                        }
                        else
                        {
                            player.isReady = true;
                            selectCursor.sfxSelect.Play();
                        }
                    }

            }

            // these keys can be used at all time while program is running
            if (newState.IsKeyDown(Keys.Escape))
                if (!oldState.IsKeyDown(Keys.Escape))
                    Environment.Exit(0);

            oldState = newState;
        }

        private void updateProjectiles(GameTime gameTime)
        {
            foreach (Projectile projectile in player.projectiles)
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
            // CHANGE INTO SINGLE METHOD DOUBLE CODE HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            foreach (Projectile projectile in this.otherProjectiles)
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

                    if (projectile.Position.X < 0 || projectile.Position.X > GraphicsDevice.Viewport.Width || projectile.Position.Y < 0 || projectile.Position.Y > GraphicsDevice.Viewport.Height)
                        projectile.IsAlive = false;
                }
            }
        }

        private void fireProjectile()
        {
            foreach (Projectile projectile in player.projectiles)
            {
                if (!projectile.IsAlive &&
                    player.Position.X < GraphicsDevice.Viewport.Width &&
                    player.Position.X > 0 &&
                    player.Position.Y < GraphicsDevice.Viewport.Height &&
                    player.Position.Y > 0)
                {
                    projectile.soundEffect.Play();
                    projectile.IsAlive = true;
                    projectile.RotationAngle = player.RotationAngle;
                    projectile.Position = player.Position;
                    OnPlayerFired(player.PlayerNumber, projectile);
                    break;
                }                    
            }
        }

        private void drawStatistics()
        {
            spriteBatch.DrawString(font, "Player " + player.PlayerNumber, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "X: " + player.X + " Y: "+ player.Y, new Vector2(0, 11), Color.White);
            spriteBatch.DrawString(font, "Angle: " + player.RotationAngle, new Vector2(0, 22), Color.White);

            int y = 44;
            foreach (StarshipClientData scd in otherPlayers)
            {              
                spriteBatch.DrawString(font, "Player " + scd.ID, new Vector2(0, y), Color.White);
                spriteBatch.DrawString(font, "X: " + scd.X + " Y: " + scd.Y, new Vector2(0, y + 11), Color.White);
                spriteBatch.DrawString(font, "Angle: " + scd.Rotation, new Vector2(0, y + 22), Color.White);
                y += 33;
            }
        }

        private void drawStartMenu()
        {    
            if(selectCursor.menuIndex == 0)
                spriteBatch.Draw(this.menu_start.Texture_on, this.menu_start.position, Color.White);
            else
                spriteBatch.Draw(this.menu_start.Texture_off, this.menu_start.position, Color.White);
            if (selectCursor.menuIndex == 1)
                spriteBatch.Draw(this.menu_exit.Texture_on, this.menu_exit.position, Color.White);
            else
                spriteBatch.Draw(this.menu_exit.Texture_off, this.menu_exit.position, Color.White);
            //spriteBatch.Draw(selectCursor.Texture, selectCursor.position, Color.White);
            spriteBatch.DrawString(font, "cursor at: " + selectCursor.menuContent[selectCursor.menuIndex], new Vector2(0, 0), Color.White);
        }

        private void drawLobbyMenu()
        {
            if(player.isReady)
                spriteBatch.Draw(this.lobby_ready.Texture_on, this.menu_start.position, Color.White);
            else
                spriteBatch.Draw(this.lobby_ready.Texture_off, this.menu_start.position, Color.White);
        }

        private void readTransmitDataOtherPlayers()
        {
            byte[] buffer = player.clientComm.Read();
            if (buffer != null)
            {
                if (buffer[0] == (int)MessageType.AddedClient)
                {
                    otherPlayers.Add(new StarshipClientData(buffer[1]));
                }
                else if (buffer[0] == (int)MessageType.Movement)
                {
                    byte[] xAs = new byte[4];
                    byte[] yAs = new byte[4];
                    byte[] rot = new byte[4];
                    byte playerId = buffer[1];

                    for (int i = 2; i < 6; i++)
                        xAs[(i - 2)] = buffer[i];
                    for (int i = 6; i < 10; i++)
                        yAs[i % 6] = buffer[i];
                    for (int i = 10; i < 14; i++)
                        rot[i % 10] = buffer[i];

                    foreach (StarshipClientData scd in otherPlayers)
                    {
                        if (scd.ID == playerId)
                            scd.Update(FloatUnion.BytesToFloat(xAs), FloatUnion.BytesToFloat(yAs), FloatUnion.BytesToFloat(rot));
                    }                    
                }
            }
        }

        protected virtual void OnPlayerFired(int playerNum, Projectile proj)
        {
            PlayerFiredHandler handler = PlayerFired;
            if (handler != null)
                handler(playerNum, proj);
        }
    }
}
