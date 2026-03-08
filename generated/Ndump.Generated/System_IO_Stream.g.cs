#nullable enable
using Ndump.Core;

namespace _.System.IO;

public class Stream : _.System.MarshalByRefObject
{
    protected Stream(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _asyncActiveSemaphore => Field<global::_.System.Object>();

    public static new Stream FromAddress(ulong address, DumpContext context)
        => new Stream(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Stream> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.Stream"))
            yield return new Stream(addr, context);
    }

    public override string ToString() => $"Stream@0x{_objAddress:X}";
}
