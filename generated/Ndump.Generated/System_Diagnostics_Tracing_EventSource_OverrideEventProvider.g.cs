#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public partial class EventSource
{
    public sealed class OverrideEventProvider : _.System.Diagnostics.Tracing.EventProvider
    {
        private OverrideEventProvider(ulong address, DumpContext context) : base(address, context) { }

        public _.System.Func<_.System.Diagnostics.Tracing.EventSource>? _eventSourceFactory => Field<_.System.Func<_.System.Diagnostics.Tracing.EventSource>>();

        public int _eventProviderType => Field<int>();

        public static new OverrideEventProvider FromAddress(ulong address, DumpContext context)
            => new OverrideEventProvider(address, context);

        public static new global::System.Collections.Generic.IEnumerable<OverrideEventProvider> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.EventSource+OverrideEventProvider"))
                yield return new OverrideEventProvider(addr, context);
        }

        public override string ToString() => $"OverrideEventProvider@0x{_objAddress:X}";
    }

}
