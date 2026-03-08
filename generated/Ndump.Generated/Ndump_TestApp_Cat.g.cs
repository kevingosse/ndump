#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Cat : _.Ndump.TestApp.Animal
{
    private Cat(ulong address, DumpContext context) : base(address, context) { }

    public bool _isIndoor => Field<bool>();

    public static new Cat FromAddress(ulong address, DumpContext context)
        => new Cat(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Cat> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Cat"))
            yield return new Cat(addr, context);
    }

    public override string ToString() => $"Cat@0x{_objAddress:X}";
}
