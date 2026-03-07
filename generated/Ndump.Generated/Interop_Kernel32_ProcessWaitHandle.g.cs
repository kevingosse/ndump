#nullable enable
using Ndump.Core;

namespace _;

public partial class Interop
{
    public partial class Kernel32
    {
        public sealed class ProcessWaitHandle : _.System.Threading.WaitHandle
        {
            private ProcessWaitHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

            public static new ProcessWaitHandle FromAddress(ulong address, DumpContext ctx)
                => new ProcessWaitHandle(address, ctx);

            public static new global::System.Collections.Generic.IEnumerable<ProcessWaitHandle> GetInstances(DumpContext ctx)
            {
                foreach (var addr in ctx.EnumerateInstances("Interop+Kernel32+ProcessWaitHandle"))
                    yield return new ProcessWaitHandle(addr, ctx);
            }

            public override string ToString() => $"ProcessWaitHandle@0x{_objAddress:X}";
        }

    }
}
