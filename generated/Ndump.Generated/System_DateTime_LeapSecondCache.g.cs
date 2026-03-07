#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class DateTime_LeapSecondCache : _.System.Object
{
    private DateTime_LeapSecondCache(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong OSFileTimeTicksAtStartOfValidityWindow => _ctx.GetFieldValue<ulong>(_objAddress, "OSFileTimeTicksAtStartOfValidityWindow");

    public ulong DotnetDateDataAtStartOfValidityWindow => _ctx.GetFieldValue<ulong>(_objAddress, "DotnetDateDataAtStartOfValidityWindow");

    public static new DateTime_LeapSecondCache FromAddress(ulong address, DumpContext ctx)
        => new DateTime_LeapSecondCache(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<DateTime_LeapSecondCache> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.DateTime+LeapSecondCache"))
            yield return new DateTime_LeapSecondCache(addr, ctx);
    }

    public override string ToString() => $"DateTime_LeapSecondCache@0x{_objAddress:X}";
}
