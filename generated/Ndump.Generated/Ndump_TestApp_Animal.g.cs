#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public class Animal : _.System.Object
{
    protected Animal(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _name => StringField();

    public int _age => Field<int>();

    public static new Animal FromAddress(ulong address, DumpContext ctx)
        => new Animal(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Animal> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Animal"))
            yield return new Animal(addr, ctx);
    }

    public override string ToString() => $"Animal@0x{_objAddress:X}";
}
