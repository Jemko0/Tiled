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
        Texture2D dbgTileSprite;
        public Effect fullTileShader;

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

        protected override void LoadContent()
        {
            localCamera = new Camera(this);
            world = new World();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            world.Init();
            //tileSprite = new Texture2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            dbgTileSprite = Content.Load<Texture2D>("Tiles/DebugTile");
            fullTileShader = Content.Load<Effect>("Shaders/FullTileShader");
            
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

            _spriteBatch.Begin(effect: fullTileShader);

            fullTileShader.Parameters["tiles"].SetValue(World.tiles[0, 0]);

            _spriteBatch.Draw(dbgTileSprite, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            _spriteBatch.End();
        }
    }
}
