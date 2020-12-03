namespace FontLoaderZ
{
    public sealed class GlyphContourInfo
    {
        public GlyphContourInfo(GlyphInfo g)
        {
            BaseInfo = g;
        }

        public GlyphInfo BaseInfo { get; }

        public GlyphPath Path { get; set; }

        public GlyphTable.GlyphHeader Header { get; set; }
    }
}