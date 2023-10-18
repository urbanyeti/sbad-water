using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SBadWater.Tiles;
using System;

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
        private Texture2D[] _tileBorderTextures;
        private Texture2D[] _tileColorTextures;
        private KeyboardState _oldKeyboardState;

        private Theme _theme;
        private Color _backgroundColor;
        private Color _textColor;
        private Color _tileColor;

        public Demo()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _theme = Theme.Classic;
            _tileBorderTextures = new Texture2D[8];
            _tileColorTextures = new Texture2D[8];
            //_theme = Theme.Retro;
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


            _tileBorderTextures[0] = Content.Load<Texture2D>("TileBorderSketch1");
            _tileBorderTextures[1] = Content.Load<Texture2D>("TileBorderSketch2");
            _tileBorderTextures[2] = Content.Load<Texture2D>("TileBorderSketch3");
            _tileBorderTextures[3] = Content.Load<Texture2D>("TileBorderSketch4");
            _tileBorderTextures[4] = Content.Load<Texture2D>("TileBorderSketch5");
            _tileBorderTextures[5] = Content.Load<Texture2D>("TileBorderSketch6");
            _tileBorderTextures[6] = Content.Load<Texture2D>("TileBorderSketch7");
            _tileBorderTextures[7] = Content.Load<Texture2D>("TileBorderSketch8");

            _tileColorTextures[0] = Content.Load<Texture2D>("TileColor1");
            _tileColorTextures[1] = Content.Load<Texture2D>("TileColor2");
            _tileColorTextures[2] = Content.Load<Texture2D>("TileColor3");
            _tileColorTextures[3] = Content.Load<Texture2D>("TileColor4");
            _tileColorTextures[4] = Content.Load<Texture2D>("TileColor5");
            _tileColorTextures[5] = Content.Load<Texture2D>("TileColor6");
            _tileColorTextures[6] = Content.Load<Texture2D>("TileColor7");
            _tileColorTextures[7] = Content.Load<Texture2D>("TileColor8");

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


            SetTheme(_theme);
            _font = Content.Load<SpriteFont>("Cascadia");

            _tileGrid = TileGrid.LoadFromConfig(_pixelTexture, _font, _theme);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (keyboardState.IsKeyDown(Keys.Tab) && _oldKeyboardState.IsKeyUp(Keys.Tab))
            {
                ToggleTheme();
            }

            float timeSinceLastUpdateMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _tileGrid.Update(timeSinceLastUpdateMs);
            _oldKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (_theme)
            {
                case Theme.Classic:
                    DrawClassic(gameTime);
                    break;
                case Theme.Retro:
                    DrawRetro(gameTime);
                    break;
                case Theme.Sketch:
                    DrawClassic(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawClassic(GameTime gameTime)
        {
            _graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
            _graphics.GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            _tileGrid.Draw(_spriteBatch);
            _spriteBatch.End();

            _graphics.GraphicsDevice.SetRenderTarget(null);
            _graphics.GraphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        private void DrawRetro(GameTime gameTime)
        {
            _graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
            _graphics.GraphicsDevice.Clear(Color.Transparent);
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

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
            _graphics.GraphicsDevice.Clear(_backgroundColor); // or any desired clear color

            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        private void ToggleTheme()
        {
            Theme theme = (Theme)(((int)_theme + 1) % Enum.GetValues(typeof(Theme)).Length);
            SetTheme(theme);
            SetGridTheme(theme);
        }

        private void SetTheme(Theme theme)
        {
            _theme = theme;
            switch (_theme)
            {
                case Theme.Classic:
                    _textColor = Color.White;
                    _tileColor = Color.CornflowerBlue;
                    _backgroundColor = Color.DarkCyan;
                    IsMouseVisible = true;
                    break;
                case Theme.Retro:
                    string hex = "#1EFF00";
                    System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hex);
                    Color xnaColor = new(color.R, color.G, color.B);
                    _textColor = xnaColor;
                    _tileColor = xnaColor;
                    _backgroundColor = Color.Black;
                    IsMouseVisible = false;
                    break;
                case Theme.Sketch:
                    _textColor = Color.DarkSlateBlue;
                    _tileColor = Color.CornflowerBlue;
                    _backgroundColor = Color.White;
                    IsMouseVisible = true;

                    foreach (Texture2D texture in _tileColorTextures)
                    {
                        Color[] pixelData = new Color[texture.Width * texture.Height];
                        texture.GetData(pixelData);

                        for (int i = 0; i < pixelData.Length; i++)
                        {
                            Color originalColor = pixelData[i];
                            pixelData[i] = new Color(
                                originalColor.R * _tileColor.R / 255,
                                originalColor.G * _tileColor.G / 255,
                                originalColor.B * _tileColor.B / 255,
                                originalColor.A * _tileColor.A / 255 // keep alpha unchanged
                            );
                        }
                        texture.SetData(pixelData);
                    }
                    break;
            }
            _pixelTexture.SetData(new Color[] { _tileColor });


        }

        private void SetGridTheme(Theme theme)
        {
            _tileGrid.Theme = _theme;

            switch (_theme)
            {
                case Theme.Classic:
                    _tileGrid.TileBorderTextures = null;
                    _tileGrid.TileColorTextures = null;
                    break;
                case Theme.Retro:
                    _tileGrid.TileBorderTextures = null;
                    _tileGrid.TileColorTextures = null;
                    break;
                case Theme.Sketch:
                    _tileGrid.TileBorderTextures = _tileBorderTextures;
                    _tileGrid.TileColorTextures = _tileColorTextures;
                    break;
            }

        }
    }
}
