#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class Dictionary_System_String__System_Object_ : _.System.Object
{
    private Dictionary_System_String__System_Object_(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::Ndump.Core.DumpArray<int>? _buckets
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<int>(addr, len, i => _ctx.GetArrayElementValue<int>(addr, i));
        }
    }

    // Array field: _entries (System.Collections.Generic.Dictionary`2+Entry[]) — element type not supported

    public ulong _fastModMultiplier => Field<ulong>();

    public int _count => Field<int>();

    public int _freeList => Field<int>();

    public int _freeCount => Field<int>();

    public int _version => Field<int>();

    public ulong _comparer => RefAddress();

    // Unknown field: _keys (object)

    // Unknown field: _values (object)

    public static new Dictionary_System_String__System_Object_ FromAddress(ulong address, DumpContext ctx)
        => new Dictionary_System_String__System_Object_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Dictionary_System_String__System_Object_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.Dictionary<System.String, System.Object>"))
            yield return new Dictionary_System_String__System_Object_(addr, ctx);
    }

    public override string ToString() => $"Dictionary_System_String__System_Object_@0x{_objAddress:X}";
}
