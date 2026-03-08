#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class ArrayHolder : _.System.Object
{
    private ArrayHolder(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::Ndump.Core.DumpArray<int>? _intArray => ArrayField<int>();

    public global::Ndump.Core.DumpArray<byte>? _byteArray => ArrayField<byte>();

    // Array field: _doubleArray (object) — element type not supported

    // Array field: _boolArray (object) — element type not supported

    public global::Ndump.Core.DumpArray<int>? _nullArray => ArrayField<int>();

    public global::Ndump.Core.DumpArray<string?>? _emptyStringArray => ArrayField<string?>();

    public static new ArrayHolder FromAddress(ulong address, DumpContext ctx)
        => new ArrayHolder(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ArrayHolder> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.ArrayHolder"))
            yield return new ArrayHolder(addr, ctx);
    }

    public override string ToString() => $"ArrayHolder@0x{_objAddress:X}";
}
