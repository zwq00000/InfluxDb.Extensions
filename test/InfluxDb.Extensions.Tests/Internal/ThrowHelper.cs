using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace InfluxDb.Extensions.Tests
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowOutOfMemoryException_BufferMaximumSizeExceeded(uint capacity)
        {
            throw new OutOfMemoryException($"Cannot allocate a buffer of size {capacity}." );
        }
    }
}