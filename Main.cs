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
using Tiled.User;

namespace Tiled
{
    public class Main : Game
    {
        private Texture2D skyTex;
        private Texture2D sunTex;
        private SpriteBatch _spriteBatch;
        private Effect skyShader;
        public static ENetMode netMode;

        public GraphicsDeviceManager _graphics;
        public Camera localCamera;
        public InputManager localInputManager = new InputManager();
        public HUD localHUD;
        public static List<Entity> entities;
        public World world;

        public Controller localPlayerController;

        public static float renderScale = 1.0f;
        public static Point screenCenter;

        public static bool inTitle = true;
        public static bool unlit = false;
        public static Texture2D undergroundBackgroundTexture;
        public static Texture2D tileBreakTexture;

        public static Settings userSettings = new Settings();
        public static bool escMenuOpen = false;

        RenderTarget2D sceneRT;
        RenderTarget2D lightRT;
        RenderTarget2D backgroundUnlitRT;
        
        Texture2D t;

        BlendState multiplyBlend;

        Effect lightingShader;

#if TILEDSERVER
        public static TiledServer netServer;
#else
        public static TiledClient netClient;
#endif

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Tiled";
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
            

            Window.ClientSizeChanged += MainWindowResized;

            t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData(new Color[1] { Color.White });

            multiplyBlend = new BlendState()
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
            };

            CalcRenderScale();
        }

#if !TILEDSERVER
        public void JoinServer(string inputUri)
        {
            netClient = new TiledClient();
            netClient.clientException += NetClient_clientException;

            try
            {
                string ipStr = inputUri.Split(':')[0];
                string portStr = inputUri.Split(':')[1];

                byte[] ip = ipStr.Split('.').Select(x => byte.Parse(x)).ToArray();
                int port = int.Parse(portStr);

                netClient.ConnectToServer(ip, port);
            }
            catch (Exception ex)
            {
                netClient.externClientInvokeException(ex);
            }
        }

        private void NetClient_clientException(Exception e)
        {
            UWMessage m = HUD.CreateWidget<UWMessage>(localHUD, "src: " + e.Source + "\n" + e.Message + "\n inner: " + e.InnerException);
            m.onWidgetDestroyed += ExceptionMsgDestroyed;
        }

        private void ExceptionMsgDestroyed(WidgetDestroyArgs e)
        {
            localHUD.ClearAllWidgets();

            var t = HUD.CreateWidget<UWTitle>(localHUD);
            t.SetGeometry(new(1920, 1080), AnchorPosition.Center);
        }
#endif

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

            //world.Init();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            localCamera = new Camera(this);

            skyTex = new Texture2D(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            skyShader = Content.Load<Effect>("Shaders/SkyShader");

            sunTex = Content.Load<Texture2D>("Sky/Sun");

            undergroundBackgroundTexture = Content.Load<Texture2D>("UndergroundBackgrounds/DefaultUndergroundBackground");

            tileBreakTexture = Content.Load<Texture2D>("TileBreakage/breakCombined");

            lightingShader = Content.Load<Effect>("Shaders/LightingRTShader");

            localHUD = new HUD(_spriteBatch, _graphics);
            world = new World();

#if TILEDSERVER
            RunServer();
#endif

        }

#if TILEDSERVER
        private async void RunServer()
        {
            netServer = new TiledServer();
            netServer.onServerLog += NetServer_onServerLog;

            Window.Title = "Tiled Server";

            return;
        }

        private void NetServer_onServerLog(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[SERVER] " + msg);
        }
#endif

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
            renderScale *= localCamera.zoom;

            lightRT?.Dispose();
            sceneRT?.Dispose();
            lightRT = null;
            sceneRT = null;

            lightRT = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            sceneRT = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            backgroundUnlitRT = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

            GC.Collect();
        }

        public void ForceCalcRenderScale()
        {
            CalcRenderScale();
        }

        public static float delta;
        public static float runtime;
        protected override void Update(GameTime gameTime)
        {
            if (!IsActive && netMode == ENetMode.Standalone)
            {
                return;
            }

            if(!inTitle)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.A))
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
            }

#if !TILEDSERVER
            localInputManager.Update();
            Mappings.Update();

            if(World.renderWorld)
            {
                Lighting.Update();
            }

            localPlayerController.Update();
