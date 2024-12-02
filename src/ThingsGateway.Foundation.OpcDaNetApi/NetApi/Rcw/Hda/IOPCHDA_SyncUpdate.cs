

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Hda
{
    [Guid("1F1217B3-DEE0-11d2-A5E5-000086339399")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IOPCHDA_SyncUpdate
    {
        void QueryCapabilities(out OPCHDA_UPDATECAPABILITIES pCapabilities);

        void Insert(
          [MarshalAs(UnmanagedType.I4)] int dwNumItems,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCHDA_FILETIME[] ftTimeStamps,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)] object[] vDataValues,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] pdwQualities,
          out IntPtr ppErrors);

        void Replace(
          [MarshalAs(UnmanagedType.I4)] int dwNumItems,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCHDA_FILETIME[] ftTimeStamps,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)] object[] vDataValues,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] pdwQualities,
          out IntPtr ppErrors);

        void InsertReplace(
          [MarshalAs(UnmanagedType.I4)] int dwNumItems,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCHDA_FILETIME[] ftTimeStamps,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)] object[] vDataValues,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] pdwQualities,
          out IntPtr ppErrors);

        void DeleteRaw(
          ref OPCHDA_TIME htStartTime,
          ref OPCHDA_TIME htEndTime,
          [MarshalAs(UnmanagedType.I4)] int dwNumItems,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.I4)] int[] phServer,
          out IntPtr ppErrors);

        void DeleteAtTime(
          [MarshalAs(UnmanagedType.I4)] int dwNumItems,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] phServer,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCHDA_FILETIME[] ftTimeStamps,
          out IntPtr ppErrors);
    }
}
