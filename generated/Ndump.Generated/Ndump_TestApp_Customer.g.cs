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

    public global::Ndump.Core.DumpArray<_.Ndump.TestApp.Order?>? _orderHistory
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<_.Ndump.TestApp.Order?>(addr, len, i => { var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : _.Ndump.TestApp.Order.FromAddress(ea, _ctx); });
        }
    }

    public global::Ndump.Core.DumpArray<_.System.Object?>? _mixedItems
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<_.System.Object?>(addr, len, i => { var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : global::_.ProxyResolver.Resolve(ea, _ctx) as _.System.Object ?? _.System.Object.FromAddress(ea, _ctx); });
        }
    }

    public global::Ndump.Core.DumpArray<_.Ndump.TestApp.Animal?>? _pets
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<_.Ndump.TestApp.Animal?>(addr, len, i => { var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : global::_.ProxyResolver.Resolve(ea, _ctx) as _.Ndump.TestApp.Animal ?? _.Ndump.TestApp.Animal.FromAddress(ea, _ctx); });
        }
    }

    public global::Ndump.Core.DumpArray<string?>? _tags
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<string?>(addr, len, i => _ctx.GetArrayElementString(addr, i));
        }
    }

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
