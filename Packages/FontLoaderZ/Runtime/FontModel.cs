using System.Collections.Generic;
using FontLoaderZ.CompactFontFormat;

namespace FontLoaderZ
{
    /// <summary>
    /// Provides glyph information of a font.
    /// </summary>
    public sealed class FontModel
    {
        public enum FontType
        {
            TrueType,
            OpenType
        }

        private Dictionary<uint, uint> _glyphIndexMap;
        internal GlyphInfo[] _glyphs;

        private FontModel() {}

        public FontType Type { get; private set; }

        public bool IsCidFont { get; set; }

        public uint GlyphCount => (uint) _glyphs.Length;

        public float BaseScale { get; private set; }

        public ushort UnitsPerEm { get; private set; }

        public float DefaultWidthX { get; set; }

        public float NormalWidthX { get; set; }

        public ByteSegment[] GSubrs { get; set; }

        public int GSubrsBias { get; set; }

        public ByteSegment[] Subrs { get; set; }

        public int SubrsBias { get; set; }

        public TopDict TopDict { get; set; }

        private uint[] _localOffsets;

        private GlyphTable _glyph;

        private CompactFontFormatTable _cff;

        public GlyphContourInfo GetGlyph(uint code, FontSource fontSource)
        {
            if (UnicodeUtility.IsControl(code))
                throw new FontException($"Cannot get control characters: U+{code:X2}");

            if (!_glyphIndexMap.ContainsKey(code))
                throw new FontException($"Glyph not found: U+{code:X4}");

            var gid = _glyphIndexMap[code];

            return GetGlyphFromGid(gid, fontSource);
        }

        public GlyphContourInfo GetNotDefGlyph(FontSource fontSource)
        {
            return GetGlyphFromGid(0, fontSource);
        }

        public bool HasGlyph(uint code)
        {
            return _glyphIndexMap.ContainsKey(code);
        }

        public GlyphContourInfo GetGlyphFromGid(uint gid, FontSource fontSource)
        {
            if (Type == FontType.TrueType)
            {
                var gcd = new GlyphContourInfo(_glyphs[gid]);
                if (HasLocalOffset(gid))
                    gcd.Header = _glyph.LoadHeader(_localOffsets[gid], fontSource);
                return gcd;
            }

            if (Type == FontType.OpenType)
            {
                var gcd = new GlyphContourInfo(_glyphs[gid]);
                gcd.Path = CffCharString.Load(gid, this, fontSource.Bytes);
                return gcd;
            }

            return null;
        }

        internal ByteSegment GetCharString(uint gid)
        {
            return _cff._charStringIndex[gid];
        }

        #region Loader

        public static FontModel Load(FontSource source)
        {
            var font = new FontModel();

            var offsetTable = OffsetTable.Load(source);
            if (offsetTable.SfntVersion == 0x10000)
                font.Type = FontType.TrueType;
            else if (offsetTable.SfntVersion == 0x4f54544f)
                font.Type = FontType.OpenType;
            else
                throw new FontException($"Unknown sfntVersion: {offsetTable.SfntVersion:X4}");

            var tableOffsets = new Dictionary<string, uint>(offsetTable.NumTables);
            for (var i = 0; i < offsetTable.NumTables; i++)
            {
                var table = TableRecord.Load(source);
                tableOffsets[table.Tag] = table.Offset;
            }

            LoadFontHeader(source, tableOffsets["head"], out var indexToLocFormat, out var unitsPerEm);
            font.BaseScale = 1f / unitsPerEm;
            font.UnitsPerEm = unitsPerEm;

            var glyphCount = LoadGlyphCount(source, tableOffsets["maxp"]);
            font._glyphs = new GlyphInfo[glyphCount];

            var numberOfHMetrics = LoadNumberOfHMetrics(source, tableOffsets["hhea"]);

            LoadHorizontalMetrics(font, source, tableOffsets["hmtx"], numberOfHMetrics, glyphCount);

            font._glyphIndexMap = LoadCharacterToGlyphIndexMappingTable(source, tableOffsets["cmap"]);

            if (tableOffsets.ContainsKey("loca"))
                font._localOffsets = LoadIndexToLocation(source, tableOffsets["loca"], glyphCount, indexToLocFormat);

            if (tableOffsets.ContainsKey("glyf"))
                font._glyph = new GlyphTable(tableOffsets["glyf"]);

            if (tableOffsets.ContainsKey("CFF"))
                font._cff = CompactFontFormatTable.Load(tableOffsets["CFF"], source, font);

            return font;
        }

        private bool HasLocalOffset(uint gid)
        {
            if (gid == _localOffsets.Length - 1)
                return true;

            if (gid >= _localOffsets.Length)
                return false;

            if (_localOffsets[gid] == _localOffsets[gid + 1])
                return false;

            return true;
        }

        private static ushort LoadNumberOfHMetrics(FontSource p, uint tableOffset)
        {
            p.Offset = tableOffset + 34;
            return p.ReadUShort();
        }

        private static ushort LoadGlyphCount(FontSource p, uint tableOffset)
        {
            p.Offset = tableOffset + 4;
            return p.ReadUShort();
        }

        private static void LoadHorizontalMetrics(FontModel font, FontSource p, uint tableOffset, int numberOfHMetrics, int numGlyphs)
        {
            ushort advancedWidth = 0;
            short leftSideBearing = 0;
            p.Offset = tableOffset;
            for (var i = 0; i < numGlyphs; i++)
            {
                if (i < numberOfHMetrics)
                {
                    advancedWidth = p.ReadUShort();
                    leftSideBearing = p.ReadShort();
                }
                font._glyphs[i] = new GlyphInfo(advancedWidth, leftSideBearing);
            }
        }

