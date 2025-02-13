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

            int startX = (int)(localCamera.position.X / World.renderTileSize + 1);
            int startY = (int)(localCamera.position.Y / World.renderTileSize + 1);
            int endX = startX + (Window.ClientBounds.Width / World.renderTileSize - 1);
            int endY = startY + (Window.ClientBounds.Height / World.renderTileSize - 1);
            _spriteBatch.Begin();

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if(x < 0 || y < 0 || x >= World.tiles.GetLength(0) || y >= World.tiles.GetLength(1))
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
            _spriteBatch.Draw(tileData.sprite, new Rectangle((x * World.renderTileSize) - (int)localCamera.position.X, (y * World.renderTileSize) - (int)localCamera.position.Y, World.renderTileSize, World.renderTileSize), Color.White);
        }

        public void RenderWall(int x, int y)
        {
            Wall wallData = WallID.GetWall(World.walls[x, y]);
        }
    }
}
