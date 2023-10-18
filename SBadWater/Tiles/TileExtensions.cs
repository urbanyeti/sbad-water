using System;

namespace SBadWater.Tiles
{
    public static class TileExtensions
    {
        public static TileDirection GetOpposite(this TileDirection tileDirection)
        {
            TileDirection[] array = Enum.GetValues<TileDirection>();
            int dir = ((int)tileDirection + 2) % array.Length;
            return array[dir];
        }
    }
}
