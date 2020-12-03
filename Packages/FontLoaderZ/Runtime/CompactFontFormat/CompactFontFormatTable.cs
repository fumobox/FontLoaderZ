using System.Collections.Generic;

namespace FontLoaderZ.CompactFontFormat
{
    public sealed class CompactFontFormatTable
    {
        private readonly ByteSegment[] _fdArrayIndex;

        internal readonly ByteSegment[] _charStringIndex;

        private CompactFontFormatTable(FontSource source, FontModel font)
        {
            var tableOffset = source.Offset;

            // Header
            // Skip h_major, h_minor, h_hdrSize, h_offSize
            source.Offset += 4;

            // Ignore Name Index
            LoadIndexData(source);

            // Top Dict Index
            var topDictIndex = LoadIndexData(source);

            // String Index
            var stringIndex = LoadIndexData(source);

            var globalSubrIndex = LoadIndexData(source);
            if (globalSubrIndex != null)
            {
                font.GSubrs = globalSubrIndex;
                font.GSubrsBias = GetSubrBias(font.GSubrs.Length);
            }

            var topDictArray = LoadTopDicts(source, tableOffset, topDictIndex, stringIndex);
            if (topDictArray.Length != 1) throw new FontException($"Too Many TopDict: {topDictArray.Length}");

            font.TopDict = topDictArray[0];

            if (font.TopDict.PrivateDict != null)
            {
                font.DefaultWidthX = font.TopDict.PrivateDict.DefaultWidthX;
                font.NormalWidthX = font.TopDict.PrivateDict.NormalWidthX;
            }

            font.IsCidFont = font.TopDict.ROS.Item1 != null && font.TopDict.ROS.Item2 != null;

            if (font.IsCidFont)
            {
                if (!font.TopDict.FDArrayIndex.HasValue || font.TopDict.FDArrayIndex.Value == 0)
                    throw new FontException("FDArray is missing.");

                if (!font.TopDict.FDSelectIndex.HasValue || font.TopDict.FDSelectIndex.Value == 0)
                    throw new FontException("FDSelect is missing");

                var fdArrayOffset = tableOffset + font.TopDict.FDArrayIndex.Value;
                var fdSelectOffset = tableOffset + font.TopDict.FDSelectIndex.Value;

                source.Offset = fdArrayOffset;
                var fdArrayIndex = LoadIndexData(source);
                font.TopDict.FDArray =
                    LoadTopDicts(source, tableOffset, fdArrayIndex, stringIndex);
                font.TopDict.FDSelect =
                    LoadFdSelect(source, fdSelectOffset, font.GlyphCount, (uint) font.TopDict.FDArray.Length);
            }

            var privateDictOffset = tableOffset + font.TopDict.Private[1];
            var privateDict = CffDictParser.LoadPrivateDict(source.Bytes, privateDictOffset, font.TopDict.Private[0]);

            font.DefaultWidthX = privateDict.DefaultWidthX;
            font.NormalWidthX = privateDict.NormalWidthX;

            if (privateDict.Subrs != 0)
            {
                var subrOffset = privateDictOffset + privateDict.Subrs;
                source.Offset = subrOffset;
                var subrIndex = LoadIndexData(source);
                font.Subrs = subrIndex;
                font.SubrsBias = GetSubrBias(font.Subrs.Length);
            }
            else
            {
                font.Subrs = new ByteSegment[0];
                font.SubrsBias = 0;
            }

            source.Offset = tableOffset + font.TopDict.CharStrings;
            _charStringIndex = LoadIndexData(source);
        }

        public static CompactFontFormatTable Load(uint tableOffset, FontSource p, FontModel font)
        {
            p.Offset = tableOffset;
            return new CompactFontFormatTable(p, font);
        }

        private static TopDict[] LoadTopDicts(FontSource p, uint offset, ByteSegment[] cffIndex, ByteSegment[] strings)
        {
            var arr = new TopDict[cffIndex.Length];
            for (var i = 0; i < cffIndex.Length; i++)
            {
                var bsi = cffIndex[i];
                var topDict = CffDictParser.LoadTopDict(p.Bytes, bsi.Offset, bsi.Count, strings);
                var privateSize = topDict.Private[0];
                var privateOffset = topDict.Private[1];
                if (privateSize != 0 && privateOffset != 0)
                {
                    var privateDict =
                        CffDictParser.LoadPrivateDict(p.Bytes, privateOffset + offset, privateSize);
                    topDict.DefaultWidthX = privateDict.DefaultWidthX;
                    topDict.NormalWidthX = privateDict.NormalWidthX;
                    if (privateDict.Subrs != 0)
                    {
                        var subrOffset = privateOffset + privateDict.Subrs;
                        p.Offset = subrOffset + offset;
                        var subrIndex = LoadIndexData(p);
                        topDict.Subrs = subrIndex;
                        topDict.SubrsBias = GetSubrBias(topDict.Subrs.Length);
                    }

                    topDict.PrivateDict = privateDict;
                }

                arr[i] = topDict;
            }
            return arr;
        }

        private static byte[] LoadFdSelect(FontSource p, uint offset, uint glyphCount, uint fdArrayCount)
        {
            var fdSelect = new List<byte>();
            p.Offset = offset;
            var format = p.ReadByte();
            if (format == 0)
            {
                // Simple list of nGlyphs elements
                for (var i = 0; i < glyphCount; i++)
                {
                    var fdIndex = p.ReadByte();
                    if (fdIndex >= fdArrayCount)
                        throw new FontException($"Invalid value: FDIndex={fdIndex}, FDArrayCount={fdArrayCount}");
                    fdSelect.Add(fdIndex);
                }
            }
            else if (format == 3)
            {
                // Ranges
                var nRanges = p.ReadUShort();
                var first = p.ReadUShort();
                if (first != 0)
                    throw new FontException($"Invalid value: first={first}");
                ushort next = 0;
                for (var i = 0; i < nRanges; i++)
                {
                    var fdIndex = p.ReadByte();
                    next = p.ReadUShort();
                    if (fdIndex > fdArrayCount)
                        throw new FontException($"Invalid value: FDIndex={fdIndex}, FDArrayCount={fdArrayCount}");
                    if (next > glyphCount)
                        throw new FontException($"Invalid value: next={next}, glyphCount={glyphCount}");
                    while (first < next)
                    {
                        fdSelect.Add(fdIndex);
                        first++;
                    }

                    first = next;
                }

                if (next != glyphCount)
                    throw new FontException($"Invalid value: next={next}, glyphCount={glyphCount}");
            }
            else
            {
                throw new FontException($"Unsupported format: {format}");
            }

            return fdSelect.ToArray();
        }

        public static string GetString(int index, ByteSegment[] strings, byte[] data)
        {
            return index <= 390 ? null : strings[index - 391].GetString(data);
        }

        public static int GetSubrBias(int subrLength)
        {
            if (subrLength <= 1240) return 107;
            if (subrLength <= 33900) return 1131;
            return 32768;
        }

        private static ByteSegment[] LoadIndexData(FontSource p)
        {
            var count = p.ReadUShort();

            if (count <= 0) return null;

            var offSize = p.ReadByte();
            var offsets = new uint[count + 1];
            for (var i = 0; i < offsets.Length; i++) offsets[i] = p.ReadUInt(offSize);
            var segments = new ByteSegment[count];
            for (var i = 0; i < offsets.Length - 1; i++)
                segments[i] = new ByteSegment(p.Offset + offsets[i] - 1, offsets[i + 1] - offsets[i]);
            p.Offset += offsets[offsets.Length - 1] - 1;
            return segments;
        }
    }
}