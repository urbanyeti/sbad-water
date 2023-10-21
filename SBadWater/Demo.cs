using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SBadWater.Tiles;
using SBadWater.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace SBadWater
{
    public class Demo : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TileGrid _tileGrid;
        private Texture2D _backgroundTexture;
        private Texture2D _scanlines1Texture;
        private Texture2D _scanlines2Texture;
        private Texture2D _crtShapeTexture;
        private SpriteFont _font;
        private RenderTarget2D _renderTarget;
        private BlendState _subtractiveBlend;
        private BlendState _multiplyBlend;
        private Texture2D _cursorTexture;
        private KeyboardState _oldKeyboardState;

        private Dictionary<ThemeType,Theme> _themes;
        private ThemeType _currentTheme;

        public Demo()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _currentTheme = ThemeType.Sketch;
            _themes = new Dictionary<ThemeType, Theme>();
            //_theme = Theme.Retro;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
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



            string themesJson = File.ReadAllText("Config//themes.json");  // Load the JSON content from a file.
            ThemeFactory themeFactory = new ThemeFactory(Content);  // Assuming `Content` is your game's content manager.
            var themesDTOs = JsonConvert.DeserializeObject<ThemeDTO[]>(themesJson);
            foreach (var themeDTO in themesDTOs)
            {
                Theme theme = themeFactory.CreateThemeFromDTO(themeDTO);
                foreach(ThemeType themeType in Enum.GetValues<ThemeType>())
                {
                    if(themeType.ToString() == theme.Name)
                    {
                        _themes[themeType] = theme;
                        continue;
                    }
                }
            }
            
            //SetTheme(_theme);
            _font = Content.Load<SpriteFont>("Cascadia");
            _tileGrid = TileGrid.LoadFromConfig(_font, _themes[_currentTheme], "config//default_tiles.json");
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
            switch (_currentTheme)
            {
                case ThemeType.Classic:
                    DrawSimple(gameTime);
                    break;
                case ThemeType.Retro:
                    DrawRetro(gameTime);
                    break;
                case ThemeType.Sketch:
                    DrawSimple(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawSimple(GameTime gameTime)
        {
            _graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
            _graphics.GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            _tileGrid.Draw(_spriteBatch);
            _spriteBatch.End();

            _graphics.GraphicsDevice.SetRenderTarget(null);
            _graphics.GraphicsDevice.Clear(_themes[_currentTheme].BackgroundColor);

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
            _graphics.GraphicsDevice.Clear(_themes[_currentTheme].BackgroundColor); // or any desired clear color

            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        private void ToggleTheme()
        {
            _currentTheme = (ThemeType)(((int)_currentTheme + 1) % Enum.GetValues(typeof(ThemeType)).Length);
            Theme theme = _themes[_currentTheme];
            _tileGrid.SetTheme(theme);
            IsMouseVisible = theme.IsMouseVisible;
        }
    }
}
