

using Opc;
using Opc.Da;
using OpcRcw.Da;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;


namespace OpcCom.Da
{
  public class Server : OpcCom.Server, Opc.Da.IServer, Opc.IServer, IDisposable
  {
    private bool m_disposed;
    protected int m_filters = 9;
    private Hashtable m_subscriptions = new Hashtable();

    internal Server()
    {
    }

    public Server(URL url, object server)
    {
      this.m_url = url != null ? (URL) url.Clone() : throw new ArgumentNullException(nameof (url));
      this.m_server = server;
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        lock (this)
        {
          if (disposing && this.m_server != null)
          {
            foreach (Subscription subscription in (IEnumerable) this.m_subscriptions.Values)
            {
              subscription.Dispose();
              try
              {
                ((IOPCServer) this.m_server).RemoveGroup((int) subscription.GetState().ServerHandle, 0);
              }
              catch
              {
              }
            }
            this.m_subscriptions.Clear();
          }
          if (this.m_server != null)
          {
            OpcCom.Interop.ReleaseServer(this.m_server);
            this.m_server = (object) null;
          }
        }
        this.m_disposed = true;
      }
      base.Dispose(disposing);
    }

    public override string GetErrorText(string locale, ResultID resultID)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        try
        {
          string ppString = (string) null;
          ((IOPCServer) this.m_server).GetErrorString(resultID.Code, OpcCom.Interop.GetLocale(locale), out ppString);
          return ppString;
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCServer.GetErrorString", ex);
        }
      }
    }

    public int GetResultFilters()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        return this.m_filters;
      }
    }

    public void SetResultFilters(int filters)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        this.m_filters = filters;
      }
    }

    public ServerStatus GetStatus()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        IntPtr ppServerStatus = IntPtr.Zero;
        try
        {
          ((IOPCServer) this.m_server).GetStatus(out ppServerStatus);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCServer.GetStatus", ex);
        }
        return Interop.GetServerStatus(ref ppServerStatus, true);
      }
    }

    public virtual ItemValueResult[] Read(Item[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        int length = items.Length;
        string[] pszItemIDs = length != 0 ? new string[length] : throw new ArgumentOutOfRangeException("items.Length", "0");
        int[] pdwMaxAge = new int[length];
        for (int index = 0; index < length; ++index)
        {
          pszItemIDs[index] = items[index].ItemName;
          pdwMaxAge[index] = items[index].MaxAgeSpecified ? items[index].MaxAge : 0;
        }
        IntPtr ppvValues = IntPtr.Zero;
        IntPtr ppwQualities = IntPtr.Zero;
        IntPtr ppftTimeStamps = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCItemIO) this.m_server).Read(length, pszItemIDs, pdwMaxAge, out ppvValues, out ppwQualities, out ppftTimeStamps, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCItemIO.Read", ex);
        }
        object[] varianTs = OpcCom.Interop.GetVARIANTs(ref ppvValues, length, true);
        short[] int16s = OpcCom.Interop.GetInt16s(ref ppwQualities, length, true);
        DateTime[] filetimEs = OpcCom.Interop.GetFILETIMEs(ref ppftTimeStamps, length, true);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, length, true);
        string locale = this.GetLocale();
        ItemValueResult[] itemValueResultArray = new ItemValueResult[length];
        for (int index = 0; index < itemValueResultArray.Length; ++index)
        {
          itemValueResultArray[index] = new ItemValueResult((ItemIdentifier) items[index]);
          itemValueResultArray[index].ServerHandle = (object) null;
          itemValueResultArray[index].Value = varianTs[index];
          itemValueResultArray[index].Quality = new Quality(int16s[index]);
          itemValueResultArray[index].QualitySpecified = true;
          itemValueResultArray[index].Timestamp = filetimEs[index];
          itemValueResultArray[index].TimestampSpecified = filetimEs[index] != DateTime.MinValue;
          itemValueResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
          itemValueResultArray[index].DiagnosticInfo = (string) null;
          if (int32s[index] == -1073479674)
            itemValueResultArray[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
          if (itemValueResultArray[index].Value != null)
          {
            if (items[index].ReqType != (System.Type) null)
            {
              try
              {
                itemValueResultArray[index].Value = this.ChangeType(varianTs[index], items[index].ReqType, locale);
              }
              catch (Exception ex)
              {
                itemValueResultArray[index].Value = (object) null;
                itemValueResultArray[index].Quality = Quality.Bad;
                itemValueResultArray[index].QualitySpecified = true;
                itemValueResultArray[index].Timestamp = DateTime.MinValue;
                itemValueResultArray[index].TimestampSpecified = false;
                itemValueResultArray[index].ResultID = !(ex.GetType() == typeof (OverflowException)) ? OpcCom.Interop.GetResultID(-1073479676) : OpcCom.Interop.GetResultID(-1073479669);
              }
            }
          }
          if ((this.m_filters & 1) == 0)
            itemValueResultArray[index].ItemName = (string) null;
          if ((this.m_filters & 2) == 0)
            itemValueResultArray[index].ItemPath = (string) null;
          if ((this.m_filters & 4) == 0)
            itemValueResultArray[index].ClientHandle = (object) null;
          if ((this.m_filters & 8) == 0)
          {
            itemValueResultArray[index].Timestamp = DateTime.MinValue;
            itemValueResultArray[index].TimestampSpecified = false;
          }
        }
        return itemValueResultArray;
      }
    }

    public virtual IdentifiedResult[] Write(ItemValue[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        int length = items.Length;
        string[] pszItemIDs = length != 0 ? new string[length] : throw new ArgumentOutOfRangeException("items.Length", "0");
        for (int index = 0; index < length; ++index)
          pszItemIDs[index] = items[index].ItemName;
        OPCITEMVQT[] opcitemvqTs = Interop.GetOPCITEMVQTs(items);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCItemIO) this.m_server).WriteVQT(length, pszItemIDs, opcitemvqTs, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCItemIO.Read", ex);
        }
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, length, true);
        IdentifiedResult[] identifiedResultArray = new IdentifiedResult[length];
        for (int index = 0; index < length; ++index)
        {
          identifiedResultArray[index] = new IdentifiedResult((ItemIdentifier) items[index]);
          identifiedResultArray[index].ServerHandle = (object) null;
          identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
          identifiedResultArray[index].DiagnosticInfo = (string) null;
          if (int32s[index] == -1073479674)
            identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
          if ((this.m_filters & 1) == 0)
            identifiedResultArray[index].ItemName = (string) null;
          if ((this.m_filters & 2) == 0)
            identifiedResultArray[index].ItemPath = (string) null;
          if ((this.m_filters & 4) == 0)
            identifiedResultArray[index].ClientHandle = (object) null;
        }
        return identifiedResultArray;
      }
    }

    public ISubscription CreateSubscription(SubscriptionState state)
    {
      if (state == null)
        throw new ArgumentNullException(nameof (state));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        SubscriptionState state1 = (SubscriptionState) state.Clone();
        Guid guid = typeof (IOPCItemMgt).GUID;
        object ppUnk = (object) null;
        int phServerGroup = 0;
        int pRevisedUpdateRate = 0;
        GCHandle gcHandle = GCHandle.Alloc((object) state1.Deadband, GCHandleType.Pinned);
        try
        {
          ((IOPCServer) this.m_server).AddGroup(state1.Name != null ? state1.Name : "", state1.Active ? 1 : 0, state1.UpdateRate, 0, IntPtr.Zero, gcHandle.AddrOfPinnedObject(), OpcCom.Interop.GetLocale(state1.Locale), out phServerGroup, out pRevisedUpdateRate, ref guid, out ppUnk);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCServer.AddGroup", ex);
        }
        finally
        {
          if (gcHandle.IsAllocated)
            gcHandle.Free();
        }
        try
        {
          int pdwRevisedKeepAliveTime = 0;
          ((IOPCGroupStateMgt2) ppUnk).SetKeepAlive(state1.KeepAlive, out pdwRevisedKeepAliveTime);
          state1.KeepAlive = pdwRevisedKeepAliveTime;
        }
        catch
        {
          state1.KeepAlive = 0;
        }
        state1.ServerHandle = (object) phServerGroup;
        if (pRevisedUpdateRate > state1.UpdateRate)
          state1.UpdateRate = pRevisedUpdateRate;
        Subscription subscription = this.CreateSubscription(ppUnk, state1, this.m_filters);
        this.m_subscriptions[(object) phServerGroup] = (object) subscription;
        return (ISubscription) subscription;
      }
    }

    public void CancelSubscription(ISubscription subscription)
    {
      if (subscription == null)
        throw new ArgumentNullException(nameof (subscription));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        SubscriptionState subscriptionState = typeof (Subscription).IsInstanceOfType((object) subscription) ? subscription.GetState() : throw new ArgumentException("Incorrect object type.", nameof (subscription));
        if (!this.m_subscriptions.ContainsKey(subscriptionState.ServerHandle))
          throw new ArgumentException("Handle not found.", nameof (subscription));
        this.m_subscriptions.Remove(subscriptionState.ServerHandle);
        subscription.Dispose();
        try
        {
          ((IOPCServer) this.m_server).RemoveGroup((int) subscriptionState.ServerHandle, 0);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCServer.RemoveGroup", ex);
        }
      }
    }

    public virtual BrowseElement[] Browse(
      ItemIdentifier itemID,
      BrowseFilters filters,
      out Opc.Da.BrowsePosition position)
    {
      if (filters == null)
        throw new ArgumentNullException(nameof (filters));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        position = (Opc.Da.BrowsePosition) null;
        int pdwCount = 0;
        int pbMoreElements = 0;
        IntPtr zero = IntPtr.Zero;
        IntPtr ppBrowseElements = IntPtr.Zero;
        try
        {
          ((IOPCBrowse) this.m_server).Browse(itemID == null || itemID.ItemName == null ? "" : itemID.ItemName, ref zero, filters.MaxElementsReturned, Interop.GetBrowseFilter(filters.BrowseFilter), filters.ElementNameFilter != null ? filters.ElementNameFilter : "", filters.VendorFilter != null ? filters.VendorFilter : "", filters.ReturnAllProperties ? 1 : 0, filters.ReturnPropertyValues ? 1 : 0, filters.PropertyIDs != null ? filters.PropertyIDs.Length : 0, Interop.GetPropertyIDs(filters.PropertyIDs), out pbMoreElements, out pdwCount, out ppBrowseElements);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCBrowse.Browse", ex);
        }
        BrowseElement[] browseElements = Interop.GetBrowseElements(ref ppBrowseElements, pdwCount, true);
        string stringUni = Marshal.PtrToStringUni(zero);
        Marshal.FreeCoTaskMem(zero);
        if (pbMoreElements != 0 || stringUni != null && stringUni != "")
          position = (Opc.Da.BrowsePosition) new BrowsePosition(itemID, filters, stringUni);
        this.ProcessResults(browseElements, filters.PropertyIDs);
        return browseElements;
      }
    }

    public virtual BrowseElement[] BrowseNext(ref Opc.Da.BrowsePosition position)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (position == null || position.GetType() != typeof (BrowsePosition))
          throw new BrowseCannotContinueException();
        BrowsePosition browsePosition = (BrowsePosition) position;
        if (browsePosition == null || browsePosition.ContinuationPoint == null || browsePosition.ContinuationPoint == "")
          throw new BrowseCannotContinueException();
        int pdwCount = 0;
        int pbMoreElements = 0;
        ItemIdentifier itemId = position.ItemID;
        BrowseFilters filters = position.Filters;
        IntPtr coTaskMemUni = Marshal.StringToCoTaskMemUni(browsePosition.ContinuationPoint);
        IntPtr ppBrowseElements = IntPtr.Zero;
        try
        {
          ((IOPCBrowse) this.m_server).Browse(itemId == null || itemId.ItemName == null ? "" : itemId.ItemName, ref coTaskMemUni, filters.MaxElementsReturned, Interop.GetBrowseFilter(filters.BrowseFilter), filters.ElementNameFilter != null ? filters.ElementNameFilter : "", filters.VendorFilter != null ? filters.VendorFilter : "", filters.ReturnAllProperties ? 1 : 0, filters.ReturnPropertyValues ? 1 : 0, filters.PropertyIDs != null ? filters.PropertyIDs.Length : 0, Interop.GetPropertyIDs(filters.PropertyIDs), out pbMoreElements, out pdwCount, out ppBrowseElements);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCBrowse.BrowseNext", ex);
        }
        BrowseElement[] browseElements = Interop.GetBrowseElements(ref ppBrowseElements, pdwCount, true);
        browsePosition.ContinuationPoint = Marshal.PtrToStringUni(coTaskMemUni);
        Marshal.FreeCoTaskMem(coTaskMemUni);
        if (pbMoreElements == 0 && (browsePosition.ContinuationPoint == null || browsePosition.ContinuationPoint == ""))
          position = (Opc.Da.BrowsePosition) null;
        this.ProcessResults(browseElements, filters.PropertyIDs);
        return browseElements;
      }
    }

    public virtual ItemPropertyCollection[] GetProperties(
      ItemIdentifier[] itemIDs,
      PropertyID[] propertyIDs,
      bool returnValues)
    {
      if (itemIDs == null)
        throw new ArgumentNullException(nameof (itemIDs));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        string[] pszItemIDs = new string[itemIDs.Length];
        for (int index = 0; index < itemIDs.Length; ++index)
          pszItemIDs[index] = itemIDs[index].ItemName;
        IntPtr ppItemProperties = IntPtr.Zero;
        try
        {
          ((IOPCBrowse) this.m_server).GetProperties(itemIDs.Length, pszItemIDs, returnValues ? 1 : 0, propertyIDs != null ? propertyIDs.Length : 0, Interop.GetPropertyIDs(propertyIDs), out ppItemProperties);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCBrowse.GetProperties", ex);
        }
        ItemPropertyCollection[] propertyCollections = Interop.GetItemPropertyCollections(ref ppItemProperties, itemIDs.Length, true);
        if (propertyIDs != null && propertyIDs.Length != 0)
        {
          foreach (ItemPropertyCollection propertyCollection in propertyCollections)
          {
            for (int index = 0; index < propertyCollection.Count; ++index)
              propertyCollection[index].ID = propertyIDs[index];
          }
        }
        return propertyCollections;
      }
    }

    protected object ChangeType(object source, System.Type type, string locale)
    {
      CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
      try
      {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
      }
      catch
      {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
      }
      try
      {
        object obj = Opc.Convert.ChangeType(source, type);
        return !(typeof (float) == type) || !float.IsInfinity(System.Convert.ToSingle(obj)) ? obj : throw new OverflowException();
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = currentCulture;
      }
    }

    protected virtual Subscription CreateSubscription(
      object group,
      SubscriptionState state,
      int filters)
    {
      return new Subscription(group, state, filters);
    }

    private void ProcessResults(BrowseElement[] elements, PropertyID[] propertyIDs)
    {
      if (elements == null)
        return;
      foreach (BrowseElement element in elements)
      {
        if (element.Properties != null)
        {
          foreach (ItemProperty property in element.Properties)
          {
            if (propertyIDs != null)
            {
              foreach (PropertyID propertyId in propertyIDs)
              {
                if (property.ID.Code == propertyId.Code)
                {
                  property.ID = propertyId;
                  break;
                }
              }
            }
          }
        }
      }
    }
  }
}
