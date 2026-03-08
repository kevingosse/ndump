#nullable enable
using Ndump.Core;

namespace _.System.Runtime.Serialization;

public sealed class DeserializationTracker : _.System.Object
{
    private DeserializationTracker(ulong address, DumpContext context) : base(address, context) { }

    public bool DeserializationInProgress => Field<bool>("<DeserializationInProgress>k__BackingField");

    public static new DeserializationTracker FromAddress(ulong address, DumpContext context)
        => new DeserializationTracker(address, context);

    public static new global::System.Collections.Generic.IEnumerable<DeserializationTracker> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Runtime.Serialization.DeserializationTracker"))
            yield return new DeserializationTracker(addr, context);
    }

    public override string ToString() => $"DeserializationTracker@0x{_objAddress:X}";
}
