#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics;

public sealed class ProcessStartInfo : _.System.Object
{
    private ProcessStartInfo(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _fileName => Field<string>();

    public string? _arguments => Field<string>();

    public string? _directory => Field<string>();

    public string? _userName => Field<string>();

    public string? _verb => Field<string>();

    public global::_.System.Object? _argumentList => Field<global::_.System.Object>();

    public int _windowStyle => Field<int>();

    public global::_.System.Object? _environmentVariables => Field<global::_.System.Object>();

    public bool CreateNoWindow => Field<bool>("<CreateNoWindow>k__BackingField");

    public bool RedirectStandardInput => Field<bool>("<RedirectStandardInput>k__BackingField");

    public bool RedirectStandardOutput => Field<bool>("<RedirectStandardOutput>k__BackingField");

    public bool RedirectStandardError => Field<bool>("<RedirectStandardError>k__BackingField");

    public _.System.Text.Encoding? StandardInputEncoding => Field<_.System.Text.Encoding>("<StandardInputEncoding>k__BackingField");

    public _.System.Text.Encoding? StandardErrorEncoding => Field<_.System.Text.Encoding>("<StandardErrorEncoding>k__BackingField");

    public _.System.Text.Encoding? StandardOutputEncoding => Field<_.System.Text.Encoding>("<StandardOutputEncoding>k__BackingField");

    public bool ErrorDialog => Field<bool>("<ErrorDialog>k__BackingField");

    public nint ErrorDialogParentHandle => Field<nint>("<ErrorDialogParentHandle>k__BackingField");

    public string? _domain => Field<string>();

    public string? PasswordInClearText => Field<string>("<PasswordInClearText>k__BackingField");

    public bool LoadUserProfile => Field<bool>("<LoadUserProfile>k__BackingField");

    public bool UseCredentialsForNetworkingOnly => Field<bool>("<UseCredentialsForNetworkingOnly>k__BackingField");

    // ValueType field: <Password>k__BackingField (System.Void) — no proxy available

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
