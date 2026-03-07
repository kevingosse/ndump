using System.Runtime.CompilerServices;
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
    /// Get the CLR type name of the object at the given address.
    /// Returns null if the object is invalid or has no type info.
    /// </summary>
    public string? GetTypeName(ulong objAddress)
    {
        var obj = Heap.GetObject(objAddress);
        if (!obj.IsValid || obj.Type is null)
            return null;
        return obj.Type.Name;
    }

    /// <summary>
    /// Read a primitive/value-type field from an object at the given address.
    /// </summary>
    public T GetFieldValue<T>(ulong objAddress, string fieldName)
    {
        var obj = Heap.GetObject(objAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{objAddress:X}");

        var type = obj.Type;
        if (type is null)
            throw new InvalidOperationException($"Cannot resolve type for object at 0x{objAddress:X}");

        var field = type.GetFieldByName(fieldName)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{type.Name}'");

        var addr = field.GetAddress(objAddress, interior: false);
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Runtime.DataTarget.DataReader.Read(addr, buffer);
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    /// <summary>
    /// Read a string field from an object at the given address.
    /// </summary>
    public string? GetStringField(ulong objAddress, string fieldName)
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
    public ulong GetObjectAddress(ulong objAddress, string fieldName)
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
    /// Get the length of an array object at the given address.
    /// </summary>
    public int GetArrayLength(ulong arrayAddress)
    {
        var obj = Heap.GetObject(arrayAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{arrayAddress:X}");
        if (obj.Type is null || !obj.Type.IsArray)
            throw new InvalidOperationException($"Object at 0x{arrayAddress:X} is not an array");

        return obj.AsArray().Length;
    }

    /// <summary>
    /// Read the address of a reference-type element in an array.
    /// Returns 0 if the element is null.
    /// </summary>
    public ulong GetArrayElementAddress(ulong arrayAddress, int index)
    {
        var obj = Heap.GetObject(arrayAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{arrayAddress:X}");

        var array = obj.AsArray();
        return array.GetObjectValue(index);
    }

    /// <summary>
    /// Read a primitive/value-type element from an array.
    /// </summary>
    public T GetArrayElementValue<T>(ulong arrayAddress, int index)
    {
        var obj = Heap.GetObject(arrayAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{arrayAddress:X}");

        var array = obj.AsArray();
        var elementAddress = array.GetStructValue(index).Address;
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Runtime.DataTarget.DataReader.Read(elementAddress, buffer);
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    /// <summary>
    /// Read a string element from an array.
    /// </summary>
    public string? GetArrayElementString(ulong arrayAddress, int index)
    {
        var obj = Heap.GetObject(arrayAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{arrayAddress:X}");

        var array = obj.AsArray();
        var elemAddr = array.GetObjectValue(index);
        if (elemAddr == 0) return null;

        var elemObj = Heap.GetObject(elemAddr);
        return elemObj.IsValid ? elemObj.AsString() : null;
    }

    /// <summary>
    /// Read the string value of a System.String object at the given address.
    /// </summary>
    public string? GetStringValue(ulong objAddress)
    {
        var obj = Heap.GetObject(objAddress);
        if (!obj.IsValid) return null;
        return obj.AsString();
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _runtime.Dispose();
        _dataTarget.Dispose();
    }
}
