

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Da
{
  [Guid("730F5F0F-55B1-4c81-9E18-FF8A0904E1FA")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCSyncIO2
  {
    void Read(
      OPCDATASOURCE dwSource,
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.I4)] int[] phServer,
      out IntPtr ppItemValues,
      out IntPtr ppErrors);

    void Write([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)] object[] pItemValues, out IntPtr ppErrors);

    void ReadMaxAge(
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] pdwMaxAge,
      out IntPtr ppvValues,
      out IntPtr ppwQualities,
      out IntPtr ppftTimeStamps,
      out IntPtr ppErrors);

    void WriteVQT([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCITEMVQT[] pItemVQT, out IntPtr ppErrors);
  }
}
