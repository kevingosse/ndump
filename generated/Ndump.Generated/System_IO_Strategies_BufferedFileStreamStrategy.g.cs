#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public sealed class BufferedFileStreamStrategy : _.System.IO.Strategies.FileStreamStrategy
{
    private BufferedFileStreamStrategy(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.IO.Strategies.FileStreamStrategy? _strategy => Field<_.System.IO.Strategies.FileStreamStrategy>();

    public int _bufferSize => Field<int>();

    public global::Ndump.Core.DumpArray<byte>? _buffer
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    public int _writePos => Field<int>();

    public int _readPos => Field<int>();

    public int _readLen => Field<int>();

    // ValueType field: _lastSyncCompletedReadTask (object) — not yet supported

    public static new BufferedFileStreamStrategy FromAddress(ulong address, DumpContext ctx)
        => new BufferedFileStreamStrategy(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<BufferedFileStreamStrategy> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.Strategies.BufferedFileStreamStrategy"))
            yield return new BufferedFileStreamStrategy(addr, ctx);
    }

    public override string ToString() => $"BufferedFileStreamStrategy@0x{_objAddress:X}";
}
