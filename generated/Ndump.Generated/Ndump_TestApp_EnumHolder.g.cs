#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class EnumHolder : _.System.Object
{
    private EnumHolder(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int _color => Field<int>();

    public byte _priority => Field<byte>();

    public int _permissions => Field<int>();

    public static new EnumHolder FromAddress(ulong address, DumpContext ctx)
        => new EnumHolder(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EnumHolder> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.EnumHolder"))
            yield return new EnumHolder(addr, ctx);
    }

    public override string ToString() => $"EnumHolder@0x{_objAddress:X}";
}
