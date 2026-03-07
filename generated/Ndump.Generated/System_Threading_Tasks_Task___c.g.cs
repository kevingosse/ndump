#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public partial class Task
{
    public sealed class __c : _.System.Object
    {
        private __c(ulong address, DumpContext ctx) : base(address, ctx) { }

        public static new __c FromAddress(ulong address, DumpContext ctx)
            => new __c(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<__c> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.Threading.Tasks.Task+<>c"))
                yield return new __c(addr, ctx);
        }

        public override string ToString() => $"__c@0x{_objAddress:X}";
    }

}
