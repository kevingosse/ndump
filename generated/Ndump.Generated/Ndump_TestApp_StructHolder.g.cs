#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class StructHolder : _.System.Object
{
    private StructHolder(ulong address, DumpContext context) : base(address, context) { }

    public _.Ndump.TestApp.Point _position => StructField<_.Ndump.TestApp.Point>("Ndump.TestApp.Point");

    public _.Ndump.TestApp.Rectangle _bounds => StructField<_.Ndump.TestApp.Rectangle>("Ndump.TestApp.Rectangle");

    public _.Ndump.TestApp.Label _label => StructField<_.Ndump.TestApp.Label>("Ndump.TestApp.Label");

    public static new StructHolder FromAddress(ulong address, DumpContext context)
        => new StructHolder(address, context);

    public static new global::System.Collections.Generic.IEnumerable<StructHolder> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.StructHolder"))
            yield return new StructHolder(addr, context);
    }

    public override string ToString() => $"StructHolder@0x{_objAddress:X}";
}
