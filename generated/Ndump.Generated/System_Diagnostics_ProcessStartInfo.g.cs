#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics;

public sealed class ProcessStartInfo : _.System.Object
{
    private ProcessStartInfo(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _fileName => StringField();

    public string? _arguments => StringField();

    public string? _directory => StringField();

    public string? _userName => StringField();

    public string? _verb => StringField();

    // Unknown field: _argumentList (object)

    public int _windowStyle => Field<int>();

    public ulong _environmentVariables => RefAddress();

    public bool CreateNoWindow => Field<bool>("<CreateNoWindow>k__BackingField");

    public bool RedirectStandardInput => Field<bool>("<RedirectStandardInput>k__BackingField");

    public bool RedirectStandardOutput => Field<bool>("<RedirectStandardOutput>k__BackingField");

    public bool RedirectStandardError => Field<bool>("<RedirectStandardError>k__BackingField");

    public _.System.Text.Encoding? StandardInputEncoding
    {
        get
        {
            var addr = RefAddress("<StandardInputEncoding>k__BackingField");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Encoding? StandardErrorEncoding
    {
        get
        {
            var addr = RefAddress("<StandardErrorEncoding>k__BackingField");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Encoding? StandardOutputEncoding
    {
        get
        {
            var addr = RefAddress("<StandardOutputEncoding>k__BackingField");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public bool ErrorDialog => Field<bool>("<ErrorDialog>k__BackingField");

    public nint ErrorDialogParentHandle => Field<nint>("<ErrorDialogParentHandle>k__BackingField");

    public string? _domain => StringField();

    public string? PasswordInClearText => StringField("<PasswordInClearText>k__BackingField");

    public bool LoadUserProfile => Field<bool>("<LoadUserProfile>k__BackingField");

    public bool UseCredentialsForNetworkingOnly => Field<bool>("<UseCredentialsForNetworkingOnly>k__BackingField");

    // ValueType field: <Password>k__BackingField (object) — not yet supported

    public bool CreateNewProcessGroup => Field<bool>("<CreateNewProcessGroup>k__BackingField");

    public bool UseShellExecute => Field<bool>("<UseShellExecute>k__BackingField");

    public static new ProcessStartInfo FromAddress(ulong address, DumpContext ctx)
        => new ProcessStartInfo(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ProcessStartInfo> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.ProcessStartInfo"))
            yield return new ProcessStartInfo(addr, ctx);
    }

    public override string ToString() => $"ProcessStartInfo@0x{_objAddress:X}";
}
