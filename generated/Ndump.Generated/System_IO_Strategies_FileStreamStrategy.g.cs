#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public class FileStreamStrategy : _.System.IO.Stream
{
    protected FileStreamStrategy(ulong address, DumpContext context) : base(address, context) { }

    public bool IsDerived => Field<bool>("<IsDerived>k__BackingField");

    public static new FileStreamStrategy FromAddress(ulong address, DumpContext context)
        => new FileStreamStrategy(address, context);

    public static new global::System.Collections.Generic.IEnumerable<FileStreamStrategy> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.Strategies.FileStreamStrategy"))
            yield return new FileStreamStrategy(addr, context);
    }

    public override string ToString() => $"FileStreamStrategy@0x{_objAddress:X}";
}
