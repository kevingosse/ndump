#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class ArrayHolder : _.System.Object
{
    private ArrayHolder(ulong address, DumpContext context) : base(address, context) { }

    public global::Ndump.Core.DumpArray<int>? _intArray => ArrayField<int>();

    public global::Ndump.Core.DumpArray<byte>? _byteArray => ArrayField<byte>();

    public global::Ndump.Core.DumpArray<double>? _doubleArray => ArrayField<double>();

    public global::Ndump.Core.DumpArray<bool>? _boolArray => ArrayField<bool>();

    public global::Ndump.Core.DumpArray<int>? _nullArray => ArrayField<int>();

    public global::Ndump.Core.DumpArray<string?>? _emptyStringArray => ArrayField<string?>();

    public static new ArrayHolder FromAddress(ulong address, DumpContext context)
        => new ArrayHolder(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ArrayHolder> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.ArrayHolder"))
            yield return new ArrayHolder(addr, context);
    }

    public override string ToString() => $"ArrayHolder@0x{_objAddress:X}";
}
