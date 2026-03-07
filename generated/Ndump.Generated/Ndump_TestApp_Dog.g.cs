#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Dog : _.Ndump.TestApp.Animal
{
    private Dog(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _breed => StringField();

    public static new Dog FromAddress(ulong address, DumpContext ctx)
        => new Dog(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Dog> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Dog"))
            yield return new Dog(addr, ctx);
    }

    public override string ToString() => $"Dog@0x{_objAddress:X}";
}
