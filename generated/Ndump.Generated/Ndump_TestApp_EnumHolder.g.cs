#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class EnumHolder : _.System.Object
{
    private EnumHolder(ulong address, DumpContext context) : base(address, context) { }

    public int _color => Field<int>();

    public byte _priority => Field<byte>();

    public int _permissions => Field<int>();

    public static new EnumHolder FromAddress(ulong address, DumpContext context)
        => new EnumHolder(address, context);

    public static new global::System.Collections.Generic.IEnumerable<EnumHolder> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.EnumHolder"))
            yield return new EnumHolder(addr, context);
    }

    public override string ToString() => $"EnumHolder@0x{_objAddress:X}";
}
