

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Da
{
  [Guid("39c13a52-011e-11d0-9675-0020afd8adb3")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCSyncIO
  {
    void Read(
      OPCDATASOURCE dwSource,
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.I4)] int[] phServer,
      out IntPtr ppItemValues,
      out IntPtr ppErrors);

    void Write([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)] object[] pItemValues, out IntPtr ppErrors);
  }
}
