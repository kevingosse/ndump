#nullable enable
using Ndump.Core;

namespace _.System;

public class SystemException : _.System.Exception
{
    protected SystemException(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new SystemException FromAddress(ulong address, DumpContext ctx)
        => new SystemException(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SystemException> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.SystemException"))
            yield return new SystemException(addr, ctx);
    }

    public override string ToString() => $"SystemException@0x{_objAddress:X}";
}
