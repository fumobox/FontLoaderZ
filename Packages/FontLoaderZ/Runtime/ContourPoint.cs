namespace FontLoaderZ
{
    public readonly struct ContourPoint
    {
        public short X { get; }
        public short Y { get; }
        public bool OnCurve { get; }

        public ContourPoint(short x, short y, bool onCurve)
        {
            X = x;
            Y = y;
            OnCurve = onCurve;
        }
    }
}