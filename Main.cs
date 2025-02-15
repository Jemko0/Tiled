using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.ID;
using Tiled.Input;
using Tiled.UI;
using Tiled.UI.Font;

namespace Tiled
{
    public class Main : Game
    {
        public GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Camera localCamera;
        public World world;
        public Effect tileShader;
        public InputManager localInputManager = new InputManager();
        public HUD localHUD;

        public static float renderScale = 1.0f;
        public static Point screenCenter;

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
            InputManager.onLeftMousePressed += LMB;
            InputManager.onRightMousePressed += RMB;
            CalcRenderScale();
            Window.ClientSizeChanged += MainWindowResized;
            base.Initialize();
        }

        private void RMB(MouseButtonEventArgs e)
        {
            Point tile = Rendering.ScreenToTile(e.position);
            World.SetWall(tile.X, tile.Y, EWallType.Air);
        }

        private void MainWindowResized(object sender, System.EventArgs e)
        {
            CalcRenderScale();
        }

        private void CalcRenderScale()
        {
            screenCenter = new Point(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            renderScale = Window.ClientBounds.Height / 1080.0f;
        }

        private void LMB(MouseButtonEventArgs e)
        {
            Point tile = Rendering.ScreenToTile(e.position);

            if(Keyboard.GetState().IsKeyDown(Keys.T))
            {
                World.SetTile(tile.X, tile.Y, ETileType.Torch);
                return;
            }

            World.SetTile(tile.X, tile.Y, ETileType.Air);
        }

        protected override void LoadContent()
        {
            Fonts.InitFonts();

            world = new World();
            world.Init();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            localCamera = new Camera(this);
            localCamera.position = new Vector2(0, 8192);
            //tileShader = Content.Load<Effect>("Shaders/TileShader");

            localHUD = new HUD(_spriteBatch, _graphics);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(Keyboard.GetState().IsKeyDown(Keys.A))
            {
                localCamera.position.X -= 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                localCamera.position.X += 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                localCamera.position.Y -= 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                localCamera.position.Y += 5;
            }

            localInputManager.Update();

            Lighting.Update();
            world.UpdateWorld();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue * Lighting.SKY_LIGHT_MULT);

            int startX = (int)((localCamera.position.X - (screenCenter.X / renderScale)) / World.TILESIZE);
            int startY = (int)((localCamera.position.Y - (screenCenter.Y / renderScale)) / World.TILESIZE);

            int tilesX = (int)Math.Ceiling((Window.ClientBounds.Width / renderScale) / World.TILESIZE);
            int tilesY = (int)Math.Ceiling((Window.ClientBounds.Height / renderScale) / World.TILESIZE);

            int endX = startX + tilesX - 1;
            int endY = startY + tilesY - 1;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if(!World.IsValidIndex(World.tiles, x, y))
                    {
                        continue;
                    }
                    RenderWall(x, y);
                    RenderTile(x, y);
                }
            }

            _spriteBatch.End();

            localHUD.DrawWidgets();
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
            finalColor *= ((float)World.lightMap[x, y] / Lighting.MAX_LIGHT);
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
    }
}
