#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics;

public sealed class Process : _.System.ComponentModel.Component
{
    private Process(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _haveProcessId => Field<bool>();

    public int _processId => Field<int>();

    public bool _haveProcessHandle => Field<bool>();

    public _.Microsoft.Win32.SafeHandles.SafeProcessHandle? _processHandle
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : _.Microsoft.Win32.SafeHandles.SafeProcessHandle.FromAddress(addr, _ctx);
        }
    }

    public bool _isRemoteMachine => Field<bool>();

    public string? _machineName => StringField();

    public ulong _processInfo => RefAddress();

    public ulong _threads => RefAddress();

    public ulong _modules => RefAddress();

    public bool _haveWorkingSetLimits => Field<bool>();

    public nint _minWorkingSet => Field<nint>();

    public nint _maxWorkingSet => Field<nint>();

    public bool _haveProcessorAffinity => Field<bool>();

    public nint _processorAffinity => Field<nint>();

    public bool _havePriorityClass => Field<bool>();

    public int _priorityClass => Field<int>();

    public _.System.Diagnostics.ProcessStartInfo? _startInfo
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : _.System.Diagnostics.ProcessStartInfo.FromAddress(addr, _ctx);
        }
    }

    public bool _watchForExit => Field<bool>();

    public bool _watchingForExit => Field<bool>();

    // ValueType field: _onExited (object) — not yet supported

    public bool _exited => Field<bool>();

    public int _exitCode => Field<int>();

    // ValueType field: _startTime (object) — not yet supported

    // ValueType field: _exitTime (object) — not yet supported

    public bool _haveExitTime => Field<bool>();

    public bool _priorityBoostEnabled => Field<bool>();

    public bool _havePriorityBoostEnabled => Field<bool>();

    public bool _raisedOnExited => Field<bool>();

    // ValueType field: _registeredWaitHandle (object) — not yet supported

    public _.System.Threading.WaitHandle? _waitHandle
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Threading.WaitHandle ?? _.System.Threading.WaitHandle.FromAddress(addr, _ctx);
        }
    }

    public _.System.IO.StreamReader? _standardOutput
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : _.System.IO.StreamReader.FromAddress(addr, _ctx);
        }
    }

    public ulong _standardInput => RefAddress();

    public _.System.IO.StreamReader? _standardError
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : _.System.IO.StreamReader.FromAddress(addr, _ctx);
        }
    }

    public bool _disposed => Field<bool>();

    public bool _standardInputAccessed => Field<bool>();

    public int _outputStreamReadMode => Field<int>();

    public int _errorStreamReadMode => Field<int>();

    public ulong OutputDataReceived => RefAddress();

    public ulong ErrorDataReceived => RefAddress();

    public ulong _output => RefAddress();

    public ulong _error => RefAddress();

    public bool _pendingOutputRead => Field<bool>();

    public bool _pendingErrorRead => Field<bool>();

    // ValueType field: <SynchronizingObject>k__BackingField (object) — not yet supported

    public string? _processName => StringField();

    public bool _signaled => Field<bool>();

    public bool _haveMainWindow => Field<bool>();

    public nint _mainWindowHandle => Field<nint>();

    public string? _mainWindowTitle => StringField();

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
