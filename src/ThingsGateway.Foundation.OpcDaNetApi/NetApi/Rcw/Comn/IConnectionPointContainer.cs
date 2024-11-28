

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Comn
{
  [Guid("B196B284-BAB4-101A-B69C-00AA00341D07")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IConnectionPointContainer
  {
    void EnumConnectionPoints(out IEnumConnectionPoints ppEnum);

    void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP);
  }
}
