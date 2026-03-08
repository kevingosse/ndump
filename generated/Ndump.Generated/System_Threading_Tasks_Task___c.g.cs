#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public partial class Task
{
    public sealed class __c : _.System.Object
    {
        private __c(ulong address, DumpContext context) : base(address, context) { }

        public static new __c FromAddress(ulong address, DumpContext context)
            => new __c(address, context);

        public static new global::System.Collections.Generic.IEnumerable<__c> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.Threading.Tasks.Task+<>c"))
                yield return new __c(addr, context);
        }

        public override string ToString() => $"__c@0x{_objAddress:X}";
    }

}
