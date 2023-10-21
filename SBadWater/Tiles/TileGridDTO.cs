namespace SBadWater.Tiles
{
    public class TileGridDTO
    {
        public string Version { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public bool[] PassableTiles { get; set; }
    }
}
