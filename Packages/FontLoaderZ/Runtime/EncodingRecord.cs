namespace FontLoaderZ
{
    public readonly struct EncodingRecord
    {
        private EncodingRecord(uint platformID, uint encodingID, uint offset, int subTableFormat)
        {
            PlatformID = platformID;
            EncodingID = encodingID;
            Offset = offset;
            SubTableFormat = subTableFormat;
        }

        /// <summary>
        /// 0: Unicode
        /// 1: Macintosh
        /// 2: Windows
        /// </summary>
        public uint PlatformID { get; }

        public uint EncodingID { get; }
        public uint Offset { get; }

        /// <summary>
        /// 0: Byte encoding table
        /// 2: High-byte mapping through table
        /// 4: Segment mapping to delta values
        /// 6: Trimmed table mapping
        /// 8: mixed 16-bit and 32-bit coverage
        /// 10: Trimmed array
        /// 12: Segmented coverage
        /// 13: Many-to-one range mappings
        /// 14: Unicode Variation Sequences
        /// </summary>
        public int SubTableFormat { get; }

        public static EncodingRecord Load(FontSource source, uint tableOffset)
        {
            var platformID = source.ReadUShort();
            var encodingID = source.ReadUShort();
            var encodingRecordOffset = source.ReadUInt();

            var offset = source.Offset;
            source.Offset = tableOffset + encodingRecordOffset;
            var subTableFormat = source.ReadUShort();
            source.Offset = offset;

            return new EncodingRecord(platformID, encodingID, encodingRecordOffset, subTableFormat);
        }
    }
}