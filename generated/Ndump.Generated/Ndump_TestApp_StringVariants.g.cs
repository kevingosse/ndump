#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class StringVariants : _.System.Object
{
    private StringVariants(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _normal => Field<string>();

    public string? _nullString => Field<string>();

    public string? _empty => Field<string>();

    public string? _unicode => Field<string>();

    public string? _long => Field<string>();

    public static new StringVariants FromAddress(ulong address, DumpContext ctx)
        => new StringVariants(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StringVariants> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.StringVariants"))
            yield return new StringVariants(addr, ctx);
    }

    public override string ToString() => $"StringVariants@0x{_objAddress:X}";
}
