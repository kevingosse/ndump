#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics;

public sealed class Process : _.System.ComponentModel.Component
{
    private Process(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _haveProcessId => _ctx.GetFieldValue<bool>(_objAddress, "_haveProcessId");

    public int _processId => _ctx.GetFieldValue<int>(_objAddress, "_processId");

    public bool _haveProcessHandle => _ctx.GetFieldValue<bool>(_objAddress, "_haveProcessHandle");

    public _.Microsoft.Win32.SafeHandles.SafeProcessHandle? _processHandle
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_processHandle");
            return addr == 0 ? null : _.Microsoft.Win32.SafeHandles.SafeProcessHandle.FromAddress(addr, _ctx);
        }
    }

    public bool _isRemoteMachine => _ctx.GetFieldValue<bool>(_objAddress, "_isRemoteMachine");

    public string? _machineName => _ctx.GetStringField(_objAddress, "_machineName");

    public ulong _processInfo => _ctx.GetObjectAddress(_objAddress, "_processInfo");

    public ulong _threads => _ctx.GetObjectAddress(_objAddress, "_threads");

    public ulong _modules => _ctx.GetObjectAddress(_objAddress, "_modules");

    public bool _haveWorkingSetLimits => _ctx.GetFieldValue<bool>(_objAddress, "_haveWorkingSetLimits");

    public nint _minWorkingSet => _ctx.GetFieldValue<nint>(_objAddress, "_minWorkingSet");

    public nint _maxWorkingSet => _ctx.GetFieldValue<nint>(_objAddress, "_maxWorkingSet");

    public bool _haveProcessorAffinity => _ctx.GetFieldValue<bool>(_objAddress, "_haveProcessorAffinity");

    public nint _processorAffinity => _ctx.GetFieldValue<nint>(_objAddress, "_processorAffinity");

    public bool _havePriorityClass => _ctx.GetFieldValue<bool>(_objAddress, "_havePriorityClass");

    public int _priorityClass => _ctx.GetFieldValue<int>(_objAddress, "_priorityClass");

    public _.System.Diagnostics.ProcessStartInfo? _startInfo
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_startInfo");
            return addr == 0 ? null : _.System.Diagnostics.ProcessStartInfo.FromAddress(addr, _ctx);
        }
    }

    public bool _watchForExit => _ctx.GetFieldValue<bool>(_objAddress, "_watchForExit");

    public bool _watchingForExit => _ctx.GetFieldValue<bool>(_objAddress, "_watchingForExit");

    // ValueType field: _onExited (object) — not yet supported

    public bool _exited => _ctx.GetFieldValue<bool>(_objAddress, "_exited");

    public int _exitCode => _ctx.GetFieldValue<int>(_objAddress, "_exitCode");

    // ValueType field: _startTime (object) — not yet supported

    // ValueType field: _exitTime (object) — not yet supported

    public bool _haveExitTime => _ctx.GetFieldValue<bool>(_objAddress, "_haveExitTime");

    public bool _priorityBoostEnabled => _ctx.GetFieldValue<bool>(_objAddress, "_priorityBoostEnabled");

    public bool _havePriorityBoostEnabled => _ctx.GetFieldValue<bool>(_objAddress, "_havePriorityBoostEnabled");

    public bool _raisedOnExited => _ctx.GetFieldValue<bool>(_objAddress, "_raisedOnExited");

    // ValueType field: _registeredWaitHandle (object) — not yet supported

    public _.System.Threading.WaitHandle? _waitHandle
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_waitHandle");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Threading.WaitHandle ?? _.System.Threading.WaitHandle.FromAddress(addr, _ctx);
        }
    }

    public _.System.IO.StreamReader? _standardOutput
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_standardOutput");
            return addr == 0 ? null : _.System.IO.StreamReader.FromAddress(addr, _ctx);
        }
    }

    public ulong _standardInput => _ctx.GetObjectAddress(_objAddress, "_standardInput");

    public _.System.IO.StreamReader? _standardError
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_standardError");
            return addr == 0 ? null : _.System.IO.StreamReader.FromAddress(addr, _ctx);
        }
    }

    public bool _disposed => _ctx.GetFieldValue<bool>(_objAddress, "_disposed");

    public bool _standardInputAccessed => _ctx.GetFieldValue<bool>(_objAddress, "_standardInputAccessed");

    public int _outputStreamReadMode => _ctx.GetFieldValue<int>(_objAddress, "_outputStreamReadMode");

    public int _errorStreamReadMode => _ctx.GetFieldValue<int>(_objAddress, "_errorStreamReadMode");

    public ulong OutputDataReceived => _ctx.GetObjectAddress(_objAddress, "OutputDataReceived");

    public ulong ErrorDataReceived => _ctx.GetObjectAddress(_objAddress, "ErrorDataReceived");

    public ulong _output => _ctx.GetObjectAddress(_objAddress, "_output");

    public ulong _error => _ctx.GetObjectAddress(_objAddress, "_error");

    public bool _pendingOutputRead => _ctx.GetFieldValue<bool>(_objAddress, "_pendingOutputRead");

    public bool _pendingErrorRead => _ctx.GetFieldValue<bool>(_objAddress, "_pendingErrorRead");

    // ValueType field: <SynchronizingObject>k__BackingField (object) — not yet supported

    public string? _processName => _ctx.GetStringField(_objAddress, "_processName");

    public bool _signaled => _ctx.GetFieldValue<bool>(_objAddress, "_signaled");

    public bool _haveMainWindow => _ctx.GetFieldValue<bool>(_objAddress, "_haveMainWindow");

    public nint _mainWindowHandle => _ctx.GetFieldValue<nint>(_objAddress, "_mainWindowHandle");

    public string? _mainWindowTitle => _ctx.GetStringField(_objAddress, "_mainWindowTitle");

    public bool _haveResponding => _ctx.GetFieldValue<bool>(_objAddress, "_haveResponding");

    public bool _responding => _ctx.GetFieldValue<bool>(_objAddress, "_responding");

    public static new Process FromAddress(ulong address, DumpContext ctx)
        => new Process(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Process> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Process"))
            yield return new Process(addr, ctx);
    }

    public override string ToString() => $"Process@0x{_objAddress:X}";
}
