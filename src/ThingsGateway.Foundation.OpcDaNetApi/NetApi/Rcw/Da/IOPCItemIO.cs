

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Da
{
    [Guid("85C0B427-2893-4cbc-BD78-E5FC5146F08F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IOPCItemIO
    {
        void Read(
          [MarshalAs(UnmanagedType.I4)] int dwCount,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] pszItemIDs,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] pdwMaxAge,
          out IntPtr ppvValues,
          out IntPtr ppwQualities,
          out IntPtr ppftTimeStamps,
          out IntPtr ppErrors);

        void WriteVQT([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] pszItemIDs, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OPCITEMVQT[] pItemVQT, out IntPtr ppErrors);
    }
}
