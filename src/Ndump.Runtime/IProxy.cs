namespace Ndump.Core;

/// <summary>
/// Implemented by generated proxy types to enable direct construction
/// without reflection via static abstract interface methods.
/// </summary>
public interface IProxy<out TSelf>
{
    static abstract TSelf FromAddress(ulong address, DumpContext ctx);
    static abstract TSelf FromInterior(ulong address, DumpContext ctx, string interiorTypeName);
}
