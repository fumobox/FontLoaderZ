namespace FontLoaderZ.CompactFontFormat
{
    public sealed class TopDict
    {
        public TopDict()
        {
            // Default Values
            IsFixedPitch = false;
            ItalicAngle = 0;
            UnderlinePosition = -100;
            UnderlineThickness = 50;
            PaintType = 0;
            CharStringType = 2;
            FontMatrix = new[] {0.001f, 0f, 0.001f, 0f};
            FontBBox = new[] {0f, 0f, 0f, 0f};
            StrokeWidth = 0;
            Charset = 0;
            Encoding = 0;
            CIDFontRevision = 0;
            CIDFontType = 0;
            CIDCount = 8720;
            Private = new uint[2];

            BlueScale = 0.039625f;
            BlueShift = 7;
            BlueFuzz = 1;
            ForceBold = false;
            LanguageGroup = 0;
            ExpansionFactor = 0.06f;
            InitialRoundSeed = 0;
        }

        // 00
        public string Version { get; set; }

        // 01
        public string Notice { get; set; }

        // 02
        public string FullName { get; set; }

        // 03
        public string FamilyName { get; set; }

        // 04
        public string Weight { get; set; }

        // 06
        public int[] BlueValues { get; set; }

        // 07
        public int[] OtherBlues { get; set; }

        // 08
        public int[] FamilyBlues { get; set; }

        // 09
        public int[] FamilyOtherBlues { get; set; }

        // 10
        public int? StdHW { get; set; }

        // 11
        public int? StdVW { get; set; }

        // 13
        public int? UniqueId { get; set; }

        // 05
        public float[] FontBBox { get; set; }

        // 14
        public int[] XUID { get; set; }

        // 15
        public uint Charset { get; set; }

        // 16
        public uint Encoding { get; set; }

        // 17
        public uint CharStrings { get; set; }

        // 18
        public uint[] Private { get; set; }

        // 12 00
        public string Copyright { get; set; }

        // 12 01
        public bool IsFixedPitch { get; set; }

        // 12 02
        public int ItalicAngle { get; set; }

        // 12 03
        public int UnderlinePosition { get; set; }

        // 12 04
        public int UnderlineThickness { get; set; }

        // 12 05
        public int PaintType { get; set; }

        // 12 06
        public int CharStringType { get; set; }

        // 12 07
        public float[] FontMatrix { get; set; }

        // 12 08
        public int StrokeWidth { get; set; }

        // 12 09
        public float BlueScale { get; set; }

        // 12 10
        public int BlueShift { get; set; }

        // 12 11
        public int BlueFuzz { get; set; }

        // 12 12
        public int? StemSnapH { get; set; }

        // 12 13
        public int? StemSnapV { get; set; }

        // 12 14
        public bool ForceBold { get; set; }

        // 12 17
        public int LanguageGroup { get; set; }

        // 12 18
        public float ExpansionFactor { get; set; }

        // 12 19
        public int InitialRoundSeed { get; set; }

        // 12 20
        public int? SyntheticBase { get; set; }

        // 12 21
        public int? PostScript { get; set; }

        // 12 22
        public string BaseFontName { get; set; }

        // 12 23
        public int[] BaseFontBlend { get; set; }

        // 12 30
        public (string, string, int) ROS { get; set; }

        // 12 31
        public string CIDFontVersion { get; set; }

        // 12 32
        public int CIDFontRevision { get; set; }

        // 12 33
        public int CIDFontType { get; set; }

        // 12 34
        public int CIDCount { get; set; }

        // 12 35
        public int? UIDBase { get; set; }

        // 12 36
        public uint? FDArrayIndex { get; set; }

        // 12 37
        public uint? FDSelectIndex { get; set; }

        // 12 38
        public string FontName { get; set; }

        public int DefaultWidthX { get; set; }

        public int NormalWidthX { get; set; }

        public ByteSegment[] Subrs { get; set; }

        public int SubrsBias { get; set; }

        public PrivateDict PrivateDict { get; set; }

        public TopDict[] FDArray { get; set; }

        public byte[] FDSelect { get; set; }
    }
}