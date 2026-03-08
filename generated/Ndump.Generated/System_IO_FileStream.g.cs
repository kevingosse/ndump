#nullable enable
using Ndump.Core;

namespace _.System.IO;

public sealed class FileStream : _.System.IO.Stream
{
    private FileStream(ulong address, DumpContext context) : base(address, context) { }

    public _.System.IO.Strategies.FileStreamStrategy? _strategy => Field<_.System.IO.Strategies.FileStreamStrategy>();

    public static new FileStream FromAddress(ulong address, DumpContext context)
        => new FileStream(address, context);

    public static new global::System.Collections.Generic.IEnumerable<FileStream> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.FileStream"))
            yield return new FileStream(addr, context);
    }

    public override string ToString() => $"FileStream@0x{_objAddress:X}";
}
