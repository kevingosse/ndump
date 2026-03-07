#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public class OSFileStreamStrategy : _.System.IO.Strategies.FileStreamStrategy
{
    protected OSFileStreamStrategy(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.Microsoft.Win32.SafeHandles.SafeFileHandle? _fileHandle
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_fileHandle");
            return addr == 0 ? null : _.Microsoft.Win32.SafeHandles.SafeFileHandle.FromAddress(addr, _ctx);
        }
    }

    public int _access => _ctx.GetFieldValue<int>(_objAddress, "_access");

    public long _filePosition => _ctx.GetFieldValue<long>(_objAddress, "_filePosition");

    public long _appendStart => _ctx.GetFieldValue<long>(_objAddress, "_appendStart");

    public static new OSFileStreamStrategy FromAddress(ulong address, DumpContext ctx)
        => new OSFileStreamStrategy(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<OSFileStreamStrategy> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.Strategies.OSFileStreamStrategy"))
            yield return new OSFileStreamStrategy(addr, ctx);
    }

    public override string ToString() => $"OSFileStreamStrategy@0x{_objAddress:X}";
}
