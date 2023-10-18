using Microsoft.Xna.Framework;
using System;

namespace SBadWater.Tiles
{
    public class LiquidTile
    {
        public Rectangle Rectangle { get; set; }
        public Color Color
        {
            get => Passable ? color : Color.DarkGray;
            set => color = new Color(value, capacity);

        }
        public int Capacity
        {
            get => capacity;
            set
            {
                capacity = value;
                color = new Color(color, capacity);
            }
        }

        public bool Passable { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Index { get; set; }
        public LiquidTile[] Neighbors { get; set; } = new LiquidTile[4];
        public LiquidTile Right => Neighbors[(int)TileDirection.RIGHT];
        public LiquidTile Bottom => Neighbors[(int)TileDirection.BOTTOM];
        public LiquidTile Left => Neighbors[(int)TileDirection.LEFT];
        public LiquidTile Top => Neighbors[(int)TileDirection.TOP];

        private int capacity;
        private Color color;

        public LiquidTile(Rectangle rectangle, Color color, int capacity, int x, int y, int index, bool passable)
        {
            Rectangle = rectangle;
            Color = color;
            Capacity = capacity;
            X = x;
            Y = y;
            Index = index;
            Passable = passable;
        }

        public LiquidTile AddNeighbor(LiquidTile neighbor, TileDirection direction)
        {
            if (neighbor == null) { return this; }
            Neighbors[(int)direction] = neighbor;
            neighbor.Neighbors[(int)direction.GetOpposite()] = this;
            return this;
        }


        public void UpdateFlow()
        {
            if (Capacity == 0)
            {
                return;
            }

            // Flow to bottom
            if (Bottom != null && Bottom.Passable)
            {
                if (Bottom.Capacity == 0)
                {
                    Bottom.Capacity = Capacity;
                    Capacity = 0;
                    return;
                }
                if (Bottom.Capacity < 255)
                {
                    int flowAmount = Math.Min(Capacity, 255 - Bottom.Capacity);
                    Bottom.Capacity += flowAmount;
                    Capacity -= flowAmount;
                }
            }

            // Split evenly
            if (Left != null && Right != null && Left.Passable && Right.Passable)
            {
                int sum = Left.Capacity + Capacity + Right.Capacity;
                int split = Math.DivRem(sum, 3, out int rem);
                Left.Capacity = split;
                Right.Capacity = split;
                Capacity = split + rem;
            }

            // Flow left
            else if (Left != null && Left.Passable && (Right == null || !Right.Passable))
            {
                int sum = Left.Capacity + Capacity;
                int split = Math.DivRem(sum, 2, out int rem);
                Left.Capacity = split;
                Capacity = split + rem;
            }


            // Flow right
            else if (Right != null && Right.Passable && (Left == null || !Left.Passable))
            {
                int sum = Capacity + Right.Capacity;
                int split = Math.DivRem(sum, 2, out int rem);
                Capacity = split + rem;
                Right.Capacity = split;
            }
        }
    }
}
