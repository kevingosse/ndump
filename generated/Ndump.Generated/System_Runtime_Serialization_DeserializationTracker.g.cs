#nullable enable
using Ndump.Core;

namespace _.System.Runtime.Serialization;

public sealed class DeserializationTracker : _.System.Object
{
    private DeserializationTracker(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool DeserializationInProgress => Field<bool>("<DeserializationInProgress>k__BackingField");

    public static new DeserializationTracker FromAddress(ulong address, DumpContext ctx)
        => new DeserializationTracker(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<DeserializationTracker> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Runtime.Serialization.DeserializationTracker"))
            yield return new DeserializationTracker(addr, ctx);
    }

    public override string ToString() => $"DeserializationTracker@0x{_objAddress:X}";
}
