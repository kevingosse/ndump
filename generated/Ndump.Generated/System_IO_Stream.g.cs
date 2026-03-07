#nullable enable
using Ndump.Core;

namespace _.System.IO;

public class Stream : _.System.MarshalByRefObject
{
    protected Stream(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::_.System.Object? _asyncActiveSemaphore => Field<global::_.System.Object>();

    public static new Stream FromAddress(ulong address, DumpContext ctx)
        => new Stream(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Stream> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.Stream"))
            yield return new Stream(addr, ctx);
    }

    public override string ToString() => $"Stream@0x{_objAddress:X}";
}
