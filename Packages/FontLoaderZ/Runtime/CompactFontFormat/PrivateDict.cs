namespace FontLoaderZ.CompactFontFormat
{
    public sealed class PrivateDict
    {
        public PrivateDict()
        {
            // Default Values
            Subrs = 0;
            DefaultWidthX = 20;
            NormalWidthX = 0;
        }

        // 19
        public uint Subrs { get; set; }

        // 20
        public int DefaultWidthX { get; set; }

        // 21
        public int NormalWidthX { get; set; }
    }
}