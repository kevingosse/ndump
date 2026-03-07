#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public sealed class SafeFileHandle : _.Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeFileHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _path => _ctx.GetStringField(_objAddress, "_path");

    public long _length => _ctx.GetFieldValue<long>(_objAddress, "_length");

    public bool _lengthCanBeCached => _ctx.GetFieldValue<bool>(_objAddress, "_lengthCanBeCached");

    public int _fileOptions => _ctx.GetFieldValue<int>(_objAddress, "_fileOptions");

    public int _fileType => _ctx.GetFieldValue<int>(_objAddress, "_fileType");

    public ulong ThreadPoolBinding => _ctx.GetObjectAddress(_objAddress, "<ThreadPoolBinding>k__BackingField");

    public ulong _reusableOverlappedValueTaskSource => _ctx.GetObjectAddress(_objAddress, "_reusableOverlappedValueTaskSource");

    public static new SafeFileHandle FromAddress(ulong address, DumpContext ctx)
        => new SafeFileHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SafeFileHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeFileHandle"))
            yield return new SafeFileHandle(addr, ctx);
    }

    public override string ToString() => $"SafeFileHandle@0x{_objAddress:X}";
}
