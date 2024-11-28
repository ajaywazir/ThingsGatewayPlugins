

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Da
{
  [Guid("39c13a54-011e-11d0-9675-0020afd8adb3")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCItemMgt
  {
    void AddItems(
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCITEMDEF[] pItemArray,
      out IntPtr ppAddResults,
      out IntPtr ppErrors);

    void ValidateItems(
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCITEMDEF[] pItemArray,
      [MarshalAs(UnmanagedType.I4)] int bBlobUpdate,
      out IntPtr ppValidationResults,
      out IntPtr ppErrors);

    void RemoveItems([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer, out IntPtr ppErrors);

    void SetActiveState([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer, [MarshalAs(UnmanagedType.I4)] int bActive, out IntPtr ppErrors);

    void SetClientHandles([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phClient, out IntPtr ppErrors);

    void SetDatatypes(
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)] short[] pRequestedDatatypes,
      out IntPtr ppErrors);

    //void CreateEnumerator(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
  }
}
