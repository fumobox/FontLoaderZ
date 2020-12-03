using System;
using System.Runtime.Serialization;

namespace FontLoaderZ
{
    public sealed class FontException : Exception
    {
        public FontException() {}

        public FontException(string message) : base(message) {}

        public FontException(string message, Exception innerException) : base(message, innerException) {}

        public FontException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}