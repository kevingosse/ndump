#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class RuntimeFieldInfoStub : _.System.Object
{
    private RuntimeFieldInfoStub(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Object? m_keepalive => Field<_.System.Object>();

    public _.System.Object? m_c => Field<_.System.Object>();

    public _.System.Object? m_d => Field<_.System.Object>();

    public int m_b => Field<int>();

    public _.System.Object? m_e => Field<_.System.Object>();

    public _.System.Object? m_f => Field<_.System.Object>();

    public _.System.RuntimeFieldHandleInternal m_fieldHandle => StructField<_.System.RuntimeFieldHandleInternal>("System.RuntimeFieldHandleInternal");

    public static new RuntimeFieldInfoStub FromAddress(ulong address, DumpContext context)
        => new RuntimeFieldInfoStub(address, context);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeFieldInfoStub> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.RuntimeFieldInfoStub"))
            yield return new RuntimeFieldInfoStub(addr, context);
    }

    public override string ToString() => $"RuntimeFieldInfoStub@0x{_objAddress:X}";
}
