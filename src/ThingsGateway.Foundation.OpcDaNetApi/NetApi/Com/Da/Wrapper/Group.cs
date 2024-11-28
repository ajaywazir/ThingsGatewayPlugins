

using Opc;
using Opc.Da;
using OpcRcw.Da;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
  public class Group : 
    ConnectionPointContainer,
    IDisposable,
    IOPCItemMgt,
    IOPCSyncIO,
    IOPCSyncIO2,
    IOPCAsyncIO2,
    IOPCAsyncIO3,
    IOPCGroupStateMgt,
    IOPCGroupStateMgt2,
    IOPCItemDeadbandMgt,
    IOPCItemSamplingMgt
  {
    private bool m_disposed;
    private Server m_server;
    private int m_serverHandle;
    private int m_clientHandle;
    private string m_name;
    private ISubscription m_subscription;
    private int m_timebias;
    private int m_lcid = 2048;
    private Hashtable m_items = new Hashtable();
    private Hashtable m_requests = new Hashtable();
    private int m_nextHandle = 1000;
    private DataChangedEventHandler m_dataChanged;
    private const int LOCALE_SYSTEM_DEFAULT = 2048;

    public Group(
      Server server,
      string name,
      int handle,
      int lcid,
      int timebias,
      ISubscription subscription)
    {
      this.RegisterInterface(typeof (IOPCDataCallback).GUID);
      this.m_server = server;
      this.m_name = name;
      this.m_serverHandle = handle;
      this.m_lcid = lcid;
      this.m_timebias = timebias;
      this.m_subscription = subscription;
    }

    public string Name
    {
      get
      {
        lock (this)
          return this.m_name;
      }
    }

    public int ServerHandle
    {
      get
      {
        lock (this)
          return this.m_serverHandle;
      }
    }

    public override void OnAdvise(Guid riid)
    {
      lock (this)
      {
        this.m_dataChanged = new DataChangedEventHandler(this.OnDataChanged);
        this.m_subscription.DataChanged += this.m_dataChanged;
      }
    }

    public override void OnUnadvise(Guid riid)
    {
      lock (this)
      {
        if (this.m_dataChanged == null)
          return;
        this.m_subscription.DataChanged -= this.m_dataChanged;
        this.m_dataChanged = (DataChangedEventHandler) null;
      }
    }

    ~Group() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      lock (this)
      {
        if (disposing)
        {
          if (this.m_subscription != null)
          {
            this.m_subscription.DataChanged -= this.m_dataChanged;
            this.m_server.IServer.CancelSubscription(this.m_subscription);
            this.m_subscription = (ISubscription) null;
          }
        }
      }
      this.m_disposed = true;
    }

    public void SetActiveState(int dwCount, int[] phServer, int bActive, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].Active = bActive != 0;
                items[index].ActiveSpecified = true;
              }
              ItemResult[] results = this.m_subscription.ModifyItems(8, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void AddItems(
      int dwCount,
      OPCITEMDEF[] pItemArray,
      out IntPtr ppAddResults,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (pItemArray != null)
          {
            try
            {
              Item[] objArray = new Item[dwCount];
              for (int index = 0; index < objArray.Length; ++index)
              {
                objArray[index] = new Item();
                objArray[index].ItemName = pItemArray[index].szItemID;
                objArray[index].ItemPath = pItemArray[index].szAccessPath;
                objArray[index].ClientHandle = (object) pItemArray[index].hClient;
                objArray[index].ServerHandle = (object) null;
                objArray[index].Active = pItemArray[index].bActive != 0;
                objArray[index].ActiveSpecified = true;
                objArray[index].ReqType = OpcCom.Interop.GetType((VarEnum) pItemArray[index].vtRequestedDataType);
              }
              ItemResult[] results = this.m_subscription.AddItems(objArray);
              if (results == null || results.Length != objArray.Length)
                throw Server.CreateException(-2147467259);
              ItemPropertyCollection[] properties = this.m_server.IServer.GetProperties((ItemIdentifier[]) objArray, new PropertyID[2]
              {
                Property.DATATYPE,
                Property.ACCESSRIGHTS
              }, true);
              ppAddResults = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (OPCITEMRESULT)) * results.Length);
              IntPtr ptr = ppAddResults;
              for (int index = 0; index < results.Length; ++index)
              {
                OPCITEMRESULT structure = new OPCITEMRESULT();
                structure.hServer = 0;
                structure.dwBlobSize = 0;
                structure.pBlob = IntPtr.Zero;
                structure.vtCanonicalDataType = (short) 0;
                structure.dwAccessRights = 0;
                structure.wReserved = (short) 0;
                if (results[index].ResultID.Succeeded())
                {
                  structure.hServer = ++this.m_nextHandle;
                  structure.vtCanonicalDataType = (short) OpcCom.Da.Interop.MarshalPropertyValue(Property.DATATYPE, properties[index][0].Value);
                  structure.dwAccessRights = (int) OpcCom.Da.Interop.MarshalPropertyValue(Property.ACCESSRIGHTS, properties[index][1].Value);
                  this.m_items[(object) this.m_nextHandle] = (object) results[index];
                }
                Marshal.StructureToPtr<OPCITEMRESULT>(structure, ptr, false);
                ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (OPCITEMRESULT)));
              }
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

    public void SetClientHandles(int dwCount, int[] phServer, int[] phClient, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (phClient != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].ClientHandle = (object) phClient[index];
              }
              ItemResult[] results = this.m_subscription.ModifyItems(2, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void SetDatatypes(
      int dwCount,
      int[] phServer,
      short[] pRequestedDatatypes,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pRequestedDatatypes != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].ReqType = OpcCom.Interop.GetType((VarEnum) pRequestedDatatypes[index]);
              }
              ItemResult[] results = this.m_subscription.ModifyItems(64, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void ValidateItems(
      int dwCount,
      OPCITEMDEF[] pItemArray,
      int bBlobUpdate,
      out IntPtr ppValidationResults,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (pItemArray != null)
          {
            try
            {
              Item[] objArray = new Item[dwCount];
              for (int index = 0; index < objArray.Length; ++index)
              {
                objArray[index] = new Item();
                objArray[index].ItemName = pItemArray[index].szItemID;
                objArray[index].ItemPath = pItemArray[index].szAccessPath;
                objArray[index].ClientHandle = (object) pItemArray[index].hClient;
                objArray[index].ServerHandle = (object) null;
                objArray[index].Active = false;
                objArray[index].ActiveSpecified = true;
                objArray[index].ReqType = OpcCom.Interop.GetType((VarEnum) pItemArray[index].vtRequestedDataType);
              }
              ItemResult[] itemResultArray = this.m_subscription.AddItems(objArray);
              if (itemResultArray == null || itemResultArray.Length != objArray.Length)
                throw Server.CreateException(-2147467259);
              this.m_subscription.RemoveItems((ItemIdentifier[]) itemResultArray);
              ItemPropertyCollection[] properties = this.m_server.IServer.GetProperties((ItemIdentifier[]) objArray, new PropertyID[2]
              {
                Property.DATATYPE,
                Property.ACCESSRIGHTS
              }, true);
              ppValidationResults = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (OPCITEMRESULT)) * itemResultArray.Length);
              IntPtr ptr = ppValidationResults;
              for (int index = 0; index < itemResultArray.Length; ++index)
              {
                OPCITEMRESULT structure = new OPCITEMRESULT();
                structure.hServer = 0;
                structure.dwBlobSize = 0;
                structure.pBlob = IntPtr.Zero;
                structure.vtCanonicalDataType = (short) 0;
                structure.dwAccessRights = 0;
                structure.wReserved = (short) 0;
                if (itemResultArray[index].ResultID.Succeeded())
                {
                  structure.vtCanonicalDataType = (short) OpcCom.Da.Interop.MarshalPropertyValue(Property.DATATYPE, properties[index][0].Value);
                  structure.dwAccessRights = (int) OpcCom.Da.Interop.MarshalPropertyValue(Property.ACCESSRIGHTS, properties[index][1].Value);
                }
                Marshal.StructureToPtr<OPCITEMRESULT>(structure, ptr, false);
                ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (OPCITEMRESULT)));
              }
              ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) itemResultArray);
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



    public void RemoveItems(int dwCount, int[] phServer, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        int[] array = (int[]) new ArrayList(this.m_items.Keys).ToArray(typeof (int));
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              ItemIdentifier[] items = new ItemIdentifier[dwCount];
              for (int index = 0; index < items.Length; ++index)
                items[index] = new ItemIdentifier((ItemIdentifier) this.m_items[(object) phServer[index]]);
              IdentifiedResult[] results = this.m_subscription.RemoveItems(items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items.Remove((object) phServer[index]);
              }
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
      OPCDATASOURCE dwSource,
      int dwCount,
      int[] phServer,
      out IntPtr ppItemValues,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].MaxAge = dwSource == OPCDATASOURCE.OPC_DS_DEVICE ? 0 : int.MaxValue;
                items[index].MaxAgeSpecified = true;
              }
              ItemValueResult[] itemValueResultArray = this.m_subscription.Read(items);
              if (itemValueResultArray == null || itemValueResultArray.Length != items.Length)
                throw Server.CreateException(-2147467259);
              ppItemValues = OpcCom.Da.Interop.GetItemStates(itemValueResultArray);
              ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) itemValueResultArray);
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

    public void Write(int dwCount, int[] phServer, object[] pItemValues, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pItemValues != null)
          {
            try
            {
              ItemValue[] items = new ItemValue[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new ItemValue((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].Value = pItemValues[index];
                items[index].Quality = Quality.Bad;
                items[index].QualitySpecified = false;
                items[index].Timestamp = DateTime.MinValue;
                items[index].TimestampSpecified = false;
              }
              IdentifiedResult[] results = this.m_subscription.Write(items);
              if (results == null || results.Length != items.Length)
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

    public void ReadMaxAge(
      int dwCount,
      int[] phServer,
      int[] pdwMaxAge,
      out IntPtr ppvValues,
      out IntPtr ppwQualities,
      out IntPtr ppftTimeStamps,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pdwMaxAge != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].MaxAge = pdwMaxAge[index] < 0 ? int.MaxValue : pdwMaxAge[index];
                items[index].MaxAgeSpecified = true;
              }
              ItemValueResult[] results = this.m_subscription.Read(items);
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

    public void WriteVQT(int dwCount, int[] phServer, OPCITEMVQT[] pItemVQT, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pItemVQT != null)
          {
            try
            {
              ItemValue[] items = new ItemValue[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new ItemValue((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].Value = pItemVQT[index].vDataValue;
                items[index].Quality = new Quality(pItemVQT[index].wQuality);
                items[index].QualitySpecified = pItemVQT[index].bQualitySpecified != 0;
                items[index].Timestamp = OpcCom.Interop.GetFILETIME(OpcCom.Da.Interop.Convert(pItemVQT[index].ftTimeStamp));
                items[index].TimestampSpecified = pItemVQT[index].bTimeStampSpecified != 0;
              }
              IdentifiedResult[] results = this.m_subscription.Write(items);
              if (results == null || results.Length != items.Length)
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
      int[] phServer,
      int dwTransactionID,
      out int pdwCancelID,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount == 0 || phServer == null)
          throw Server.CreateException(-2147024809);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          Item[] items = new Item[dwCount];
          for (int index = 0; index < items.Length; ++index)
          {
            items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
            items[index].MaxAge = 0;
            items[index].MaxAgeSpecified = true;
          }
          pdwCancelID = this.AssignHandle();
          IRequest request = (IRequest) null;
          IdentifiedResult[] results = this.m_subscription.Read(items, (object) pdwCancelID, new ReadCompleteEventHandler(this.OnReadComplete), out request);
          if (results == null || results.Length != items.Length)
            throw Server.CreateException(-2147467259);
          if (request != null)
            this.m_requests[(object) request] = (object) dwTransactionID;
          ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) results);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Write(
      int dwCount,
      int[] phServer,
      object[] pItemValues,
      int dwTransactionID,
      out int pdwCancelID,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount == 0 || phServer == null || pItemValues == null)
          throw Server.CreateException(-2147024809);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          ItemValue[] items = new ItemValue[dwCount];
          for (int index = 0; index < items.Length; ++index)
          {
            items[index] = new ItemValue((ItemIdentifier) this.m_items[(object) phServer[index]]);
            items[index].Value = pItemValues[index];
            items[index].Quality = Quality.Bad;
            items[index].QualitySpecified = false;
            items[index].Timestamp = DateTime.MinValue;
            items[index].TimestampSpecified = false;
          }
          pdwCancelID = this.AssignHandle();
          IRequest request = (IRequest) null;
          IdentifiedResult[] results = this.m_subscription.Write(items, (object) pdwCancelID, new WriteCompleteEventHandler(this.OnWriteComplete), out request);
          if (results == null || results.Length != items.Length)
            throw Server.CreateException(-2147467259);
          if (request != null)
            this.m_requests[(object) request] = (object) dwTransactionID;
          ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) results);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Cancel2(int dwCancelID)
    {
      // ISSUE: unable to decompile the method.
    }

    public void Refresh2(OPCDATASOURCE dwSource, int dwTransactionID, out int pdwCancelID)
    {
      lock (this)
      {
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        this.RefreshMaxAge(dwSource == OPCDATASOURCE.OPC_DS_DEVICE ? 0 : int.MaxValue, dwTransactionID, out pdwCancelID);
      }
    }

    public void SetEnable(int bEnable)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          this.m_subscription.SetEnabled(bEnable != 0);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetEnable(out int pbEnable)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          pbEnable = this.m_subscription.GetEnabled() ? 1 : 0;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void ReadMaxAge(
      int dwCount,
      int[] phServer,
      int[] pdwMaxAge,
      int dwTransactionID,
      out int pdwCancelID,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount == 0 || phServer == null || pdwMaxAge == null)
          throw Server.CreateException(-2147024809);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          Item[] items = new Item[dwCount];
          for (int index = 0; index < items.Length; ++index)
          {
            items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
            items[index].MaxAge = pdwMaxAge[index] < 0 ? int.MaxValue : pdwMaxAge[index];
            items[index].MaxAgeSpecified = true;
          }
          pdwCancelID = this.AssignHandle();
          IRequest request = (IRequest) null;
          IdentifiedResult[] results = this.m_subscription.Read(items, (object) pdwCancelID, new ReadCompleteEventHandler(this.OnReadComplete), out request);
          if (results == null || results.Length != items.Length)
            throw Server.CreateException(-2147467259);
          if (request != null)
            this.m_requests[(object) request] = (object) dwTransactionID;
          ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) results);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void WriteVQT(
      int dwCount,
      int[] phServer,
      OPCITEMVQT[] pItemVQT,
      int dwTransactionID,
      out int pdwCancelID,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount == 0 || phServer == null || pItemVQT == null)
          throw Server.CreateException(-2147024809);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          ItemValue[] items = new ItemValue[dwCount];
          for (int index = 0; index < items.Length; ++index)
          {
            items[index] = new ItemValue((ItemIdentifier) this.m_items[(object) phServer[index]]);
            items[index].Value = pItemVQT[index].vDataValue;
            items[index].Quality = new Quality(pItemVQT[index].wQuality);
            items[index].QualitySpecified = pItemVQT[index].bQualitySpecified != 0;
            items[index].Timestamp = OpcCom.Interop.GetFILETIME(OpcCom.Da.Interop.Convert(pItemVQT[index].ftTimeStamp));
            items[index].TimestampSpecified = pItemVQT[index].bTimeStampSpecified != 0;
          }
          pdwCancelID = this.AssignHandle();
          IRequest request = (IRequest) null;
          IdentifiedResult[] results = this.m_subscription.Write(items, (object) pdwCancelID, new WriteCompleteEventHandler(this.OnWriteComplete), out request);
          if (results == null || results.Length != items.Length)
            throw Server.CreateException(-2147467259);
          if (request != null)
            this.m_requests[(object) request] = (object) dwTransactionID;
          ppErrors = OpcCom.Da.Interop.GetHRESULTs((IResult[]) results);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void RefreshMaxAge(int dwMaxAge, int dwTransactionID, out int pdwCancelID)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (!this.IsConnected(typeof (IOPCDataCallback).GUID))
          throw Server.CreateException(-2147220992);
        try
        {
          pdwCancelID = this.AssignHandle();
          IRequest request = (IRequest) null;
          this.m_subscription.Refresh((object) pdwCancelID, out request);
          if (request == null)
            return;
          this.m_requests[(object) request] = (object) dwTransactionID;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetState(
      out int pUpdateRate,
      out int pActive,
      out string ppName,
      out int pTimeBias,
      out float pPercentDeadband,
      out int pLCID,
      out int phClientGroup,
      out int phServerGroup)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        try
        {
          SubscriptionState state = this.m_subscription.GetState();
          pUpdateRate = state != null ? state.UpdateRate : throw Server.CreateException(-2147467259);
          pActive = state.Active ? 1 : 0;
          ppName = state.Name;
          pTimeBias = this.m_timebias;
          pPercentDeadband = state.Deadband;
          pLCID = this.m_lcid;
          phClientGroup = this.m_clientHandle = (int) state.ClientHandle;
          phServerGroup = this.m_serverHandle;
          this.m_name = state.Name;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void CloneGroup(string szName, ref Guid riid, out object ppUnk)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        Group group = (Group) null;
        try
        {
          SubscriptionState state = this.m_subscription.GetState();
          state.Name = szName;
          state.Active = false;
          group = this.m_server.CreateGroup(ref state, this.m_lcid, this.m_timebias);
          Item[] items = new Item[this.m_items.Count];
          int num = 0;
          foreach (Item obj in (IEnumerable) this.m_items.Values)
            items[num++] = obj;
          group.AddItems(items);
          ppUnk = (object) group;
        }
        catch (Exception ex)
        {
          if (group != null)
          {
            try
            {
              this.m_server.RemoveGroup(group.ServerHandle, 0);
            }
            catch
            {
            }
          }
          throw Server.CreateException(ex);
        }
      }
    }

    public void SetName(string szName)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        try
        {
          SubscriptionState state = this.m_subscription.GetState();
          if (state == null)
            throw Server.CreateException(-2147467259);
          int errorCode = this.m_server.SetGroupName(state.Name, szName);
          if (errorCode != 0)
            throw new ExternalException(nameof (SetName), errorCode);
          this.m_name = state.Name = szName;
          this.m_subscription.ModifyState(1, state);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void SetState(
      IntPtr pRequestedUpdateRate,
      out int pRevisedUpdateRate,
      IntPtr pActive,
      IntPtr pTimeBias,
      IntPtr pPercentDeadband,
      IntPtr pLCID,
      IntPtr phClientGroup)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        try
        {
          SubscriptionState state = new SubscriptionState();
          if (state == null)
            throw Server.CreateException(-2147467259);
          int masks = 0;
          if (pRequestedUpdateRate != IntPtr.Zero)
          {
            state.UpdateRate = Marshal.ReadInt32(pRequestedUpdateRate);
            masks |= 16;
          }
          if (pActive != IntPtr.Zero)
          {
            state.Active = Marshal.ReadInt32(pActive) != 0;
            masks |= 8;
          }
          if (pTimeBias != IntPtr.Zero)
            this.m_timebias = Marshal.ReadInt32(pTimeBias);
          if (pPercentDeadband != IntPtr.Zero)
          {
            float[] destination = new float[1];
            Marshal.Copy(pPercentDeadband, destination, 0, 1);
            state.Deadband = destination[0];
            masks |= 128;
          }
          if (pLCID != IntPtr.Zero)
          {
            this.m_lcid = Marshal.ReadInt32(pLCID);
            state.Locale = OpcCom.Interop.GetLocale(this.m_lcid);
            masks |= 4;
          }
          if (phClientGroup != IntPtr.Zero)
          {
            state.ClientHandle = (object) (this.m_clientHandle = Marshal.ReadInt32(phClientGroup));
            masks |= 2;
          }
          SubscriptionState subscriptionState = this.m_subscription.ModifyState(masks, state);
          pRevisedUpdateRate = subscriptionState.UpdateRate;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetKeepAlive(out int pdwKeepAliveTime)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        try
        {
          pdwKeepAliveTime = (this.m_subscription.GetState() ?? throw Server.CreateException(-2147467259)).KeepAlive;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void SetKeepAlive(int dwKeepAliveTime, out int pdwRevisedKeepAliveTime)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        try
        {
          SubscriptionState state = new SubscriptionState();
          if (state == null)
            throw Server.CreateException(-2147467259);
          state.KeepAlive = dwKeepAliveTime;
          SubscriptionState subscriptionState = this.m_subscription.ModifyState(32, state);
          pdwRevisedKeepAliveTime = subscriptionState.KeepAlive;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void SetItemDeadband(
      int dwCount,
      int[] phServer,
      float[] pPercentDeadband,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pPercentDeadband != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].Deadband = pPercentDeadband[index];
                items[index].DeadbandSpecified = true;
              }
              ItemResult[] results = this.m_subscription.ModifyItems(128, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void GetItemDeadband(
      int dwCount,
      int[] phServer,
      out IntPtr ppPercentDeadband,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              float[] source1 = new float[dwCount];
              int[] source2 = new int[dwCount];
              for (int index = 0; index < dwCount; ++index)
              {
                ItemResult itemResult = (ItemResult) this.m_items[(object) phServer[index]];
                source2[index] = -1073479679;
                if (itemResult != null && itemResult.ResultID.Succeeded())
                {
                  if (itemResult.DeadbandSpecified)
                  {
                    source1[index] = itemResult.Deadband;
                    source2[index] = 0;
                  }
                  else
                    source2[index] = -1073478656;
                }
              }
              ppPercentDeadband = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (float)) * dwCount);
              Marshal.Copy(source1, 0, ppPercentDeadband, dwCount);
              ppErrors = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (int)) * dwCount);
              Marshal.Copy(source2, 0, ppErrors, dwCount);
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

    public void ClearItemDeadband(int dwCount, int[] phServer, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              ArrayList arrayList = new ArrayList();
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                Item obj = (Item) this.m_items[(object) phServer[index]];
                items[index] = new Item((ItemIdentifier) obj);
                if (obj != null)
                {
                  if (obj.DeadbandSpecified)
                  {
                    items[index].Deadband = 0.0f;
                    items[index].DeadbandSpecified = false;
                  }
                  else
                    arrayList.Add((object) index);
                }
              }
              ItemResult[] results = this.m_subscription.ModifyItems(128, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              foreach (int index in arrayList)
              {
                if (results[index].ResultID.Succeeded())
                  results[index].ResultID = new ResultID(-1073478656L);
              }
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void SetItemSamplingRate(
      int dwCount,
      int[] phServer,
      int[] pdwRequestedSamplingRate,
      out IntPtr ppdwRevisedSamplingRate,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pdwRequestedSamplingRate != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].SamplingRate = pdwRequestedSamplingRate[index];
                items[index].SamplingRateSpecified = true;
              }
              ItemResult[] results = this.m_subscription.ModifyItems(256, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              int[] source = new int[dwCount];
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                {
                  this.m_items[(object) phServer[index]] = (object) results[index];
                  source[index] = results[index].SamplingRate;
                }
              }
              ppdwRevisedSamplingRate = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (int)) * dwCount);
              Marshal.Copy(source, 0, ppdwRevisedSamplingRate, dwCount);
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

    public void GetItemSamplingRate(
      int dwCount,
      int[] phServer,
      out IntPtr ppdwSamplingRate,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              int[] source1 = new int[dwCount];
              int[] source2 = new int[dwCount];
              for (int index = 0; index < dwCount; ++index)
              {
                ItemResult itemResult = (ItemResult) this.m_items[(object) phServer[index]];
                source2[index] = -1073479679;
                if (itemResult != null && itemResult.ResultID.Succeeded())
                {
                  if (itemResult.SamplingRateSpecified)
                  {
                    source1[index] = itemResult.SamplingRate;
                    source2[index] = 0;
                  }
                  else
                    source2[index] = -1073478651;
                }
              }
              ppdwSamplingRate = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (int)) * dwCount);
              Marshal.Copy(source1, 0, ppdwSamplingRate, dwCount);
              ppErrors = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (int)) * dwCount);
              Marshal.Copy(source2, 0, ppErrors, dwCount);
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

    public void ClearItemSamplingRate(int dwCount, int[] phServer, out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].SamplingRate = 0;
                items[index].SamplingRateSpecified = false;
              }
              ItemResult[] results = this.m_subscription.ModifyItems(256, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void SetItemBufferEnable(
      int dwCount,
      int[] phServer,
      int[] pbEnable,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0 && phServer != null)
        {
          if (pbEnable != null)
          {
            try
            {
              Item[] items = new Item[dwCount];
              for (int index = 0; index < items.Length; ++index)
              {
                items[index] = new Item((ItemIdentifier) this.m_items[(object) phServer[index]]);
                items[index].EnableBuffering = pbEnable[index] != 0;
                items[index].EnableBufferingSpecified = pbEnable[index] != 0;
              }
              ItemResult[] results = this.m_subscription.ModifyItems(512, items);
              if (results == null || results.Length != items.Length)
                throw Server.CreateException(-2147467259);
              for (int index = 0; index < dwCount; ++index)
              {
                if (results[index].ResultID.Succeeded())
                  this.m_items[(object) phServer[index]] = (object) results[index];
              }
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

    public void GetItemBufferEnable(
      int dwCount,
      int[] phServer,
      out IntPtr ppbEnable,
      out IntPtr ppErrors)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        if (dwCount != 0)
        {
          if (phServer != null)
          {
            try
            {
              int[] source1 = new int[dwCount];
              int[] source2 = new int[dwCount];
              for (int index = 0; index < dwCount; ++index)
              {
                ItemResult itemResult = (ItemResult) this.m_items[(object) phServer[index]];
                source2[index] = -1073479679;
                if (itemResult != null && itemResult.ResultID.Succeeded())
                {
                  source1[index] = !itemResult.EnableBuffering || !itemResult.EnableBufferingSpecified ? 0 : 1;
                  source2[index] = 0;
                }
              }
              ppbEnable = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (int)) * dwCount);
              Marshal.Copy(source1, 0, ppbEnable, dwCount);
              ppErrors = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (int)) * dwCount);
              Marshal.Copy(source2, 0, ppErrors, dwCount);
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

    private int AssignHandle() => ++this.m_nextHandle;

    private void OnDataChanged(
      object subscriptionHandle,
      object requestHandle,
      ItemValueResult[] results)
    {
      this.InvokeCallback(requestHandle, results, true);
    }

    private void OnReadComplete(object requestHandle, ItemValueResult[] results)
    {
      this.InvokeCallback(requestHandle, results, false);
    }

    private void InvokeCallback(object requestHandle, ItemValueResult[] results, bool dataChanged)
    {
      try
      {
        object obj = (object) null;
        int dwTransid = 0;
        int hGroup = 0;
        int hrMastererror = 0;
        int hrMasterquality = 0;
        int[] phClientItems = (int[]) null;
        object[] pvValues = (object[]) null;
        short[] pwQualities = (short[]) null;
        OpcRcw.Da.FILETIME[] pftTimeStamps = (OpcRcw.Da.FILETIME[]) null;
        int[] pErrors = (int[]) null;
        lock (this)
        {
          bool flag = false;
          IDictionaryEnumerator enumerator = this.m_requests.GetEnumerator();
          while (enumerator.MoveNext())
          {
            Opc.Da.Request key = (Opc.Da.Request) enumerator.Key;
            if (key.Handle.Equals(requestHandle))
            {
              dwTransid = (int) enumerator.Value;
              this.m_requests.Remove((object) key);
              flag = true;
              break;
            }
          }
          if (!dataChanged && !flag)
            return;
          obj = this.GetCallback(typeof (IOPCDataCallback).GUID);
          if (obj == null)
            return;
          hGroup = this.m_clientHandle;
          if (results != null)
          {
            phClientItems = new int[results.Length];
            pvValues = new object[results.Length];
            pwQualities = new short[results.Length];
            pftTimeStamps = new OpcRcw.Da.FILETIME[results.Length];
            pErrors = new int[results.Length];
            for (int index1 = 0; index1 < results.Length; ++index1)
            {
              phClientItems[index1] = (int) results[index1].ClientHandle;
              pvValues[index1] = results[index1].Value;
              short[] numArray = pwQualities;
              int index2 = index1;
              Quality quality;
              int num;
              if (!results[index1].QualitySpecified)
              {
                num = 0;
              }
              else
              {
                quality = results[index1].Quality;
                num = (int) quality.GetCode();
              }
              numArray[index2] = (short) num;
              pftTimeStamps[index1] = OpcCom.Da.Interop.Convert(OpcCom.Interop.GetFILETIME(results[index1].Timestamp));
              pErrors[index1] = OpcCom.Interop.GetResultID(results[index1].ResultID);
              quality = results[index1].Quality;
              if (quality.QualityBits != qualityBits.good)
                hrMasterquality = 1;
              if (results[index1].ResultID != ResultID.S_OK)
                hrMastererror = 1;
            }
          }
        }
        if (dataChanged)
          ((IOPCDataCallback) obj).OnDataChange(dwTransid, hGroup, hrMasterquality, hrMastererror, phClientItems.Length, phClientItems, pvValues, pwQualities, pftTimeStamps, pErrors);
        else
          ((IOPCDataCallback) obj).OnReadComplete(dwTransid, hGroup, hrMasterquality, hrMastererror, phClientItems.Length, phClientItems, pvValues, pwQualities, pftTimeStamps, pErrors);
      }
      catch (Exception ex)
      {
        string message = ex.Message;
      }
    }

    private void OnWriteComplete(object clientHandle, IdentifiedResult[] results)
    {
      try
      {
        object obj = (object) null;
        int dwTransid = -1;
        int hGroup = -1;
        int hrMastererr = 0;
        int[] pClienthandles = (int[]) null;
        int[] pErrors = (int[]) null;
        lock (this)
        {
          bool flag = false;
          IDictionaryEnumerator enumerator = this.m_requests.GetEnumerator();
          while (enumerator.MoveNext())
          {
            Opc.Da.Request key = (Opc.Da.Request) enumerator.Key;
            if (key.Handle.Equals(clientHandle))
            {
              dwTransid = (int) enumerator.Value;
              this.m_requests.Remove((object) key);
              flag = true;
              break;
            }
          }
          if (!flag)
            return;
          obj = this.GetCallback(typeof (IOPCDataCallback).GUID);
          if (obj == null)
            return;
          hGroup = this.m_clientHandle;
          if (results != null)
          {
            pClienthandles = new int[results.Length];
            pErrors = new int[results.Length];
            for (int index = 0; index < results.Length; ++index)
            {
              pClienthandles[index] = (int) results[index].ClientHandle;
              pErrors[index] = OpcCom.Interop.GetResultID(results[index].ResultID);
              if (results[index].ResultID != ResultID.S_OK)
                hrMastererr = 1;
            }
          }
        }
        ((IOPCDataCallback) obj).OnWriteComplete(dwTransid, hGroup, hrMastererr, pClienthandles.Length, pClienthandles, pErrors);
      }
      catch (Exception ex)
      {
        string message = ex.Message;
      }
    }

    private void AddItems(Item[] items)
    {
      lock (this)
      {
        if (this.m_subscription == null)
          throw Server.CreateException(-2147467259);
        ItemResult[] itemResultArray = items != null ? this.m_subscription.AddItems(items) : throw Server.CreateException(-2147024809);
        if (itemResultArray == null || itemResultArray.Length != items.Length)
          throw Server.CreateException(-2147467259);
        for (int index = 0; index < itemResultArray.Length; ++index)
        {
          if (itemResultArray[index].ResultID.Succeeded())
            this.m_items[(object) ++this.m_nextHandle] = (object) itemResultArray[index];
        }
      }
    }
  }
}
