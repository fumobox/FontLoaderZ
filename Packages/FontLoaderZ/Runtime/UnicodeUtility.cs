using System.Collections.Generic;
using System.Text;

namespace FontLoaderZ
{
    public static class UnicodeUtility
    {
        public static readonly uint HorizontalTabulation = 0x9;
        public static readonly uint LineFeed = 0xa;
        public static readonly uint Space = 0x20;
        public static readonly uint FullWidthSpace = 0x3000;

        /// <summary>
        /// Convert a string to unicode code points.
        /// </summary>
        public static uint[] ToCodePoints(string text)
        {
            var list = new List<uint>(text.Length);
            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];
                if (char.IsSurrogate(c))
                {
                    var plane = (c - 0xd800) / 0x0040 + 1;
                    var lead2 = c - (0xd800 + 0x0040 * (plane - 1));
                    var trail2 = text[i + 1] - 0xdc00;
                    list.Add((uint) (plane * 0x10000 + lead2 * 0x100 + lead2 * 0x300 + trail2));
                    i += 2;
                }
                else
                {
                    list.Add(c);
                    i += 1;
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Indicates whether a code point is categorized as a control character.
        /// </summary>
        public static bool IsControl(uint codePoint)
        {
            return codePoint <= 0x1f || codePoint == 0x7f || codePoint >= 0x80 && codePoint <= 0x9f;
        }

        /// <summary>
        /// Get alphabets.
        /// </summary>
        public static byte[] GetAtoZ(bool isUpperCase = true)
        {
            if (isUpperCase)
            {
                return GetAsciiCodes(0x41, 26);
            }
            else
            {
                return GetAsciiCodes(0x61, 26);
            }
        }

        /// <summary>
        /// Get numbers.
        /// </summary>
        public static byte[] Get0to9()
        {
            return GetAsciiCodes(0x30, 10);
        }

        /// <summary>
        /// Get non control code points.
        /// </summary>
        public static byte[] GetNonControlCharacters()
        {
            return GetAsciiCodes(0x21, 0x7e - 0x21 + 1);
        }

        /// <summary>
        /// Get Ascii code points.
        /// </summary>
        public static byte[] GetAsciiCodes(byte offset, byte count)
        {
            var data = new byte[count];
            for (byte i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(offset + i);
            }
            return data;
        }

        /// <summary>
        /// Get Japanese Katakana code points.
        /// </summary>
        public static uint[] GetKatakana(bool isHalfWidth = false)
        {
            var start = (uint)(isHalfWidth ? 0xff66 : 0x30a1);
            var end = (uint)(isHalfWidth ? 0xff9d : 0x30fa);
            var data = new uint[end - start + 1];
            for (byte i = 0; i < data.Length; i++)
            {
                data[i] = start + i;
            }
            return data;
        }

        /// <summary>
        /// Get Japanese Hiragana code points.
        /// </summary>
        public static uint[] GetHiragana()
        {
            uint start = 0x3041;
            uint end = 0x3094;
            var data = new uint[end - start + 1];
            for (byte i = 0; i < data.Length; i++)
            {
                data[i] = start + i;
            }
            return data;
        }

        /// <summary>
        /// Convert a unicode code point to UTF8 bytes.
        /// </summary>
        public static byte[] ToUTF8Byte(uint codePoint)
        {
            if (codePoint < 0x7f)
            {
                // 0xxxxxxx
                return new [] {(byte)codePoint};
            }
            else if (codePoint <= 0x7ff)
            {
                // 110xxxxx 10xxxxxx
                return new []
                {
                    (byte)(((codePoint & 0x7c0) >> 6) | 0xc0),
                    (byte)((codePoint & 0x3f) | 0x80)
                };
            }
            else if (codePoint <= 0xffff)
            {
                // 1110xxxx 10xxxxxx 10xxxxxx
                return new []
                {
                    (byte)(((codePoint & 0xf000) >> 12) | 0xe0),
                    (byte)(((codePoint & 0xfc0) >> 6) | 0x80),
                    (byte)((codePoint & 0x3f) | 0x80)
                };
            }
            else if (codePoint <= 0x1fffff)
            {
                // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
                return new []
                {
                    (byte)(((codePoint & 0x1c0000) >> 18) | 0xf0),
                    (byte)(((codePoint & 0x3f000) >> 12) | 0x80),
                    (byte)(((codePoint & 0xfc0) >> 6) | 0x80),
                    (byte)((codePoint & 0x3f) | 0x80)
                };
            }
            else
            {
                throw new FontException($"Code point is out of range: 0x{codePoint:X}");
            }
        }

        public static string CodePointToString(uint codePoint)
        {
            return Encoding.UTF8.GetString(ToUTF8Byte(codePoint));
        }

    }
}