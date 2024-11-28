

using Opc;
using Opc.Hda;
using OpcRcw.Hda;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Hda
{
  public class Server : OpcCom.Server, Opc.Hda.IServer, Opc.IServer, IDisposable
  {
    private bool m_disposed;
    private static int NextHandle = 1;
    private Hashtable m_items = new Hashtable();
    private DataCallback m_callback = new DataCallback();
    private ConnectionPoint m_connection;

    internal Server()
    {
    }

    public Server(URL url, object server)
    {
      this.m_url = url != null ? (URL) url.Clone() : throw new ArgumentNullException(nameof (url));
      this.m_server = server;
      this.Advise();
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        lock (this)
        {
          if (disposing)
            this.Unadvise();
          this.m_disposed = true;
        }
      }
      base.Dispose(disposing);
    }

    public ServerStatus GetStatus()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        IntPtr zero = IntPtr.Zero;
        OPCHDA_SERVERSTATUS pwStatus = OPCHDA_SERVERSTATUS.OPCHDA_INDETERMINATE;
        IntPtr pftCurrentTime = IntPtr.Zero;
        IntPtr pftStartTime = IntPtr.Zero;
        short pwMajorVersion = 0;
        short wMinorVersion = 0;
        short pwBuildNumber = 0;
        int pdwMaxReturnValues = 0;
        string ppszStatusString = (string) null;
        string ppszVendorInfo = (string) null;
        try
        {
          ((IOPCHDA_Server) this.m_server).GetHistorianStatus(out pwStatus, out pftCurrentTime, out pftStartTime, out pwMajorVersion, out wMinorVersion, out pwBuildNumber, out pdwMaxReturnValues, out ppszStatusString, out ppszVendorInfo);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.GetHistorianStatus", ex);
        }
        ServerStatus status = new ServerStatus();
        status.VendorInfo = ppszVendorInfo;
        status.ProductVersion = string.Format("{0}.{1}.{2}", (object) pwMajorVersion, (object) wMinorVersion, (object) pwBuildNumber);
        status.ServerState = (ServerState) pwStatus;
        status.StatusInfo = ppszStatusString;
        status.StartTime = DateTime.MinValue;
        status.CurrentTime = DateTime.MinValue;
        status.MaxReturnValues = pdwMaxReturnValues;
        if (pftStartTime != IntPtr.Zero)
        {
          status.StartTime = OpcCom.Interop.GetFILETIME(pftStartTime);
          Marshal.FreeCoTaskMem(pftStartTime);
        }
        if (pftCurrentTime != IntPtr.Zero)
        {
          status.CurrentTime = OpcCom.Interop.GetFILETIME(pftCurrentTime);
          Marshal.FreeCoTaskMem(pftCurrentTime);
        }
        return status;
      }
    }

    public Opc.Hda.Attribute[] GetAttributes()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        int pdwCount = 0;
        IntPtr ppdwAttrID = IntPtr.Zero;
        IntPtr ppszAttrName = IntPtr.Zero;
        IntPtr ppszAttrDesc = IntPtr.Zero;
        IntPtr ppvtAttrDataType = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Server) this.m_server).GetItemAttributes(out pdwCount, out ppdwAttrID, out ppszAttrName, out ppszAttrDesc, out ppvtAttrDataType);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.GetItemAttributes", ex);
        }
        if (pdwCount == 0)
          return new Opc.Hda.Attribute[0];
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppdwAttrID, pdwCount, true);
        string[] unicodeStrings1 = OpcCom.Interop.GetUnicodeStrings(ref ppszAttrName, pdwCount, true);
        string[] unicodeStrings2 = OpcCom.Interop.GetUnicodeStrings(ref ppszAttrDesc, pdwCount, true);
        short[] int16s = OpcCom.Interop.GetInt16s(ref ppvtAttrDataType, pdwCount, true);
        if (int32s == null || unicodeStrings1 == null || unicodeStrings2 == null || int16s == null)
          throw new InvalidResponseException();
        Opc.Hda.Attribute[] attributes = new Opc.Hda.Attribute[pdwCount];
        for (int index = 0; index < pdwCount; ++index)
        {
          attributes[index] = new Opc.Hda.Attribute();
          attributes[index].ID = int32s[index];
          attributes[index].Name = unicodeStrings1[index];
          attributes[index].Description = unicodeStrings2[index];
          attributes[index].DataType = OpcCom.Interop.GetType((VarEnum) Enum.ToObject(typeof (VarEnum), int16s[index]));
        }
        return attributes;
      }
    }

    public Aggregate[] GetAggregates()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        int pdwCount = 0;
        IntPtr ppdwAggrID = IntPtr.Zero;
        IntPtr ppszAggrName = IntPtr.Zero;
        IntPtr ppszAggrDesc = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Server) this.m_server).GetAggregates(out pdwCount, out ppdwAggrID, out ppszAggrName, out ppszAggrDesc);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.GetAggregates", ex);
        }
        if (pdwCount == 0)
          return new Aggregate[0];
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppdwAggrID, pdwCount, true);
        string[] unicodeStrings1 = OpcCom.Interop.GetUnicodeStrings(ref ppszAggrName, pdwCount, true);
        string[] unicodeStrings2 = OpcCom.Interop.GetUnicodeStrings(ref ppszAggrDesc, pdwCount, true);
        if (int32s == null || unicodeStrings1 == null || unicodeStrings2 == null)
          throw new InvalidResponseException();
        Aggregate[] aggregates = new Aggregate[pdwCount];
        for (int index = 0; index < pdwCount; ++index)
        {
          aggregates[index] = new Aggregate();
          aggregates[index].ID = int32s[index];
          aggregates[index].Name = unicodeStrings1[index];
          aggregates[index].Description = unicodeStrings2[index];
        }
        return aggregates;
      }
    }

    public IBrowser CreateBrowser(BrowseFilter[] filters, out ResultID[] results)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        int length = filters != null ? filters.Length : 0;
        int[] pdwAttrID = new int[length];
        object[] vFilter = new object[length];
        OPCHDA_OPERATORCODES[] pOperator = new OPCHDA_OPERATORCODES[length];
        for (int index = 0; index < length; ++index)
        {
          pdwAttrID[index] = filters[index].AttributeID;
          pOperator[index] = (OPCHDA_OPERATORCODES) Enum.ToObject(typeof (OPCHDA_OPERATORCODES), (object) filters[index].Operator);
          vFilter[index] = OpcCom.Interop.GetVARIANT(filters[index].FilterValue);
        }
        IOPCHDA_Browser pphBrowser = (IOPCHDA_Browser) null;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Server) this.m_server).CreateBrowse(length, pdwAttrID, pOperator, vFilter, out pphBrowser, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.CreateBrowse", ex);
        }
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, length, true);
        if (length > 0 && int32s == null || pphBrowser == null)
          throw new InvalidResponseException();
        results = new ResultID[length];
        for (int index = 0; index < length; ++index)
          results[index] = OpcCom.Interop.GetResultID(int32s[index]);
        return (IBrowser) new Browser(this, pphBrowser, filters, results);
      }
    }

    public IdentifiedResult[] CreateItems(ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        string[] pszItemID = new string[items.Length];
        int[] phClient = new int[items.Length];
        for (int index = 0; index < items.Length; ++index)
        {
          if (items[index] != null)
          {
            pszItemID[index] = items[index].ItemName;
            phClient[index] = this.CreateHandle();
          }
        }
        IntPtr pphServer = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Server) this.m_server).GetItemHandles(items.Length, pszItemID, phClient, out pphServer, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.GetItemHandles", ex);
        }
        int[] int32s1 = OpcCom.Interop.GetInt32s(ref pphServer, items.Length, true);
        int[] int32s2 = OpcCom.Interop.GetInt32s(ref ppErrors, items.Length, true);
        if (int32s1 == null || int32s2 == null)
          throw new InvalidResponseException();
        IdentifiedResult[] items1 = new IdentifiedResult[items.Length];
        for (int index = 0; index < items1.Length; ++index)
        {
          items1[index] = new IdentifiedResult(items[index]);
          items1[index].ResultID = OpcCom.Interop.GetResultID(int32s2[index]);
          if (items1[index].ResultID.Succeeded())
          {
            this.m_items.Add((object) phClient[index], (object) new ItemIdentifier()
            {
              ItemName = items[index].ItemName,
              ItemPath = items[index].ItemPath,
              ServerHandle = (object) int32s1[index],
              ClientHandle = items[index].ClientHandle
            });
            items1[index].ServerHandle = (object) phClient[index];
            items1[index].ClientHandle = items[index].ClientHandle;
          }
        }
        return items1;
      }
    }

    public IdentifiedResult[] ReleaseItems(ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        int[] serverHandles = this.GetServerHandles(items);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Server) this.m_server).ReleaseItemHandles(items.Length, serverHandles, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.ReleaseItemHandles", ex);
        }
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, items.Length, true);
        if (int32s == null)
          throw new InvalidResponseException();
        IdentifiedResult[] identifiedResultArray = new IdentifiedResult[items.Length];
        for (int index = 0; index < identifiedResultArray.Length; ++index)
        {
          identifiedResultArray[index] = new IdentifiedResult(items[index]);
          identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
          if (identifiedResultArray[index].ResultID.Succeeded() && items[index].ServerHandle != null)
          {
            ItemIdentifier itemIdentifier = (ItemIdentifier) this.m_items[items[index].ServerHandle];
            if (itemIdentifier != null)
            {
              identifiedResultArray[index].ItemName = itemIdentifier.ItemName;
              identifiedResultArray[index].ItemPath = itemIdentifier.ItemPath;
              identifiedResultArray[index].ClientHandle = itemIdentifier.ClientHandle;
              this.m_items.Remove(items[index].ServerHandle);
            }
          }
        }
        return identifiedResultArray;
      }
    }

    public IdentifiedResult[] ValidateItems(ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        string[] pszItemID = new string[items.Length];
        for (int index = 0; index < items.Length; ++index)
        {
          if (items[index] != null)
            pszItemID[index] = items[index].ItemName;
        }
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Server) this.m_server).ValidateItemIDs(items.Length, pszItemID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Server.ValidateItemIDs", ex);
        }
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, items.Length, true);
        if (int32s == null)
          throw new InvalidResponseException();
        IdentifiedResult[] identifiedResultArray = new IdentifiedResult[items.Length];
        for (int index = 0; index < identifiedResultArray.Length; ++index)
        {
          identifiedResultArray[index] = new IdentifiedResult(items[index]);
          identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
        }
        return identifiedResultArray;
      }
    }

    public ItemValueCollection[] ReadRaw(
      Time startTime,
      Time endTime,
      int maxValues,
      bool includeBounds,
      ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ItemValueCollection[0];
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppItemValues = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncRead) this.m_server).ReadRaw(ref time1, ref time2, maxValues, includeBounds ? 1 : 0, serverHandles.Length, serverHandles, out ppItemValues, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncRead.ReadRaw", ex);
        }
        ItemValueCollection[] valueCollections = Interop.GetItemValueCollections(ref ppItemValues, items.Length, true);
        this.UpdateResults(items, (ItemIdentifier[]) valueCollections, ref ppErrors);
        this.UpdateActualTimes((IActualTime[]) valueCollections, time1, time2);
        return valueCollections;
      }
    }

    public IdentifiedResult[] ReadRaw(
      Time startTime,
      Time endTime,
      int maxValues,
      bool includeBounds,
      ItemIdentifier[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).ReadRaw(request1.RequestID, ref time1, ref time2, maxValues, includeBounds ? 1 : 0, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.ReadRaw", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) results))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return results;
        }
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) request1
        }, time1, time2);
        request = (IRequest) request1;
        return results;
      }
    }

    public IdentifiedResult[] AdviseRaw(
      Time startTime,
      Decimal updateInterval,
      ItemIdentifier[] items,
      object requestHandle,
      DataUpdateEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time = Interop.GetTime(startTime);
        OPCHDA_FILETIME filetime = Interop.GetFILETIME(updateInterval);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).AdviseRaw(request1.RequestID, ref time, filetime, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.AdviseRaw", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        request1.Update(pdwCancelID, (ItemIdentifier[]) results);
        request = (IRequest) request1;
        return results;
      }
    }

    public IdentifiedResult[] PlaybackRaw(
      Time startTime,
      Time endTime,
      int maxValues,
      Decimal updateInterval,
      Decimal playbackDuration,
      ItemIdentifier[] items,
      object requestHandle,
      DataUpdateEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        OPCHDA_FILETIME filetime1 = Interop.GetFILETIME(updateInterval);
        OPCHDA_FILETIME filetime2 = Interop.GetFILETIME(playbackDuration);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Playback) this.m_server).ReadRawWithUpdate(request1.RequestID, ref time1, ref time2, maxValues, filetime2, filetime1, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Playback.ReadRawWithUpdate", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        request1.Update(pdwCancelID, (ItemIdentifier[]) results);
        request = (IRequest) request1;
        return results;
      }
    }

    public ItemValueCollection[] ReadProcessed(
      Time startTime,
      Time endTime,
      Decimal resampleInterval,
      Item[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ItemValueCollection[0];
        int[] serverHandles = this.GetServerHandles((ItemIdentifier[]) items);
        int[] aggregateIds = this.GetAggregateIDs(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        OPCHDA_FILETIME filetime = Interop.GetFILETIME(resampleInterval);
        IntPtr ppItemValues = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncRead) this.m_server).ReadProcessed(ref time1, ref time2, filetime, serverHandles.Length, serverHandles, aggregateIds, out ppItemValues, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncRead.ReadProcessed", ex);
        }
        ItemValueCollection[] valueCollections = Interop.GetItemValueCollections(ref ppItemValues, items.Length, true);
        this.UpdateResults((ItemIdentifier[]) items, (ItemIdentifier[]) valueCollections, ref ppErrors);
        this.UpdateActualTimes((IActualTime[]) valueCollections, time1, time2);
        return valueCollections;
      }
    }

    public IdentifiedResult[] ReadProcessed(
      Time startTime,
      Time endTime,
      Decimal resampleInterval,
      Item[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles((ItemIdentifier[]) items);
        int[] aggregateIds = this.GetAggregateIDs(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        OPCHDA_FILETIME filetime = Interop.GetFILETIME(resampleInterval);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).ReadProcessed(request1.RequestID, ref time1, ref time2, filetime, serverHandles.Length, serverHandles, aggregateIds, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.ReadProcessed", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults((ItemIdentifier[]) items, (ItemIdentifier[]) results, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) results))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return results;
        }
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) request1
        }, time1, time2);
        request = (IRequest) request1;
        return results;
      }
    }

    public IdentifiedResult[] AdviseProcessed(
      Time startTime,
      Decimal resampleInterval,
      int numberOfIntervals,
      Item[] items,
      object requestHandle,
      DataUpdateEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles((ItemIdentifier[]) items);
        int[] aggregateIds = this.GetAggregateIDs(items);
        OPCHDA_TIME time = Interop.GetTime(startTime);
        OPCHDA_FILETIME filetime = Interop.GetFILETIME(resampleInterval);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).AdviseProcessed(request1.RequestID, ref time, filetime, serverHandles.Length, serverHandles, aggregateIds, numberOfIntervals, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.AdviseProcessed", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults((ItemIdentifier[]) items, (ItemIdentifier[]) results, ref ppErrors);
        request1.Update(pdwCancelID, (ItemIdentifier[]) results);
        request = (IRequest) request1;
        return results;
      }
    }

    public IdentifiedResult[] PlaybackProcessed(
      Time startTime,
      Time endTime,
      Decimal resampleInterval,
      int numberOfIntervals,
      Decimal updateInterval,
      Item[] items,
      object requestHandle,
      DataUpdateEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles((ItemIdentifier[]) items);
        int[] aggregateIds = this.GetAggregateIDs(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        OPCHDA_FILETIME filetime1 = Interop.GetFILETIME(resampleInterval);
        OPCHDA_FILETIME filetime2 = Interop.GetFILETIME(updateInterval);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_Playback) this.m_server).ReadProcessedWithUpdate(request1.RequestID, ref time1, ref time2, filetime1, numberOfIntervals, filetime2, serverHandles.Length, serverHandles, aggregateIds, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_Playback.ReadProcessedWithUpdate", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults((ItemIdentifier[]) items, (ItemIdentifier[]) results, ref ppErrors);
        request1.Update(pdwCancelID, (ItemIdentifier[]) results);
        request = (IRequest) request1;
        return results;
      }
    }

    public ItemValueCollection[] ReadAtTime(DateTime[] timestamps, ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ItemValueCollection[0];
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        IntPtr ppItemValues = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncRead) this.m_server).ReadAtTime(filetimEs.Length, filetimEs, serverHandles.Length, serverHandles, out ppItemValues, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncRead.ReadAtTime", ex);
        }
        ItemValueCollection[] valueCollections = Interop.GetItemValueCollections(ref ppItemValues, items.Length, true);
        this.UpdateResults(items, (ItemIdentifier[]) valueCollections, ref ppErrors);
        return valueCollections;
      }
    }

    public IdentifiedResult[] ReadAtTime(
      DateTime[] timestamps,
      ItemIdentifier[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).ReadAtTime(request1.RequestID, filetimEs.Length, filetimEs, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.ReadAtTime", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) results))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return results;
        }
        request = (IRequest) request1;
        return results;
      }
    }

    public ModifiedValueCollection[] ReadModified(
      Time startTime,
      Time endTime,
      int maxValues,
      ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ModifiedValueCollection[0];
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppItemValues = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncRead) this.m_server).ReadModified(ref time1, ref time2, maxValues, serverHandles.Length, serverHandles, out ppItemValues, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncRead.ReadModified", ex);
        }
        ModifiedValueCollection[] valueCollections = Interop.GetModifiedValueCollections(ref ppItemValues, items.Length, true);
        this.UpdateResults(items, (ItemIdentifier[]) valueCollections, ref ppErrors);
        this.UpdateActualTimes((IActualTime[]) valueCollections, time1, time2);
        return valueCollections;
      }
    }

    public IdentifiedResult[] ReadModified(
      Time startTime,
      Time endTime,
      int maxValues,
      ItemIdentifier[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).ReadModified(request1.RequestID, ref time1, ref time2, maxValues, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.ReadModified", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) results))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return results;
        }
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) request1
        }, time1, time2);
        request = (IRequest) request1;
        return results;
      }
    }

    public ItemAttributeCollection ReadAttributes(
      Time startTime,
      Time endTime,
      ItemIdentifier item,
      int[] attributeIDs)
    {
      if (item == null)
        throw new ArgumentNullException(nameof (item));
      if (attributeIDs == null)
        throw new ArgumentNullException(nameof (attributeIDs));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (attributeIDs.Length == 0)
          return new ItemAttributeCollection(item);
        int[] serverHandles = this.GetServerHandles(new ItemIdentifier[1]
        {
          item
        });
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppAttributeValues = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncRead) this.m_server).ReadAttribute(ref time1, ref time2, serverHandles[0], attributeIDs.Length, attributeIDs, out ppAttributeValues, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncRead.ReadAttribute", ex);
        }
        AttributeValueCollection[] valueCollections = Interop.GetAttributeValueCollections(ref ppAttributeValues, attributeIDs.Length, true);
        ItemAttributeCollection attributeCollection = this.UpdateResults(item, valueCollections, ref ppErrors);
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) attributeCollection
        }, time1, time2);
        return attributeCollection;
      }
    }

    public ResultCollection ReadAttributes(
      Time startTime,
      Time endTime,
      ItemIdentifier item,
      int[] attributeIDs,
      object requestHandle,
      ReadAttributesEventHandler callback,
      out IRequest request)
    {
      if (item == null)
        throw new ArgumentNullException(nameof (item));
      if (attributeIDs == null)
        throw new ArgumentNullException(nameof (attributeIDs));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (attributeIDs.Length == 0)
          return new ResultCollection();
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(new ItemIdentifier[1]
        {
          item
        });
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).ReadAttribute(request1.RequestID, ref time1, ref time2, serverHandles[0], attributeIDs.Length, attributeIDs, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.ReadAttribute", ex);
        }
        ResultCollection result1 = new ResultCollection(item);
        this.UpdateResult(item, (ItemIdentifier) result1, 0);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, attributeIDs.Length, true);
        if (int32s == null)
          throw new InvalidResponseException();
        foreach (int input in int32s)
        {
          Result result2 = new Result(OpcCom.Interop.GetResultID(input));
          result1.Add(result2);
        }
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) new ResultCollection[1]
        {
          result1
        }))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return result1;
        }
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) request1
        }, time1, time2);
        request = (IRequest) request1;
        return result1;
      }
    }

    public AnnotationValueCollection[] ReadAnnotations(
      Time startTime,
      Time endTime,
      ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new AnnotationValueCollection[0];
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppAnnotationValues = IntPtr.Zero;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncAnnotations) this.m_server).Read(ref time1, ref time2, serverHandles.Length, serverHandles, out ppAnnotationValues, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncAnnotations.Read", ex);
        }
        AnnotationValueCollection[] valueCollections = Interop.GetAnnotationValueCollections(ref ppAnnotationValues, items.Length, true);
        this.UpdateResults(items, (ItemIdentifier[]) valueCollections, ref ppErrors);
        this.UpdateActualTimes((IActualTime[]) valueCollections, time1, time2);
        return valueCollections;
      }
    }

    public IdentifiedResult[] ReadAnnotations(
      Time startTime,
      Time endTime,
      ItemIdentifier[] items,
      object requestHandle,
      ReadAnnotationsEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncAnnotations) this.m_server).Read(request1.RequestID, ref time1, ref time2, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncAnnotations.Read", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) results))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return results;
        }
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) request1
        }, time1, time2);
        request = (IRequest) request1;
        return results;
      }
    }

    public ResultCollection[] InsertAnnotations(AnnotationValueCollection[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ResultCollection[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] serverHandles = (int[]) null;
        OPCHDA_ANNOTATION[] annotations = (OPCHDA_ANNOTATION[]) null;
        OPCHDA_FILETIME[] ftTimestamps = (OPCHDA_FILETIME[]) null;
        int count = this.MarshalAnnotatations(items, ref serverHandles, ref ftTimestamps, ref annotations);
        if (count == 0)
          return resultCollections;
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncAnnotations) this.m_server).Insert(serverHandles.Length, serverHandles, ftTimestamps, annotations, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncAnnotations.Insert", ex);
        }
        for (int index = 0; index < annotations.Length; ++index)
        {
          OpcCom.Interop.GetFILETIMEs(ref annotations[index].ftTimeStamps, 1, true);
          OpcCom.Interop.GetUnicodeStrings(ref annotations[index].szAnnotation, 1, true);
          OpcCom.Interop.GetFILETIMEs(ref annotations[index].ftAnnotationTime, 1, true);
          OpcCom.Interop.GetUnicodeStrings(ref annotations[index].szUser, 1, true);
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        return resultCollections;
      }
    }

    public IdentifiedResult[] InsertAnnotations(
      AnnotationValueCollection[] items,
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] serverHandles = (int[]) null;
        OPCHDA_ANNOTATION[] annotations = (OPCHDA_ANNOTATION[]) null;
        OPCHDA_FILETIME[] ftTimestamps = (OPCHDA_FILETIME[]) null;
        int count = this.MarshalAnnotatations(items, ref serverHandles, ref ftTimestamps, ref annotations);
        if (count == 0)
          return this.GetIdentifiedResults(resultCollections);
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        IntPtr ppErrors = IntPtr.Zero;
        int pdwCancelID = 0;
        try
        {
          ((IOPCHDA_AsyncAnnotations) this.m_server).Insert(request1.RequestID, serverHandles.Length, serverHandles, ftTimestamps, annotations, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncAnnotations.Insert", ex);
        }
        for (int index = 0; index < annotations.Length; ++index)
        {
          OpcCom.Interop.GetFILETIMEs(ref annotations[index].ftTimeStamps, 1, true);
          OpcCom.Interop.GetUnicodeStrings(ref annotations[index].szAnnotation, 1, true);
          OpcCom.Interop.GetFILETIMEs(ref annotations[index].ftAnnotationTime, 1, true);
          OpcCom.Interop.GetUnicodeStrings(ref annotations[index].szUser, 1, true);
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) resultCollections))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return this.GetIdentifiedResults(resultCollections);
        }
        request = (IRequest) request1;
        return this.GetIdentifiedResults(resultCollections);
      }
    }

    public ResultCollection[] Insert(ItemValueCollection[] items, bool replace)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ResultCollection[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] handles = (int[]) null;
        object[] values = (object[]) null;
        int[] qualities = (int[]) null;
        DateTime[] timestamps = (DateTime[]) null;
        int count = this.MarshalValues(items, ref handles, ref values, ref qualities, ref timestamps);
        if (count == 0)
          return resultCollections;
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        IntPtr ppErrors = IntPtr.Zero;
        if (replace)
        {
          try
          {
            ((IOPCHDA_SyncUpdate) this.m_server).InsertReplace(handles.Length, handles, filetimEs, values, qualities, out ppErrors);
          }
          catch (Exception ex)
          {
            throw OpcCom.Interop.CreateException("IOPCHDA_SyncUpdate.InsertReplace", ex);
          }
        }
        else
        {
          try
          {
            ((IOPCHDA_SyncUpdate) this.m_server).Insert(handles.Length, handles, filetimEs, values, qualities, out ppErrors);
          }
          catch (Exception ex)
          {
            throw OpcCom.Interop.CreateException("IOPCHDA_SyncUpdate.Insert", ex);
          }
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        return resultCollections;
      }
    }

    public IdentifiedResult[] Insert(
      ItemValueCollection[] items,
      bool replace,
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] handles = (int[]) null;
        object[] values = (object[]) null;
        int[] qualities = (int[]) null;
        DateTime[] timestamps = (DateTime[]) null;
        int count = this.MarshalValues(items, ref handles, ref values, ref qualities, ref timestamps);
        if (count == 0)
          return this.GetIdentifiedResults(resultCollections);
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        IntPtr ppErrors = IntPtr.Zero;
        int pdwCancelID = 0;
        if (replace)
        {
          try
          {
            ((IOPCHDA_AsyncUpdate) this.m_server).InsertReplace(request1.RequestID, handles.Length, handles, filetimEs, values, qualities, out pdwCancelID, out ppErrors);
          }
          catch (Exception ex)
          {
            throw OpcCom.Interop.CreateException("IOPCHDA_AsyncUpdate.InsertReplace", ex);
          }
        }
        else
        {
          try
          {
            ((IOPCHDA_AsyncUpdate) this.m_server).Insert(request1.RequestID, handles.Length, handles, filetimEs, values, qualities, out pdwCancelID, out ppErrors);
          }
          catch (Exception ex)
          {
            throw OpcCom.Interop.CreateException("IOPCHDA_AsyncUpdate.Insert", ex);
          }
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) resultCollections))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return this.GetIdentifiedResults(resultCollections);
        }
        request = (IRequest) request1;
        return this.GetIdentifiedResults(resultCollections);
      }
    }

    public ResultCollection[] Replace(ItemValueCollection[] items)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ResultCollection[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] handles = (int[]) null;
        object[] values = (object[]) null;
        int[] qualities = (int[]) null;
        DateTime[] timestamps = (DateTime[]) null;
        int count = this.MarshalValues(items, ref handles, ref values, ref qualities, ref timestamps);
        if (count == 0)
          return resultCollections;
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncUpdate) this.m_server).Replace(handles.Length, handles, filetimEs, values, qualities, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncUpdate.Replace", ex);
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        return resultCollections;
      }
    }

    public IdentifiedResult[] Replace(
      ItemValueCollection[] items,
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] handles = (int[]) null;
        object[] values = (object[]) null;
        int[] qualities = (int[]) null;
        DateTime[] timestamps = (DateTime[]) null;
        int count = this.MarshalValues(items, ref handles, ref values, ref qualities, ref timestamps);
        if (count == 0)
          return this.GetIdentifiedResults(resultCollections);
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        IntPtr ppErrors = IntPtr.Zero;
        int pdwCancelID = 0;
        try
        {
          ((IOPCHDA_AsyncUpdate) this.m_server).Replace(request1.RequestID, handles.Length, handles, filetimEs, values, qualities, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncUpdate.Replace", ex);
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) resultCollections))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return this.GetIdentifiedResults(resultCollections);
        }
        request = (IRequest) request1;
        return this.GetIdentifiedResults(resultCollections);
      }
    }

    public IdentifiedResult[] Delete(Time startTime, Time endTime, ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncUpdate) this.m_server).DeleteRaw(ref time1, ref time2, serverHandles.Length, serverHandles, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncUpdate.DeleteRaw", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        return results;
      }
    }

    public IdentifiedResult[] Delete(
      Time startTime,
      Time endTime,
      ItemIdentifier[] items,
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        int requestId = request1.RequestID;
        int pdwCancelID = 0;
        int[] serverHandles = this.GetServerHandles(items);
        OPCHDA_TIME time1 = Interop.GetTime(startTime);
        OPCHDA_TIME time2 = Interop.GetTime(endTime);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_AsyncUpdate) this.m_server).DeleteRaw(request1.RequestID, ref time1, ref time2, serverHandles.Length, serverHandles, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncUpdate.DeleteRaw", ex);
        }
        IdentifiedResult[] results = new IdentifiedResult[items.Length];
        for (int index = 0; index < items.Length; ++index)
          results[index] = new IdentifiedResult();
        this.UpdateResults(items, (ItemIdentifier[]) results, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) results))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return results;
        }
        this.UpdateActualTimes(new IActualTime[1]
        {
          (IActualTime) request1
        }, time1, time2);
        request = (IRequest) request1;
        return results;
      }
    }

    public ResultCollection[] DeleteAtTime(ItemTimeCollection[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new ResultCollection[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] handles = (int[]) null;
        DateTime[] timestamps = (DateTime[]) null;
        int count = this.MarshalTimestamps(items, ref handles, ref timestamps);
        if (count == 0)
          return resultCollections;
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        IntPtr ppErrors = IntPtr.Zero;
        try
        {
          ((IOPCHDA_SyncUpdate) this.m_server).DeleteAtTime(handles.Length, handles, filetimEs, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_SyncUpdate.DeleteAtTime", ex);
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        return resultCollections;
      }
    }

    public IdentifiedResult[] DeleteAtTime(
      ItemTimeCollection[] items,
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      request = (IRequest) null;
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        if (items.Length == 0)
          return new IdentifiedResult[0];
        ResultCollection[] resultCollections = this.CreateResultCollections((ItemIdentifier[]) items);
        int[] handles = (int[]) null;
        DateTime[] timestamps = (DateTime[]) null;
        int count = this.MarshalTimestamps(items, ref handles, ref timestamps);
        if (count == 0)
          return this.GetIdentifiedResults(resultCollections);
        OPCHDA_FILETIME[] filetimEs = Interop.GetFILETIMEs(timestamps);
        Request request1 = this.m_callback.CreateRequest(requestHandle, (Delegate) callback);
        IntPtr ppErrors = IntPtr.Zero;
        int pdwCancelID = 0;
        try
        {
          ((IOPCHDA_AsyncUpdate) this.m_server).DeleteAtTime(request1.RequestID, handles.Length, handles, filetimEs, out pdwCancelID, out ppErrors);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCHDA_AsyncUpdate.DeleteAtTime", ex);
        }
        this.UpdateResults((ICollection[]) items, resultCollections, count, ref ppErrors);
        if (request1.Update(pdwCancelID, (ItemIdentifier[]) resultCollections))
        {
          request = (IRequest) null;
          this.m_callback.CancelRequest(request1, (CancelCompleteEventHandler) null);
          return this.GetIdentifiedResults(resultCollections);
        }
        request = (IRequest) request1;
        return this.GetIdentifiedResults(resultCollections);
      }
    }

    public void CancelRequest(IRequest request)
    {
      this.CancelRequest(request, (CancelCompleteEventHandler) null);
    }

    public void CancelRequest(IRequest request, CancelCompleteEventHandler callback)
    {
      if (request == null)
        throw new ArgumentNullException(nameof (request));
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        Request request1 = (Request) request;
        this.m_callback.CancelRequest(request1, callback);
        try
        {
          ((IOPCHDA_AsyncRead) this.m_server).Cancel(request1.CancelID);
        }
        catch (Exception ex)
        {
          if (-2147467259 != Marshal.GetHRForException(ex))
            throw OpcCom.Interop.CreateException("IOPCHDA_AsyncRead.Cancel", ex);
        }
      }
    }

    private void Advise()
    {
      if (this.m_connection != null)
        return;
      try
      {
        this.m_connection = new ConnectionPoint(this.m_server, typeof (IOPCHDA_DataCallback).GUID);
        this.m_connection.Advise((object) this.m_callback);
      }
      catch
      {
        this.m_connection = (ConnectionPoint) null;
      }
    }

    private void Unadvise()
    {
      if (this.m_connection == null || this.m_connection.Unadvise() != 0)
        return;
      this.m_connection.Dispose();
      this.m_connection = (ConnectionPoint) null;
    }

    private int CreateHandle() => Server.NextHandle++;

    private int GetInvalidHandle()
    {
      int num = 0;
      foreach (ItemIdentifier itemIdentifier in (IEnumerable) this.m_items.Values)
      {
        int serverHandle = (int) itemIdentifier.ServerHandle;
        if (num < serverHandle)
          num = serverHandle;
      }
      return num + 1;
    }

    private int GetCount(ICollection[] collections)
    {
      int count = 0;
      if (collections != null)
      {
        foreach (ICollection collection in collections)
        {
          if (collection != null)
            count += collection.Count;
        }
      }
      return count;
    }

    private ResultCollection[] CreateResultCollections(ItemIdentifier[] items)
    {
      ResultCollection[] resultCollections = (ResultCollection[]) null;
      if (items != null)
      {
        resultCollections = new ResultCollection[items.Length];
        for (int index = 0; index < items.Length; ++index)
        {
          resultCollections[index] = new ResultCollection();
          if (items[index] != null)
            this.UpdateResult(items[index], (ItemIdentifier) resultCollections[index], 0);
        }
      }
      return resultCollections;
    }

    private int[] GetServerHandles(ItemIdentifier[] items)
    {
      int invalidHandle = this.GetInvalidHandle();
      int[] serverHandles = new int[items.Length];
      for (int index = 0; index < items.Length; ++index)
      {
        serverHandles[index] = invalidHandle;
        if (items[index] != null && items[index].ServerHandle != null)
        {
          ItemIdentifier itemIdentifier = (ItemIdentifier) this.m_items[items[index].ServerHandle];
          if (itemIdentifier != null)
            serverHandles[index] = (int) itemIdentifier.ServerHandle;
        }
      }
      return serverHandles;
    }

    private int[] GetAggregateIDs(Item[] items)
    {
      int[] aggregateIds = new int[items.Length];
      for (int index = 0; index < items.Length; ++index)
      {
        aggregateIds[index] = 0;
        if (items[index].AggregateID != 0)
          aggregateIds[index] = items[index].AggregateID;
      }
      return aggregateIds;
    }

    private void UpdateResult(ItemIdentifier item, ItemIdentifier result, int error)
    {
      result.ItemName = item.ItemName;
      result.ItemPath = item.ItemPath;
      result.ClientHandle = item.ClientHandle;
      result.ServerHandle = item.ServerHandle;
      if (error < 0 || item.ServerHandle == null)
        return;
      ItemIdentifier itemIdentifier = (ItemIdentifier) this.m_items[item.ServerHandle];
      if (itemIdentifier == null)
        return;
      result.ItemName = itemIdentifier.ItemName;
      result.ItemPath = itemIdentifier.ItemPath;
      result.ClientHandle = itemIdentifier.ClientHandle;
    }

    private void UpdateActualTimes(
      IActualTime[] results,
      OPCHDA_TIME pStartTime,
      OPCHDA_TIME pEndTime)
    {
      DateTime filetime1 = OpcCom.Interop.GetFILETIME(Interop.Convert(pStartTime.ftTime));
      DateTime filetime2 = OpcCom.Interop.GetFILETIME(Interop.Convert(pEndTime.ftTime));
      foreach (IActualTime result in results)
      {
        result.StartTime = filetime1;
        result.EndTime = filetime2;
      }
    }

    private ItemAttributeCollection UpdateResults(
      ItemIdentifier item,
      AttributeValueCollection[] attributes,
      ref IntPtr pErrors)
    {
      int[] int32s = OpcCom.Interop.GetInt32s(ref pErrors, attributes.Length, true);
      if (attributes == null || int32s == null)
        throw new InvalidResponseException();
      for (int index = 0; index < attributes.Length; ++index)
        attributes[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
      ItemAttributeCollection result = new ItemAttributeCollection();
      foreach (AttributeValueCollection attribute in attributes)
        result.Add(attribute);
      this.UpdateResult(item, (ItemIdentifier) result, 0);
      return result;
    }

    private void UpdateResults(
      ItemIdentifier[] items,
      ItemIdentifier[] results,
      ref IntPtr pErrors)
    {
      int[] int32s = OpcCom.Interop.GetInt32s(ref pErrors, items.Length, true);
      if (results == null || int32s == null)
        throw new InvalidResponseException();
      for (int index = 0; index < results.Length; ++index)
      {
        this.UpdateResult(items[index], results[index], int32s[index]);
        if (typeof (IResult).IsInstanceOfType((object) results[index]))
          ((IResult) results[index]).ResultID = OpcCom.Interop.GetResultID(int32s[index]);
      }
    }

    private void UpdateResults(
      ICollection[] items,
      ResultCollection[] results,
      int count,
      ref IntPtr pErrors)
    {
      int[] int32s = OpcCom.Interop.GetInt32s(ref pErrors, count, true);
      if (int32s == null)
        throw new InvalidResponseException();
      int num = 0;
      for (int index1 = 0; index1 < items.Length; ++index1)
      {
        for (int index2 = 0; index2 < items[index1].Count && num < count; ++index2)
        {
          Result result = new Result(OpcCom.Interop.GetResultID(int32s[num++]));
          results[index1].Add(result);
        }
      }
    }

    private int MarshalValues(
      ItemValueCollection[] items,
      ref int[] handles,
      ref object[] values,
      ref int[] qualities,
      ref DateTime[] timestamps)
    {
      int count = this.GetCount((ICollection[]) items);
      handles = new int[count];
      timestamps = new DateTime[count];
      values = new object[count];
      qualities = new int[count];
      int[] serverHandles = this.GetServerHandles((ItemIdentifier[]) items);
      int index1 = 0;
      for (int index2 = 0; index2 < items.Length; ++index2)
      {
        foreach (ItemValue itemValue in items[index2])
        {
          handles[index1] = serverHandles[index2];
          timestamps[index1] = itemValue.Timestamp;
          values[index1] = OpcCom.Interop.GetVARIANT(itemValue.Value);
          qualities[index1] = (int) itemValue.Quality.GetCode();
          ++index1;
        }
      }
      return count;
    }

    private int MarshalTimestamps(
      ItemTimeCollection[] items,
      ref int[] handles,
      ref DateTime[] timestamps)
    {
      int count = this.GetCount((ICollection[]) items);
      handles = new int[count];
      timestamps = new DateTime[count];
      int[] serverHandles = this.GetServerHandles((ItemIdentifier[]) items);
      int index1 = 0;
      for (int index2 = 0; index2 < items.Length; ++index2)
      {
        foreach (DateTime dateTime in items[index2])
        {
          handles[index1] = serverHandles[index2];
          timestamps[index1] = dateTime;
          ++index1;
        }
      }
      return count;
    }

    private int MarshalAnnotatations(
      AnnotationValueCollection[] items,
      ref int[] serverHandles,
      ref OPCHDA_FILETIME[] ftTimestamps,
      ref OPCHDA_ANNOTATION[] annotations)
    {
      int count = this.GetCount((ICollection[]) items);
      int[] serverHandles1 = this.GetServerHandles((ItemIdentifier[]) items);
      serverHandles = new int[count];
      annotations = new OPCHDA_ANNOTATION[count];
      DateTime[] input = new DateTime[count];
      int index1 = 0;
      for (int index2 = 0; index2 < items.Length; ++index2)
      {
        for (int index3 = 0; index3 < items[index2].Count; ++index3)
        {
          serverHandles[index1] = serverHandles1[index2];
          input[index1] = items[index2][index3].Timestamp;
          annotations[index1] = new OPCHDA_ANNOTATION();
          annotations[index1].dwNumValues = 1;
          annotations[index1].ftTimeStamps = OpcCom.Interop.GetFILETIMEs(new DateTime[1]
          {
            input[index3]
          });
          annotations[index1].szAnnotation = OpcCom.Interop.GetUnicodeStrings(new string[1]
          {
            items[index2][index3].Annotation
          });
          annotations[index1].ftAnnotationTime = OpcCom.Interop.GetFILETIMEs(new DateTime[1]
          {
            items[index2][index3].CreationTime
          });
          annotations[index1].szUser = OpcCom.Interop.GetUnicodeStrings(new string[1]
          {
            items[index2][index3].User
          });
          ++index1;
        }
      }
      ftTimestamps = Interop.GetFILETIMEs(input);
      return count;
    }

    private IdentifiedResult[] GetIdentifiedResults(ResultCollection[] results)
    {
      if (results == null || results.Length == 0)
        return new IdentifiedResult[0];
      IdentifiedResult[] identifiedResults = new IdentifiedResult[results.Length];
      for (int index = 0; index < results.Length; ++index)
      {
        identifiedResults[index] = new IdentifiedResult((ItemIdentifier) results[index]);
        if (results[index] == null || results[index].Count == 0)
        {
          identifiedResults[index].ResultID = ResultID.Hda.S_NODATA;
        }
        else
        {
          ResultID resultId = results[index][0].ResultID;
          foreach (Result result in results[index])
          {
            if (resultId.Code != result.ResultID.Code)
            {
              resultId = ResultID.E_FAIL;
              break;
            }
          }
        }
      }
      return identifiedResults;
    }
  }
}
