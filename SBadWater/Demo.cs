using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SBadWater.Tiles;

namespace SBadWater
{
    public class Demo : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TileGrid _tileGrid;
        private Texture2D _pixelTexture;
        private Texture2D _backgroundTexture;
        private Texture2D _scanlines1Texture;
        private Texture2D _scanlines2Texture;
        private Texture2D _crtShapeTexture;
        private SpriteFont _font;
        private RenderTarget2D _renderTarget;
        private BlendState _subtractiveBlend;
        private BlendState _multiplyBlend;
        private Texture2D _cursorTexture;


        public Demo()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            _pixelTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _backgroundTexture = Content.Load<Texture2D>("ScanlinesBack");
            _scanlines1Texture = Content.Load<Texture2D>("Scanlines1");
            _scanlines2Texture = Content.Load<Texture2D>("Scanlines2");
            _crtShapeTexture = Content.Load<Texture2D>("CRTShape");
            _cursorTexture = Content.Load<Texture2D>("Cursor");


            _renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);


            _subtractiveBlend = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.ReverseSubtract,

                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.ReverseSubtract
            };

            _multiplyBlend = new BlendState
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.Zero
            };



            string hex = "#1EFF00";
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hex);
            Color _color = new(color.R, color.G, color.B);

            _pixelTexture.SetData(new Color[] { _color });
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
            _graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
            _graphics.GraphicsDevice.Clear(Color.Transparent);
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            //_graphics.GraphicsDevice.Clear(Color.DarkGreen);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            _spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            _tileGrid.Draw(_spriteBatch);
            
            _spriteBatch.Draw(_cursorTexture, mousePosition, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(blendState: _subtractiveBlend);
            _spriteBatch.Draw(_scanlines1Texture, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(blendState: _multiplyBlend);
            _spriteBatch.Draw(_crtShapeTexture, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _graphics.GraphicsDevice.SetRenderTarget(null);
            _graphics.GraphicsDevice.Clear(Color.Black); // or any desired clear color

            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
