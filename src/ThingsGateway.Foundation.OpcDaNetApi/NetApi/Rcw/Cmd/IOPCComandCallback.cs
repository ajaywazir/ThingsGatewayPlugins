

using System.Runtime.InteropServices;


namespace OpcRcw.Cmd
{
    [Guid("3104B527-2016-442d-9696-1275DE978778")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IOPCComandCallback
    {
        void OnStateChange(
          [MarshalAs(UnmanagedType.I4)] int dwNoOfEvents,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] OpcCmdStateChangeEvent[] pEvents,
          [MarshalAs(UnmanagedType.I4)] int dwNoOfPermittedControls,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.LPWStr)] string pszPermittedControls,
          [MarshalAs(UnmanagedType.I4)] int bNoStateChange);
    }
}
