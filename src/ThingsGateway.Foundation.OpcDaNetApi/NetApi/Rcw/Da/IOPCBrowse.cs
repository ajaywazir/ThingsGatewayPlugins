

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Da
{
  [Guid("39227004-A18F-4b57-8B0A-5235670F4468")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCBrowse
  {
    void GetProperties(
      [MarshalAs(UnmanagedType.I4)] int dwItemCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] pszItemIDs,
      [MarshalAs(UnmanagedType.I4)] int bReturnPropertyValues,
      [MarshalAs(UnmanagedType.I4)] int dwPropertyCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3, ArraySubType = UnmanagedType.I4)] int[] dwPropertyIDs,
      out IntPtr ppItemProperties);

    void Browse(
      [MarshalAs(UnmanagedType.LPWStr)] string szItemID,
      ref IntPtr pszContinuationPoint,
      [MarshalAs(UnmanagedType.I4)] int dwMaxElementsReturned,
      OPCBROWSEFILTER dwBrowseFilter,
      [MarshalAs(UnmanagedType.LPWStr)] string szElementNameFilter,
      [MarshalAs(UnmanagedType.LPWStr)] string szVendorFilter,
      [MarshalAs(UnmanagedType.I4)] int bReturnAllProperties,
      [MarshalAs(UnmanagedType.I4)] int bReturnPropertyValues,
      [MarshalAs(UnmanagedType.I4)] int dwPropertyCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8, ArraySubType = UnmanagedType.I4)] int[] pdwPropertyIDs,
      [MarshalAs(UnmanagedType.I4)] out int pbMoreElements,
      [MarshalAs(UnmanagedType.I4)] out int pdwCount,
      out IntPtr ppBrowseElements);
  }
}
