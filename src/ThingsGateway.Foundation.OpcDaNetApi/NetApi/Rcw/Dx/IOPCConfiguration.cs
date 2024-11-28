

using System;
using System.Runtime.InteropServices;


namespace OpcRcw.Dx
{
  [Guid("C130D281-F4AA-4779-8846-C2C4CB444F2A")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  public interface IOPCConfiguration
  {
    void GetServers([MarshalAs(UnmanagedType.I4)] out int pdwCount, out IntPtr ppServers);

    void AddServers([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] SourceServer[] pServers, out DXGeneralResponse pResponse);

    void ModifyServers([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] SourceServer[] pServers, out DXGeneralResponse pResponse);

    void DeleteServers([MarshalAs(UnmanagedType.I4)] int dwCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] ItemIdentifier[] pServers, out DXGeneralResponse pResponse);

    void CopyDefaultServerAttributes(
      [MarshalAs(UnmanagedType.I4)] int bConfigToStatus,
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] ItemIdentifier[] pServers,
      out DXGeneralResponse pResponse);

    void QueryDXConnections(
      [MarshalAs(UnmanagedType.LPWStr)] string szBrowsePath,
      [MarshalAs(UnmanagedType.I4)] int dwNoOfMasks,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] DXConnection[] pDXConnectionMasks,
      [MarshalAs(UnmanagedType.I4)] int bRecursive,
      out IntPtr ppErrors,
      [MarshalAs(UnmanagedType.I4)] out int pdwCount,
      out IntPtr ppConnections);

    void AddDXConnections(
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] DXConnection[] pConnections,
      out DXGeneralResponse pResponse);

    void UpdateDXConnections(
      [MarshalAs(UnmanagedType.LPWStr)] string szBrowsePath,
      [MarshalAs(UnmanagedType.I4)] int dwNoOfMasks,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] DXConnection[] pDXConnectionMasks,
      [MarshalAs(UnmanagedType.I4)] int bRecursive,
      ref DXConnection pDXConnectionDefinition,
      [MarshalAs(UnmanagedType.I4)] out IntPtr ppErrors,
      out DXGeneralResponse pResponse);

    void ModifyDXConnections(
      [MarshalAs(UnmanagedType.I4)] int dwCount,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] DXConnection[] pDXConnectionDefinitions,
      out DXGeneralResponse pResponse);

    void DeleteDXConnections(
      [MarshalAs(UnmanagedType.LPWStr)] string szBrowsePath,
      [MarshalAs(UnmanagedType.I4)] int dwNoOfMasks,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] DXConnection[] pDXConnectionMasks,
      [MarshalAs(UnmanagedType.I4)] int bRecursive,
      [MarshalAs(UnmanagedType.I4)] out IntPtr ppErrors,
      out DXGeneralResponse pResponse);

    void CopyDXConnectionDefaultAttributes(
      [MarshalAs(UnmanagedType.I4)] int bConfigToStatus,
      [MarshalAs(UnmanagedType.LPWStr)] string szBrowsePath,
      [MarshalAs(UnmanagedType.I4)] int dwNoOfMasks,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.LPStruct)] DXConnection[] pDXConnectionMasks,
      [MarshalAs(UnmanagedType.I4)] int bRecursive,
      [MarshalAs(UnmanagedType.I4)] out IntPtr ppErrors,
      out DXGeneralResponse pResponse);

    void ResetConfiguration([MarshalAs(UnmanagedType.LPWStr)] string szConfigurationVersion, [MarshalAs(UnmanagedType.LPWStr)] out string pszConfigurationVersion);
  }
}
