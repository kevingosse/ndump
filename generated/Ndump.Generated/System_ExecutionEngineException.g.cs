#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class ExecutionEngineException : _.System.SystemException
{
    private ExecutionEngineException(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new ExecutionEngineException FromAddress(ulong address, DumpContext ctx)
        => new ExecutionEngineException(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ExecutionEngineException> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.ExecutionEngineException"))
            yield return new ExecutionEngineException(addr, ctx);
    }

    public override string ToString() => $"ExecutionEngineException@0x{_objAddress:X}";
}
