using System;
using JetBrains.Annotations;

namespace RectpackSharp
{
    [PublicAPI]
    public class PackingException : Exception
    {
        public PackingException() { }

        public PackingException(string message) : base(message) { }

        public PackingException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
