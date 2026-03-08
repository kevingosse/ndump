#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class NullableHolder : _.System.Object
{
    private NullableHolder(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int? _intHasValue => NullableField<int>();

    public int? _intNull => NullableField<int>();

    public double? _doubleHasValue => NullableField<double>();

    public double? _doubleNull => NullableField<double>();

    public bool? _boolHasValue => NullableField<bool>();

    public bool? _boolNull => NullableField<bool>();

    public long? _longHasValue => NullableField<long>();

    public long? _longNull => NullableField<long>();

    public static new NullableHolder FromAddress(ulong address, DumpContext ctx)
        => new NullableHolder(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<NullableHolder> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.NullableHolder"))
            yield return new NullableHolder(addr, ctx);
    }

    public override string ToString() => $"NullableHolder@0x{_objAddress:X}";
}
