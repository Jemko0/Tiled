using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tiled.DataStructures;

namespace Tiled
{
    public class Main : Game
    {
        public GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Camera localCamera;
        public World world;
        Texture2D image;
        Texture2D tileSprite;
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

        protected override void LoadContent()
        {
            localCamera = new Camera(this);
            world = new World();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            world.Init();
            image = new Texture2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            tileShader = Content.Load<Effect>("Shaders/TileShader");
            tileSprite = Content.Load<Texture2D>("Tiles/DebugTile");
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
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(effect: tileShader);

            tileShader.Parameters["cameraPosition"].SetValue(localCamera.position);
            tileShader.Parameters["tileTexture"].SetValue(localCamera.position);
            //tileShader.Parameters["tileStart"].SetValue(16);
            //tileShader.Parameters["tileEnd"].SetValue(16);

            _spriteBatch.Draw(image, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            _spriteBatch.End();
        }
    }
}
