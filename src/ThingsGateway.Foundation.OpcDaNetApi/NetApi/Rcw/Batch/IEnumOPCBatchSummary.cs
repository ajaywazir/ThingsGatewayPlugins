

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Batch
{
  [Guid("a8080da2-e23e-11d2-afa7-00c04f539421")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IEnumOPCBatchSummary
  {
    void Next([MarshalAs(UnmanagedType.I4)] int celt, out IntPtr ppSummaryArray, [MarshalAs(UnmanagedType.I4)] out int celtFetched);

    void Skip([MarshalAs(UnmanagedType.I4)] int celt);

    void Reset();

    void Clone(out IEnumOPCBatchSummary ppEnumBatchSummary);

    void Count([MarshalAs(UnmanagedType.I4)] out int pcelt);
  }
}
