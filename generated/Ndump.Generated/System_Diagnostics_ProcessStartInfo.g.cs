#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics;

public sealed class ProcessStartInfo : _.System.Object
{
    private ProcessStartInfo(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _fileName => _ctx.GetStringField(_objAddress, "_fileName");

    public string? _arguments => _ctx.GetStringField(_objAddress, "_arguments");

    public string? _directory => _ctx.GetStringField(_objAddress, "_directory");

    public string? _userName => _ctx.GetStringField(_objAddress, "_userName");

    public string? _verb => _ctx.GetStringField(_objAddress, "_verb");

    // Unknown field: _argumentList (object)

    public int _windowStyle => _ctx.GetFieldValue<int>(_objAddress, "_windowStyle");

    public ulong _environmentVariables => _ctx.GetObjectAddress(_objAddress, "_environmentVariables");

    public bool CreateNoWindow => _ctx.GetFieldValue<bool>(_objAddress, "<CreateNoWindow>k__BackingField");

    public bool RedirectStandardInput => _ctx.GetFieldValue<bool>(_objAddress, "<RedirectStandardInput>k__BackingField");

    public bool RedirectStandardOutput => _ctx.GetFieldValue<bool>(_objAddress, "<RedirectStandardOutput>k__BackingField");

    public bool RedirectStandardError => _ctx.GetFieldValue<bool>(_objAddress, "<RedirectStandardError>k__BackingField");

    public _.System.Text.Encoding? StandardInputEncoding
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "<StandardInputEncoding>k__BackingField");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Encoding? StandardErrorEncoding
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "<StandardErrorEncoding>k__BackingField");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Encoding? StandardOutputEncoding
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "<StandardOutputEncoding>k__BackingField");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public bool ErrorDialog => _ctx.GetFieldValue<bool>(_objAddress, "<ErrorDialog>k__BackingField");

    public nint ErrorDialogParentHandle => _ctx.GetFieldValue<nint>(_objAddress, "<ErrorDialogParentHandle>k__BackingField");

    public string? _domain => _ctx.GetStringField(_objAddress, "_domain");

    public string? PasswordInClearText => _ctx.GetStringField(_objAddress, "<PasswordInClearText>k__BackingField");

    public bool LoadUserProfile => _ctx.GetFieldValue<bool>(_objAddress, "<LoadUserProfile>k__BackingField");

    public bool UseCredentialsForNetworkingOnly => _ctx.GetFieldValue<bool>(_objAddress, "<UseCredentialsForNetworkingOnly>k__BackingField");

    // ValueType field: <Password>k__BackingField (object) — not yet supported

    public bool CreateNewProcessGroup => _ctx.GetFieldValue<bool>(_objAddress, "<CreateNewProcessGroup>k__BackingField");

    public bool UseShellExecute => _ctx.GetFieldValue<bool>(_objAddress, "<UseShellExecute>k__BackingField");

    public static new ProcessStartInfo FromAddress(ulong address, DumpContext ctx)
        => new ProcessStartInfo(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ProcessStartInfo> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.ProcessStartInfo"))
            yield return new ProcessStartInfo(addr, ctx);
    }

    public override string ToString() => $"ProcessStartInfo@0x{_objAddress:X}";
}
