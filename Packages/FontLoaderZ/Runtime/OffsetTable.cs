namespace FontLoaderZ
{
    public readonly struct OffsetTable
    {
        public ushort NumTables { get; }

        public uint SfntVersion { get; }

        private OffsetTable(ushort numTables, uint sfntVersion)
        {
            NumTables = numTables;
            SfntVersion = sfntVersion;
        }

        public static OffsetTable Load(FontSource data)
        {
            var sfntVersion = data.ReadUInt();
            var numTables = data.ReadUShort();
            // Skip search range
            // Skip entry selector
            // Skip range shift
            data.Offset += 6;
            return new OffsetTable(numTables, sfntVersion);
        }
    }
}