// https://docs.microsoft.com/ja-jp/typography/opentype/spec/glyf

using System.Collections.Generic;

namespace FontLoaderZ
{
    public sealed class GlyphTable
    {
        private readonly uint _offset;

        public GlyphTable(uint tableOffset)
        {
            _offset = tableOffset;
        }

        public GlyphHeader LoadHeader(uint localOffset, FontSource source)
        {
            source.Offset = _offset + localOffset;

            return new GlyphHeader(
                source.ReadShort(),
                source.ReadShort(),
                source.ReadShort(),
                source.ReadShort(),
                source.ReadShort(),
                source.Offset
            );
        }

        public readonly struct GlyphHeader
        {
            public uint Offset { get; }

            // Header Parameters
            public short NumberOfContours { get; }
            public short XMax { get; }
            public short XMin { get; }
            public short YMax { get; }
            public short YMin { get; }

            public GlyphHeader(short numberOfContours,
                short xMin, short yMin, short xMax, short yMax,
                uint offset)
            {
                NumberOfContours = numberOfContours;
                XMin = xMin;
                YMin = yMin;
                XMax = xMax;
                YMax = yMax;
                Offset = offset;
            }

            public GlyphPoint[] GetGlyphPoints(FontSource source)
            {
                source.Offset = Offset;
                var endPtsOfContours = new ushort[NumberOfContours];
                for (var i = 0; i < NumberOfContours; i++)
                {
                    endPtsOfContours[i] = source.ReadUShort();
                }

                //instructionLength = data.ReadUShort();
                //instructions = new byte[instructionLength];
                //for (var i = 0; i < instructionLength; i++) instructions[i] = data.ReadByte();

                // Ignore instructions.
                var instructionLength = source.ReadUShort();
                source.Offset += instructionLength;

                var numberOfCoordinates = endPtsOfContours[endPtsOfContours.Length - 1] + 1;
                var points = new List<GlyphPoint>(numberOfCoordinates);

                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    var flag = source.ReadByte();
                    var glyphPoint = new GlyphPoint(flag);
                    points.Add(glyphPoint);
                    if (glyphPoint.IsRepeat)
                    {
                        var count = source.ReadByte();
                        for (var k = 0; k < count; k++)
                        {
                            points.Add(new GlyphPoint(flag));
                            i++;
                        }
                    }
                }

                foreach (var ep in endPtsOfContours) points[ep].IsEndPoint = true;

                short xSum = 0;
                short ySum = 0;

                // Read X
                foreach (var point in points)
                {
                    var flag = point.Flag;
                    var isShortX = GetFlag(flag, 1);
                    var isSameX = GetFlag(flag, 4);
                    if (isShortX)
                    {
                        if (isSameX)
                            xSum += source.ReadByte();
                        else
                            xSum += (short) -source.ReadByte();
                    }
                    else
                    {
                        if (!isSameX)
                            xSum += source.ReadShort();
                    }

                    point.X = xSum;
                }

                // Read Y
                foreach (var point in points)
                {
                    var flag = point.Flag;
                    var isShortY = GetFlag(flag, 2);
                    var isSameY = GetFlag(flag, 5);
                    if (isShortY)
                    {
                        if (isSameY)
                            ySum += source.ReadByte();
                        else
                            ySum += (short) -source.ReadByte();
                    }
                    else
                    {
                        if (!isSameY)
                            ySum += source.ReadShort();
                    }

                    point.Y = ySum;
                }

                return points.ToArray();
            }

            public GlyphComponent[] GetGlyphComponents(FontSource source)
            {
                source.Offset = Offset;

                var list = new List<GlyphComponent>();

                var limit = 10;
                while (limit-- > 0)
                {
                    var component = GlyphComponent.Load(source);
                    list.Add(component);
                    if (!component.MoreComponents) break;
                }

                return list.ToArray();
            }

            private static bool GetFlag(byte data, byte bitPosition)
            {
                return (data & (1 << bitPosition)) != 0;
            }

            public sealed class GlyphPoint
            {
                public GlyphPoint(byte flag)
                {
                    Flag = flag;
                }

                public bool OnCurve => GetFlag(Flag, 0);

                public bool IsRepeat => GetFlag(Flag, 3);

                public bool IsOverlap => GetFlag(Flag, 6);

                public short X { get; set; }

                public short Y { get; set; }

                public byte Flag { get; }

                public bool IsEndPoint { get; set; }
            }

            public sealed class GlyphComponent
            {
                public ushort GlyphId { get; set; }
                public float XScale { get; set; } = 1;
                public float Scale01 { get; set; }
                public float Scale10 { get; set; }
                public float YScale { get; set; } = 1;
                public short DX { get; set; }
                public short DY { get; set; }

                public ushort[] MatchedPoints { get; set; }

                public bool MoreComponents { get; set; }

                public static GlyphComponent Load(FontSource p)
                {
                    var gc = new GlyphComponent();
                    var flags = p.ReadUShort();
                    gc.GlyphId = p.ReadUShort();

                    if ((flags & 1) > 0)
                    {
                        if ((flags & 2) > 0)
                        {
                            gc.DX = p.ReadShort();
                            gc.DY = p.ReadShort();
                        }
                        else
                        {
                            gc.MatchedPoints = new[] {p.ReadUShort(), p.ReadUShort()};
                        }
                    }
                    else
                    {
                        if ((flags & 2) > 0)
                        {
                            gc.DX = p.ReadSByte();
                            gc.DY = p.ReadSByte();
                        }
                        else
                        {
                            gc.MatchedPoints = new[] {p.ReadUShort(), p.ReadUShort()};
                        }
                    }

                    if ((flags & 8) > 0)
                    {
                        gc.XScale = gc.YScale = p.ReadF2Dot14();
                    }
                    else if ((flags & 64) > 0)
                    {
                        gc.XScale = p.ReadF2Dot14();
                        gc.YScale = p.ReadF2Dot14();
                    }
                    else if ((flags & 128) > 0)
                    {
                        gc.XScale = p.ReadF2Dot14();
                        gc.Scale01 = p.ReadF2Dot14();
                        gc.Scale10 = p.ReadF2Dot14();
                        gc.YScale = p.ReadF2Dot14();
                    }

                    gc.MoreComponents = (flags & 32) > 0;

                    return gc;
                }

                public static void TransformPoints(GlyphPoint[] p, GlyphComponent c)
                {
                    foreach (var t in p)
                    {
                        var px = t.X;
                        var py = t.Y;
                        t.X = (short) (c.XScale * px + c.Scale01 * py + c.DX);
                        t.Y = (short) (c.Scale10 * px + c.YScale * py + c.DY);
                    }
                }
            }
        }
    }
}