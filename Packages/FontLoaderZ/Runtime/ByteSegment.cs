using System;
using System.Text;

namespace FontLoaderZ
{
    public readonly struct ByteSegment
    {
        public uint Offset { get; }
        public uint Count { get; }

        public ByteSegment(uint offset, uint count)
        {
            if (count == 0)
                throw new ArgumentOutOfRangeException($"Invalid count: {count}");

            Offset = offset;
            Count = count;
        }

        public string GetString(byte[] data)
        {
            var subArr = new byte[Count];
            Buffer.BlockCopy(data, (int) Offset, subArr, 0, (int) Count);
            return Encoding.ASCII.GetString(subArr);
        }
    }
}