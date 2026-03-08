#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class OutOfMemoryException : _.System.SystemException
{
    private OutOfMemoryException(ulong address, DumpContext context) : base(address, context) { }

    public static new OutOfMemoryException FromAddress(ulong address, DumpContext context)
        => new OutOfMemoryException(address, context);

    public static new global::System.Collections.Generic.IEnumerable<OutOfMemoryException> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.OutOfMemoryException"))
            yield return new OutOfMemoryException(addr, context);
    }

    public override string ToString() => $"OutOfMemoryException@0x{_objAddress:X}";
}
