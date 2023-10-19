using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBadWater.UI
{
    public class ThemeFactory
    {
        private ContentManager _content;

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
                BackgroundColor = ParseColor(themeDTO.BackgroundColor),
                IsMouseVisible = themeDTO.IsMouseVisible,
                TileBorderTextures = themeDTO.TileBorderTextures.Select(t => _content.Load<Texture2D>(t)).ToArray(),
                TileColorTextures = themeDTO.TileColorTextures.Select(t => _content.Load<Texture2D>(t)).ToArray()
            };

            return theme;
        }

        public Theme CreateThemeFromJson(string json)
        {
            var themeData = JsonConvert.DeserializeObject<ThemeDTO>(json);
            return CreateThemeFromDTO(themeData);
        }

        private static Color ParseColor(string colorHex)
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(colorHex);
            return new(color.R, color.G, color.B);
        }
    }
}
