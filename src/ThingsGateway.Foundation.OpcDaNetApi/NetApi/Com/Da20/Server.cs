

using Opc;
using Opc.Da;
using OpcRcw.Comn;
using OpcRcw.Da;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da20
{
  public class Server : OpcCom.Da.Server
  {
    private bool m_disposed;
    private object m_group;
    private int m_groupHandle;
    private char[] m_separators;
    private object m_separatorsLock = new object();

    internal Server()
    {
    }

    public Server(URL url, object server)
      : base(url, server)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        lock (this)
        {
          if (disposing)
          {
            if (this.m_group != null)
            {
              try
              {
                ((IOPCServer) this.m_server).RemoveGroup(this.m_groupHandle, 0);
              }
              catch
              {
              }
            }
          }
          if (this.m_group != null)
          {
            OpcCom.Interop.ReleaseServer(this.m_group);
            this.m_group = (object) null;
            this.m_groupHandle = 0;
          }
        }
        this.m_disposed = true;
      }
      base.Dispose(disposing);
    }

    public override void Initialize(URL url, ConnectData connectData)
    {
      lock (this)
      {
        base.Initialize(url, connectData);
        this.m_separators = (char[]) null;
        try
        {
          int pdwLcid = 0;
          ((IOPCCommon) this.m_server).GetLocaleID(out pdwLcid);
          int pRevisedUpdateRate = 0;
          Guid guid = typeof (IOPCItemMgt).GUID;
          ((IOPCServer) this.m_server).AddGroup("", 1, 0, 0, IntPtr.Zero, IntPtr.Zero, pdwLcid, out this.m_groupHandle, out pRevisedUpdateRate, ref guid, out this.m_group);
        }
        catch (Exception ex)
        {
          this.Uninitialize();
          throw OpcCom.Interop.CreateException("IOPCServer.AddGroup", ex);
        }
      }
    }

    public override ItemValueResult[] Read(Item[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (items.Length == 0)
        return new ItemValueResult[0];
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        IdentifiedResult[] items1 = this.AddItems(items);
        ItemValueResult[] itemValueResultArray = new ItemValueResult[items.Length];
        try
        {
          ArrayList arrayList1 = new ArrayList(items.Length);
          ArrayList arrayList2 = new ArrayList(items.Length);
          ArrayList arrayList3 = new ArrayList(items.Length);
          ArrayList arrayList4 = new ArrayList(items.Length);
          for (int index = 0; index < items.Length; ++index)
          {
            itemValueResultArray[index] = new ItemValueResult((ItemIdentifier) items1[index]);
            if (items1[index].ResultID.Failed())
            {
              itemValueResultArray[index].ResultID = items1[index].ResultID;
              itemValueResultArray[index].DiagnosticInfo = items1[index].DiagnosticInfo;
            }
            else if (items[index].MaxAgeSpecified && (items[index].MaxAge < 0 || items[index].MaxAge == int.MaxValue))
            {
              arrayList1.Add((object) items[index]);
              arrayList2.Add((object) itemValueResultArray[index]);
            }
            else
            {
              arrayList3.Add((object) items[index]);
              arrayList4.Add((object) itemValueResultArray[index]);
            }
          }
          if (arrayList2.Count > 0)
          {
            try
            {
              int[] phServer = new int[arrayList2.Count];
              for (int index = 0; index < arrayList2.Count; ++index)
                phServer[index] = (int) ((ItemIdentifier) arrayList2[index]).ServerHandle;
              IntPtr ppErrors = IntPtr.Zero;
              ((IOPCItemMgt) this.m_group).SetActiveState(arrayList2.Count, phServer, 1, out ppErrors);
              Marshal.FreeCoTaskMem(ppErrors);
            }
            catch (Exception ex)
            {
              throw OpcCom.Interop.CreateException("IOPCItemMgt.SetActiveState", ex);
            }
            this.ReadValues((Item[]) arrayList1.ToArray(typeof (Item)), (ItemValueResult[]) arrayList2.ToArray(typeof (ItemValueResult)), true);
          }
          if (arrayList4.Count > 0)
            this.ReadValues((Item[]) arrayList3.ToArray(typeof (Item)), (ItemValueResult[]) arrayList4.ToArray(typeof (ItemValueResult)), false);
        }
        finally
        {
          this.RemoveItems(items1);
        }
        return itemValueResultArray;
      }
    }

    public override IdentifiedResult[] Write(ItemValue[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (items.Length == 0)
        return new IdentifiedResult[0];
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        Item[] items1 = new Item[items.Length];
        for (int index = 0; index < items.Length; ++index)
          items1[index] = new Item((ItemIdentifier) items[index]);
        IdentifiedResult[] items2 = this.AddItems(items1);
        try
        {
          ArrayList arrayList1 = new ArrayList(items.Length);
          ArrayList arrayList2 = new ArrayList(items.Length);
          for (int index = 0; index < items.Length; ++index)
          {
            if (!items2[index].ResultID.Failed())
            {
              if (items[index].QualitySpecified || items[index].TimestampSpecified)
              {
                items2[index].ResultID = ResultID.Da.E_NO_WRITEQT;
                items2[index].DiagnosticInfo = (string) null;
              }
              else
              {
                arrayList1.Add((object) items2[index]);
                arrayList2.Add((object) items[index]);
              }
            }
          }
          if (arrayList1.Count > 0)
          {
            int[] phServer = new int[arrayList1.Count];
            object[] pItemValues = new object[arrayList1.Count];
            for (int index = 0; index < phServer.Length; ++index)
            {
              phServer[index] = (int) ((ItemIdentifier) arrayList1[index]).ServerHandle;
              pItemValues[index] = OpcCom.Interop.GetVARIANT(((ItemValue) arrayList2[index]).Value);
            }
            IntPtr ppErrors = IntPtr.Zero;
            try
            {
              ((IOPCSyncIO) this.m_group).Write(arrayList1.Count, phServer, pItemValues, out ppErrors);
            }
            catch (Exception ex)
            {
              throw OpcCom.Interop.CreateException("IOPCSyncIO.Write", ex);
            }
            int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, arrayList1.Count, true);
            for (int index = 0; index < arrayList1.Count; ++index)
            {
              IdentifiedResult identifiedResult = (IdentifiedResult) arrayList1[index];
              identifiedResult.ResultID = OpcCom.Interop.GetResultID(int32s[index]);
              identifiedResult.DiagnosticInfo = (string) null;
              if (int32s[index] == -1073479674)
                items2[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
            }
          }
        }
        finally
        {
          this.RemoveItems(items2);
        }
        return items2;
      }
    }

    public override BrowseElement[] Browse(
      ItemIdentifier itemID,
      BrowseFilters filters,
      out Opc.Da.BrowsePosition position)
    {
      if (filters == null)
        throw new ArgumentNullException(nameof (filters));
      position = (Opc.Da.BrowsePosition) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        BrowsePosition position1 = (BrowsePosition) null;
        ArrayList arrayList = new ArrayList();
        if (filters.BrowseFilter != browseFilter.item)
        {
          BrowseElement[] elements = this.GetElements(arrayList.Count, itemID, filters, true, ref position1);
          if (elements != null)
            arrayList.AddRange((ICollection) elements);
          position = (Opc.Da.BrowsePosition) position1;
          if (position != null)
            return (BrowseElement[]) arrayList.ToArray(typeof (BrowseElement));
        }
        if (filters.BrowseFilter != browseFilter.branch)
        {
          BrowseElement[] elements = this.GetElements(arrayList.Count, itemID, filters, false, ref position1);
          if (elements != null)
            arrayList.AddRange((ICollection) elements);
          position = (Opc.Da.BrowsePosition) position1;
        }
        return (BrowseElement[]) arrayList.ToArray(typeof (BrowseElement));
      }
    }

    public override BrowseElement[] BrowseNext(ref Opc.Da.BrowsePosition position)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (position == null && position.GetType() != typeof (BrowsePosition))
          throw new BrowseCannotContinueException();
        BrowsePosition position1 = (BrowsePosition) position;
        ItemIdentifier itemId = position1.ItemID;
        BrowseFilters filters = position1.Filters;
        ArrayList arrayList = new ArrayList();
        if (position1.IsBranch)
        {
          BrowseElement[] elements = this.GetElements(arrayList.Count, itemId, filters, true, ref position1);
          if (elements != null)
            arrayList.AddRange((ICollection) elements);
          position = (Opc.Da.BrowsePosition) position1;
          if (position != null)
            return (BrowseElement[]) arrayList.ToArray(typeof (BrowseElement));
        }
        if (filters.BrowseFilter != browseFilter.branch)
        {
          BrowseElement[] elements = this.GetElements(arrayList.Count, itemId, filters, false, ref position1);
          if (elements != null)
            arrayList.AddRange((ICollection) elements);
          position = (Opc.Da.BrowsePosition) position1;
        }
        return (BrowseElement[]) arrayList.ToArray(typeof (BrowseElement));
      }
    }

    public override ItemPropertyCollection[] GetProperties(
      ItemIdentifier[] itemIDs,
      PropertyID[] propertyIDs,
      bool returnValues)
    {
      if (itemIDs == null)
        throw new ArgumentNullException(nameof (itemIDs));
      if (itemIDs.Length == 0)
        return new ItemPropertyCollection[0];
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        ItemPropertyCollection[] properties1 = new ItemPropertyCollection[itemIDs.Length];
        for (int index = 0; index < itemIDs.Length; ++index)
        {
          properties1[index] = new ItemPropertyCollection();
          properties1[index].ItemName = itemIDs[index].ItemName;
          properties1[index].ItemPath = itemIDs[index].ItemPath;
          try
          {
            ItemProperty[] properties2 = this.GetProperties(itemIDs[index].ItemName, propertyIDs, returnValues);
            if (properties2 != null)
              properties1[index].AddRange((ICollection) properties2);
            properties1[index].ResultID = ResultID.S_OK;
          }
          catch (ResultIDException ex)
          {
            properties1[index].ResultID = ex.Result;
          }
          catch (Exception ex)
          {
            properties1[index].ResultID = new ResultID((long) Marshal.GetHRForException(ex));
          }
        }
        return properties1;
      }
    }

    private IdentifiedResult[] AddItems(Item[] items)
    {
      int length = items.Length;
      OPCITEMDEF[] opcitemdeFs = OpcCom.Da.Interop.GetOPCITEMDEFs(items);
      for (int index = 0; index < opcitemdeFs.Length; ++index)
        opcitemdeFs[index].bActive = 0;
      IntPtr ppAddResults = IntPtr.Zero;
      IntPtr ppErrors = IntPtr.Zero;
      int pdwLcid = 0;
      ((IOPCCommon) this.m_server).GetLocaleID(out pdwLcid);
      GCHandle gcHandle = GCHandle.Alloc((object) pdwLcid, GCHandleType.Pinned);
      try
      {
        int pRevisedUpdateRate = 0;
        ((IOPCGroupStateMgt) this.m_group).SetState(IntPtr.Zero, out pRevisedUpdateRate, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, gcHandle.AddrOfPinnedObject(), IntPtr.Zero);
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCGroupStateMgt.SetState", ex);
      }
      finally
      {
        if (gcHandle.IsAllocated)
          gcHandle.Free();
      }
      try
      {
        ((IOPCItemMgt) this.m_group).AddItems(length, opcitemdeFs, out ppAddResults, out ppErrors);
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCItemMgt.AddItems", ex);
      }
      finally
      {
        if (gcHandle.IsAllocated)
          gcHandle.Free();
      }
      int[] itemResults = OpcCom.Da.Interop.GetItemResults(ref ppAddResults, length, true);
      int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, length, true);
      IdentifiedResult[] identifiedResultArray = new IdentifiedResult[length];
      for (int index = 0; index < length; ++index)
      {
        identifiedResultArray[index] = new IdentifiedResult((ItemIdentifier) items[index]);
        identifiedResultArray[index].ServerHandle = (object) null;
        identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
        identifiedResultArray[index].DiagnosticInfo = (string) null;
        if (identifiedResultArray[index].ResultID.Succeeded())
          identifiedResultArray[index].ServerHandle = (object) itemResults[index];
      }
      return identifiedResultArray;
    }

    private void RemoveItems(IdentifiedResult[] items)
    {
      try
      {
        ArrayList arrayList = new ArrayList(items.Length);
        foreach (IdentifiedResult identifiedResult in items)
        {
          if (identifiedResult.ResultID.Succeeded() && identifiedResult.ServerHandle.GetType() == typeof (int))
            arrayList.Add((object) (int) identifiedResult.ServerHandle);
        }
        if (arrayList.Count == 0)
          return;
        IntPtr ppErrors = IntPtr.Zero;
        ((IOPCItemMgt) this.m_group).RemoveItems(arrayList.Count, (int[]) arrayList.ToArray(typeof (int)), out ppErrors);
        OpcCom.Interop.GetInt32s(ref ppErrors, arrayList.Count, true);
      }
      catch
      {
      }
    }

    private void ReadValues(Item[] items, ItemValueResult[] results, bool cache)
    {
      if (items.Length == 0 || results.Length == 0)
        return;
      int[] phServer = new int[results.Length];
      for (int index = 0; index < results.Length; ++index)
        phServer[index] = System.Convert.ToInt32(results[index].ServerHandle);
      IntPtr ppItemValues = IntPtr.Zero;
      IntPtr ppErrors = IntPtr.Zero;
      try
      {
        ((IOPCSyncIO) this.m_group).Read(cache ? OPCDATASOURCE.OPC_DS_CACHE : OPCDATASOURCE.OPC_DS_DEVICE, results.Length, phServer, out ppItemValues, out ppErrors);
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCSyncIO.Read", ex);
      }
      ItemValue[] itemValues = OpcCom.Da.Interop.GetItemValues(ref ppItemValues, results.Length, true);
      int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, results.Length, true);
      this.GetLocale();
      for (int index = 0; index < results.Length; ++index)
      {
        results[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
        results[index].DiagnosticInfo = (string) null;
        if (results[index].ResultID.Succeeded())
        {
          results[index].Value = itemValues[index].Value;
          results[index].Quality = itemValues[index].Quality;
          results[index].QualitySpecified = itemValues[index].QualitySpecified;
          results[index].Timestamp = itemValues[index].Timestamp;
          results[index].TimestampSpecified = itemValues[index].TimestampSpecified;
        }
        if (int32s[index] == -1073479674)
          results[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
        if (results[index].Value != null)
        {
          if (items[index].ReqType != (System.Type) null)
          {
            try
            {
              results[index].Value = this.ChangeType(results[index].Value, items[index].ReqType, "en-US");
            }
            catch (Exception ex)
            {
              results[index].Value = (object) null;
              results[index].Quality = Quality.Bad;
              results[index].QualitySpecified = true;
              results[index].Timestamp = DateTime.MinValue;
              results[index].TimestampSpecified = false;
              results[index].ResultID = !(ex.GetType() == typeof (OverflowException)) ? OpcCom.Interop.GetResultID(-1073479676) : OpcCom.Interop.GetResultID(-1073479669);
            }
          }
        }
      }
    }

    private ItemProperty[] GetAvailableProperties(string itemID)
    {
      if (itemID == null || itemID.Length == 0)
        throw new ResultIDException(ResultID.Da.E_INVALID_ITEM_NAME);
      int pdwCount = 0;
      IntPtr ppPropertyIDs = IntPtr.Zero;
      IntPtr ppDescriptions = IntPtr.Zero;
      IntPtr ppvtDataTypes = IntPtr.Zero;
      try
      {
        ((IOPCItemProperties) this.m_server).QueryAvailableProperties(itemID, out pdwCount, out ppPropertyIDs, out ppDescriptions, out ppvtDataTypes);
      }
      catch (Exception ex)
      {
        throw new ResultIDException(ResultID.Da.E_UNKNOWN_ITEM_NAME);
      }
      int[] int32s = OpcCom.Interop.GetInt32s(ref ppPropertyIDs, pdwCount, true);
      short[] int16s = OpcCom.Interop.GetInt16s(ref ppvtDataTypes, pdwCount, true);
      string[] unicodeStrings = OpcCom.Interop.GetUnicodeStrings(ref ppDescriptions, pdwCount, true);
      if (pdwCount == 0)
        return (ItemProperty[]) null;
      ItemProperty[] availableProperties = new ItemProperty[pdwCount];
      for (int index = 0; index < pdwCount; ++index)
      {
        availableProperties[index] = new ItemProperty();
        availableProperties[index].ID = OpcCom.Da.Interop.GetPropertyID(int32s[index]);
        availableProperties[index].Description = unicodeStrings[index];
        availableProperties[index].DataType = OpcCom.Interop.GetType((VarEnum) int16s[index]);
        availableProperties[index].ItemName = (string) null;
        availableProperties[index].ItemPath = (string) null;
        availableProperties[index].ResultID = ResultID.S_OK;
        availableProperties[index].Value = (object) null;
      }
      return availableProperties;
    }

    private void GetItemIDs(string itemID, ItemProperty[] properties)
    {
      try
      {
        int[] pdwPropertyIDs = new int[properties.Length];
        for (int index = 0; index < properties.Length; ++index)
          pdwPropertyIDs[index] = properties[index].ID.Code;
        IntPtr ppszNewItemIDs = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        ((IOPCItemProperties) this.m_server).LookupItemIDs(itemID, properties.Length, pdwPropertyIDs, out ppszNewItemIDs, out ppErrors);
        string[] unicodeStrings = OpcCom.Interop.GetUnicodeStrings(ref ppszNewItemIDs, properties.Length, true);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, properties.Length, true);
        for (int index = 0; index < properties.Length; ++index)
        {
          properties[index].ItemName = (string) null;
          properties[index].ItemPath = (string) null;
          if (int32s[index] >= 0)
            properties[index].ItemName = unicodeStrings[index];
        }
      }
      catch
      {
        foreach (ItemProperty property in properties)
        {
          property.ItemName = (string) null;
          property.ItemPath = (string) null;
        }
      }
    }

    private void GetValues(string itemID, ItemProperty[] properties)
    {
      try
      {
        int[] pdwPropertyIDs = new int[properties.Length];
        for (int index = 0; index < properties.Length; ++index)
          pdwPropertyIDs[index] = properties[index].ID.Code;
        IntPtr ppvData = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        ((IOPCItemProperties) this.m_server).GetItemProperties(itemID, properties.Length, pdwPropertyIDs, out ppvData, out ppErrors);
        object[] varianTs = OpcCom.Interop.GetVARIANTs(ref ppvData, properties.Length, true);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, properties.Length, true);
        for (int index = 0; index < properties.Length; ++index)
        {
          properties[index].Value = (object) null;
          ResultID resultId = properties[index].ResultID;
          if (resultId.Succeeded())
          {
            properties[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
            if (int32s[index] == -1073479674)
              properties[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
            resultId = properties[index].ResultID;
            if (resultId.Succeeded())
              properties[index].Value = OpcCom.Da.Interop.UnmarshalPropertyValue(properties[index].ID, varianTs[index]);
          }
        }
      }
      catch (Exception ex)
      {
        ResultID resultId = new ResultID((long) Marshal.GetHRForException(ex));
        foreach (ItemProperty property in properties)
        {
          property.Value = (object) null;
          property.ResultID = resultId;
        }
      }
    }

    private ItemProperty[] GetProperties(
      string itemID,
      PropertyID[] propertyIDs,
      bool returnValues)
    {
      ItemProperty[] properties;
      if (propertyIDs == null)
      {
        properties = this.GetAvailableProperties(itemID);
      }
      else
      {
        ItemProperty[] availableProperties = this.GetAvailableProperties(itemID);
        properties = new ItemProperty[propertyIDs.Length];
        for (int index = 0; index < propertyIDs.Length; ++index)
        {
          foreach (ItemProperty itemProperty in availableProperties)
          {
            if (itemProperty.ID == propertyIDs[index])
            {
              properties[index] = (ItemProperty) itemProperty.Clone();
              properties[index].ID = propertyIDs[index];
              break;
            }
          }
          if (properties[index] == null)
          {
            properties[index] = new ItemProperty();
            properties[index].ID = propertyIDs[index];
            properties[index].ResultID = ResultID.Da.E_INVALID_PID;
          }
        }
      }
      if (properties != null)
      {
        this.GetItemIDs(itemID, properties);
        if (returnValues)
          this.GetValues(itemID, properties);
      }
      return properties;
    }

    private EnumString GetEnumerator(
      string itemID,
      BrowseFilters filters,
      bool branches,
      bool flat)
    {
      IOPCBrowseServerAddressSpace server = (IOPCBrowseServerAddressSpace) this.m_server;
      if (!flat)
      {
        string szString = itemID != null ? itemID : "";
        try
        {
          server.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, szString);
        }
        catch
        {
          try
          {
            server.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, szString);
          }
          catch
          {
            while (true)
            {
              try
              {
                server.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_UP, string.Empty);
              }
              catch
              {
                break;
              }
            }
            string[] strArray = (string[]) null;
            lock (this.m_separatorsLock)
              strArray = this.m_separators == null ? szString.Split(this.m_separators) : szString.Split(this.m_separators);
            for (int index = 0; index < strArray.Length; ++index)
            {
              if (strArray[index] != null)
              {
                if (strArray[index].Length != 0)
                {
                  try
                  {
                    server.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, strArray[index]);
                  }
                  catch
                  {
                    throw new ResultIDException(ResultID.Da.E_UNKNOWN_ITEM_NAME, "Cannot browse because the server is not compliant because it does not support the BROWSE_TO function.");
                  }
                }
              }
            }
          }
        }
      }
      try
      {
        IEnumString ppIEnumString = (IEnumString) null;
        OPCBROWSETYPE dwBrowseFilterType = branches ? OPCBROWSETYPE.OPC_BRANCH : OPCBROWSETYPE.OPC_LEAF;
        if (flat)
          dwBrowseFilterType = OPCBROWSETYPE.OPC_FLAT;
        server.BrowseOPCItemIDs(dwBrowseFilterType, filters.ElementNameFilter != null ? filters.ElementNameFilter : "", (short) 0, 0, out ppIEnumString);
        return new EnumString((object) ppIEnumString);
      }
      catch
      {
        throw new ResultIDException(ResultID.Da.E_UNKNOWN_ITEM_NAME);
      }
    }

    private void DetectAndSaveSeparators(string browseName, string itemID)
    {
      if (!itemID.EndsWith(browseName))
        return;
      char ch = itemID[itemID.Length - browseName.Length - 1];
      lock (this.m_separatorsLock)
      {
        int num = -1;
        if (this.m_separators != null)
        {
          for (int index = 0; index < this.m_separators.Length; ++index)
          {
            if ((int) this.m_separators[index] == (int) ch)
            {
              num = index;
              break;
            }
          }
          if (num == -1)
          {
            char[] destinationArray = new char[this.m_separators.Length + 1];
            Array.Copy((Array) this.m_separators, (Array) destinationArray, this.m_separators.Length);
            this.m_separators = destinationArray;
          }
        }
        if (num != -1)
          return;
        if (this.m_separators == null)
          this.m_separators = new char[1];
        this.m_separators[this.m_separators.Length - 1] = ch;
      }
    }

    private BrowseElement GetElement(
      ItemIdentifier itemID,
      string name,
      BrowseFilters filters,
      bool isBranch)
    {
      if (name == null)
        return (BrowseElement) null;
      BrowseElement element = new BrowseElement();
      element.Name = name;
      element.HasChildren = isBranch;
      element.ItemPath = (string) null;
      try
      {
        string szItemID = (string) null;
        ((IOPCBrowseServerAddressSpace) this.m_server).GetItemID(element.Name, out szItemID);
        element.ItemName = szItemID;
        if (element.ItemName != null)
          this.DetectAndSaveSeparators(element.Name, element.ItemName);
      }
      catch
      {
        element.ItemName = name;
      }
      try
      {
        OPCITEMDEF opcitemdef = new OPCITEMDEF();
        opcitemdef.szItemID = element.ItemName;
        opcitemdef.szAccessPath = (string) null;
        opcitemdef.hClient = 0;
        opcitemdef.bActive = 0;
        opcitemdef.vtRequestedDataType = (short) 0;
        opcitemdef.dwBlobSize = 0;
        opcitemdef.pBlob = IntPtr.Zero;
        IntPtr ppValidationResults = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        ((IOPCItemMgt) this.m_group).ValidateItems(1, new OPCITEMDEF[1]
        {
          opcitemdef
        }, 0, out ppValidationResults, out ppErrors);
        OpcCom.Da.Interop.GetItemResults(ref ppValidationResults, 1, true);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, 1, true);
        element.IsItem = int32s[0] >= 0;
      }
      catch
      {
        element.IsItem = false;
      }
      try
      {
        if (filters.ReturnAllProperties)
          element.Properties = this.GetProperties(element.ItemName, (PropertyID[]) null, filters.ReturnPropertyValues);
        else if (filters.PropertyIDs != null)
          element.Properties = this.GetProperties(element.ItemName, filters.PropertyIDs, filters.ReturnPropertyValues);
      }
      catch
      {
        element.Properties = (ItemProperty[]) null;
      }
      return element;
    }

    private BrowseElement[] GetElements(
      int elementsFound,
      ItemIdentifier itemID,
      BrowseFilters filters,
      bool branches,
      ref BrowsePosition position)
    {
      EnumString enumerator;
      if (position == null)
      {
        IOPCBrowseServerAddressSpace server = (IOPCBrowseServerAddressSpace) this.m_server;
        OPCNAMESPACETYPE pNameSpaceType = OPCNAMESPACETYPE.OPC_NS_HIERARCHIAL;
        try
        {
          server.QueryOrganization(out pNameSpaceType);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCBrowseServerAddressSpace.QueryOrganization", ex);
        }
        if (pNameSpaceType == OPCNAMESPACETYPE.OPC_NS_FLAT)
        {
          if (branches)
            return new BrowseElement[0];
          if (itemID != null && itemID.ItemName != null && itemID.ItemName.Length > 0)
            throw new ResultIDException(ResultID.Da.E_UNKNOWN_ITEM_NAME);
        }
        enumerator = this.GetEnumerator(itemID?.ItemName, filters, branches, pNameSpaceType == OPCNAMESPACETYPE.OPC_NS_FLAT);
      }
      else
        enumerator = position.Enumerator;
      ArrayList arrayList = new ArrayList();
      int num = 0;
      string[] strArray = (string[]) null;
      if (position != null)
      {
        num = position.Index;
        strArray = position.Names;
        position = (BrowsePosition) null;
      }
      do
      {
        if (strArray != null)
        {
          for (int index = num; index < strArray.Length; ++index)
          {
            if (filters.MaxElementsReturned != 0 && filters.MaxElementsReturned == arrayList.Count + elementsFound)
            {
              position = new BrowsePosition(itemID, filters, enumerator, branches);
              position.Names = strArray;
              position.Index = index;
              break;
            }
            BrowseElement element = this.GetElement(itemID, strArray[index], filters, branches);
            if (element != null)
              arrayList.Add((object) element);
            else
              break;
          }
        }
        if (position == null)
        {
          strArray = enumerator.Next(10);
          num = 0;
        }
        else
          break;
      }
      while (strArray != null && strArray.Length != 0);
      if (position == null)
        enumerator.Dispose();
      return (BrowseElement[]) arrayList.ToArray(typeof (BrowseElement));
    }

    protected override OpcCom.Da.Subscription CreateSubscription(
      object group,
      SubscriptionState state,
      int filters)
    {
      return (OpcCom.Da.Subscription) new Subscription(group, state, filters);
    }
  }
}
