#nullable enable
using Ndump.Core;

namespace _.System;

public partial class RuntimeType
{
    public sealed class ActivatorCache : _.System.Object
    {
        private ActivatorCache(ulong address, DumpContext context) : base(address, context) { }

        // Unknown field: _pfnAllocator (object)

        // Unknown field: _allocatorFirstArg (object)

        // Unknown field: _pfnRefCtor (object)

        // Unknown field: _pfnValueCtor (object)

        public bool _ctorIsPublic => Field<bool>();

        public static new ActivatorCache FromAddress(ulong address, DumpContext context)
            => new ActivatorCache(address, context);

        public static new global::System.Collections.Generic.IEnumerable<ActivatorCache> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.RuntimeType+ActivatorCache"))
                yield return new ActivatorCache(addr, context);
        }

        public override string ToString() => $"ActivatorCache@0x{_objAddress:X}";
    }

}
