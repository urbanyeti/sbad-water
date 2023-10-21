using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SBadWater.IO;
using SBadWater.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SBadWater.Tiles
{
    public class TileGrid
    {
        public int Columns => _config.Columns;
        public int Rows => _config.Rows;
        public bool[] PassableTiles => _config.PassableTiles;
        public Color TextColor => _theme.TextColor;
        public Texture2D[] TileBorderTextures => _theme.TileBorderTextures;
        public Texture2D[] TileColorTextures => _theme.TileColorTextures;
        private TileGridConfig _config;

        private readonly LiquidTile[] _tiles;
        private readonly InputManager _inputManager;
        private readonly Random _random = new();
        private readonly SpriteFont _font;
        private readonly Dictionary<LiquidTile, bool> _beamedTiles = new();
        private LiquidTile _hoveredTile;
        private LiquidTile _clickedTile;
        private Theme _theme;
        private TileBuildMode _buildMode;


        public void SetTheme(Theme theme)
        {
            _theme = theme;

            foreach (LiquidTile tile in _tiles)
            {
                tile.ApplyTheme(_theme, _random);
            }
        }

        public TileGrid(TileGridConfig config, Theme theme, SpriteFont font)
        {
            _config = config;
            _theme = theme;
            _font = font;
            _tiles = new LiquidTile[Rows * Columns];
            _inputManager = new InputManager();
            _inputManager.OnButtonPressed += ButtonPressed;
            _inputManager.OnButtonReleased += ButtonReleased;
            _inputManager.OnMouseMoved += MouseMoved;

            CreateTiles();
        }

        private void CreateTiles()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    int index = (row * Columns) + col;
                    int x = (col * 10) + 120;
                    int y = (row * 10) + 40;

                    _tiles[index] = new LiquidTile(new Rectangle(x, y, 10, 10), 0, col, row, index, PassableTiles[index], _theme, random: _random);

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

            for (int i = 0; i < _tiles.Length; i++)
            {
                _tiles[i].UpdateFlow();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                LiquidTile tile = _tiles[i];
                if (tile == null) { continue; }

                spriteBatch.Draw(tile.ColorTexture, tile.Rectangle, tile.Color);

                if (tile == _clickedTile)
                {
                    DrawBorder(spriteBatch, tile, 1, Color.Red);
                }
                else if (_beamedTiles.ContainsKey(tile))
                {
                    DrawBorder(spriteBatch, tile, 2, Color.Green);
                }
                else if (tile == _hoveredTile)
                {
                    DrawBorder(spriteBatch, tile, 1, Color.Yellow);
                }
                else
                {
                    DrawBorder(spriteBatch, tile, 1, Color.Black);
                }
            }

            if (_hoveredTile != null)
            {
                DrawTileInfo(spriteBatch);
            }
        }

        public static TileGrid LoadFromConfig(SpriteFont font, Theme theme, string path = "config//default_tiles.json")
        {
            string json = File.ReadAllText(path);
            TileGridConfig config = JsonConvert.DeserializeObject<TileGridConfig>(json);
            return new TileGrid(config, theme, font);
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

            if (mouseState.RightButton == ButtonState.Pressed && _hoveredTile != oldHoverTile)
            {
                ButtonPressed(InputKey.RightButton, 0f);
            }

            if (mouseState.MiddleButton == ButtonState.Pressed && _hoveredTile != oldHoverTile)
            {
                BuildHoveredTile();
            }

        }

        private void ButtonPressed(InputKey key, float holdDurationMs)
        {
            if (_hoveredTile == null)
            {
                return;
            }

            if (key == InputKey.LeftButton)
            {
                _clickedTile = _hoveredTile;
                if (_clickedTile.Passable)
                {
                    _clickedTile.Capacity += 500;
                }
            }

            if (key == InputKey.RightButton)
            {
                _beamedTiles.Clear();
                // Calculate column and row of _hoveredTile
                int hoveredColumn = _hoveredTile.Index % Columns;
                int hoveredRow = _hoveredTile.Index / Columns;

                // Loop through tiles in the same column as _hoveredTile and below it
                for (int row = hoveredRow; row < Rows; row++)
                {
                    int tileIndex = (row * Columns) + hoveredColumn;
                    if (PassableTiles[tileIndex])
                    {
                        _beamedTiles[_tiles[tileIndex]] = true;
                        _tiles[tileIndex].Capacity = Math.Max(_tiles[tileIndex].Capacity - 100, 0);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (key == InputKey.MiddleButton)
            {
                if (holdDurationMs == 0) // Initial click and not a hold
                {
                    _buildMode = _hoveredTile.Passable ? TileBuildMode.Block : TileBuildMode.Empty;
                    BuildHoveredTile();
                }
            }

        }

        private void BuildHoveredTile()
        {
            if (_hoveredTile == null) {  return; }
            switch (_buildMode)
            {
                case TileBuildMode.Empty:
                    _hoveredTile.Passable = true;
                    break;
                case TileBuildMode.Block:
                    _config.PassableTiles[_hoveredTile.Index] = false;
                    _hoveredTile.Passable = false;
                    _hoveredTile.Capacity = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported build mode: {_buildMode}");
                    }
        }

        private void ButtonReleased(InputKey key)
        {
            if (key == InputKey.LeftButton)
            {
                _clickedTile = null;
                return;
            }

            if (key == InputKey.RightButton)
            {
                _beamedTiles.Clear();
                return;
            }
        }

        // Utility method to draw a border around a rectangle.
        private void DrawBorder(SpriteBatch spriteBatch, LiquidTile tile, int thicknessOfBorder, Color borderColor)
        {
            Rectangle rectangleToDraw = tile.Rectangle;
            switch (_theme.BorderStyle)
            {
                case BorderStyle.None:
                    break;
                case BorderStyle.Drawn:
                    // Draw top line
                    spriteBatch.Draw(tile.BorderTexture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor);

                    // Draw left line
                    spriteBatch.Draw(tile.BorderTexture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor);

                    // Draw right line
                    spriteBatch.Draw(tile.BorderTexture, new Rectangle(rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder,
                                                          rectangleToDraw.Y,
                                                          thicknessOfBorder,
                                                          rectangleToDraw.Height), borderColor);
                    // Draw bottom line
                    spriteBatch.Draw(tile.BorderTexture, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder, rectangleToDraw.Width, thicknessOfBorder), borderColor);
                    break;
                case BorderStyle.Overlay:
                    spriteBatch.Draw(tile.BorderTexture, tile.Rectangle, borderColor);
                    break;
            }
        }


        private void DrawTileInfo(SpriteBatch spriteBatch)
        {
            int infoX = 540 /*some x-coordinate on the side*/;
            int infoY = 140 /*some y-coordinate on the side*/;
            int spacing = 30;  // Spacing between lines of text.

            spriteBatch.DrawString(_font, $"Index: {_hoveredTile.Index}", new Vector2(infoX, infoY), _theme.TextColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"X: {_hoveredTile.X}", new Vector2(infoX, infoY), _theme.TextColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Y: {_hoveredTile.Y}", new Vector2(infoX, infoY), _theme.TextColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Capacity: {_hoveredTile.Capacity}", new Vector2(infoX, infoY), _theme.TextColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Passable: {_hoveredTile.Passable}", new Vector2(infoX, infoY), _theme.TextColor);
        }
    }



}
