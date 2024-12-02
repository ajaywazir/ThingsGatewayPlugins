

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Comn
{
    [Guid("F31DFDE2-07B6-11d2-B2D8-0060083BA1FB")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IOPCCommon
    {
        void SetLocaleID([MarshalAs(UnmanagedType.I4)] int dwLcid);

        void GetLocaleID([MarshalAs(UnmanagedType.I4)] out int pdwLcid);

        void QueryAvailableLocaleIDs([MarshalAs(UnmanagedType.I4)] out int pdwCount, out IntPtr pdwLcid);

        void GetErrorString([MarshalAs(UnmanagedType.I4)] int dwError, [MarshalAs(UnmanagedType.LPWStr)] out string ppString);

        void SetClientName([MarshalAs(UnmanagedType.LPWStr)] string szName);
    }
}
