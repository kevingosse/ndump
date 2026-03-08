#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics;

public sealed class Process : _.System.ComponentModel.Component
{
    private Process(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _haveProcessId => Field<bool>();

    public int _processId => Field<int>();

    public bool _haveProcessHandle => Field<bool>();

    public _.Microsoft.Win32.SafeHandles.SafeProcessHandle? _processHandle => Field<_.Microsoft.Win32.SafeHandles.SafeProcessHandle>();

    public bool _isRemoteMachine => Field<bool>();

    public string? _machineName => Field<string>();

    public global::_.System.Object? _processInfo => Field<global::_.System.Object>();

    public global::_.System.Object? _threads => Field<global::_.System.Object>();

    public global::_.System.Object? _modules => Field<global::_.System.Object>();

    public bool _haveWorkingSetLimits => Field<bool>();

    public nint _minWorkingSet => Field<nint>();

    public nint _maxWorkingSet => Field<nint>();

    public bool _haveProcessorAffinity => Field<bool>();

    public nint _processorAffinity => Field<nint>();

    public bool _havePriorityClass => Field<bool>();

    public int _priorityClass => Field<int>();

    public _.System.Diagnostics.ProcessStartInfo? _startInfo => Field<_.System.Diagnostics.ProcessStartInfo>();

    public bool _watchForExit => Field<bool>();

    public bool _watchingForExit => Field<bool>();

    public ulong _onExited => RawFieldAddress();

    public bool _exited => Field<bool>();

    public int _exitCode => Field<int>();

    public _.System.DateTime? _startTime => NullableStructField<_.System.DateTime>("System.DateTime");

    public _.System.DateTime _exitTime => StructField<_.System.DateTime>("System.DateTime");

    public bool _haveExitTime => Field<bool>();

    public bool _priorityBoostEnabled => Field<bool>();

    public bool _havePriorityBoostEnabled => Field<bool>();

    public bool _raisedOnExited => Field<bool>();

    public ulong _registeredWaitHandle => RawFieldAddress();

    public _.System.Threading.WaitHandle? _waitHandle => Field<_.System.Threading.WaitHandle>();

    public _.System.IO.StreamReader? _standardOutput => Field<_.System.IO.StreamReader>();

    public global::_.System.Object? _standardInput => Field<global::_.System.Object>();

    public _.System.IO.StreamReader? _standardError => Field<_.System.IO.StreamReader>();

    public bool _disposed => Field<bool>();

    public bool _standardInputAccessed => Field<bool>();

    public int _outputStreamReadMode => Field<int>();

    public int _errorStreamReadMode => Field<int>();

    public global::_.System.Object? OutputDataReceived => Field<global::_.System.Object>();

    public global::_.System.Object? ErrorDataReceived => Field<global::_.System.Object>();

    public global::_.System.Object? _output => Field<global::_.System.Object>();

    public global::_.System.Object? _error => Field<global::_.System.Object>();

    public bool _pendingOutputRead => Field<bool>();

    public bool _pendingErrorRead => Field<bool>();

    public ulong SynchronizingObject => RawFieldAddress("<SynchronizingObject>k__BackingField");

    public string? _processName => Field<string>();

    public bool _signaled => Field<bool>();

    public bool _haveMainWindow => Field<bool>();

    public nint _mainWindowHandle => Field<nint>();

    public string? _mainWindowTitle => Field<string>();

    public bool _haveResponding => Field<bool>();

    public bool _responding => Field<bool>();

    public static new Process FromAddress(ulong address, DumpContext ctx)
        => new Process(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Process> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Process"))
            yield return new Process(addr, ctx);
    }

    public override string ToString() => $"Process@0x{_objAddress:X}";
}
