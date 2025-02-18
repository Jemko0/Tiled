using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Tiled.DataStructures;
using Tiled.Gameplay;
using Tiled.ID;
using Tiled.Input;
using Tiled.UI;
using Tiled.UI.Font;
using Tiled.UI.UserWidgets;

namespace Tiled
{
    public class Main : Game
    {
        private Texture2D skyTex;
        private Texture2D sunTex;
        private SpriteBatch _spriteBatch;
        private Effect skyShader;

        public GraphicsDeviceManager _graphics;
        public Camera localCamera;
        public InputManager localInputManager = new InputManager();
        public HUD localHUD;
        public static List<Gameplay.Entity> entities;
        public World world;
        public Controller localPlayerController;

        public static float renderScale = 1.0f;
        public static Point screenCenter;

        public static bool inTitle = true;
        public static Texture2D undergroundBackgroundTexture;
        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "cool game";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            TargetElapsedTime = TimeSpan.FromMilliseconds(16);
            Mappings.InitializeMappings();
            entities = new List<Entity>();
            localPlayerController = new Controller();

            

            localCamera.position.Y = 8912.0f / 3f;
            

            Window.ClientSizeChanged += MainWindowResized;
            CalcRenderScale();
        }

        public void CreatePlayer(Vector2 location)
        {
            EPlayer e = Entity.NewEntity<EPlayer>();
            e.position = location;
            e.Initialize(EEntityType.Player);
            localPlayerController.Possess(e);
        }

        public EPlayer GetLocalPlayer()
        {
            return ((EPlayer)Program.GetGame().localPlayerController.controlledEntity);
        }

        protected override void LoadContent()
        {
            Fonts.InitFonts();

            world = new World();
            //world.Init();
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            localCamera = new Camera(this);

            skyTex = new Texture2D(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            skyShader = Content.Load<Effect>("Shaders/SkyShader");

            sunTex = Content.Load<Texture2D>("Sky/Sun");
            undergroundBackgroundTexture = Content.Load<Texture2D>("UndergroundBackgrounds/DefaultUndergroundBackground");
            localHUD = new HUD(_spriteBatch, _graphics);
        }

        

        private void MainWindowResized(object sender, EventArgs e)
        {
            CalcRenderScale();
        }

        public static void RegisterEntity(Gameplay.Entity e)
        {
            entities.Add(e);
        }

        public static void UnregisterEntity(Gameplay.Entity e)
        {
            entities.Remove(e);
        }

        private void CalcRenderScale()
        {
            screenCenter = new Point(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            renderScale = Window.ClientBounds.Height / 1080.0f;
        }

        public static float delta;
        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                return;
            }

                delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(Keyboard.GetState().IsKeyDown(Keys.A))
            {
                localCamera.position.X -= 50;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                localCamera.position.X += 50;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                localCamera.position.Y -= 50;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                localCamera.position.Y += 50;
            }

            localInputManager.Update();
            Mappings.Update();

            if(World.renderWorld)
            {
                Lighting.Update();
            }
            
            world.UpdateWorld();

            localPlayerController.Update();
            
            foreach(Entity entity in entities.ToList())
            {
                entity.Update();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            RenderSky();
            RenderSun();
            

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            RenderBackground();
            RenderWorld();
            RenderEntities();

            _spriteBatch.End();

            localHUD.DrawWidgets();
        }

        public void RenderBackground()
        {
            if(localCamera.position.Y >  World.averageSurfaceHeight * World.TILESIZE)
            {
                _spriteBatch.Draw(undergroundBackgroundTexture, new Rectangle(0, 1920 + World.averageSurfaceHeight * World.TILESIZE - (int)localCamera.position.Y, Window.ClientBounds.Width, Window.ClientBounds.Height), new Rectangle((int)localCamera.position.X, (int)localCamera.position.Y, (int)(Window.ClientBounds.Width / renderScale), (int)(Window.ClientBounds.Height / renderScale)), Color.White);
            }
        }

        public void RenderWorld()
        {

            if(!World.renderWorld)
            {
                return;
            }

            
            int startX = (int)((localCamera.position.X - (screenCenter.X / renderScale)) / World.TILESIZE);
            int startY = (int)((localCamera.position.Y - (screenCenter.Y / renderScale)) / World.TILESIZE);

            int tilesX = (int)Math.Ceiling((Window.ClientBounds.Width / renderScale) / World.TILESIZE);
            int tilesY = (int)Math.Ceiling((Window.ClientBounds.Height / renderScale) / World.TILESIZE);

            int endX = startX + tilesX - 1;
            int endY = startY + tilesY - 1;

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if (!World.IsValidIndex(World.tiles, x, y))
                    {
                        continue;
                    }
                    RenderWall(x, y);
                    RenderTile(x, y);
                }
            }

        }

        public void RenderTile(int x, int y)
        {
            if(!World.IsValidTile(x, y))
            {
                return;
            }

            Tile tileData = TileID.GetTile(World.tiles[x, y]);
            Rectangle frame = World.GetTileFrame(x, y, tileData);

            Color finalColor = Color.White;
            finalColor *= (float)World.lightMap[x, y] / Lighting.MAX_LIGHT;
            finalColor.A = 255;
            _spriteBatch.Draw(tileData.sprite, Rendering.GetTileTransform(x, y), frame, finalColor);
        }

        public void RenderWall(int x, int y)
        {
            if (!World.IsValidWall(x, y))
            {
                return;
            }

            Wall wallData = WallID.GetWall(World.walls[x, y]);
            Rectangle frame = World.GetWallFrame(x, y, wallData);

            Color finalColor = Color.Gray;
            finalColor *= ((float)World.lightMap[x, y] / Lighting.MAX_LIGHT);
            finalColor.A = 255;
            _spriteBatch.Draw(wallData.sprite, Rendering.GetTileTransform(x, y), frame, finalColor);
        }
    
        public void RenderSky()
        {
            _spriteBatch.Begin(effect: skyShader);
            skyShader.Parameters["timeLerp"].SetValue(Lighting.SKY_LIGHT_MULT);
            //skyShader.Parameters["surface"]?.SetValue((int)(World.surfaceHeights[0] * (float)World.TILESIZE));
            //skyShader.Parameters["cameraY"].SetValue(localCamera.position.Y);
            _spriteBatch.Draw(skyTex, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            _spriteBatch.End();
        }

        public void RenderSun()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            Rectangle sunRect = new Rectangle();

            sunRect.Width = (int)(64.0f * renderScale + 64.0f * Math.Pow(Lighting.SKY_LIGHT_MULT, 4.0f));
            sunRect.Height = (int)(64.0f * renderScale + 64.0f * Math.Pow(Lighting.SKY_LIGHT_MULT, 4.0f));

            sunRect.X = (int)MathHelper.Lerp(-2048.0f, Window.ClientBounds.Width + 128.0f, world.worldTime / 18.0f);

            float progress = sunRect.X / (float)Window.ClientBounds.Width;
            sunRect.Y = 144 - (int)(Math.Sin(progress * Math.PI) * 144.0f);
            _spriteBatch.Draw(sunTex, sunRect, Color.White);

            _spriteBatch.End();
        }
    
        public void RenderEntities()
        {
            for(int i = 0; i < entities.Count; i++)
            {
                entities[i].Draw(ref _spriteBatch);
            }
        }
    }
}
