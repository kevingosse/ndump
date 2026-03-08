#nullable enable
using Ndump.Core;

namespace _.System;

public class SystemException : _.System.Exception
{
    protected SystemException(ulong address, DumpContext context) : base(address, context) { }

    public static new SystemException FromAddress(ulong address, DumpContext context)
        => new SystemException(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SystemException> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.SystemException"))
            yield return new SystemException(addr, context);
    }

    public override string ToString() => $"SystemException@0x{_objAddress:X}";
}
