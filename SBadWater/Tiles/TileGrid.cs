﻿using Microsoft.Xna.Framework;
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
        public Texture2D[] TileBorderTextures
        {
            get { return _tileBorderTextures; }
            set
            {
                _tileBorderTextures = value;
                if (_tileBorderTextures == null)
                {
                    _tileBorderIndex = null;
                    return;
                }

                _tileBorderIndex = new int[_tiles.Length];
                Random random = new();
                for (int i= 0; i < _tiles.Length; i++)
                {
                    _tileBorderIndex[i] = random.Next(_tileBorderTextures.Length);
                }
            }
        }

        public Texture2D[] TileColorTextures
        {
            get { return _tileColorTextures; }
            set
            {
                _tileColorTextures = value;
                if (_tileColorTextures == null)
                {
                    _tileColorTextures = null;
                    return;
                }

                _tileColorIndex = new int[_tiles.Length];
                Random random = new();
                for (int i = 0; i < _tiles.Length; i++)
                {
                    _tileColorIndex[i] = random.Next(_tileColorTextures.Length);
                }
            }
        }

        public ThemeType Theme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                switch (_theme)
                {
                    case ThemeType.Classic:
                        _textColor = Color.White;
                        break;
                    case ThemeType.Retro:
                        string hex = "#1EFF00";
                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hex);
                        _textColor = new(color.R, color.G, color.B);
                        break;
                    case ThemeType.Sketch:
                        _textColor = Color.DarkSlateBlue;
                        break;
                }
            }
        }

        private readonly TileGridConfig _config;

        private readonly LiquidTile[] _tiles;
        private readonly InputManager _inputManager;
        private readonly Texture2D _texture;

        private readonly SpriteFont _font;
        private readonly Dictionary<LiquidTile, bool> _beamedTiles = new();
        private LiquidTile _hoveredTile;
        private LiquidTile _clickedTile;
        private ThemeType _theme;
        private Color _textColor;
        private Texture2D[] _tileBorderTextures;
        private int[] _tileBorderIndex;
        private Texture2D[] _tileColorTextures;
        private int[] _tileColorIndex;


        public TileGrid(TileGridConfig config, Texture2D texture, SpriteFont font, ThemeType theme = ThemeType.Classic)
        {
            _config = config;
            _texture = texture;
            _font = font;
            _tiles = new LiquidTile[Rows * Columns];
            _inputManager = new InputManager();
            _inputManager.OnButtonPressed += ButtonPressed;
            _inputManager.OnButtonReleased += ButtonReleased;
            _inputManager.OnMouseMoved += MouseMoved;

            Theme = theme;

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    int index = (row * Columns) + col;
                    int x = (col * 10) + 120;
                    int y = (row * 10) + 40;

                    _tiles[index] = new LiquidTile(new Rectangle(x, y, 10, 10), Color.White, 0, col, row, index, PassableTiles[index]);

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

                if (_tileColorTextures == null)
                {
                    spriteBatch.Draw(_texture, _tiles[i].Rectangle, tile.Color);
                } else
                {
                    spriteBatch.Draw(_tileColorTextures[_tileColorIndex[i]], _tiles[i].Rectangle, tile.Color);
                }

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

        public static TileGrid LoadFromConfig(Texture2D texture, SpriteFont font, ThemeType theme, string path = "config//default_tiles.json")
        {
            string json = File.ReadAllText(path);
            TileGridConfig config = JsonConvert.DeserializeObject<TileGridConfig>(json);
            return new TileGrid(config, texture, font, theme);
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
            if (TileBorderTextures == null)
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
                return;
            }
            spriteBatch.Draw(TileBorderTextures[_tileBorderIndex[tile.Index]], rectangleToDraw, borderColor);

        }

        private void DrawTileInfo(SpriteBatch spriteBatch)
        {
            int infoX = 540 /*some x-coordinate on the side*/;
            int infoY = 140 /*some y-coordinate on the side*/;
            int spacing = 30;  // Spacing between lines of text.

            spriteBatch.DrawString(_font, $"Index: {_hoveredTile.Index}", new Vector2(infoX, infoY), _textColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"X: {_hoveredTile.X}", new Vector2(infoX, infoY), _textColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Y: {_hoveredTile.Y}", new Vector2(infoX, infoY), _textColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Capacity: {_hoveredTile.Capacity}", new Vector2(infoX, infoY), _textColor);
            infoY += spacing;

            spriteBatch.DrawString(_font, $"Passable: {_hoveredTile.Passable}", new Vector2(infoX, infoY), _textColor);
        }
    }



}