#endif

            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            runtime += delta;

            world.UpdateWorld();

            for(int i = 0; i < entities.Count; i++)
            {
                entities[i].Update();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(backgroundUnlitRT); //Draw unlit Background RT
            GraphicsDevice.Clear(Color.Black);

            RenderSky();
            RenderSun();

            GraphicsDevice.SetRenderTarget(sceneRT); //Draw to Scene
            GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            RenderBackground();
            RenderWorld();
            RenderEntities();

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(lightRT);

            RenderLightTarget();

            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(effect: lightingShader);
            lightingShader.Parameters["Lighting"]?.SetValue(lightRT);
            lightingShader.Parameters["Sky"]?.SetValue(backgroundUnlitRT);
            _spriteBatch.Draw(sceneRT, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            _spriteBatch.End();

            /*_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap);
            _spriteBatch.Draw(sceneRT, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Immediate, multiplyBlend, SamplerState.PointWrap);
            _spriteBatch.Draw(lightRT, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            _spriteBatch.End();*/

            localHUD.DrawWidgets();

            RenderMouseItem();
        }

        public void RenderMouseItem()
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            if(InputManager.mouseHasItem)
            {
                var s = ItemID.GetItem(InputManager.mouseItem.type).sprite;
                _spriteBatch.Draw(s, new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, (int)(32 * HUD.DPIScale), (int)(32 * HUD.DPIScale)), Color.White);
            }
            _spriteBatch.End();
        }

        public void RenderBackground()
        {
            int backgroundStartY = (World.averageSurfaceHeight + 113) * World.TILESIZE;

            if (localCamera.position.Y + (Window.ClientBounds.Height / renderScale) >= backgroundStartY)
            {
                int textureWidth = undergroundBackgroundTexture.Width;
                int textureHeight = undergroundBackgroundTexture.Height;

                int screenY = 0;
                if (localCamera.position.Y < backgroundStartY)
                {
                    screenY = (int)((backgroundStartY - localCamera.position.Y) * renderScale);
                }

                float verticalScrollOffset = Math.Max(0, localCamera.position.Y - backgroundStartY);
                float horizontalScrollOffset = localCamera.position.X / 2;

                Rectangle destRect = new Rectangle(0, screenY, Window.ClientBounds.Width, Window.ClientBounds.Height - screenY);

                float texCoordX = horizontalScrollOffset / textureWidth;
                float texCoordY = verticalScrollOffset / textureHeight;

                float texWidth = (Window.ClientBounds.Width / renderScale) / textureWidth;
                float texHeight = (Window.ClientBounds.Height - screenY) / renderScale / textureHeight;

                Rectangle sourceRect = new Rectangle((int)(texCoordX * textureWidth) % textureWidth, (int)(texCoordY * textureHeight) % textureHeight, (int)(texWidth * textureWidth), (int)(texHeight * textureHeight));

                _spriteBatch.Draw(undergroundBackgroundTexture, destRect, sourceRect, Color.White);
            }
        }



        public void RenderLightTarget()
        {
            const int TILE_PAD = 1;

            int startX = (int)((localCamera.position.X - (screenCenter.X / renderScale)) / World.TILESIZE) + TILE_PAD - 1;
            int startY = (int)((localCamera.position.Y - (screenCenter.Y / renderScale)) / World.TILESIZE) + TILE_PAD - 1;

            int tilesX = (int)Math.Ceiling((Window.ClientBounds.Width / renderScale) / World.TILESIZE);
            int tilesY = (int)Math.Ceiling((Window.ClientBounds.Height / renderScale) / World.TILESIZE);

            int endX = startX + tilesX + TILE_PAD;
            int endY = startY + tilesY + TILE_PAD;

            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap);
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if (!World.IsValidIndex(World.lightMap, x, y))
                    {
                        continue;
                    }

                    Color c = Color.White;
                    float lerpValue = World.lightMap[x, y] / (float)Lighting.MAX_LIGHT;
                    c = unlit? Color.White : Color.Lerp(Color.Black, Color.White, lerpValue);

                    _spriteBatch.Draw(t, Rendering.GetLightTileTransform(x, y), c);
                }
            }
            _spriteBatch.End();
        }

        public void RenderWorld()
        {

            if(!World.renderWorld)
            {
                return;
            }

            const int TILE_PAD = 1;
            
            int startX = (int)((localCamera.position.X - (screenCenter.X / renderScale)) / World.TILESIZE) + TILE_PAD - 1;
            int startY = (int)((localCamera.position.Y - (screenCenter.Y / renderScale)) / World.TILESIZE) + TILE_PAD - 1;

            int tilesX = (int)Math.Ceiling((Window.ClientBounds.Width / renderScale) / World.TILESIZE);
            int tilesY = (int)Math.Ceiling((Window.ClientBounds.Height / renderScale) / World.TILESIZE);

            int endX = startX + tilesX + TILE_PAD;
            int endY = startY + tilesY + TILE_PAD;

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
            
            _spriteBatch.Draw(tileData.sprite, Rendering.GetTileTransform(x, y), frame, Color.White);

            Rectangle? breakFrame = BreakTextureID.GetTextureFrame(World.tileBreak[x, y], tileData.hardness);

            if (breakFrame != null)
            {
                _spriteBatch.Draw(tileBreakTexture, Rendering.GetTileTransform(x, y), breakFrame, Color.White);
            }
        }

        public void RenderWall(int x, int y)
        {

            if (!World.IsValidWall(x, y))
            {
                return;
            }

            Wall wallData = WallID.GetWall(World.walls[x, y]);
            Rectangle frame = World.GetWallFrame(x, y, wallData);

            _spriteBatch.Draw(wallData.sprite, Rendering.GetTileTransform(x, y), frame, Color.White);
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
                if(localCamera.IsInView(entities[i].GetRectF()))
                {
                    entities[i].Draw(ref _spriteBatch);
                }
                
            }
        }
    }
}
