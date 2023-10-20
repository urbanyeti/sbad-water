using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SBadWater.UI
{
    public class Theme
    {
        public string Name { get; set; }
        public Color TextColor { get; set; }
        public Color SpriteColor { get; set; }
        public Color BlockColor { get; set; }
        public Color BackgroundColor { get; set; }
        public bool IsMouseVisible { get; set; }
        public TileStyle TileStyle { get; set; }
        public Texture2D[] TileBorderTextures { get; set; }
        public Texture2D[] TileColorTextures { get; set; }
    }
}
