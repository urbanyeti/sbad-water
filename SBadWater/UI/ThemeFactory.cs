using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace SBadWater.UI
{
    public class ThemeFactory
    {
        private readonly ContentManager _content;

        public ThemeFactory(ContentManager content)
        {
            _content = content;
        }

        public Theme CreateThemeFromDTO(ThemeDTO themeDTO)
        {
            Theme theme = new()
            {
                Name = themeDTO.Name,
                TextColor = ParseColor(themeDTO.TextColor),
                SpriteColor = ParseColor(themeDTO.SpriteColor),
                BlockColor = ParseColor(themeDTO.BlockColor),
                BackgroundColor = ParseColor(themeDTO.BackgroundColor),
                IsMouseVisible = themeDTO.IsMouseVisible,
                TileStyle = Enum.Parse<TileStyle>(themeDTO.TileStyle),
                BorderStyle = Enum.Parse<BorderStyle>(themeDTO.BorderStyle),
                TileBorderTextures = themeDTO.TileBorderTextures.Select(t => _content.Load<Texture2D>(t)).ToArray(),
                TileColorTextures = themeDTO.TileColorTextures.Select(t => _content.Load<Texture2D>(t)).ToArray()
            };

            return theme;
        }

        public Theme CreateThemeFromJson(string json)
        {
            ThemeDTO themeData = JsonConvert.DeserializeObject<ThemeDTO>(json);
            return CreateThemeFromDTO(themeData);
        }

        private static Color ParseColor(string colorHex)
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(colorHex);
            return new(color.R, color.G, color.B);
        }
    }
}
