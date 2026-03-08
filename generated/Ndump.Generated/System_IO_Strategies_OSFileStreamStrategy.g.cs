#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public class OSFileStreamStrategy : _.System.IO.Strategies.FileStreamStrategy
{
    protected OSFileStreamStrategy(ulong address, DumpContext context) : base(address, context) { }

    public _.Microsoft.Win32.SafeHandles.SafeFileHandle? _fileHandle => Field<_.Microsoft.Win32.SafeHandles.SafeFileHandle>();

    public int _access => Field<int>();

    public long _filePosition => Field<long>();

    public long _appendStart => Field<long>();

    public static new OSFileStreamStrategy FromAddress(ulong address, DumpContext context)
        => new OSFileStreamStrategy(address, context);

    public static new global::System.Collections.Generic.IEnumerable<OSFileStreamStrategy> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.Strategies.OSFileStreamStrategy"))
            yield return new OSFileStreamStrategy(addr, context);
    }

    public override string ToString() => $"OSFileStreamStrategy@0x{_objAddress:X}";
}
