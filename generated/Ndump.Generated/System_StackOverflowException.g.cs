#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class StackOverflowException : _.System.SystemException
{
    private StackOverflowException(ulong address, DumpContext context) : base(address, context) { }

    public static new StackOverflowException FromAddress(ulong address, DumpContext context)
        => new StackOverflowException(address, context);

    public static new global::System.Collections.Generic.IEnumerable<StackOverflowException> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.StackOverflowException"))
            yield return new StackOverflowException(addr, context);
    }

    public override string ToString() => $"StackOverflowException@0x{_objAddress:X}";
}
