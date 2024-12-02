

using System.Runtime.InteropServices;


namespace OpcRcw.Security
{
    [Guid("7AA83A02-6C77-11d3-84F9-00008630A38B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IOPCSecurityPrivate
    {
        void IsAvailablePriv([MarshalAs(UnmanagedType.I4)] out int pbAvailable);

        void Logon([MarshalAs(UnmanagedType.LPWStr)] string szUserID, [MarshalAs(UnmanagedType.LPWStr)] string szPassword);

        void Logoff();
    }
}
