using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FontLoaderZ.CompactFontFormat
{
    public static class CffDictParser
    {
        private static readonly string[] Nibbles =
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ".", "E", "E-", "", "-"
        };

        public static TopDict LoadTopDict(byte[] data, uint offset, uint count, ByteSegment[] stringIndex)
        {
            var values = new List<decimal>();

            var d = new TopDict();

            var i = offset;
            while (i < offset + count)
            {
                var b0 = data[i++];

                if (ParseValue(b0, data, ref i, values)) continue;

                if (b0 == 12)
                {
                    // Key
                    var b1 = data[i++];
                    if (b0 == 12)
                    {
                        if (b1 == 0)
                        {
                            d.Copyright = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                        }
                        else if (b1 == 1)
                        {
                            d.IsFixedPitch = values[0] != 0;
                        }
                        else if (b1 == 2)
                        {
                            d.ItalicAngle = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 3)
                        {
                            d.UnderlinePosition = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 4)
                        {
                            d.UnderlineThickness = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 5)
                        {
                            d.PaintType = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 6)
                        {
                            d.CharStringType = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 7)
                        {
                            d.FontMatrix = values.Select(decimal.ToSingle).ToArray();
                        }
                        else if (b1 == 8)
                        {
                            d.StrokeWidth = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 12)
                        {
                            d.StemSnapH = (int) values[0];
                        }
                        else if (b1 == 13)
                        {
                            d.StemSnapV = (int) values[0];
                        }
                        else if (b1 == 14)
                        {
                            d.ForceBold = values[0] != 0;
                        }
                        else if (b1 == 17)
                        {
                            d.LanguageGroup = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 18)
                        {
                            d.ExpansionFactor = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 19)
                        {
                            d.InitialRoundSeed = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 20)
                        {
                            d.SyntheticBase = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 21)
                        {
                            d.PostScript = (int) values[0];
                        }
                        else if (b1 == 22)
                        {
                            d.BaseFontName = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                        }
                        else if (b1 == 23)
                        {
                            d.BaseFontBlend = values.Select(decimal.ToInt32).ToArray();
                        }
                        else if (b1 == 30)
                        {
                            d.ROS = (
                                CompactFontFormatTable.GetString((int) values[0], stringIndex, data),
                                CompactFontFormatTable.GetString((int) values[1], stringIndex, data),
                                (int) values[2]);
                        }
                        else if (b1 == 31)
                        {
                            d.CIDFontVersion = values[0].ToString();
                        }
                        else if (b1 == 32)
                        {
                            d.CIDFontRevision = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 33)
                        {
                            d.CIDFontType = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 34)
                        {
                            d.CIDCount = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 35)
                        {
                            d.UIDBase = decimal.ToInt32(values[0]);
                        }
                        else if (b1 == 36)
                        {
                            d.FDArrayIndex = (uint) values[0];
                        }
                        else if (b1 == 37)
                        {
                            d.FDSelectIndex = (uint) values[0];
                        }
                        else if (b1 == 38)
                        {
                            d.FontName = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                        }
                    }

                    values.Clear();
                }
                else if (b0 <= 21)
                {
                    // Key
                    if (b0 == 0) d.Version = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                    if (b0 == 1) d.Notice = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                    if (b0 == 2) d.FullName = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                    if (b0 == 3) d.FamilyName = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                    if (b0 == 4) d.Weight = CompactFontFormatTable.GetString((int) values[0], stringIndex, data);
                    if (b0 == 5) d.FontBBox = values.Select(decimal.ToSingle).ToArray();
                    if (b0 == 6) d.BlueValues = values.Select(decimal.ToInt32).ToArray();
                    if (b0 == 7) d.OtherBlues = values.Select(decimal.ToInt32).ToArray();
                    if (b0 == 8) d.FamilyBlues = values.Select(decimal.ToInt32).ToArray();
                    if (b0 == 9) d.FamilyOtherBlues = values.Select(decimal.ToInt32).ToArray();
                    if (b0 == 10) d.StdHW = decimal.ToInt32(values[0]);
                    if (b0 == 11) d.StdVW = decimal.ToInt32(values[0]);
                    if (b0 == 13) d.UniqueId = decimal.ToInt32(values[0]);
                    if (b0 == 14) d.XUID = values.Select(decimal.ToInt32).ToArray();
                    if (b0 == 15) d.Charset = (uint) values[0];
                    if (b0 == 16) d.Encoding = (uint) values[0];
                    if (b0 == 17) d.CharStrings = (uint) values[0];
                    if (b0 == 18) d.Private = values.Select(decimal.ToUInt32).ToArray();

                    values.Clear();
                }
                else
                {
                    throw new FontException(string.Format("Invalid data: {0}(0x{0:X2})", b0));
                }
            }

            return d;
        }

        public static PrivateDict LoadPrivateDict(byte[] data, uint offset, uint count)
        {
            var values = new List<decimal>();

            var d = new PrivateDict();

            var i = offset;
            while (i < offset + count)
            {
                var b0 = data[i++];

                if (ParseValue(b0, data, ref i, values)) continue;

                if (b0 <= 21)
                {
                    // Key
                    //Debug.Log(string.Format("<{0}>", b0));
                    if (b0 == 19) d.Subrs = (uint) values[0];
                    if (b0 == 20) d.DefaultWidthX = (int) values[0];
                    if (b0 == 21) d.NormalWidthX = (int) values[0];
                    values.Clear();
                }
                else
                {
                    throw new FontException(string.Format("Invalid data: {0}(0x{0:X2})", b0));
                }
            }

            return d;
        }

        public static int TwosComp(int val, int bits)
        {
            if ((val & (1 << (bits - 1))) != 0) val -= 1 << bits;
            return val;
        }

        private static bool ParseValue(byte b0, byte[] data, ref uint i, List<decimal> values)
        {
            if (32 <= b0 && b0 <= 246)
            {
                var v = b0 - 139;
                values.Add(v);
                return true;
            }

            if (247 <= b0 && b0 <= 250)
            {
                var b1 = data[i++];
                var v = (b0 - 247) * 256 + b1 + 108;
                values.Add(v);
                return true;
            }

            if (251 <= b0 && b0 <= 254)
            {
                var b1 = data[i++];
                var v = -(b0 - 251) * 256 - b1 - 108;
                values.Add(v);
                return true;
            }
            else if (b0 == 28)
            {
                var b1 = data[i++];
                var b2 = data[i++];
                var v = TwosComp((b1 << 8) | b2, 16);
                values.Add(v);
                return true;
            }

            if (b0 == 29)
            {
                var b1 = data[i++];
                var b2 = data[i++];
                var b3 = data[i++];
                var b4 = data[i++];
                var v = TwosComp((b1 << 24) | (b2 << 16) | (b3 << 8) | b4, 32);
                values.Add(v);
                return true;
            }

            if (b0 == 30)
            {
                var sb = new StringBuilder();
                while(i < data.Length)
                {
                    var nibble = data[i++];
                    var left = nibble >> 4;
                    var right = nibble & 0xf;
                    if (left == 0xf) break;
                    sb.Append(Nibbles[left]);
                    if (right == 0xf) break;
                    sb.Append(Nibbles[right]);
                }

                var ns = NumberStyles.AllowExponent | NumberStyles.Float;
                var fmt = CultureInfo.InvariantCulture;

                if (!decimal.TryParse(sb.ToString(), ns, fmt, out var v)) throw new FontException($"Cannot parse value: {sb}");

                values.Add(v);
                return true;
            }

            return false;
        }
    }
}