        private static uint[] LoadIndexToLocation(FontSource p, uint tableOffset, ushort glyphCount, short indexToLocFormat)
        {
            p.Offset = tableOffset;

            var isLong = indexToLocFormat == 1;

            var n = glyphCount + 1;
            var localOffsets = new uint[n];
            if (isLong)
                for (var i = 0; i < n; i++)
                    localOffsets[i] = p.ReadUInt();
            else
                for (var i = 0; i < n; i++)
                    localOffsets[i] = (uint) p.ReadUShort() * 2;

            return localOffsets;
        }

        private static void LoadFontHeader(FontSource p, uint tableOffset, out short indexToLocFormat, out ushort unitsPerEm)
        {
            p.Offset = tableOffset;

            p.ReadUShort();
            p.ReadUShort();
            p.ReadFixed();
            p.ReadUInt();
            p.ReadUInt();
            p.ReadUShort();
            unitsPerEm = p.ReadUShort();
            p.Offset += 8 * 2;
            p.ReadShort();
            p.ReadShort();
            p.ReadShort();
            p.ReadShort();
            p.ReadUShort();
            p.ReadUShort();
            p.ReadShort();
            indexToLocFormat = p.ReadShort();
            p.ReadShort();
        }

        private static Dictionary<uint, uint> LoadCharacterToGlyphIndexMappingTable(FontSource p, uint tableOffset)
        {
            p.Offset = tableOffset;

            var version = p.ReadUShort();
            var numTables = p.ReadUShort();
            var encodingRecords = new EncodingRecord[numTables];

            for (var i = 0; i < numTables; i++) encodingRecords[i] = EncodingRecord.Load(p, tableOffset);

            var targetRecordId = -1;

            for (var i = numTables - 1; i >= 0; i--)
            {
                var r = encodingRecords[i];
                if (r.PlatformID == 3 && (r.EncodingID == 0 || r.EncodingID == 1 || r.EncodingID == 10) ||
                    r.PlatformID == 0 && (r.EncodingID == 0 || r.EncodingID == 1 || r.EncodingID == 2 || r.EncodingID == 3 || r.EncodingID == 4))
                {
                    targetRecordId = i;
                    break;
                }
            }

            if (targetRecordId < 0) throw new FontException("Encoding record not found.");

            var targetRecord = encodingRecords[targetRecordId];

            if (targetRecord.SubTableFormat == 12)
            {
                p.Offset = tableOffset + targetRecord.Offset;
                return LoadSegmentedCoverage(p);
            }

            if (targetRecord.SubTableFormat == 4)
            {
                p.Offset = tableOffset + targetRecord.Offset;
                return LoadSegmentMappingToDeltaValues(p);
            }

            throw new FontException($"Unsupported format: {targetRecord.SubTableFormat}");
        }

        private static Dictionary<uint, uint> LoadSegmentedCoverage(FontSource p)
        {
            var dic = new Dictionary<uint, uint>();

            // Skip format
            p.ReadUShort();

            // Skip reserved
            p.ReadUShort();

            var length = p.ReadUInt();

            var language = p.ReadUInt();

            var numGroups = p.ReadUInt();

            for (var i = 0; i < numGroups; i++)
            {
                var startCharCode = p.ReadUInt();
                var endCharCode = p.ReadUInt();
                var startGlyphId = p.ReadUInt();

                for (var c = startCharCode; c <= endCharCode; c++)
                {
                    dic[c] = startGlyphId;
                    startGlyphId++;
                }
            }

            return dic;
        }

        private static Dictionary<uint, uint> LoadSegmentMappingToDeltaValues(FontSource p)
        {
            var dic = new Dictionary<uint, uint>();

            var offset = p.Offset;

            // Skip format
            p.ReadUShort();

            var length = p.ReadUShort();
            var language = p.ReadUShort();
            var segCount = p.ReadUShort() >> 1;
            var searchRange = p.ReadUShort();
            var entrySelector = p.ReadUShort();
            var rangeShift = p.ReadUShort();

            var endCodeParser = new FontSource(p.Bytes, offset + 14);
            var startCodeParser = new FontSource(p.Bytes, (uint) (offset + 16 + segCount * 2));
            var idDeltaParser = new FontSource(p.Bytes, (uint) (offset + 16 + segCount * 4));
            var idRangeOffsetParser = new FontSource(p.Bytes, (uint) (offset + 16 + segCount * 6));

            for (var i = 0; i < segCount - 1; i++)
            {
                var endCode = endCodeParser.ReadUShort();
                var startCode = startCodeParser.ReadUShort();
                var idDelta = idDeltaParser.ReadShort();
                var idRangeOffset = idRangeOffsetParser.ReadUShort();

                for (var c = startCode; c <= endCode; c++)
                {
                    uint glyphIndex;
                    if (idRangeOffset != 0)
                    {
                        var glyphIndexOffset = idRangeOffsetParser.Offset - 2;
                        glyphIndexOffset += idRangeOffset;
                        glyphIndexOffset += (uint) (c - startCode) * 2;
                        p.Offset = glyphIndexOffset;
                        glyphIndex = p.ReadUShort();
                        if (glyphIndex != 0) glyphIndex = (uint) (glyphIndex + idDelta) & 0xFFFF;
                    }
                    else
                    {
                        glyphIndex = (uint) (c + idDelta) & 0xFFFF;
                    }

                    dic[c] = glyphIndex;
                }
            }

            return dic;
        }

        #endregion
    }
}