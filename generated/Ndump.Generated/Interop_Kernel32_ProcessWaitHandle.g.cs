#nullable enable
using Ndump.Core;

namespace _;

public partial class Interop
{
    public partial class Kernel32
    {
        public sealed class ProcessWaitHandle : _.System.Threading.WaitHandle
        {
            private ProcessWaitHandle(ulong address, DumpContext context) : base(address, context) { }

            public static new ProcessWaitHandle FromAddress(ulong address, DumpContext context)
                => new ProcessWaitHandle(address, context);

            public static new global::System.Collections.Generic.IEnumerable<ProcessWaitHandle> GetInstances(DumpContext context)
            {
                foreach (var addr in context.EnumerateInstances("Interop+Kernel32+ProcessWaitHandle"))
                    yield return new ProcessWaitHandle(addr, context);
            }

            public override string ToString() => $"ProcessWaitHandle@0x{_objAddress:X}";
        }

    }
}
