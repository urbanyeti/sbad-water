using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SBadWater.IO;
using SBadWater.Tiles;

namespace SBadWater
{
    public class Demo : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TileGrid _tileGrid;
        private Texture2D _pixelTexture;
        private SpriteFont _font;

        public Demo()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            _pixelTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new Color[] { Color.White });
            _font = Content.Load<SpriteFont>("Cascadia");

            _tileGrid = TileGrid.LoadFromConfig(_pixelTexture, _font);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            float timeSinceLastUpdateMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _tileGrid.Update(timeSinceLastUpdateMs);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.DarkCyan);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            _tileGrid.Draw(_spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
