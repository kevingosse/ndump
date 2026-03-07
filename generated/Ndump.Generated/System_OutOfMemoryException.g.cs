#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class OutOfMemoryException : _.System.SystemException
{
    private OutOfMemoryException(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new OutOfMemoryException FromAddress(ulong address, DumpContext ctx)
        => new OutOfMemoryException(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<OutOfMemoryException> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.OutOfMemoryException"))
            yield return new OutOfMemoryException(addr, ctx);
    }

    public override string ToString() => $"OutOfMemoryException@0x{_objAddress:X}";
}
