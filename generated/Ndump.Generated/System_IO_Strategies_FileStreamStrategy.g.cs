#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public class FileStreamStrategy : _.System.IO.Stream
{
    protected FileStreamStrategy(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool IsDerived => Field<bool>("<IsDerived>k__BackingField");

    public static new FileStreamStrategy FromAddress(ulong address, DumpContext ctx)
        => new FileStreamStrategy(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<FileStreamStrategy> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.Strategies.FileStreamStrategy"))
            yield return new FileStreamStrategy(addr, ctx);
    }

    public override string ToString() => $"FileStreamStrategy@0x{_objAddress:X}";
}
