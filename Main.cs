using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tiled.DataStructures;
using Tiled.ID;

namespace Tiled
{
    public class Main : Game
    {
        public GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Camera localCamera;
        public World world;
        public Effect tileShader;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        Texture2D currentTileSprite;
        protected override void LoadContent()
        {
            localCamera = new Camera(this);
            world = new World();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            world.Init();
            tileShader = Content.Load<Effect>("Shaders/TileShader");
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
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int startX = (int)(localCamera.position.X / World.TILESIZE + 1);
            int startY = (int)(localCamera.position.Y / World.TILESIZE + 1);
            int endX = startX + (Window.ClientBounds.Width / World.TILESIZE - 1);
            int endY = startY + (Window.ClientBounds.Height / World.TILESIZE - 1);
            _spriteBatch.Begin();

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
        }

        public void RenderTile(int x, int y)
        {
            Tile tileData = TileID.GetTile(World.tiles[x, y]);
            Rectangle frame = World.GetTileFrame(x, y, tileData);
            _spriteBatch.Draw(tileData.sprite, Rendering.GetTileTransform(x, y), frame, Color.White);
        }

        public void RenderWall(int x, int y)
        {
            Wall wallData = WallID.GetWall(World.walls[x, y]);
            Rectangle frame = World.GetWallFrame(x, y, wallData);
            _spriteBatch.Draw(wallData.sprite, Rendering.GetTileTransform(x, y), frame, Color.Gray);
        }
    }
}
