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
    private readonly Dictionary<string, ClrType> _specializedTypes = [];
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
    /// Read a primitive/value-type field. When typeName is null, resolves type from the heap object header.
    /// When typeName is provided, uses it to resolve the field on an interior struct.
    /// </summary>
    public T GetFieldValue<T>(ulong address, string fieldName, string? typeName = null)
    {
        var (field, interior) = ResolveField(address, fieldName, typeName);
        var addr = field.GetAddress(address, interior);
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Runtime.DataTarget.DataReader.Read(addr, buffer);
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    /// <summary>
    /// Read a string field. When typeName is null, resolves type from the heap object header.
    /// </summary>
    public string? GetStringField(ulong address, string fieldName, string? typeName = null)
    {
        var (field, interior) = ResolveField(address, fieldName, typeName);
        return field.ReadString(address, interior);
    }

    /// <summary>
    /// Read the address of a reference-type field. Returns 0 if the reference is null.
    /// When typeName is null, resolves type from the heap object header.
    /// </summary>
    public ulong GetObjectAddress(ulong address, string fieldName, string? typeName = null)
    {
        var (field, interior) = ResolveField(address, fieldName, typeName);
        return field.ReadObject(address, interior);
    }

    /// <summary>
    /// Get the interior address of a value type field.
    /// When typeName is null, resolves type from the heap object header.
    /// </summary>
    public ulong GetValueTypeFieldAddress(ulong address, string fieldName, string? typeName = null)
    {
        var (field, interior) = ResolveField(address, fieldName, typeName);
        return field.GetAddress(address, interior);
    }

    /// <summary>
    /// Get the address of a struct array element.
    /// </summary>
    public ulong GetArrayStructElementAddress(ulong arrayAddress, int index)
    {
        var obj = Heap.GetObject(arrayAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{arrayAddress:X}");

        return obj.AsArray().GetStructValue(index).Address;
    }

    /// <summary>
    /// Get the CLR type name of an array's component (element) type.
    /// Registers the specialized component type so that FindTypeByName resolves
    /// to the correct instantiation (important for generic nested value types
    /// like Dictionary&lt;K,V&gt;+Entry which module lookup cannot resolve).
    /// </summary>
    public string GetArrayComponentTypeName(ulong arrayAddress)
    {
        var obj = Heap.GetObject(arrayAddress);
        if (!obj.IsValid)
            throw new InvalidOperationException($"Invalid object at address 0x{arrayAddress:X}");

        var componentType = obj.Type?.ComponentType
            ?? throw new InvalidOperationException($"Object at 0x{arrayAddress:X} has no component type");

        var name = componentType.Name!;
        // Register the specialized component type so FindTypeByName resolves
        // the correct instantiation (e.g. Dictionary<Int32, String>+Entry)
        // rather than the generic definition from module lookup.
        _specializedTypes.TryAdd(name, componentType);
        return name;
    }

    /// <summary>
    /// Resolve a CLR type by name. Checks registered specialized types first,
    /// then searches all loaded modules.
    /// </summary>
    public ClrType? FindTypeByName(string typeName)
    {
        // Specialized generic types (e.g. Dictionary<Int32, String>+Entry) cannot be
        // found through module lookup, which only returns the generic definition.
        // Check registered specialized types first.
        if (_specializedTypes.TryGetValue(typeName, out var specialized))
            return specialized;

        // ClrMD modules use backtick-arity notation, not angle brackets.
        // Convert "Dictionary<String, Int32>+Entry" to "Dictionary`2+Entry" for lookup.
        var lookupName = TypeNameHelper.ConvertToBacktickForm(typeName);

        ClrType? found = null;
        foreach (var module in Runtime.EnumerateModules())
        {
            found = module.GetTypeByName(lookupName);
            if (found is not null) break;
        }

        return found;
    }


    private (ClrInstanceField Field, bool Interior) ResolveField(ulong address, string fieldName, string? typeName)
    {
        if (typeName is not null)
        {
            var type = FindTypeByName(typeName)
                ?? throw new InvalidOperationException($"Type '{typeName}' not found in any module");
            var field = type.GetFieldByName(fieldName)
                ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{typeName}'");
            return (field, true);
        }
        else
        {
            var obj = Heap.GetObject(address);
            if (!obj.IsValid)
                throw new InvalidOperationException($"Invalid object at address 0x{address:X}");
            var type = obj.Type
                ?? throw new InvalidOperationException($"Cannot resolve type for object at 0x{address:X}");
            var field = type.GetFieldByName(fieldName)
                ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{type.Name}'");
            return (field, false);
        }
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

    /// <summary>
    /// Read a Nullable&lt;T&gt; field. Returns null if hasValue is false.
    /// When typeName is null, resolves type from the heap object header.
    /// </summary>
    public T? GetNullableFieldValue<T>(ulong address, string fieldName, string? typeName = null) where T : struct
    {
        var (field, interior) = ResolveField(address, fieldName, typeName);
        return ReadNullableFromField<T>(address, field, interior);
    }

    private T? ReadNullableFromField<T>(ulong baseAddr, ClrInstanceField field, bool interior) where T : struct
    {
        var nullableType = field.Type
            ?? throw new InvalidOperationException($"Cannot resolve type for Nullable field '{field.Name}'");

        var nullableAddr = field.GetAddress(baseAddr, interior);

        var hasValueField = nullableType.GetFieldByName("hasValue")
            ?? throw new InvalidOperationException($"hasValue field not found on Nullable type '{nullableType.Name}'");

        var hasValueAddr = hasValueField.GetAddress(nullableAddr, interior: true);
        Span<byte> boolBuf = stackalloc byte[1];
        Runtime.DataTarget.DataReader.Read(hasValueAddr, boolBuf);

        if (boolBuf[0] == 0)
            return null;

        var valueField = nullableType.GetFieldByName("value")
            ?? throw new InvalidOperationException($"value field not found on Nullable type '{nullableType.Name}'");

        var valueAddr = valueField.GetAddress(nullableAddr, interior: true);
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Runtime.DataTarget.DataReader.Read(valueAddr, buffer);
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    /// <summary>
    /// Get the interior address of the value sub-field of a Nullable&lt;T&gt; field.
    /// Used for nullable struct proxies that need to navigate into the value.
    /// Returns (hasValue, valueAddress).
    /// When typeName is null, resolves type from the heap object header.
    /// </summary>
    public (bool HasValue, ulong ValueAddress) GetNullableFieldInfo(ulong address, string fieldName, string? typeName = null)
    {
        var (field, interior) = ResolveField(address, fieldName, typeName);
        return ReadNullableInfo(address, field, interior);
    }

    private (bool HasValue, ulong ValueAddress) ReadNullableInfo(ulong baseAddr, ClrInstanceField field, bool interior)
    {
        var nullableType = field.Type
            ?? throw new InvalidOperationException($"Cannot resolve type for Nullable field '{field.Name}'");

        var nullableAddr = field.GetAddress(baseAddr, interior);

        var hasValueField = nullableType.GetFieldByName("hasValue")
            ?? throw new InvalidOperationException($"hasValue field not found on Nullable type '{nullableType.Name}'");

        var hasValueAddr = hasValueField.GetAddress(nullableAddr, interior: true);
        Span<byte> boolBuf = stackalloc byte[1];
        Runtime.DataTarget.DataReader.Read(hasValueAddr, boolBuf);
        var hasValue = boolBuf[0] != 0;

        var valueField = nullableType.GetFieldByName("value")
            ?? throw new InvalidOperationException($"value field not found on Nullable type '{nullableType.Name}'");

        var valueAddr = valueField.GetAddress(nullableAddr, interior: true);
        return (hasValue, valueAddr);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _runtime.Dispose();
        _dataTarget.Dispose();
    }
}
