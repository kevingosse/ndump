#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class ExecutionEngineException : _.System.SystemException
{
    private ExecutionEngineException(ulong address, DumpContext context) : base(address, context) { }

    public static new ExecutionEngineException FromAddress(ulong address, DumpContext context)
        => new ExecutionEngineException(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ExecutionEngineException> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.ExecutionEngineException"))
            yield return new ExecutionEngineException(addr, context);
    }

    public override string ToString() => $"ExecutionEngineException@0x{_objAddress:X}";
}
