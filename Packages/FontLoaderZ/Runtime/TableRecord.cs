namespace FontLoaderZ
{
    public readonly struct TableRecord
    {
        public string Tag { get; }

        public uint Offset { get; }

        private TableRecord(string tag, uint offset)
        {
            Tag = tag;
            Offset = offset;
        }

        public static TableRecord Load(FontSource data)
        {
            var tag = data.ReadTag().Trim();
            // Skip check sum
            data.Offset += 4;
            var offset = data.ReadUInt();
            // Skip length
            data.Offset += 4;
            return new TableRecord(tag, offset);
        }
    }
}