#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Order : _.System.Object
{
    private Order(ulong address, DumpContext context) : base(address, context) { }

    public int _orderId => Field<int>();

    public double _total => Field<double>();

    public string? _description => Field<string>();

    public _.System.DateTime _createdAt => StructField<_.System.DateTime>("System.DateTime");

    public _.System.DateTime? _shippedAt => NullableStructField<_.System.DateTime>("System.DateTime");

    public int? _rating => NullableField<int>();

    public static new Order FromAddress(ulong address, DumpContext context)
        => new Order(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Order> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Order"))
            yield return new Order(addr, context);
    }

    public override string ToString() => $"Order@0x{_objAddress:X}";
}
