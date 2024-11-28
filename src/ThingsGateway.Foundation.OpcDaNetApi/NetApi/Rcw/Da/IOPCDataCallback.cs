

using System.Runtime.InteropServices;


namespace OpcRcw.Da
{
  [Guid("39c13a70-011e-11d0-9675-0020afd8adb3")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCDataCallback
  {
    void OnDataChange(
      [MarshalAs(UnmanagedType.I4)] int dwTransid,
      [MarshalAs(UnmanagedType.I4)] int hGroup,
      [MarshalAs(UnmanagedType.I4)] int hrMasterquality,
      [MarshalAs(UnmanagedType.I4)] int hrMastererror,
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.I4)] int[] phClientItems,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.Struct)] object[] pvValues,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.I2)] short[] pwQualities,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.LPStruct)] FILETIME[] pftTimeStamps,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.I4)] int[] pErrors);

    void OnReadComplete(
      [MarshalAs(UnmanagedType.I4)] int dwTransid,
      [MarshalAs(UnmanagedType.I4)] int hGroup,
      [MarshalAs(UnmanagedType.I4)] int hrMasterquality,
      [MarshalAs(UnmanagedType.I4)] int hrMastererror,
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.I4)] int[] phClientItems,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.Struct)] object[] pvValues,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.I2)] short[] pwQualities,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.LPStruct)] FILETIME[] pftTimeStamps,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4, ArraySubType = UnmanagedType.I4)] int[] pErrors);

    void OnWriteComplete(
      [MarshalAs(UnmanagedType.I4)] int dwTransid,
      [MarshalAs(UnmanagedType.I4)] int hGroup,
      [MarshalAs(UnmanagedType.I4)] int hrMastererr,
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3, ArraySubType = UnmanagedType.I4)] int[] pClienthandles,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3, ArraySubType = UnmanagedType.I4)] int[] pErrors);

    void OnCancelComplete([MarshalAs(UnmanagedType.I4)] int dwTransid, [MarshalAs(UnmanagedType.I4)] int hGroup);
  }
}
