#nullable enable
using Ndump.Core;

namespace _.System.IO;

public sealed class FileStream : _.System.IO.Stream
{
    private FileStream(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.IO.Strategies.FileStreamStrategy? _strategy => Field<_.System.IO.Strategies.FileStreamStrategy>();

    public static new FileStream FromAddress(ulong address, DumpContext ctx)
        => new FileStream(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<FileStream> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.FileStream"))
            yield return new FileStream(addr, ctx);
    }

    public override string ToString() => $"FileStream@0x{_objAddress:X}";
}
