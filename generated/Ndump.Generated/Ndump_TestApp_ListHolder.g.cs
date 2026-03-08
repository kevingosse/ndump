#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class ListHolder : _.System.Object
{
    private ListHolder(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Collections.Generic.List<string>? _items => Field<_.System.Collections.Generic.List<string>>();

    public _.System.Collections.Generic.List<_.Ndump.TestApp.Order>? _orders => Field<_.System.Collections.Generic.List<_.Ndump.TestApp.Order>>();

    public static new ListHolder FromAddress(ulong address, DumpContext context)
        => new ListHolder(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ListHolder> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.ListHolder"))
            yield return new ListHolder(addr, context);
    }

    public override string ToString() => $"ListHolder@0x{_objAddress:X}";
}
