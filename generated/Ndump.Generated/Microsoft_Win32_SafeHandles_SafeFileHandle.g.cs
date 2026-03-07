#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public sealed class SafeFileHandle : _.Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeFileHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _path => Field<string>();

    public long _length => Field<long>();

    public bool _lengthCanBeCached => Field<bool>();

    public int _fileOptions => Field<int>();

    public int _fileType => Field<int>();

    public global::_.System.Object? ThreadPoolBinding => Field<global::_.System.Object>("<ThreadPoolBinding>k__BackingField");

    public global::_.System.Object? _reusableOverlappedValueTaskSource => Field<global::_.System.Object>();

    public static new SafeFileHandle FromAddress(ulong address, DumpContext ctx)
        => new SafeFileHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SafeFileHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeFileHandle"))
            yield return new SafeFileHandle(addr, ctx);
    }

    public override string ToString() => $"SafeFileHandle@0x{_objAddress:X}";
}
