using System;
using System.Text;

namespace FontLoaderZ
{
    /// <summary>
    /// Provides control functions for a font binary.
    /// </summary>
    public sealed class FontSource
    {
        public FontSource(byte[] bytes, uint initialOffset = 0)
        {
            Bytes = bytes;
            InitialOffset = Offset = initialOffset;
        }

        public uint InitialOffset { get; }

        public uint Offset { get; set; }

        public byte[] Bytes { get; }

        public uint ReadFixed()
        {
            var x = (uint) (
                (Bytes[Offset] << 24) |
                (Bytes[Offset + 1] << 16) |
                (Bytes[Offset + 2] << 8) |
                Bytes[Offset + 3]
            );
            Offset += 4;
            return x;
        }

        public uint ReadUInt()
        {
            var x = (uint) (
                (Bytes[Offset] << 24) |
                (Bytes[Offset + 1] << 16) |
                (Bytes[Offset + 2] << 8) |
                Bytes[Offset + 3]
            );
            Offset += 4;
            return x;
        }

        public uint ReadUInt(byte loadByte)
        {
            uint x = 0;
            if (loadByte == 4)
                x = (uint) (
                    (Bytes[Offset] << 24) |
                    (Bytes[Offset + 1] << 16) |
                    (Bytes[Offset + 2] << 8) |
                    Bytes[Offset + 3]);
            else if (loadByte == 3)
                x = (uint) (
                    (Bytes[Offset] << 16) |
                    (Bytes[Offset + 1] << 8) |
                    Bytes[Offset + 2]);
            else if (loadByte == 2)
                x = (uint) (
                    (Bytes[Offset] << 8) |
                    Bytes[Offset + 1]);
            else if (loadByte == 1)
                x = Bytes[Offset];
            else
                throw new ArgumentOutOfRangeException("Invalid byte count: " + loadByte);
            Offset += loadByte;
            return x;
        }

        public string ReadTag()
        {
            var tag = Encoding.ASCII.GetString(new[]
            {
                Bytes[Offset], Bytes[Offset + 1], Bytes[Offset + 2], Bytes[Offset + 3]
            });
            Offset += 4;
            return tag;
        }

        /// <summary>
        /// Card16, SID
        /// </summary>
        public ushort ReadUShort()
        {
            var x = (ushort) (
                (Bytes[Offset] << 8) |
                Bytes[Offset + 1]
            );
            Offset += 2;
            return x;
        }

        public short ReadShort()
        {
            var x = (short) (
                (Bytes[Offset] << 8) |
                Bytes[Offset + 1]
            );
            Offset += 2;
            return x;
        }

        public byte ReadByte()
        {
            var b = Bytes[Offset];
            Offset += 1;
            return b;
        }

        public sbyte ReadSByte()
        {
            var b = Bytes[Offset];
            Offset += 1;
            return (sbyte) b;
        }

        public byte[] ReadBytes(uint count)
        {
            var arr = new byte[count];
            for (var i = 0; i < count; i++) arr[i] = Bytes[Offset + i];
            Offset += count;
            return arr;
        }

        public byte[] GetSegment(int offset, int count)
        {
            var segment = new byte[count];
            Buffer.BlockCopy(Bytes, offset, segment, 0, count);
            return segment;
        }

        public string GetSegmentString(int offset, int count)
        {
            var segment = new byte[count];
            Buffer.BlockCopy(Bytes, offset, segment, 0, count);
            return Encoding.ASCII.GetString(segment);
        }

        public char ReadChar()
        {
            var b = (char) Bytes[Offset];
            Offset += 1;
            return b;
        }

        public float ReadF2Dot14()
        {
            return ReadShort() / 16384f;
        }
    }
}