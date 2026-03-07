#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class RuntimeType_ActivatorCache : _.System.Object
{
    private RuntimeType_ActivatorCache(ulong address, DumpContext ctx) : base(address, ctx) { }

    // Unknown field: _pfnAllocator (object)

    // Unknown field: _allocatorFirstArg (object)

    // Unknown field: _pfnRefCtor (object)

    // Unknown field: _pfnValueCtor (object)

    public bool _ctorIsPublic => _ctx.GetFieldValue<bool>(_objAddress, "_ctorIsPublic");

    public static new RuntimeType_ActivatorCache FromAddress(ulong address, DumpContext ctx)
        => new RuntimeType_ActivatorCache(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeType_ActivatorCache> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.RuntimeType+ActivatorCache"))
            yield return new RuntimeType_ActivatorCache(addr, ctx);
    }

    public override string ToString() => $"RuntimeType_ActivatorCache@0x{_objAddress:X}";
}
