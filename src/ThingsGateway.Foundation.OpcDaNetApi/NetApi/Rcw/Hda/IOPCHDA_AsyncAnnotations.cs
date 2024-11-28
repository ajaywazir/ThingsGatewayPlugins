

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Hda
{
  [Guid("1F1217B7-DEE0-11d2-A5E5-000086339399")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCHDA_AsyncAnnotations
  {
    void QueryCapabilities(out OPCHDA_ANNOTATIONCAPABILITIES pCapabilities);

    void Read(
      [MarshalAs(UnmanagedType.I4)] int dwTransactionID,
      ref OPCHDA_TIME htStartTime,
      ref OPCHDA_TIME htEndTime,
      [MarshalAs(UnmanagedType.I4)] int dwNumItems,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3, ArraySubType = UnmanagedType.I4)] int[] phServer,
      out int pdwCancelID,
      out IntPtr ppErrors);

    void Insert(
      [MarshalAs(UnmanagedType.I4)] int dwTransactionID,
      [MarshalAs(UnmanagedType.I4)] int dwNumItems,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.I4)] int[] phServer,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] OPCHDA_FILETIME[] ftTimeStamps,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] OPCHDA_ANNOTATION[] pAnnotationValues,
      out int pdwCancelID,
      out IntPtr ppErrors);

    void Cancel([MarshalAs(UnmanagedType.I4)] int dwCancelID);
  }
}
