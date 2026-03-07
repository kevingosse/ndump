#nullable enable
using Ndump.Core;

namespace _.System;

public partial class RuntimeType
{
    public sealed class ActivatorCache : _.System.Object
    {
        private ActivatorCache(ulong address, DumpContext ctx) : base(address, ctx) { }

        // Unknown field: _pfnAllocator (object)

        // Unknown field: _allocatorFirstArg (object)

        // Unknown field: _pfnRefCtor (object)

        // Unknown field: _pfnValueCtor (object)

        public bool _ctorIsPublic => Field<bool>();

        public static new ActivatorCache FromAddress(ulong address, DumpContext ctx)
            => new ActivatorCache(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<ActivatorCache> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.RuntimeType+ActivatorCache"))
                yield return new ActivatorCache(addr, ctx);
        }

        public override string ToString() => $"ActivatorCache@0x{_objAddress:X}";
    }

}
