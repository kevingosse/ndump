#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Customer : _.System.Object
{
    private Customer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _name => Field<string>();

    public int _age => Field<int>();

    public bool _isActive => Field<bool>();

    public _.Ndump.TestApp.Order? _lastOrder => Field<_.Ndump.TestApp.Order>();

    public _.Ndump.TestApp.Address? _address => Field<_.Ndump.TestApp.Address>();

    public global::Ndump.Core.DumpArray<_.Ndump.TestApp.Order?>? _orderHistory => ArrayField<_.Ndump.TestApp.Order?>();

    public global::Ndump.Core.DumpArray<_.System.Object?>? _mixedItems => ArrayField<_.System.Object?>();

    public global::Ndump.Core.DumpArray<_.Ndump.TestApp.Animal?>? _pets => ArrayField<_.Ndump.TestApp.Animal?>();

    public global::Ndump.Core.DumpArray<string?>? _tags => ArrayField<string?>();

    public _.System.Collections.Generic.Dictionary<string, int>? _scores => Field<_.System.Collections.Generic.Dictionary<string, int>>();

    public static new Customer FromAddress(ulong address, DumpContext ctx)
        => new Customer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Customer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Customer"))
            yield return new Customer(addr, ctx);
    }

    public override string ToString() => $"Customer@0x{_objAddress:X}";
}
