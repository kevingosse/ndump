#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public partial class EventSource
{
    public sealed class OverrideEventProvider : _.System.Diagnostics.Tracing.EventProvider
    {
        private OverrideEventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

        public _.System.Func<_.System.Diagnostics.Tracing.EventSource>? _eventSourceFactory => Field<_.System.Func<_.System.Diagnostics.Tracing.EventSource>>();

        public int _eventProviderType => Field<int>();

        public static new OverrideEventProvider FromAddress(ulong address, DumpContext ctx)
            => new OverrideEventProvider(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<OverrideEventProvider> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventSource+OverrideEventProvider"))
                yield return new OverrideEventProvider(addr, ctx);
        }

        public override string ToString() => $"OverrideEventProvider@0x{_objAddress:X}";
    }

}
