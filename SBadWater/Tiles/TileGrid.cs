using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SBadWater.IO;
using System.Diagnostics;
using System.IO;

namespace SBadWater.Tiles
{
    public class TileGrid
    {
        private readonly TileGridConfig _config;

        private readonly LiquidTile[] _tiles;
        private readonly InputManager _inputManager;
        private readonly Texture2D _texture;
        private readonly SpriteFont _font;
        private LiquidTile _hoveredTile;
        private LiquidTile _clickedTile;

        public int Columns => _config.Columns;
        public int Rows => _config.Rows;
        public bool[] PassableTiles => _config.PassableTiles;

        public TileGrid(TileGridConfig config, Texture2D texture, SpriteFont font)
        {
            _config = config;
            _texture = texture;
            _font = font;
            _tiles = new LiquidTile[Rows * Columns];
            _inputManager = new InputManager();
            _inputManager.OnButtonPressed += ButtonPressed;
            _inputManager.OnButtonReleased += ButtonReleased;
            _inputManager.OnMouseMoved += MouseMoved;

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    int index = (row * Columns) + col;
                    int x = (col * 20) + 10;
                    int y = (row * 20) + 10;

                    _tiles[index] = new LiquidTile(new Rectangle(x, y, 20, 20), Color.CornflowerBlue, 0, col, row, index, PassableTiles[index]);

                    if (col > 0)
                    {
                        // Add left neighbor. This will automatically set the right neighbor for the tile to the left.
                        _ = _tiles[index].AddNeighbor(_tiles[(row * Columns) + (col - 1)], TileDirection.LEFT);
                    }

                    if (row > 0)
                    {
                        // Add top neighbor. This will automatically set the bottom neighbor for the tile above.
                        _ = _tiles[index].AddNeighbor(_tiles[((row - 1) * Columns) + col], TileDirection.TOP);
                    }
                }
            }
        }

        public void Update(float elapsedTimeMs)
        {
            _inputManager.Update(elapsedTimeMs);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                LiquidTile tile = _tiles[i];
                if (tile == null) { continue; }
                spriteBatch.Draw(_texture, _tiles[i].Rectangle, tile.Color);
                if (tile == _clickedTile)
                {
                    DrawBorder(spriteBatch, tile.Rectangle, 1, Color.Red);
                }
                else if (tile == _hoveredTile)
                {
                    DrawBorder(spriteBatch, tile.Rectangle, 1, Color.Yellow);  // 2 is the border thickness here.
                }
                else
                {
                    DrawBorder(spriteBatch, tile.Rectangle, 1, Color.Black);
                }
            }

            if (_hoveredTile != null)
            {
                DrawTileInfo(spriteBatch);
            }
        }

        public static TileGrid LoadFromConfig(Texture2D texture, SpriteFont font, string path = "config//default_tiles.json")
        {
            string json = File.ReadAllText(path);
            TileGridConfig config = JsonConvert.DeserializeObject<TileGridConfig>(json);
            return new TileGrid(config, texture, font);
        }

        private void MouseMoved(MouseState mouseState, MouseState oldMouseState)
        {
            LiquidTile oldHoverTile = _hoveredTile;
            _hoveredTile = null;

            foreach (LiquidTile tile in _tiles)
            {
                if (tile == null) { continue; }
                if (tile.Rectangle.Contains(mouseState.Position))
                {
                    _hoveredTile = tile;
                    break;
                }
            }

            if (mouseState.LeftButton == ButtonState.Pressed && _hoveredTile != oldHoverTile)
            {
                ButtonPressed(InputKey.LeftButton, 0f);
            }

        }

        private void ButtonPressed(InputKey key, float holdDurationMs)
        {
            if (key == InputKey.LeftButton)
            {
                _clickedTile = _hoveredTile;

                if (_clickedTile != null)
                {
                    Debug.WriteLine($"Index: {_clickedTile.Index}");
                    Debug.WriteLine($"Col: {_clickedTile.X}");
                    Debug.WriteLine($"Row: {_clickedTile.Y}");
                }
            }
        }

        private void ButtonReleased(InputKey key)
        {
            if (key == InputKey.LeftButton)
            {
                _clickedTile = null;
            }
        }

        // Utility method to draw a border around a rectangle.
        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor)
        {
            // Draw top line
            spriteBatch.Draw(_texture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor);

            // Draw left line
            spriteBatch.Draw(_texture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor);

            // Draw right line
            spriteBatch.Draw(_texture, new Rectangle(rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder,
                                                  rectangleToDraw.Y,
                                                  thicknessOfBorder,
                                                  rectangleToDraw.Height), borderColor);

            // Draw bottom line
            spriteBatch.Draw(_texture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder, rectangleToDraw.Width, thicknessOfBorder), borderColor);
        }

        private void DrawTileInfo(SpriteBatch spriteBatch)
        {
            int infoX = 420 /*some x-coordinate on the side*/;
            int infoY = 10 /*some y-coordinate on the side*/;
            int spacing = 20;  // Spacing between lines of text.

            spriteBatch.DrawString(_font, $"Index: {_hoveredTile.Index}", new Vector2(infoX, infoY), Color.Black);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"X: {_hoveredTile.X}", new Vector2(infoX, infoY), Color.Black);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Y: {_hoveredTile.Y}", new Vector2(infoX, infoY), Color.Black);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Capacity: {_hoveredTile.Capacity}", new Vector2(infoX, infoY), Color.Black);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Passable: {_hoveredTile.Passable}", new Vector2(infoX, infoY), Color.Black);
        }
    }



}
