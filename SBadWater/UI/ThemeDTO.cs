namespace SBadWater.UI
{
    public class ThemeDTO
    {
        public string Name { get; set; }
        public string TextColor { get; set; }
        public string SpriteColor { get; set; }
        public string BlockColor { get; set; }
        public string BackgroundColor { get; set; }
        public string TileStyle { get; set; }
        public string BorderStyle { get; set; }
        public string[] TileBorderTextures { get; set; }
        public string[] TileColorTextures { get; set; }
        public string FontSmall { get; set; }
        public string FontMedium { get; set; }
        public string TileInfoOffset { get; set; }
        public float TileInfoSpacing { get; set; }
        public string InstructionsOffset { get; set; }
        public float InstructionsSpacing { get; set; }
        public bool IsMouseVisible { get; set; }

    }
}
