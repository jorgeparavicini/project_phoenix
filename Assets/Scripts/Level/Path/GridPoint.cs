namespace Phoenix.Level.Path
{
    public struct GridPoint
    {
        public int X;
        public int Y;

        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(GridPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public GridPoint TranslateX(int x)
        {
            X += x;
            return this;
        }

        public GridPoint TranslateY(int y)
        {
            Y += y;
            return this;
        }
    }
}
