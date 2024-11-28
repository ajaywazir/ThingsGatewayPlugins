

using Opc;
using Opc.Da;
using OpcRcw.Comn;
using OpcRcw.Da;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
  public class Server : 
    ConnectionPointContainer,
    IOPCCommon,
    IOPCServer,
    IOPCBrowseServerAddressSpace,
    IOPCItemProperties,
    IOPCBrowse,
    IOPCItemIO,
    IOPCWrappedServer
  {
    private Opc.Da.IServer m_server;
    private Hashtable m_groups = new Hashtable();
    private Hashtable m_continuationPoints = new Hashtable();
    private Stack m_browseStack = new Stack();
    private int m_lcid = 2048;
    private int m_nextHandle = 1;

    protected Server() => this.RegisterInterface(typeof (IOPCShutdown).GUID);

    public Opc.Da.IServer IServer
    {
      get => this.m_server;
      set => this.m_server = value;
    }

    public int SetGroupName(string oldName, string newName)
    {
      lock (this)
      {
        Group group = (Group) this.m_groups[(object) oldName];
        if (newName == null || newName.Length == 0 || group == null)
          return -2147024809;
        if (this.m_groups.Contains((object) newName))
          return -1073479668;
        this.m_groups.Remove((object) oldName);
        this.m_groups[(object) newName] = (object) group;
        return 0;
      }
    }

    public static Exception CreateException(Exception e)
    {
      if (typeof (ExternalException).IsInstanceOfType((object) e))
        return e;
      return typeof (ResultIDException).IsInstanceOfType((object) e) ? (Exception) new ExternalException(e.Message, OpcCom.Interop.GetResultID(((ResultIDException) e).Result)) : (Exception) new ExternalException(e.Message, -2147467259);
    }

    public static Exception CreateException(int code)
    {
      return (Exception) new ExternalException(string.Format("0x{0:X8}", (object) code), code);
    }

    internal Group CreateGroup(ref SubscriptionState state, int lcid, int timebias)
    {
      lock (this)
      {
        ISubscription subscription = this.m_server.CreateSubscription(state);
        state = subscription.GetState();
        if (state == null)
          throw Server.CreateException(-2147467259);
        if (this.m_groups.Contains((object) state.Name))
        {
          this.m_server.CancelSubscription(subscription);
          throw new ExternalException("E_DUPLICATENAME", -1073479668);
        }
        Group group = new Group(this, state.Name, ++this.m_nextHandle, lcid, timebias, subscription);
        this.m_groups[(object) state.Name] = (object) group;
        return group;
      }
    }

    public virtual void Load(Guid clsid)
    {
    }

    public virtual void Unload()
    {
    }

    public void SetLocaleID(int dwLcid)
    {
      lock (this)
      {
        try
        {
          this.m_server.SetLocale(OpcCom.Interop.GetLocale(dwLcid));
          this.m_lcid = dwLcid;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void QueryAvailableLocaleIDs(out int pdwCount, out IntPtr pdwLcid)
    {
      lock (this)
      {
        try
        {
          pdwCount = 0;
          pdwLcid = IntPtr.Zero;
          string[] supportedLocales = this.m_server.GetSupportedLocales();
          if (supportedLocales == null || supportedLocales.Length == 0)
            return;
          pdwLcid = Marshal.AllocCoTaskMem(supportedLocales.Length * Marshal.SizeOf(typeof (int)));
          int[] source = new int[supportedLocales.Length];
          for (int index = 0; index < supportedLocales.Length; ++index)
            source[index] = OpcCom.Interop.GetLocale(supportedLocales[index]);
          Marshal.Copy(source, 0, pdwLcid, supportedLocales.Length);
          pdwCount = supportedLocales.Length;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetLocaleID(out int pdwLcid)
    {
      lock (this)
      {
        try
        {
          pdwLcid = this.m_lcid;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    void IOPCCommon.GetErrorString(int dwError, out string ppString)
    {
      lock (this)
      {
        try
        {
          ppString = this.m_server.GetErrorText(this.m_server.GetLocale(), OpcCom.Interop.GetResultID(dwError));
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void SetClientName(string szName)
    {
    }

    public void GetGroupByName(string szName, ref Guid riid, out object ppUnk)
    {
      lock (this)
      {
        try
        {
          foreach (Group group in (IEnumerable) this.m_groups.Values)
          {
            if (group.Name == szName)
            {
              ppUnk = (object) group;
              return;
            }
          }
          throw new ExternalException("E_INVALIDARG", -2147024809);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetErrorString(int dwError, int dwLocale, out string ppString)
    {
      lock (this)
      {
        try
        {
          ppString = this.m_server.GetErrorText(OpcCom.Interop.GetLocale(dwLocale), OpcCom.Interop.GetResultID(dwError));
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void RemoveGroup(int hServerGroup, int bForce)
    {
      lock (this)
      {
        try
        {
          foreach (Group group in (IEnumerable) this.m_groups.Values)
          {
            if (group.ServerHandle == hServerGroup)
            {
              this.m_groups.Remove((object) group.Name);
              group.Dispose();
              return;
            }
          }
          throw new ExternalException("E_FAIL", -2147467259);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void CreateGroupEnumerator(OPCENUMSCOPE dwScope, ref Guid riid, out object ppUnk)
    {
      lock (this)
      {
        try
        {
          if (dwScope == OPCENUMSCOPE.OPC_ENUM_PUBLIC_CONNECTIONS || dwScope == OPCENUMSCOPE.OPC_ENUM_PUBLIC)
          {
            if (riid == typeof (IEnumString).GUID)
            {
              ppUnk = (object) new EnumString((ICollection) null);
            }
            else
            {
              if (!(riid == typeof (IEnumUnknown).GUID))
                throw new ExternalException("E_NOINTERFACE", -2147467262);
              ppUnk = (object) new EnumUnknown((ICollection) null);
            }
          }
          else if (riid == typeof (IEnumUnknown).GUID)
          {
            ppUnk = (object) new EnumUnknown((ICollection) this.m_groups);
          }
          else
          {
            if (!(riid == typeof (IEnumString).GUID))
              throw new ExternalException("E_NOINTERFACE", -2147467262);
            ArrayList strings = new ArrayList(this.m_groups.Count);
            foreach (Group group in (IEnumerable) this.m_groups.Values)
              strings.Add((object) group.Name);
            ppUnk = (object) new EnumString((ICollection) strings);
          }
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void AddGroup(
      string szName,
      int bActive,
      int dwRequestedUpdateRate,
      int hClientGroup,
      IntPtr pTimeBias,
      IntPtr pPercentDeadband,
      int dwLCID,
      out int phServerGroup,
      out int pRevisedUpdateRate,
      ref Guid riid,
      out object ppUnk)
    {
      lock (this)
      {
        try
        {
          SubscriptionState state = new SubscriptionState();
          state.Name = szName;
          state.ServerHandle = (object) null;
          state.ClientHandle = (object) hClientGroup;
          state.Active = bActive != 0;
          state.Deadband = 0.0f;
          state.KeepAlive = 0;
          state.Locale = OpcCom.Interop.GetLocale(dwLCID);
          state.UpdateRate = dwRequestedUpdateRate;
          if (pPercentDeadband != IntPtr.Zero)
          {
            float[] destination = new float[1];
            Marshal.Copy(pPercentDeadband, destination, 0, 1);
            state.Deadband = destination[0];
          }
          DateTime now = DateTime.Now;
          int timebias = (int) -TimeZoneInfo.Local.GetUtcOffset(now).TotalMinutes;
          if (TimeZoneInfo.Local.IsDaylightSavingTime(now))
            timebias += 60;
          if (pTimeBias != IntPtr.Zero)
            timebias = Marshal.ReadInt32(pTimeBias);
          Group group = this.CreateGroup(ref state, dwLCID, timebias);
          phServerGroup = group.ServerHandle;
          pRevisedUpdateRate = state.UpdateRate;
          ppUnk = (object) group;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetStatus(out IntPtr ppServerStatus)
    {
      lock (this)
      {
        try
        {
          OPCSERVERSTATUS serverStatus = OpcCom.Da.Interop.GetServerStatus(this.m_server.GetStatus(), this.m_groups.Count);
          ppServerStatus = Marshal.AllocCoTaskMem(Marshal.SizeOf(serverStatus.GetType()));
          Marshal.StructureToPtr<OPCSERVERSTATUS>(serverStatus, ppServerStatus, false);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Browse(
      string szItemID,
      ref IntPtr pszContinuationPoint,
      int dwMaxElementsReturned,
      OPCBROWSEFILTER dwBrowseFilter,
      string szElementNameFilter,
      string szVendorFilter,
      int bReturnAllProperties,
      int bReturnPropertyValues,
      int dwPropertyCount,
      int[] pdwPropertyIDs,
      out int pbMoreElements,
      out int pdwCount,
      out IntPtr ppBrowseElements)
    {
      lock (this)
      {
        try
        {
          ItemIdentifier itemID = new ItemIdentifier(szItemID);
          BrowseFilters filters = new BrowseFilters();
          filters.MaxElementsReturned = dwMaxElementsReturned;
          filters.BrowseFilter = OpcCom.Da.Interop.GetBrowseFilter(dwBrowseFilter);
          filters.ElementNameFilter = szElementNameFilter;
          filters.VendorFilter = szVendorFilter;
          filters.ReturnAllProperties = bReturnAllProperties != 0;
          filters.ReturnPropertyValues = bReturnPropertyValues != 0;
          filters.PropertyIDs = OpcCom.Da.Interop.GetPropertyIDs(pdwPropertyIDs);
          Opc.Da.BrowsePosition position = (Opc.Da.BrowsePosition) null;
          string key = (string) null;
          if (pszContinuationPoint != IntPtr.Zero)
            key = Marshal.PtrToStringUni(pszContinuationPoint);
          BrowseElement[] input;
          if (key == null || key.Length == 0)
          {
            input = this.m_server.Browse(itemID, filters, out position);
          }
          else
          {
            Server.ContinuationPoint continuationPoint = (Server.ContinuationPoint) this.m_continuationPoints[(object) key];
            if (continuationPoint != null)
            {
              position = continuationPoint.Position;
              this.m_continuationPoints.Remove((object) key);
            }
            if (position == null)
              throw new ExternalException("E_INVALIDCONTINUATIONPOINT", -1073478653);
            Marshal.FreeCoTaskMem(pszContinuationPoint);
            pszContinuationPoint = IntPtr.Zero;
            position.MaxElementsReturned = dwMaxElementsReturned;
            input = this.m_server.BrowseNext(ref position);
          }
          this.CleanupContinuationPoints();
          if (position != null)
          {
            string str = Guid.NewGuid().ToString();
            this.m_continuationPoints[(object) str] = (object) new Server.ContinuationPoint(position);
            pszContinuationPoint = Marshal.StringToCoTaskMemUni(str);
          }
          if (pszContinuationPoint == IntPtr.Zero)
            pszContinuationPoint = Marshal.StringToCoTaskMemUni(string.Empty);
          pbMoreElements = 0;
          pdwCount = 0;
          ppBrowseElements = IntPtr.Zero;
          if (input == null)
            return;
          pdwCount = input.Length;
          ppBrowseElements = OpcCom.Da.Interop.GetBrowseElements(input, dwPropertyCount > 0);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetProperties(
      int dwItemCount,
      string[] pszItemIDs,
      int bReturnPropertyValues,
      int dwPropertyCount,
      int[] pdwPropertyIDs,
      out IntPtr ppItemProperties)
    {
      lock (this)
      {
        try
        {
          if (dwItemCount == 0 || pszItemIDs == null)
            throw new ExternalException("E_INVALIDARG", -2147024809);
          ppItemProperties = IntPtr.Zero;
          ItemIdentifier[] itemIDs = new ItemIdentifier[dwItemCount];
          for (int index = 0; index < dwItemCount; ++index)
            itemIDs[index] = new ItemIdentifier(pszItemIDs[index]);
          PropertyID[] propertyIDs = (PropertyID[]) null;
          if (dwPropertyCount > 0 && pdwPropertyIDs != null)
            propertyIDs = OpcCom.Da.Interop.GetPropertyIDs(pdwPropertyIDs);
          ItemPropertyCollection[] properties = this.m_server.GetProperties(itemIDs, propertyIDs, bReturnPropertyValues != 0);
          if (properties == null)
            return;
          ppItemProperties = OpcCom.Da.Interop.GetItemPropertyCollections(properties);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetItemID(string szItemDataID, out string szItemID)
    {
      lock (this)
      {
        try
        {
          if (szItemDataID == null || szItemDataID.Length == 0)
          {
            if (this.m_browseStack.Count == 0)
              szItemID = "";
            else
              szItemID = ((ItemIdentifier) this.m_browseStack.Peek()).ItemName;
          }
          else if (this.IsItem(szItemDataID))
            szItemID = szItemDataID;
          else
            szItemID = (this.FindChild(szItemDataID) ?? throw Server.CreateException(-2147024809)).ItemName;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void BrowseAccessPaths(string szItemID, out IEnumString ppIEnumString)
    {
      lock (this)
      {
        try
        {
          throw new ExternalException(nameof (BrowseAccessPaths), -2147467263);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void QueryOrganization(out OPCNAMESPACETYPE pNameSpaceType)
    {
      lock (this)
      {
        try
        {
          pNameSpaceType = OPCNAMESPACETYPE.OPC_NS_HIERARCHIAL;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void ChangeBrowsePosition(OPCBROWSEDIRECTION dwBrowseDirection, string szString)
    {
      lock (this)
      {
        try
        {
          BrowseFilters filters = new BrowseFilters();
          filters.MaxElementsReturned = 0;
          filters.BrowseFilter = browseFilter.all;
          filters.ElementNameFilter = (string) null;
          filters.VendorFilter = (string) null;
          filters.ReturnAllProperties = false;
          filters.PropertyIDs = (PropertyID[]) null;
          filters.ReturnPropertyValues = false;
          Opc.Da.BrowsePosition position = (Opc.Da.BrowsePosition) null;
          switch (dwBrowseDirection)
          {
            case OPCBROWSEDIRECTION.OPC_BROWSE_UP:
              ItemIdentifier itemID1 = this.m_browseStack.Count != 0 ? (ItemIdentifier) this.m_browseStack.Pop() : throw Server.CreateException(-2147467259);
              if (this.m_browseStack.Count > 0 && this.m_browseStack.Peek() == null)
              {
                this.BuildBrowseStack(itemID1);
                break;
              }
              break;
            case OPCBROWSEDIRECTION.OPC_BROWSE_DOWN:
              BrowseElement browseElement = szString != null && szString.Length != 0 ? this.FindChild(szString) : throw Server.CreateException(-2147024809);
              if (browseElement == null || !browseElement.HasChildren)
                throw Server.CreateException(-2147024809);
              this.m_browseStack.Push((object) new ItemIdentifier(browseElement.ItemName));
              break;
            case OPCBROWSEDIRECTION.OPC_BROWSE_TO:
              if (szString == null || szString.Length == 0)
              {
                this.m_browseStack.Clear();
                break;
              }
              ItemIdentifier itemID2 = new ItemIdentifier(szString);
              BrowseElement[] browseElementArray;
              try
              {
                browseElementArray = this.m_server.Browse(itemID2, filters, out position);
              }
              catch (Exception ex)
              {
                throw Server.CreateException(-2147024809);
              }
              if (browseElementArray == null || browseElementArray.Length == 0)
                throw Server.CreateException(-2147024809);
              this.m_browseStack.Clear();
              this.m_browseStack.Push((object) null);
              this.m_browseStack.Push((object) itemID2);
              break;
          }
          position?.Dispose();
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void BrowseOPCItemIDs(
      OPCBROWSETYPE dwBrowseFilterType,
      string szFilterCriteria,
      short vtDataTypeFilter,
      int dwAccessRightsFilter,
      out IEnumString ppIEnumString)
    {
      lock (this)
      {
        try
        {
          ItemIdentifier itemID = (ItemIdentifier) null;
          if (this.m_browseStack.Count > 0)
            itemID = (ItemIdentifier) this.m_browseStack.Peek();
          ArrayList arrayList = new ArrayList();
          this.Browse(itemID, dwBrowseFilterType, szFilterCriteria, vtDataTypeFilter, dwAccessRightsFilter, arrayList);
          ppIEnumString = (IEnumString) new EnumString((ICollection) arrayList);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void LookupItemIDs(
      string szItemID,
      int dwCount,
      int[] pdwPropertyIDs,
      out IntPtr ppszNewItemIDs,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        try
        {
          if (szItemID == null || szItemID.Length == 0 || dwCount == 0 || pdwPropertyIDs == null)
            throw Server.CreateException(-2147024809);
          ItemIdentifier[] itemIDs = new ItemIdentifier[1]
          {
            new ItemIdentifier(szItemID)
          };
          PropertyID[] propertyIDs = new PropertyID[pdwPropertyIDs.Length];
          for (int index = 0; index < propertyIDs.Length; ++index)
            propertyIDs[index] = OpcCom.Da.Interop.GetPropertyID(pdwPropertyIDs[index]);
          ItemPropertyCollection[] properties = this.m_server.GetProperties(itemIDs, propertyIDs, false);
          if (properties == null || properties.Length != 1)
            throw Server.CreateException(-2147467259);
          string[] values = !properties[0].ResultID.Failed() ? new string[properties[0].Count] : throw new ResultIDException(properties[0].ResultID);
          for (int index = 0; index < properties[0].Count; ++index)
          {
            ItemProperty itemProperty = properties[0][index];
            PropertyID propertyId = itemProperty.ID;
            int code1 = propertyId.Code;
            propertyId = Property.EUINFO;
            int code2 = propertyId.Code;
            if (code1 <= code2)
              itemProperty.ResultID = ResultID.Da.E_INVALID_PID;
            if (itemProperty.ResultID.Succeeded())
              values[index] = itemProperty.ItemName;
          }
          ppszNewItemIDs = OpcCom.Interop.GetUnicodeStrings(values);
          ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) properties[0].ToArray(typeof (IResult)));
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void QueryAvailableProperties(
      string szItemID,
      out int pdwCount,
      out IntPtr ppPropertyIDs,
      out IntPtr ppDescriptions,
      out IntPtr ppvtDataTypes)
    {
      lock (this)
      {
        try
        {
          ItemPropertyCollection[] propertyCollectionArray = szItemID != null && szItemID.Length != 0 ? this.m_server.GetProperties(new ItemIdentifier[1]
          {
            new ItemIdentifier(szItemID)
          }, (PropertyID[]) null, false) : throw new ExternalException(nameof (QueryAvailableProperties), -2147024809);
          if (propertyCollectionArray == null || propertyCollectionArray.Length != 1)
            throw new ExternalException("LookupItemIDs", -2147467259);
          int[] input1 = !propertyCollectionArray[0].ResultID.Failed() ? new int[propertyCollectionArray[0].Count] : throw new ResultIDException(propertyCollectionArray[0].ResultID);
          string[] values = new string[propertyCollectionArray[0].Count];
          short[] input2 = new short[propertyCollectionArray[0].Count];
          for (int index = 0; index < propertyCollectionArray[0].Count; ++index)
          {
            ItemProperty itemProperty = propertyCollectionArray[0][index];
            if (itemProperty.ResultID.Succeeded())
            {
              input1[index] = itemProperty.ID.Code;
              PropertyDescription propertyDescription = PropertyDescription.Find(itemProperty.ID);
              if (propertyDescription != null)
              {
                values[index] = propertyDescription.Name;
                input2[index] = (short) OpcCom.Interop.GetType(propertyDescription.Type);
              }
            }
          }
          pdwCount = input1.Length;
          ppPropertyIDs = OpcCom.Interop.GetInt32s(input1);
          ppDescriptions = OpcCom.Interop.GetUnicodeStrings(values);
          ppvtDataTypes = OpcCom.Interop.GetInt16s(input2);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetItemProperties(
      string szItemID,
      int dwCount,
      int[] pdwPropertyIDs,
      out IntPtr ppvData,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        try
        {
          if (dwCount == 0 || pdwPropertyIDs == null)
            throw Server.CreateException(-2147024809);
          ItemIdentifier[] itemIDs = szItemID != null && szItemID.Length != 0 ? new ItemIdentifier[1]
          {
            new ItemIdentifier(szItemID)
          } : throw Server.CreateException(-1073479672);
          PropertyID[] propertyIDs = new PropertyID[pdwPropertyIDs.Length];
          for (int index = 0; index < propertyIDs.Length; ++index)
            propertyIDs[index] = OpcCom.Da.Interop.GetPropertyID(pdwPropertyIDs[index]);
          ItemPropertyCollection[] properties = this.m_server.GetProperties(itemIDs, propertyIDs, true);
          if (properties == null || properties.Length != 1)
            throw Server.CreateException(-2147467259);
          object[] values = !properties[0].ResultID.Failed() ? new object[properties[0].Count] : throw new ResultIDException(properties[0].ResultID);
          for (int index = 0; index < properties[0].Count; ++index)
          {
            ItemProperty itemProperty = properties[0][index];
            if (itemProperty.ResultID.Succeeded())
              values[index] = OpcCom.Da.Interop.MarshalPropertyValue(itemProperty.ID, itemProperty.Value);
          }
          ppvData = OpcCom.Interop.GetVARIANTs(values, false);
          ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) properties[0].ToArray(typeof (IResult)));
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void WriteVQT(
      int dwCount,
      string[] pszItemIDs,
      OPCITEMVQT[] pItemVQT,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (dwCount != 0 && pszItemIDs != null)
        {
          if (pItemVQT != null)
          {
            try
            {
              ItemValue[] values = new ItemValue[dwCount];
              for (int index = 0; index < values.Length; ++index)
              {
                values[index] = new ItemValue(new ItemIdentifier(pszItemIDs[index]));
                values[index].Value = pItemVQT[index].vDataValue;
                values[index].Quality = new Quality(pItemVQT[index].wQuality);
                values[index].QualitySpecified = pItemVQT[index].bQualitySpecified != 0;
                values[index].Timestamp = OpcCom.Interop.GetFILETIME(OpcCom.Da.Interop.Convert(pItemVQT[index].ftTimeStamp));
                values[index].TimestampSpecified = pItemVQT[index].bTimeStampSpecified != 0;
              }
              IdentifiedResult[] results = this.m_server.Write(values);
              if (results == null || results.Length != values.Length)
                throw Server.CreateException(-2147467259);
              ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) results);
              return;
            }
            catch (Exception ex)
            {
              throw Server.CreateException(ex);
            }
          }
        }
        throw Server.CreateException(-2147024809);
      }
    }

    public void Read(
      int dwCount,
      string[] pszItemIDs,
      int[] pdwMaxAge,
      out IntPtr ppvValues,
      out IntPtr ppwQualities,
      out IntPtr ppftTimeStamps,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (dwCount != 0 && pszItemIDs != null)
        {
          if (pdwMaxAge != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item(new ItemIdentifier(pszItemIDs[index]));
                items[index].MaxAge = pdwMaxAge[index] < 0 ? int.MaxValue : pdwMaxAge[index];
                items[index].MaxAgeSpecified = true;
              }
              ItemValueResult[] results = this.m_server.Read(items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              object[] values = new object[results.Length];
              short[] input = new short[results.Length];
              DateTime[] datetimes = new DateTime[results.Length];
              for (int index = 0; index < results.Length; ++index)
              {
                values[index] = results[index].Value;
                input[index] = results[index].QualitySpecified ? results[index].Quality.GetCode() : (short) 0;
                datetimes[index] = results[index].TimestampSpecified ? results[index].Timestamp : DateTime.MinValue;
              }
              ppvValues = OpcCom.Interop.GetVARIANTs(values, false);
              ppwQualities = OpcCom.Interop.GetInt16s(input);
              ppftTimeStamps = OpcCom.Interop.GetFILETIMEs(datetimes);
              ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) results);
              return;
            }
            catch (Exception ex)
            {
              throw Server.CreateException(ex);
            }
          }
        }
        throw Server.CreateException(-2147024809);
      }
    }

    private void CleanupContinuationPoints()
    {
      ArrayList arrayList = new ArrayList();
      foreach (DictionaryEntry continuationPoint in this.m_continuationPoints)
      {
        try
        {
          if (DateTime.UtcNow.Ticks - (continuationPoint.Value as Server.ContinuationPoint).Timestamp.Ticks > 6000000000L)
            arrayList.Add(continuationPoint.Key);
        }
        catch
        {
          arrayList.Add(continuationPoint.Key);
        }
      }
      foreach (string key in arrayList)
      {
        Server.ContinuationPoint continuationPoint = (Server.ContinuationPoint) this.m_continuationPoints[(object) key];
        this.m_continuationPoints.Remove((object) key);
        continuationPoint.Position.Dispose();
      }
    }

    private bool IsItem(string name)
    {
      ItemIdentifier itemIdentifier = new ItemIdentifier(name);
      try
      {
        ItemPropertyCollection[] properties = this.m_server.GetProperties(new ItemIdentifier[1]
        {
          itemIdentifier
        }, new PropertyID[1]{ Property.DATATYPE }, false);
        if (properties == null || properties.Length != 1)
          return false;
        ResultID resultId = properties[0].ResultID;
        if (!resultId.Failed())
        {
          resultId = properties[0][0].ResultID;
          if (!resultId.Failed())
            return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    private BrowseElement FindChild(string name)
    {
      ItemIdentifier itemID = (ItemIdentifier) null;
      if (this.m_browseStack.Count > 0)
        itemID = (ItemIdentifier) this.m_browseStack.Peek();
      BrowseElement[] browseElementArray;
      try
      {
        BrowseFilters filters = new BrowseFilters();
        filters.MaxElementsReturned = 0;
        filters.BrowseFilter = browseFilter.all;
        filters.ElementNameFilter = name;
        filters.VendorFilter = (string) null;
        filters.ReturnAllProperties = false;
        filters.PropertyIDs = (PropertyID[]) null;
        filters.ReturnPropertyValues = false;
        Opc.Da.BrowsePosition position = (Opc.Da.BrowsePosition) null;
        browseElementArray = this.m_server.Browse(itemID, filters, out position);
        position?.Dispose();
      }
      catch (Exception ex)
      {
        return (BrowseElement) null;
      }
      return browseElementArray != null && browseElementArray.Length != 0 ? browseElementArray[0] : (BrowseElement) null;
    }

    private void BuildBrowseStack(ItemIdentifier itemID)
    {
      this.m_browseStack.Clear();
      this.BuildBrowseStack((ItemIdentifier) null, itemID);
    }

    private bool BuildBrowseStack(ItemIdentifier itemID, ItemIdentifier targetID)
    {
      BrowseFilters filters = new BrowseFilters();
      filters.MaxElementsReturned = 0;
      filters.BrowseFilter = browseFilter.all;
      filters.ElementNameFilter = (string) null;
      filters.VendorFilter = (string) null;
      filters.ReturnAllProperties = false;
      filters.PropertyIDs = (PropertyID[]) null;
      filters.ReturnPropertyValues = false;
      Opc.Da.BrowsePosition position = (Opc.Da.BrowsePosition) null;
      BrowseElement[] browseElementArray;
      try
      {
        browseElementArray = this.m_server.Browse(itemID, filters, out position);
      }
      catch (Exception ex)
      {
        this.m_browseStack.Clear();
        return false;
      }
      if (position != null)
      {
        position.Dispose();
        position = (Opc.Da.BrowsePosition) null;
      }
      if (browseElementArray == null || browseElementArray.Length == 0)
      {
        this.m_browseStack.Clear();
        return false;
      }
      foreach (BrowseElement browseElement in browseElementArray)
      {
        if (browseElement.ItemName == targetID.ItemName)
          return true;
        if (targetID.ItemName.StartsWith(browseElement.ItemName))
        {
          ItemIdentifier itemID1 = new ItemIdentifier(targetID.ItemName);
          this.m_browseStack.Push((object) itemID1);
          return this.BuildBrowseStack(itemID1, targetID);
        }
      }
      return false;
    }

    private void Browse(
      ItemIdentifier itemID,
      OPCBROWSETYPE dwBrowseFilterType,
      string szFilterCriteria,
      short vtDataTypeFilter,
      int dwAccessRightsFilter,
      ArrayList hits)
    {
      BrowseFilters filters = new BrowseFilters();
      filters.MaxElementsReturned = 0;
      filters.BrowseFilter = browseFilter.all;
      filters.ElementNameFilter = dwBrowseFilterType != OPCBROWSETYPE.OPC_FLAT ? szFilterCriteria : "";
      filters.VendorFilter = (string) null;
      filters.ReturnAllProperties = false;
      filters.PropertyIDs = new PropertyID[2]
      {
        Property.DATATYPE,
        Property.ACCESSRIGHTS
      };
      filters.ReturnPropertyValues = true;
      BrowseElement[] browseElementArray;
      try
      {
        Opc.Da.BrowsePosition position = (Opc.Da.BrowsePosition) null;
        browseElementArray = this.m_server.Browse(itemID, filters, out position);
        position?.Dispose();
      }
      catch
      {
        throw new ExternalException("BrowseOPCItemIDs", -2147467259);
      }
      foreach (BrowseElement browseElement in browseElementArray)
      {
        switch (dwBrowseFilterType)
        {
          case OPCBROWSETYPE.OPC_BRANCH:
            if (browseElement.HasChildren)
              goto default;
            else
              break;
          case OPCBROWSETYPE.OPC_LEAF:
            if (browseElement.HasChildren)
              break;
            goto default;
          case OPCBROWSETYPE.OPC_FLAT:
            if (browseElement.HasChildren)
            {
              this.Browse(new ItemIdentifier(browseElement.ItemName), dwBrowseFilterType, szFilterCriteria, vtDataTypeFilter, dwAccessRightsFilter, hits);
              goto default;
            }
            else
              goto default;
          default:
            if (browseElement.IsItem)
            {
              if (vtDataTypeFilter == (short) 0 || (int) (short) OpcCom.Interop.GetType((System.Type) browseElement.Properties[0].Value) == (int) vtDataTypeFilter)
              {
                if (dwAccessRightsFilter != 0)
                {
                  accessRights accessRights = (accessRights) browseElement.Properties[1].Value;
                  if (dwAccessRightsFilter == 1 && accessRights == accessRights.writable || dwAccessRightsFilter == 2 && accessRights == accessRights.readable)
                    break;
                }
              }
              else
                break;
            }
            if (dwBrowseFilterType != OPCBROWSETYPE.OPC_FLAT)
            {
              hits.Add((object) browseElement.Name);
              break;
            }
            if (browseElement.IsItem && (szFilterCriteria.Length == 0 || Opc.Convert.Match(browseElement.ItemName, szFilterCriteria, true)))
            {
              hits.Add((object) browseElement.ItemName);
              break;
            }
            break;
        }
      }
    }

    private class ContinuationPoint
    {
      public DateTime Timestamp;
      public Opc.Da.BrowsePosition Position;

      public ContinuationPoint(Opc.Da.BrowsePosition position)
      {
        this.Timestamp = DateTime.UtcNow;
        this.Position = position;
      }
    }
  }
}
