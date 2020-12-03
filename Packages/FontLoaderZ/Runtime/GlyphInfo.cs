namespace FontLoaderZ
{
    public readonly struct GlyphInfo
    {
        public ushort AdvancedWidth { get; }

        public short LeftSideBearing { get; }

        public GlyphInfo(ushort advancedWidth, short leftSideBearing)
        {
            AdvancedWidth = advancedWidth;
            LeftSideBearing = leftSideBearing;
        }
    }
}