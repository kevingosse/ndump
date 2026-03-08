#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class NullableHolder : _.System.Object
{
    private NullableHolder(ulong address, DumpContext context) : base(address, context) { }

    public int? _intHasValue => NullableField<int>();

    public int? _intNull => NullableField<int>();

    public double? _doubleHasValue => NullableField<double>();

    public double? _doubleNull => NullableField<double>();

    public bool? _boolHasValue => NullableField<bool>();

    public bool? _boolNull => NullableField<bool>();

    public long? _longHasValue => NullableField<long>();

    public long? _longNull => NullableField<long>();

    public static new NullableHolder FromAddress(ulong address, DumpContext context)
        => new NullableHolder(address, context);

    public static new global::System.Collections.Generic.IEnumerable<NullableHolder> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.NullableHolder"))
            yield return new NullableHolder(addr, context);
    }

    public override string ToString() => $"NullableHolder@0x{_objAddress:X}";
}
