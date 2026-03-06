using Microsoft.Diagnostics.Runtime;

namespace Ndump.Core;

/// <summary>
/// Provides access to a memory dump's CLR heap via ClrMD.
/// Generated proxy types use this to read field values lazily.
/// </summary>
public sealed class DumpContext : IDisposable
{
    private readonly DataTarget _dataTarget;
    private readonly ClrRuntime _runtime;
    private bool _disposed;

    public ClrRuntime Runtime => _runtime;
    public ClrHeap Heap => _runtime.Heap;

    private DumpContext(DataTarget dataTarget, ClrRuntime runtime)
    {
        _dataTarget = dataTarget;
        _runtime = runtime;
    }

    public static DumpContext Open(string dumpPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dumpPath);
        if (!File.Exists(dumpPath))
            throw new FileNotFoundException("Dump file not found.", dumpPath);

        var dataTarget = DataTarget.LoadDump(dumpPath);
        var runtime = dataTarget.ClrVersions[0].CreateRuntime();
        return new DumpContext(dataTarget, runtime);
    }

    /// <summary>
    /// Read a primitive/value-type field from an object at the given address.
    /// </summary>
    public T GetFieldValue<T>(ulong objAddress, string typeName, string fieldName) where T : unmanaged
    {
        var obj = Heap.GetObject(objAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{objAddress:X}");

        var type = obj.Type;
        if (type is null)
            throw new InvalidOperationException($"Cannot resolve type for object at 0x{objAddress:X}");

        var field = type.GetFieldByName(fieldName)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{type.Name}'");

        return field.Read<T>(objAddress, interior: false);
    }

    /// <summary>
    /// Read a string field from an object at the given address.
    /// </summary>
    public string? GetStringField(ulong objAddress, string typeName, string fieldName)
    {
        var obj = Heap.GetObject(objAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{objAddress:X}");

        var type = obj.Type;
        if (type is null)
            throw new InvalidOperationException($"Cannot resolve type for object at 0x{objAddress:X}");

        var field = type.GetFieldByName(fieldName)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{type.Name}'");

        return field.ReadString(objAddress, interior: false);
    }

    /// <summary>
    /// Read the address of a reference-type field from an object.
    /// Returns 0 if the reference is null.
    /// </summary>
    public ulong GetObjectAddress(ulong objAddress, string typeName, string fieldName)
    {
        var obj = Heap.GetObject(objAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{objAddress:X}");

        var type = obj.Type;
        if (type is null)
            throw new InvalidOperationException($"Cannot resolve type for object at 0x{objAddress:X}");

        var field = type.GetFieldByName(fieldName)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{type.Name}'");

        return field.ReadObject(objAddress, interior: false);
    }

    /// <summary>
    /// Enumerate all heap objects whose type name matches exactly.
    /// </summary>
    public IEnumerable<ulong> EnumerateInstances(string typeName)
    {
        foreach (var obj in Heap.EnumerateObjects())
        {
            if (obj.IsValid && obj.Type is not null && obj.Type.Name == typeName)
                yield return obj.Address;
        }
    }

    /// <summary>
    /// Enumerate all heap objects whose type name matches exactly or is a subtype.
    /// </summary>
    public IEnumerable<ulong> EnumerateInstancesIncludingDerived(string typeName)
    {
        foreach (var obj in Heap.EnumerateObjects())
        {
            if (!obj.IsValid || obj.Type is null) continue;

            var t = obj.Type;
            while (t is not null)
            {
                if (t.Name == typeName)
                {
                    yield return obj.Address;
                    break;
                }
                t = t.BaseType;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _runtime.Dispose();
        _dataTarget.Dispose();
    }
}
