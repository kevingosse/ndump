#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public sealed class BufferedFileStreamStrategy : _.System.IO.Strategies.FileStreamStrategy
{
    private BufferedFileStreamStrategy(ulong address, DumpContext context) : base(address, context) { }

    public _.System.IO.Strategies.FileStreamStrategy? _strategy => Field<_.System.IO.Strategies.FileStreamStrategy>();

    public int _bufferSize => Field<int>();

    public global::Ndump.Core.DumpArray<byte>? _buffer => ArrayField<byte>();

    public int _writePos => Field<int>();

    public int _readPos => Field<int>();

    public int _readLen => Field<int>();

    public _.System.Threading.Tasks.CachedCompletedInt32Task _lastSyncCompletedReadTask => StructField<_.System.Threading.Tasks.CachedCompletedInt32Task>("System.Threading.Tasks.CachedCompletedInt32Task");

    public static new BufferedFileStreamStrategy FromAddress(ulong address, DumpContext context)
        => new BufferedFileStreamStrategy(address, context);

    public static new global::System.Collections.Generic.IEnumerable<BufferedFileStreamStrategy> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.Strategies.BufferedFileStreamStrategy"))
            yield return new BufferedFileStreamStrategy(addr, context);
    }

    public override string ToString() => $"BufferedFileStreamStrategy@0x{_objAddress:X}";
}
