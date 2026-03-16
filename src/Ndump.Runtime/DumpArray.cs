using System.Collections;

namespace Ndump.Core;

/// <summary>
/// Lazy wrapper over an array object in a memory dump.
/// Reads elements on demand via the provided reader function.
/// </summary>
public sealed class DumpArray<T> : IEnumerable<T>
{
    private readonly ulong _address;
    private readonly int _length;
    private readonly Func<int, T> _reader;

    public DumpArray(ulong address, int length, Func<int, T> reader)
    {
        _address = address;
        _length = length;
        _reader = reader;
    }

    public int Length => _length;

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_length)
                throw new IndexOutOfRangeException();
            return _reader(index);
        }
    }

    public ulong GetObjectAddress() => _address;

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _length; i++)
            yield return _reader(i);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
