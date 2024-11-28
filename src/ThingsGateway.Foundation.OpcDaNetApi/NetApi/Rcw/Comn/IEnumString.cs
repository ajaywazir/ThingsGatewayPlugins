

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Comn
{
  [Guid("00000101-0000-0000-C000-000000000046")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IEnumString
  {
    void RemoteNext([MarshalAs(UnmanagedType.I4)] int celt, IntPtr rgelt, [MarshalAs(UnmanagedType.I4)] out int pceltFetched);

    void Skip([MarshalAs(UnmanagedType.I4)] int celt);

    void Reset();

    void Clone(out IEnumString ppenum);
  }
}
