#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class StackOverflowException : _.System.SystemException
{
    private StackOverflowException(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new StackOverflowException FromAddress(ulong address, DumpContext ctx)
        => new StackOverflowException(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StackOverflowException> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.StackOverflowException"))
            yield return new StackOverflowException(addr, ctx);
    }

    public override string ToString() => $"StackOverflowException@0x{_objAddress:X}";
}
