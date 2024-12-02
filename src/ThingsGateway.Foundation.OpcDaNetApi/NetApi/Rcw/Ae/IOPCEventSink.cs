

using System.Runtime.InteropServices;


namespace OpcRcw.Ae
{
    [Guid("6516885F-5783-11D1-84A0-00608CB8A7E9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IOPCEventSink
    {
        void OnEvent(
          [MarshalAs(UnmanagedType.I4)] int hClientSubscription,
          [MarshalAs(UnmanagedType.I4)] int bRefresh,
          [MarshalAs(UnmanagedType.I4)] int bLastRefresh,
          [MarshalAs(UnmanagedType.I4)] int dwCount,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3, ArraySubType = UnmanagedType.LPStruct)] ONEVENTSTRUCT[] pEvents);
    }
}
