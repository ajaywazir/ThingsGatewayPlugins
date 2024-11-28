

using Opc;
using Opc.Dx;
using OpcRcw.Dx;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Dx
{
  public class Interop
  {
    internal static ResultID[] GetResultIDs(ref IntPtr pInput, int count, bool deallocate)
    {
      ResultID[] resultIds = (ResultID[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        resultIds = new ResultID[count];
        int[] int32s = OpcCom.Interop.GetInt32s(ref pInput, count, deallocate);
        for (int index = 0; index < count; ++index)
          resultIds[index] = OpcCom.Interop.GetResultID(int32s[index]);
      }
      return resultIds;
    }

    internal static Opc.Dx.SourceServer[] GetSourceServers(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      Opc.Dx.SourceServer[] sourceServers = (Opc.Dx.SourceServer[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        sourceServers = new Opc.Dx.SourceServer[count];
        IntPtr ptr = pInput;
        for (int index = 0; index < count; ++index)
        {
          OpcRcw.Dx.SourceServer structure = (OpcRcw.Dx.SourceServer) Marshal.PtrToStructure(ptr, typeof (OpcRcw.Dx.SourceServer));
          sourceServers[index] = new Opc.Dx.SourceServer();
          sourceServers[index].ItemName = structure.szItemName;
          sourceServers[index].ItemPath = structure.szItemPath;
          sourceServers[index].Version = structure.szVersion;
          sourceServers[index].Name = structure.szName;
          sourceServers[index].Description = structure.szDescription;
          sourceServers[index].ServerType = structure.szServerType;
          sourceServers[index].ServerURL = structure.szServerURL;
          sourceServers[index].DefaultConnected = structure.bDefaultSourceServerConnected != 0;
          ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (OpcRcw.Dx.SourceServer)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return sourceServers;
    }

    internal static OpcRcw.Dx.SourceServer[] GetSourceServers(Opc.Dx.SourceServer[] input)
    {
      OpcRcw.Dx.SourceServer[] sourceServers = (OpcRcw.Dx.SourceServer[]) null;
      if (input != null && input.Length != 0)
      {
        sourceServers = new OpcRcw.Dx.SourceServer[input.Length];
        for (int index = 0; index < input.Length; ++index)
        {
          sourceServers[index] = new OpcRcw.Dx.SourceServer();
          sourceServers[index].dwMask = (uint) int.MaxValue;
          sourceServers[index].szItemName = input[index].ItemName;
          sourceServers[index].szItemPath = input[index].ItemPath;
          sourceServers[index].szVersion = input[index].Version;
          sourceServers[index].szName = input[index].Name;
          sourceServers[index].szDescription = input[index].Description;
          sourceServers[index].szServerType = input[index].ServerType;
          sourceServers[index].szServerURL = input[index].ServerURL;
          sourceServers[index].bDefaultSourceServerConnected = input[index].DefaultConnected ? 1 : 0;
        }
      }
      return sourceServers;
    }

    internal static GeneralResponse GetGeneralResponse(DXGeneralResponse input, bool deallocate)
    {
      Opc.Dx.IdentifiedResult[] identifiedResults = Interop.GetIdentifiedResults(ref input.pIdentifiedResults, input.dwCount, deallocate);
      return new GeneralResponse(input.szConfigurationVersion, (ICollection) identifiedResults);
    }

    internal static Opc.Dx.IdentifiedResult[] GetIdentifiedResults(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      Opc.Dx.IdentifiedResult[] identifiedResults = (Opc.Dx.IdentifiedResult[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        identifiedResults = new Opc.Dx.IdentifiedResult[count];
        IntPtr ptr = pInput;
        for (int index = 0; index < count; ++index)
        {
          OpcRcw.Dx.IdentifiedResult structure = (OpcRcw.Dx.IdentifiedResult) Marshal.PtrToStructure(ptr, typeof (OpcRcw.Dx.IdentifiedResult));
          identifiedResults[index] = new Opc.Dx.IdentifiedResult();
          identifiedResults[index].ItemName = structure.szItemName;
          identifiedResults[index].ItemPath = structure.szItemPath;
          identifiedResults[index].Version = structure.szVersion;
          identifiedResults[index].ResultID = OpcCom.Interop.GetResultID(structure.hResultCode);
          if (deallocate)
            Marshal.DestroyStructure(ptr, typeof (OpcRcw.Dx.IdentifiedResult));
          ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (OpcRcw.Dx.IdentifiedResult)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return identifiedResults;
    }

    internal static Opc.Dx.DXConnection[] GetDXConnections(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      Opc.Dx.DXConnection[] dxConnections = (Opc.Dx.DXConnection[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        dxConnections = new Opc.Dx.DXConnection[count];
        IntPtr ptr = pInput;
        for (int index = 0; index < count; ++index)
        {
          OpcRcw.Dx.DXConnection structure = (OpcRcw.Dx.DXConnection) Marshal.PtrToStructure(ptr, typeof (OpcRcw.Dx.DXConnection));
          dxConnections[index] = Interop.GetDXConnection(structure, deallocate);
          if (deallocate)
            Marshal.DestroyStructure(ptr, typeof (OpcRcw.Dx.DXConnection));
          ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (OpcRcw.Dx.DXConnection)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return dxConnections;
    }

    internal static OpcRcw.Dx.DXConnection[] GetDXConnections(Opc.Dx.DXConnection[] input)
    {
      OpcRcw.Dx.DXConnection[] dxConnections = (OpcRcw.Dx.DXConnection[]) null;
      if (input != null && input.Length != 0)
      {
        dxConnections = new OpcRcw.Dx.DXConnection[input.Length];
        for (int index = 0; index < input.Length; ++index)
          dxConnections[index] = Interop.GetDXConnection(input[index]);
      }
      return dxConnections;
    }

    internal static OpcRcw.Dx.DXConnection GetDXConnection(Opc.Dx.DXConnection input)
    {
      OpcRcw.Dx.DXConnection dxConnection = new OpcRcw.Dx.DXConnection();
      dxConnection.dwMask = 0U;
      dxConnection.szItemPath = (string) null;
      dxConnection.szItemName = (string) null;
      dxConnection.szVersion = (string) null;
      dxConnection.dwBrowsePathCount = 0;
      dxConnection.pszBrowsePaths = IntPtr.Zero;
      dxConnection.szName = (string) null;
      dxConnection.szDescription = (string) null;
      dxConnection.szKeyword = (string) null;
      dxConnection.bDefaultSourceItemConnected = 0;
      dxConnection.bDefaultTargetItemConnected = 0;
      dxConnection.bDefaultOverridden = 0;
      dxConnection.vDefaultOverrideValue = (object) null;
      dxConnection.vSubstituteValue = (object) null;
      dxConnection.bEnableSubstituteValue = 0;
      dxConnection.szTargetItemPath = (string) null;
      dxConnection.szTargetItemName = (string) null;
      dxConnection.szSourceServerName = (string) null;
      dxConnection.szSourceItemPath = (string) null;
      dxConnection.szSourceItemName = (string) null;
      dxConnection.dwSourceItemQueueSize = 0;
      dxConnection.dwUpdateRate = 0;
      dxConnection.fltDeadBand = 0.0f;
      dxConnection.szVendorData = (string) null;
      if (input.ItemName != null)
      {
        dxConnection.dwMask |= 2U;
        dxConnection.szItemName = input.ItemName;
      }
      if (input.ItemPath != null)
      {
        dxConnection.dwMask |= 1U;
        dxConnection.szItemPath = input.ItemPath;
      }
      if (input.Version != null)
      {
        dxConnection.dwMask |= 4U;
        dxConnection.szVersion = input.Version;
      }
      if (input.BrowsePaths.Count > 0)
      {
        dxConnection.dwMask |= 8U;
        dxConnection.dwBrowsePathCount = input.BrowsePaths.Count;
        dxConnection.pszBrowsePaths = OpcCom.Interop.GetUnicodeStrings(input.BrowsePaths.ToArray());
      }
      if (input.Name != null)
      {
        dxConnection.dwMask |= 16U;
        dxConnection.szName = input.Name;
      }
      if (input.Description != null)
      {
        dxConnection.dwMask |= 32U;
        dxConnection.szDescription = input.Description;
      }
      if (input.Keyword != null)
      {
        dxConnection.dwMask |= 64U;
        dxConnection.szKeyword = input.Keyword;
      }
      if (input.DefaultSourceItemConnectedSpecified)
      {
        dxConnection.dwMask |= 128U;
        dxConnection.bDefaultSourceItemConnected = input.DefaultSourceItemConnected ? 1 : 0;
      }
      if (input.DefaultTargetItemConnectedSpecified)
      {
        dxConnection.dwMask |= 256U;
        dxConnection.bDefaultTargetItemConnected = input.DefaultTargetItemConnected ? 1 : 0;
      }
      if (input.DefaultOverriddenSpecified)
      {
        dxConnection.dwMask |= 512U;
        dxConnection.bDefaultOverridden = input.DefaultOverridden ? 1 : 0;
      }
      if (input.DefaultOverrideValue != null)
      {
        dxConnection.dwMask |= 1024U;
        dxConnection.vDefaultOverrideValue = input.DefaultOverrideValue;
      }
      if (input.SubstituteValue != null)
      {
        dxConnection.dwMask |= 2048U;
        dxConnection.vSubstituteValue = input.SubstituteValue;
      }
      if (input.EnableSubstituteValueSpecified)
      {
        dxConnection.dwMask |= 4096U;
        dxConnection.bEnableSubstituteValue = input.EnableSubstituteValue ? 1 : 0;
      }
      if (input.TargetItemName != null)
      {
        dxConnection.dwMask |= 16384U;
        dxConnection.szTargetItemName = input.TargetItemName;
      }
      if (input.TargetItemPath != null)
      {
        dxConnection.dwMask |= 8192U;
        dxConnection.szTargetItemPath = input.TargetItemPath;
      }
      if (input.SourceServerName != null)
      {
        dxConnection.dwMask |= 32768U;
        dxConnection.szSourceServerName = input.SourceServerName;
      }
      if (input.SourceItemName != null)
      {
        dxConnection.dwMask |= 131072U;
        dxConnection.szSourceItemName = input.SourceItemName;
      }
      if (input.SourceItemPath != null)
      {
        dxConnection.dwMask |= 65536U;
        dxConnection.szSourceItemPath = input.SourceItemPath;
      }
      if (input.SourceItemQueueSizeSpecified)
      {
        dxConnection.dwMask |= 262144U;
        dxConnection.dwSourceItemQueueSize = input.SourceItemQueueSize;
      }
      if (input.UpdateRateSpecified)
      {
        dxConnection.dwMask |= 524288U;
        dxConnection.dwUpdateRate = input.UpdateRate;
      }
      if (input.DeadbandSpecified)
      {
        dxConnection.dwMask |= 1048576U;
        dxConnection.fltDeadBand = input.Deadband;
      }
      if (input.VendorData != null)
      {
        dxConnection.dwMask |= 2097152U;
        dxConnection.szVendorData = input.VendorData;
      }
      return dxConnection;
    }

    internal static Opc.Dx.DXConnection GetDXConnection(OpcRcw.Dx.DXConnection input, bool deallocate)
    {
      Opc.Dx.DXConnection dxConnection = new Opc.Dx.DXConnection();
      dxConnection.ItemPath = (string) null;
      dxConnection.ItemName = (string) null;
      dxConnection.Version = (string) null;
      dxConnection.BrowsePaths.Clear();
      dxConnection.Name = (string) null;
      dxConnection.Description = (string) null;
      dxConnection.Keyword = (string) null;
      dxConnection.DefaultSourceItemConnected = false;
      dxConnection.DefaultSourceItemConnectedSpecified = false;
      dxConnection.DefaultTargetItemConnected = false;
      dxConnection.DefaultTargetItemConnectedSpecified = false;
      dxConnection.DefaultOverridden = false;
      dxConnection.DefaultOverriddenSpecified = false;
      dxConnection.DefaultOverrideValue = (object) null;
      dxConnection.SubstituteValue = (object) null;
      dxConnection.EnableSubstituteValue = false;
      dxConnection.EnableSubstituteValueSpecified = false;
      dxConnection.TargetItemPath = (string) null;
      dxConnection.TargetItemName = (string) null;
      dxConnection.SourceServerName = (string) null;
      dxConnection.SourceItemPath = (string) null;
      dxConnection.SourceItemName = (string) null;
      dxConnection.SourceItemQueueSize = 0;
      dxConnection.SourceItemQueueSizeSpecified = false;
      dxConnection.UpdateRate = 0;
      dxConnection.UpdateRateSpecified = false;
      dxConnection.Deadband = 0.0f;
      dxConnection.DeadbandSpecified = false;
      dxConnection.VendorData = (string) null;
      if (((int) input.dwMask & 2) != 0)
        dxConnection.ItemName = input.szItemName;
      if (((int) input.dwMask & 1) != 0)
        dxConnection.ItemPath = input.szItemPath;
      if (((int) input.dwMask & 4) != 0)
        dxConnection.Version = input.szVersion;
      if (((int) input.dwMask & 8) != 0)
      {
        string[] unicodeStrings = OpcCom.Interop.GetUnicodeStrings(ref input.pszBrowsePaths, input.dwBrowsePathCount, deallocate);
        if (unicodeStrings != null)
          dxConnection.BrowsePaths.AddRange((ICollection) unicodeStrings);
      }
      if (((int) input.dwMask & 16) != 0)
        dxConnection.Name = input.szName;
      if (((int) input.dwMask & 32) != 0)
        dxConnection.Description = input.szDescription;
      if (((int) input.dwMask & 64) != 0)
        dxConnection.Keyword = input.szKeyword;
      if (((int) input.dwMask & 128) != 0)
      {
        dxConnection.DefaultSourceItemConnected = input.bDefaultSourceItemConnected != 0;
        dxConnection.DefaultSourceItemConnectedSpecified = true;
      }
      if (((int) input.dwMask & 256) != 0)
      {
        dxConnection.DefaultTargetItemConnected = input.bDefaultTargetItemConnected != 0;
        dxConnection.DefaultTargetItemConnectedSpecified = true;
      }
      if (((int) input.dwMask & 512) != 0)
      {
        dxConnection.DefaultOverridden = input.bDefaultOverridden != 0;
        dxConnection.DefaultOverriddenSpecified = true;
      }
      if (((int) input.dwMask & 1024) != 0)
        dxConnection.DefaultOverrideValue = input.vDefaultOverrideValue;
      if (((int) input.dwMask & 2048) != 0)
        dxConnection.SubstituteValue = input.vSubstituteValue;
      if (((int) input.dwMask & 4096) != 0)
      {
        dxConnection.EnableSubstituteValue = input.bEnableSubstituteValue != 0;
        dxConnection.EnableSubstituteValueSpecified = true;
      }
      if (((int) input.dwMask & 16384) != 0)
        dxConnection.TargetItemName = input.szTargetItemName;
      if (((int) input.dwMask & 8192) != 0)
        dxConnection.TargetItemPath = input.szTargetItemPath;
      if (((int) input.dwMask & 32768) != 0)
        dxConnection.SourceServerName = input.szSourceServerName;
      if (((int) input.dwMask & 131072) != 0)
        dxConnection.SourceItemName = input.szSourceItemName;
      if (((int) input.dwMask & 65536) != 0)
        dxConnection.SourceItemPath = input.szSourceItemPath;
      if (((int) input.dwMask & 262144) != 0)
      {
        dxConnection.SourceItemQueueSize = input.dwSourceItemQueueSize;
        dxConnection.SourceItemQueueSizeSpecified = true;
      }
      if (((int) input.dwMask & 524288) != 0)
      {
        dxConnection.UpdateRate = input.dwUpdateRate;
        dxConnection.UpdateRateSpecified = true;
      }
      if (((int) input.dwMask & 1048576) != 0)
      {
        dxConnection.Deadband = input.fltDeadBand;
        dxConnection.DeadbandSpecified = true;
      }
      if (((int) input.dwMask & 2097152) != 0)
        dxConnection.VendorData = input.szVendorData;
      return dxConnection;
    }

    internal static OpcRcw.Dx.ItemIdentifier[] GetItemIdentifiers(Opc.Dx.ItemIdentifier[] input)
    {
      OpcRcw.Dx.ItemIdentifier[] itemIdentifiers = (OpcRcw.Dx.ItemIdentifier[]) null;
      if (input != null && input.Length != 0)
      {
        itemIdentifiers = new OpcRcw.Dx.ItemIdentifier[input.Length];
        for (int index = 0; index < input.Length; ++index)
        {
          itemIdentifiers[index] = new OpcRcw.Dx.ItemIdentifier();
          itemIdentifiers[index].szItemName = input[index].ItemName;
          itemIdentifiers[index].szItemPath = input[index].ItemPath;
          itemIdentifiers[index].szVersion = input[index].Version;
        }
      }
      return itemIdentifiers;
    }
  }
}
