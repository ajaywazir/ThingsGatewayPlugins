

using Opc;
using Opc.Da;

using OpcRcw.Da;

using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da
{
    public class Subscription : ISubscription, IDisposable
    {
        private bool m_disposed;
        protected object m_group;
        protected ConnectionPoint m_connection;
        private Subscription.Callback m_callback;
        protected string m_name;
        protected object m_handle;
        protected int m_filters = 9;
        private Subscription.ItemTable m_items = new Subscription.ItemTable();
        protected int m_counter;

        internal Subscription(object group, SubscriptionState state, int filters)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            m_group = group;
            m_name = state.Name;
            m_handle = state.ClientHandle;
            m_filters = filters;
            m_callback = new Subscription.Callback(state.ClientHandle, m_filters, m_items);
        }

        ~Subscription() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;
            lock (this)
            {
                if (disposing && m_group != null && m_connection != null)
                {
                    m_connection.Dispose();
                    m_connection = (ConnectionPoint)null;
                }
                if (m_group != null)
                {
                    OpcCom.Interop.ReleaseServer(m_group);
                    m_group = (object)null;
                }
            }
            m_disposed = true;
        }

        public event DataChangedEventHandler DataChanged
        {
            add
            {
                lock (this)
                {
                    m_callback.DataChanged += value;
                    Advise();
                }
            }
            remove
            {
                lock (this)
                {
                    m_callback.DataChanged -= value;
                    Unadvise();
                }
            }
        }

        public int GetResultFilters()
        {
            lock (this)
                return m_filters;
        }

        public void SetResultFilters(int filters)
        {
            lock (this)
            {
                m_filters = filters;
                m_callback.SetFilters(m_handle, m_filters);
            }
        }

        public SubscriptionState GetState()
        {
            lock (this)
            {
                SubscriptionState state = new SubscriptionState();
                state.ClientHandle = m_handle;
                try
                {
                    string ppName = (string)null;
                    int pActive = 0;
                    int pUpdateRate = 0;
                    float pPercentDeadband = 0.0f;
                    int pTimeBias = 0;
                    int pLCID = 0;
                    int phClientGroup = 0;
                    int phServerGroup = 0;
                    ((IOPCGroupStateMgt)m_group).GetState(out pUpdateRate, out pActive, out ppName, out pTimeBias, out pPercentDeadband, out pLCID, out phClientGroup, out phServerGroup);
                    state.Name = ppName;
                    state.ServerHandle = (object)phServerGroup;
                    state.Active = pActive != 0;
                    state.UpdateRate = pUpdateRate;
                    state.Deadband = pPercentDeadband;
                    state.Locale = OpcCom.Interop.GetLocale(pLCID);
                    m_name = state.Name;
                    try
                    {
                        int pdwKeepAliveTime = 0;
                        ((IOPCGroupStateMgt2)m_group).GetKeepAlive(out pdwKeepAliveTime);
                        state.KeepAlive = pdwKeepAliveTime;
                    }
                    catch
                    {
                        state.KeepAlive = 0;
                    }
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCGroupStateMgt.GetState", ex);
                }
                return state;
            }
        }

        public SubscriptionState ModifyState(int masks, SubscriptionState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            lock (this)
            {
                if ((masks & 1) != 0)
                {
                    if (state.Name != m_name)
                    {
                        try
                        {
                            ((IOPCGroupStateMgt)m_group).SetName(state.Name);
                            m_name = state.Name;
                        }
                        catch (Exception ex)
                        {
                            throw OpcCom.Interop.CreateException("IOPCGroupStateMgt.SetName", ex);
                        }
                    }
                }
                if ((masks & 2) != 0)
                {
                    m_handle = state.ClientHandle;
                    m_callback.SetFilters(m_handle, m_filters);
                }
                int num = state.Active ? 1 : 0;
                int locale = (masks & 4) != 0 ? OpcCom.Interop.GetLocale(state.Locale) : 0;
                GCHandle gcHandle1 = GCHandle.Alloc((object)num, GCHandleType.Pinned);
                GCHandle gcHandle2 = GCHandle.Alloc((object)locale, GCHandleType.Pinned);
                GCHandle gcHandle3 = GCHandle.Alloc((object)state.UpdateRate, GCHandleType.Pinned);
                GCHandle gcHandle4 = GCHandle.Alloc((object)state.Deadband, GCHandleType.Pinned);
                int pRevisedUpdateRate = 0;
                try
                {
                    ((IOPCGroupStateMgt)m_group).SetState((masks & 16) != 0 ? gcHandle3.AddrOfPinnedObject() : IntPtr.Zero, out pRevisedUpdateRate, (masks & 8) != 0 ? gcHandle1.AddrOfPinnedObject() : IntPtr.Zero, IntPtr.Zero, (masks & 128) != 0 ? gcHandle4.AddrOfPinnedObject() : IntPtr.Zero, (masks & 4) != 0 ? gcHandle2.AddrOfPinnedObject() : IntPtr.Zero, IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCGroupStateMgt.SetState", ex);
                }
                finally
                {
                    if (gcHandle1.IsAllocated)
                        gcHandle1.Free();
                    if (gcHandle2.IsAllocated)
                        gcHandle2.Free();
                    if (gcHandle3.IsAllocated)
                        gcHandle3.Free();
                    if (gcHandle4.IsAllocated)
                        gcHandle4.Free();
                }
                if ((masks & 32) != 0)
                {
                    int pdwRevisedKeepAliveTime = 0;
                    try
                    {
                        ((IOPCGroupStateMgt2)m_group).SetKeepAlive(state.KeepAlive, out pdwRevisedKeepAliveTime);
                    }
                    catch
                    {
                        state.KeepAlive = 0;
                    }
                }
                return GetState();
            }
        }

        public ItemResult[] AddItems(Item[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<ItemResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                int length = items.Length;
                OPCITEMDEF[] opcitemdeFs = Interop.GetOPCITEMDEFs(items);
                ItemResult[] itemResultArray1 = (ItemResult[])null;
                lock (m_items)
                {
                    for (int index = 0; index < length; ++index)
                        opcitemdeFs[index].hClient = ++m_counter;
                    IntPtr ppAddResults = IntPtr.Zero;
                    IntPtr ppErrors = IntPtr.Zero;
                    try
                    {
                        ((IOPCItemMgt)m_group).AddItems(length, opcitemdeFs, out ppAddResults, out ppErrors);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCItemMgt.AddItems", ex);
                    }
                    int[] itemResults = Interop.GetItemResults(ref ppAddResults, length, true);
                    int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, length, true);
                    itemResultArray1 = new ItemResult[length];
                    for (int index = 0; index < length; ++index)
                    {
                        itemResultArray1[index] = new ItemResult(items[index]);
                        itemResultArray1[index].ServerHandle = (object)itemResults[index];
                        itemResultArray1[index].ClientHandle = (object)opcitemdeFs[index].hClient;
                        if (!itemResultArray1[index].ActiveSpecified)
                        {
                            itemResultArray1[index].Active = true;
                            itemResultArray1[index].ActiveSpecified = true;
                        }
                        itemResultArray1[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
                        itemResultArray1[index].DiagnosticInfo = (string)null;
                        if (itemResultArray1[index].ResultID.Succeeded())
                        {
                            itemResultArray1[index].ClientHandle = items[index].ClientHandle;
                            m_items[(object)opcitemdeFs[index].hClient] = new ItemIdentifier((ItemIdentifier)itemResultArray1[index]);
                            itemResultArray1[index].ClientHandle = (object)opcitemdeFs[index].hClient;
                        }
                    }
                }
                UpdateDeadbands(itemResultArray1);
                UpdateSamplingRates(itemResultArray1);
                SetEnableBuffering(itemResultArray1);
                lock (m_items)
                {
                    ItemResult[] itemResultArray2 = (ItemResult[])m_items.ApplyFilters(m_filters, (ItemIdentifier[])itemResultArray1);
                    if ((m_filters & 4) != 0)
                    {
                        for (int index = 0; index < length; ++index)
                        {
                            if (itemResultArray2[index].ResultID.Failed())
                                itemResultArray2[index].ClientHandle = items[index].ClientHandle;
                        }
                    }
                    return itemResultArray2;
                }
            }
        }

        public ItemResult[] ModifyItems(int masks, Item[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<ItemResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                ItemResult[] itemResultArray = (ItemResult[])null;
                lock (m_items)
                    itemResultArray = m_items.CreateItems(items);
                if ((masks & 64) != 0)
                    SetReqTypes(itemResultArray);
                if ((masks & 8) != 0)
                    UpdateActive(itemResultArray);
                if ((masks & 128) != 0)
                    UpdateDeadbands(itemResultArray);
                if ((masks & 256) != 0)
                    UpdateSamplingRates(itemResultArray);
                if ((masks & 512) != 0)
                    SetEnableBuffering(itemResultArray);
                lock (m_items)
                    return (ItemResult[])m_items.ApplyFilters(m_filters, (ItemIdentifier[])itemResultArray);
            }
        }

        public IdentifiedResult[] RemoveItems(ItemIdentifier[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<IdentifiedResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                ItemIdentifier[] itemIdentifierArray = (ItemIdentifier[])null;
                lock (m_items)
                    itemIdentifierArray = m_items.GetItemIDs(items);
                int[] phServer = new int[itemIdentifierArray.Length];
                for (int index = 0; index < itemIdentifierArray.Length; ++index)
                    phServer[index] = (int)itemIdentifierArray[index].ServerHandle;
                IntPtr ppErrors = IntPtr.Zero;
                try
                {
                    ((IOPCItemMgt)m_group).RemoveItems(itemIdentifierArray.Length, phServer, out ppErrors);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCItemMgt.RemoveItems", ex);
                }
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, itemIdentifierArray.Length, true);
                IdentifiedResult[] results = new IdentifiedResult[itemIdentifierArray.Length];
                ArrayList arrayList = new ArrayList(itemIdentifierArray.Length);
                for (int index = 0; index < itemIdentifierArray.Length; ++index)
                {
                    results[index] = new IdentifiedResult(itemIdentifierArray[index]);
                    results[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
                    results[index].DiagnosticInfo = (string)null;
                    if (results[index].ResultID.Succeeded())
                        arrayList.Add(results[index].ClientHandle);
                }
                lock (m_items)
                {
                    IdentifiedResult[] identifiedResultArray = (IdentifiedResult[])m_items.ApplyFilters(m_filters, (ItemIdentifier[])results);
                    foreach (int handle in arrayList)
                        m_items[(object)handle] = (ItemIdentifier)null;
                    return identifiedResultArray;
                }
            }
        }

        public ItemValueResult[] Read(Item[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<ItemValueResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                ItemIdentifier[] itemIDs = (ItemIdentifier[])null;
                lock (m_items)
                    itemIDs = m_items.GetItemIDs((ItemIdentifier[])items);
                ItemValueResult[] results = Read(itemIDs, items);
                lock (m_items)
                    return (ItemValueResult[])m_items.ApplyFilters(m_filters, (ItemIdentifier[])results);
            }
        }

        public IdentifiedResult[] Write(ItemValue[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<IdentifiedResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                ItemIdentifier[] itemIDs = (ItemIdentifier[])null;
                lock (m_items)
                    itemIDs = m_items.GetItemIDs((ItemIdentifier[])items);
                IdentifiedResult[] results = Write(itemIDs, items);
                lock (m_items)
                    return (IdentifiedResult[])m_items.ApplyFilters(m_filters, (ItemIdentifier[])results);
            }
        }

        public IdentifiedResult[] Read(
          Item[] items,
          object requestHandle,
          ReadCompleteEventHandler callback,
          out IRequest request)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            request = (IRequest)null;
            if (items.Length == 0)
                return Array.Empty<IdentifiedResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                if (m_connection == null)
                    Advise();
                ItemIdentifier[] itemIDs = (ItemIdentifier[])null;
                lock (m_items)
                    itemIDs = m_items.GetItemIDs((ItemIdentifier[])items);
                Request request1 = new Request((ISubscription)this, requestHandle, m_filters, m_counter++, (Delegate)callback);
                m_callback.BeginRequest(request1);
                request = (IRequest)request1;
                int cancelID = 0;
                IdentifiedResult[] results;
                try
                {
                    results = BeginRead(itemIDs, items, request1.RequestID, out cancelID);
                }
                catch (Exception ex)
                {
                    m_callback.EndRequest(request1);
                    throw ex;
                }
                lock (m_items)
                    m_items.ApplyFilters(m_filters | 4, (ItemIdentifier[])results);
                lock (request1)
                {
                    if (request1.BeginRead(cancelID, results))
                    {
                        m_callback.EndRequest(request1);
                        request = (IRequest)null;
                    }
                }
                return results;
            }
        }

        public IdentifiedResult[] Write(
          ItemValue[] items,
          object requestHandle,
          WriteCompleteEventHandler callback,
          out IRequest request)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            request = (IRequest)null;
            if (items.Length == 0)
                return Array.Empty<IdentifiedResult>();
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                if (m_connection == null)
                    Advise();
                ItemIdentifier[] itemIDs = (ItemIdentifier[])null;
                lock (m_items)
                    itemIDs = m_items.GetItemIDs((ItemIdentifier[])items);
                Request request1 = new Request((ISubscription)this, requestHandle, m_filters, m_counter++, (Delegate)callback);
                m_callback.BeginRequest(request1);
                request = (IRequest)request1;
                int cancelID = 0;
                IdentifiedResult[] results;
                try
                {
                    results = BeginWrite(itemIDs, items, request1.RequestID, out cancelID);
                }
                catch (Exception ex)
                {
                    m_callback.EndRequest(request1);
                    throw ex;
                }
                lock (m_items)
                    m_items.ApplyFilters(m_filters | 4, (ItemIdentifier[])results);
                lock (request1)
                {
                    if (request1.BeginWrite(cancelID, results))
                    {
                        m_callback.EndRequest(request1);
                        request = (IRequest)null;
                    }
                }
                return results;
            }
        }

        public void Cancel(IRequest request, CancelCompleteEventHandler callback)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            lock (this)
            {
                lock (request)
                {
                    if (!m_callback.CancelRequest((Request)request))
                        return;
                    ((Request)request).Callback = (Delegate)callback;
                    try
                    {
                        ((IOPCAsyncIO2)m_group).Cancel2(((Request)request).CancelID);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCAsyncIO2.Cancel2", ex);
                    }
                }
            }
        }

        public virtual void Refresh()
        {
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                try
                {
                    int pdwCancelID = 0;
                    ((IOPCAsyncIO3)m_group).RefreshMaxAge(int.MaxValue, ++m_counter, out pdwCancelID);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCAsyncIO3.RefreshMaxAge", ex);
                }
            }
        }

        public virtual void Refresh(object requestHandle, out IRequest request)
        {
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                if (m_connection == null)
                    Advise();
                Request request1 = new Request((ISubscription)this, requestHandle, m_filters, m_counter++, (Delegate)null);
                int pdwCancelID = 0;
                try
                {
                    ((IOPCAsyncIO3)m_group).RefreshMaxAge(0, request1.RequestID, out pdwCancelID);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCAsyncIO3.RefreshMaxAge", ex);
                }
                request = (IRequest)request1;
                lock (request)
                    request1.BeginRefresh(pdwCancelID);
            }
        }

        public virtual void SetEnabled(bool enabled)
        {
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                try
                {
                    ((IOPCAsyncIO3)m_group).SetEnable(enabled ? 1 : 0);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCAsyncIO3.SetEnable", ex);
                }
            }
        }

        public virtual bool GetEnabled()
        {
            lock (this)
            {
                if (m_group == null)
                    throw new NotConnectedException();
                try
                {
                    int pbEnable = 0;
                    ((IOPCAsyncIO3)m_group).GetEnable(out pbEnable);
                    return pbEnable != 0;
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCAsyncIO3.GetEnable", ex);
                }
            }
        }

        protected virtual ItemValueResult[] Read(ItemIdentifier[] itemIDs, Item[] items)
        {
            try
            {
                int[] phServer = new int[itemIDs.Length];
                int[] pdwMaxAge = new int[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                {
                    phServer[index] = (int)itemIDs[index].ServerHandle;
                    pdwMaxAge[index] = items[index].MaxAgeSpecified ? items[index].MaxAge : 0;
                }
                IntPtr ppvValues = IntPtr.Zero;
                IntPtr ppwQualities = IntPtr.Zero;
                IntPtr ppftTimeStamps = IntPtr.Zero;
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCSyncIO2)m_group).ReadMaxAge(itemIDs.Length, phServer, pdwMaxAge, out ppvValues, out ppwQualities, out ppftTimeStamps, out ppErrors);
                object[] varianTs = OpcCom.Interop.GetVARIANTs(ref ppvValues, itemIDs.Length, true);
                short[] int16s = OpcCom.Interop.GetInt16s(ref ppwQualities, itemIDs.Length, true);
                DateTime[] filetimEs = OpcCom.Interop.GetFILETIMEs(ref ppftTimeStamps, itemIDs.Length, true);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, itemIDs.Length, true);
                ItemValueResult[] itemValueResultArray = new ItemValueResult[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                {
                    itemValueResultArray[index] = new ItemValueResult(itemIDs[index]);
                    itemValueResultArray[index].Value = varianTs[index];
                    itemValueResultArray[index].Quality = new Quality(int16s[index]);
                    itemValueResultArray[index].QualitySpecified = varianTs[index] != null;
                    itemValueResultArray[index].Timestamp = filetimEs[index];
                    itemValueResultArray[index].TimestampSpecified = varianTs[index] != null;
                    itemValueResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
                    itemValueResultArray[index].DiagnosticInfo = (string)null;
                    if (int32s[index] == -1073479674)
                        itemValueResultArray[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
                }
                return itemValueResultArray;
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCSyncIO2.ReadMaxAge", ex);
            }
        }

        protected virtual IdentifiedResult[] Write(ItemIdentifier[] itemIDs, ItemValue[] items)
        {
            try
            {
                int[] phServer = new int[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                    phServer[index] = (int)itemIDs[index].ServerHandle;
                OPCITEMVQT[] opcitemvqTs = Interop.GetOPCITEMVQTs(items);
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCSyncIO2)m_group).WriteVQT(itemIDs.Length, phServer, opcitemvqTs, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, itemIDs.Length, true);
                IdentifiedResult[] identifiedResultArray = new IdentifiedResult[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                {
                    identifiedResultArray[index] = new IdentifiedResult(itemIDs[index]);
                    identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
                    identifiedResultArray[index].DiagnosticInfo = (string)null;
                    if (int32s[index] == -1073479674)
                        identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
                }
                return identifiedResultArray;
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCSyncIO2.WriteVQT", ex);
            }
        }

        protected virtual IdentifiedResult[] BeginRead(
          ItemIdentifier[] itemIDs,
          Item[] items,
          int requestID,
          out int cancelID)
        {
            try
            {
                int[] phServer = new int[itemIDs.Length];
                int[] pdwMaxAge = new int[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                {
                    phServer[index] = (int)itemIDs[index].ServerHandle;
                    pdwMaxAge[index] = items[index].MaxAgeSpecified ? items[index].MaxAge : 0;
                }
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCAsyncIO3)m_group).ReadMaxAge(itemIDs.Length, phServer, pdwMaxAge, requestID, out cancelID, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, itemIDs.Length, true);
                IdentifiedResult[] identifiedResultArray = new IdentifiedResult[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                {
                    identifiedResultArray[index] = new IdentifiedResult(itemIDs[index]);
                    identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
                    identifiedResultArray[index].DiagnosticInfo = (string)null;
                    if (int32s[index] == -1073479674)
                        identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
                }
                return identifiedResultArray;
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCAsyncIO3.ReadMaxAge", ex);
            }
        }

        protected virtual IdentifiedResult[] BeginWrite(
          ItemIdentifier[] itemIDs,
          ItemValue[] items,
          int requestID,
          out int cancelID)
        {
            try
            {
                int[] phServer = new int[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                    phServer[index] = (int)itemIDs[index].ServerHandle;
                OPCITEMVQT[] opcitemvqTs = Interop.GetOPCITEMVQTs(items);
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCAsyncIO3)m_group).WriteVQT(itemIDs.Length, phServer, opcitemvqTs, requestID, out cancelID, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, itemIDs.Length, true);
                IdentifiedResult[] identifiedResultArray = new IdentifiedResult[itemIDs.Length];
                for (int index = 0; index < itemIDs.Length; ++index)
                {
                    identifiedResultArray[index] = new IdentifiedResult(itemIDs[index]);
                    identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
                    identifiedResultArray[index].DiagnosticInfo = (string)null;
                    if (int32s[index] == -1073479674)
                        identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
                }
                return identifiedResultArray;
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCAsyncIO3.WriteVQT", ex);
            }
        }

        private void SetReqTypes(ItemResult[] items)
        {
            if (items == null || items.Length == 0)
                return;
            ArrayList arrayList = new ArrayList();
            ResultID resultId;
            foreach (ItemResult itemResult in items)
            {
                resultId = itemResult.ResultID;
                if (resultId.Succeeded() && itemResult.ReqType != (System.Type)null)
                    arrayList.Add((object)itemResult);
            }
            if (arrayList.Count == 0)
                return;
            try
            {
                int[] phServer = new int[arrayList.Count];
                short[] pRequestedDatatypes = new short[arrayList.Count];
                for (int index = 0; index < arrayList.Count; ++index)
                {
                    ItemResult itemResult = (ItemResult)arrayList[index];
                    phServer[index] = System.Convert.ToInt32(itemResult.ServerHandle);
                    pRequestedDatatypes[index] = (short)OpcCom.Interop.GetType(itemResult.ReqType);
                }
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemMgt)m_group).SetDatatypes(arrayList.Count, phServer, pRequestedDatatypes, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s.Length; ++index)
                {
                    resultId = OpcCom.Interop.GetResultID(int32s[index]);
                    if (resultId.Failed())
                    {
                        ItemResult itemResult = (ItemResult)arrayList[index];
                        itemResult.ResultID = ResultID.Da.E_BADTYPE;
                        itemResult.DiagnosticInfo = (string)null;
                    }
                }
            }
            catch
            {
                for (int index = 0; index < arrayList.Count; ++index)
                {
                    ItemResult itemResult = (ItemResult)arrayList[index];
                    itemResult.ResultID = ResultID.Da.E_BADTYPE;
                    itemResult.DiagnosticInfo = (string)null;
                }
            }
        }

        private void SetActive(ItemResult[] items, bool active)
        {
            if (items == null)
                return;
            if (items.Length == 0)
                return;
            try
            {
                int[] phServer = new int[items.Length];
                for (int index = 0; index < items.Length; ++index)
                    phServer[index] = System.Convert.ToInt32(items[index].ServerHandle);
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemMgt)m_group).SetActiveState(items.Length, phServer, active ? 1 : 0, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s.Length; ++index)
                {
                    if (OpcCom.Interop.GetResultID(int32s[index]).Failed())
                    {
                        items[index].Active = false;
                        items[index].ActiveSpecified = true;
                    }
                }
            }
            catch
            {
                for (int index = 0; index < items.Length; ++index)
                {
                    items[index].Active = false;
                    items[index].ActiveSpecified = true;
                }
            }
        }

        private void UpdateActive(ItemResult[] items)
        {
            if (items == null || items.Length == 0)
                return;
            ArrayList arrayList1 = new ArrayList();
            ArrayList arrayList2 = new ArrayList();
            foreach (ItemResult itemResult in items)
            {
                if (itemResult.ResultID.Succeeded() && itemResult.ActiveSpecified)
                {
                    if (itemResult.Active)
                        arrayList1.Add((object)itemResult);
                    else
                        arrayList2.Add((object)itemResult);
                }
            }
            SetActive((ItemResult[])arrayList1.ToArray(typeof(ItemResult)), true);
            SetActive((ItemResult[])arrayList2.ToArray(typeof(ItemResult)), false);
        }

        private void SetDeadbands(ItemResult[] items)
        {
            if (items == null)
                return;
            if (items.Length == 0)
                return;
            try
            {
                int[] phServer = new int[items.Length];
                float[] pPercentDeadband = new float[items.Length];
                for (int index = 0; index < items.Length; ++index)
                {
                    phServer[index] = System.Convert.ToInt32(items[index].ServerHandle);
                    pPercentDeadband[index] = items[index].Deadband;
                }
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemDeadbandMgt)m_group).SetItemDeadband(phServer.Length, phServer, pPercentDeadband, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s.Length; ++index)
                {
                    if (OpcCom.Interop.GetResultID(int32s[index]).Failed())
                    {
                        items[index].Deadband = 0.0f;
                        items[index].DeadbandSpecified = false;
                    }
                }
            }
            catch
            {
                for (int index = 0; index < items.Length; ++index)
                {
                    items[index].Deadband = 0.0f;
                    items[index].DeadbandSpecified = false;
                }
            }
        }

        private void ClearDeadbands(ItemResult[] items)
        {
            if (items == null)
                return;
            if (items.Length == 0)
                return;
            try
            {
                int[] phServer = new int[items.Length];
                for (int index = 0; index < items.Length; ++index)
                    phServer[index] = System.Convert.ToInt32(items[index].ServerHandle);
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemDeadbandMgt)m_group).ClearItemDeadband(phServer.Length, phServer, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s.Length; ++index)
                {
                    if (OpcCom.Interop.GetResultID(int32s[index]).Failed())
                    {
                        items[index].Deadband = 0.0f;
                        items[index].DeadbandSpecified = false;
                    }
                }
            }
            catch
            {
                for (int index = 0; index < items.Length; ++index)
                {
                    items[index].Deadband = 0.0f;
                    items[index].DeadbandSpecified = false;
                }
            }
        }

        private void UpdateDeadbands(ItemResult[] items)
        {
            if (items == null || items.Length == 0)
                return;
            ArrayList arrayList1 = new ArrayList();
            ArrayList arrayList2 = new ArrayList();
            foreach (ItemResult itemResult in items)
            {
                if (itemResult.ResultID.Succeeded())
                {
                    if (itemResult.DeadbandSpecified)
                        arrayList1.Add((object)itemResult);
                    else
                        arrayList2.Add((object)itemResult);
                }
            }
            SetDeadbands((ItemResult[])arrayList1.ToArray(typeof(ItemResult)));
            ClearDeadbands((ItemResult[])arrayList2.ToArray(typeof(ItemResult)));
        }

        private void SetSamplingRates(ItemResult[] items)
        {
            if (items == null)
                return;
            if (items.Length == 0)
                return;
            try
            {
                int[] phServer = new int[items.Length];
                int[] pdwRequestedSamplingRate = new int[items.Length];
                for (int index = 0; index < items.Length; ++index)
                {
                    phServer[index] = System.Convert.ToInt32(items[index].ServerHandle);
                    pdwRequestedSamplingRate[index] = items[index].SamplingRate;
                }
                IntPtr ppdwRevisedSamplingRate = IntPtr.Zero;
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemSamplingMgt)m_group).SetItemSamplingRate(phServer.Length, phServer, pdwRequestedSamplingRate, out ppdwRevisedSamplingRate, out ppErrors);
                int[] int32s1 = OpcCom.Interop.GetInt32s(ref ppdwRevisedSamplingRate, phServer.Length, true);
                int[] int32s2 = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s2.Length; ++index)
                {
                    if (items[index].SamplingRate != int32s1[index])
                    {
                        items[index].SamplingRate = int32s1[index];
                        items[index].SamplingRateSpecified = true;
                    }
                    else if (OpcCom.Interop.GetResultID(int32s2[index]).Failed())
                    {
                        items[index].SamplingRate = 0;
                        items[index].SamplingRateSpecified = false;
                    }
                }
            }
            catch
            {
                for (int index = 0; index < items.Length; ++index)
                {
                    items[index].SamplingRate = 0;
                    items[index].SamplingRateSpecified = false;
                }
            }
        }

        private void ClearSamplingRates(ItemResult[] items)
        {
            if (items == null)
                return;
            if (items.Length == 0)
                return;
            try
            {
                int[] phServer = new int[items.Length];
                for (int index = 0; index < items.Length; ++index)
                    phServer[index] = System.Convert.ToInt32(items[index].ServerHandle);
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemSamplingMgt)m_group).ClearItemSamplingRate(phServer.Length, phServer, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s.Length; ++index)
                {
                    if (OpcCom.Interop.GetResultID(int32s[index]).Failed())
                    {
                        items[index].SamplingRate = 0;
                        items[index].SamplingRateSpecified = false;
                    }
                }
            }
            catch
            {
                for (int index = 0; index < items.Length; ++index)
                {
                    items[index].SamplingRate = 0;
                    items[index].SamplingRateSpecified = false;
                }
            }
        }

        private void UpdateSamplingRates(ItemResult[] items)
        {
            if (items == null || items.Length == 0)
                return;
            ArrayList arrayList1 = new ArrayList();
            ArrayList arrayList2 = new ArrayList();
            foreach (ItemResult itemResult in items)
            {
                if (itemResult.ResultID.Succeeded())
                {
                    if (itemResult.SamplingRateSpecified)
                        arrayList1.Add((object)itemResult);
                    else
                        arrayList2.Add((object)itemResult);
                }
            }
            SetSamplingRates((ItemResult[])arrayList1.ToArray(typeof(ItemResult)));
            ClearSamplingRates((ItemResult[])arrayList2.ToArray(typeof(ItemResult)));
        }

        private void SetEnableBuffering(ItemResult[] items)
        {
            if (items == null || items.Length == 0)
                return;
            ArrayList arrayList = new ArrayList();
            ResultID resultId;
            foreach (ItemResult itemResult in items)
            {
                resultId = itemResult.ResultID;
                if (resultId.Succeeded())
                    arrayList.Add((object)itemResult);
            }
            if (arrayList.Count == 0)
                return;
            try
            {
                int[] phServer = new int[arrayList.Count];
                int[] pbEnable = new int[arrayList.Count];
                for (int index = 0; index < arrayList.Count; ++index)
                {
                    ItemResult itemResult = (ItemResult)arrayList[index];
                    phServer[index] = System.Convert.ToInt32(itemResult.ServerHandle);
                    pbEnable[index] = !itemResult.EnableBufferingSpecified || !itemResult.EnableBuffering ? 0 : 1;
                }
                IntPtr ppErrors = IntPtr.Zero;
                ((IOPCItemSamplingMgt)m_group).SetItemBufferEnable(phServer.Length, phServer, pbEnable, out ppErrors);
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, phServer.Length, true);
                for (int index = 0; index < int32s.Length; ++index)
                {
                    ItemResult itemResult = (ItemResult)arrayList[index];
                    resultId = OpcCom.Interop.GetResultID(int32s[index]);
                    if (resultId.Failed())
                    {
                        itemResult.EnableBuffering = false;
                        itemResult.EnableBufferingSpecified = true;
                    }
                }
            }
            catch
            {
                foreach (ItemResult itemResult in arrayList)
                {
                    itemResult.EnableBuffering = false;
                    itemResult.EnableBufferingSpecified = true;
                }
            }
        }

        private void Advise()
        {
            if (m_connection != null)
                return;
            m_connection = new ConnectionPoint(m_group, typeof(IOPCDataCallback).GUID);
            m_connection.Advise((object)m_callback);
        }

        private void Unadvise()
        {
            if (m_connection == null || m_connection.Unadvise() != 0)
                return;
            m_connection.Dispose();
            m_connection = (ConnectionPoint)null;
        }

        private sealed class ItemTable
        {
            private Hashtable m_items = new Hashtable();

            public ItemIdentifier this[object handle]
            {
                get => handle != null ? (ItemIdentifier)m_items[handle] : (ItemIdentifier)null;
                set
                {
                    if (handle == null)
                        return;
                    if (value == null)
                        m_items.Remove(handle);
                    else
                        m_items[handle] = (object)value;
                }
            }

            private int GetInvalidHandle()
            {
                int invalidHandle = 0;
                foreach (ItemIdentifier itemIdentifier in (IEnumerable)m_items.Values)
                {
                    if (itemIdentifier.ServerHandle != null && itemIdentifier.ServerHandle.GetType() == typeof(int) && invalidHandle < (int)itemIdentifier.ServerHandle)
                        invalidHandle = (int)itemIdentifier.ServerHandle + 1;
                }
                return invalidHandle;
            }

            public ItemIdentifier[] GetItemIDs(ItemIdentifier[] items)
            {
                int invalidHandle = GetInvalidHandle();
                ItemIdentifier[] itemIds = new ItemIdentifier[items.Length];
                for (int index = 0; index < items.Length; ++index)
                {
                    ItemIdentifier itemIdentifier = this[items[index].ServerHandle];
                    if (itemIdentifier != null)
                    {
                        itemIds[index] = (ItemIdentifier)itemIdentifier.Clone();
                    }
                    else
                    {
                        itemIds[index] = new ItemIdentifier();
                        itemIds[index].ServerHandle = (object)invalidHandle;
                    }
                    itemIds[index].ClientHandle = items[index].ServerHandle;
                }
                return itemIds;
            }

            public ItemResult[] CreateItems(Item[] items)
            {
                if (items == null)
                    return (ItemResult[])null;
                ItemResult[] items1 = new ItemResult[items.Length];
                for (int index = 0; index < items.Length; ++index)
                {
                    items1[index] = new ItemResult(items[index]);
                    ItemIdentifier itemIdentifier = this[items[index].ServerHandle];
                    if (itemIdentifier != null)
                    {
                        items1[index].ItemName = itemIdentifier.ItemName;
                        items1[index].ItemPath = itemIdentifier.ItemName;
                        items1[index].ServerHandle = itemIdentifier.ServerHandle;
                        itemIdentifier.ClientHandle = items[index].ClientHandle;
                    }
                    if (items1[index].ServerHandle == null)
                    {
                        items1[index].ResultID = ResultID.Da.E_INVALIDHANDLE;
                        items1[index].DiagnosticInfo = (string)null;
                    }
                    else
                        items1[index].ClientHandle = items[index].ServerHandle;
                }
                return items1;
            }

            public ItemIdentifier[] ApplyFilters(int filters, ItemIdentifier[] results)
            {
                if (results == null)
                    return (ItemIdentifier[])null;
                foreach (ItemIdentifier result in results)
                {
                    ItemIdentifier itemIdentifier = this[result.ClientHandle];
                    if (itemIdentifier != null)
                    {
                        result.ItemName = (filters & 1) != 0 ? itemIdentifier.ItemName : (string)null;
                        result.ItemPath = (filters & 2) != 0 ? itemIdentifier.ItemPath : (string)null;
                        result.ServerHandle = result.ClientHandle;
                        result.ClientHandle = (filters & 4) != 0 ? itemIdentifier.ClientHandle : (object)null;
                    }
                    if ((filters & 8) == 0 && result.GetType() == typeof(ItemValueResult))
                    {
                        ((ItemValue)result).Timestamp = DateTime.MinValue;
                        ((ItemValue)result).TimestampSpecified = false;
                    }
                }
                return results;
            }
        }

        private sealed class Callback : IOPCDataCallback
        {
            private object m_handle;
            private int m_filters = 9;
            private Subscription.ItemTable m_items;
            private Hashtable m_requests = new Hashtable();

            public Callback(object handle, int filters, Subscription.ItemTable items)
            {
                m_handle = handle;
                m_filters = filters;
                m_items = items;
            }

            public void SetFilters(object handle, int filters)
            {
                lock (this)
                {
                    m_handle = handle;
                    m_filters = filters;
                }
            }

            public void BeginRequest(Request request)
            {
                lock (this)
                    m_requests[(object)request.RequestID] = (object)request;
            }

            public bool CancelRequest(Request request)
            {
                lock (this)
                    return m_requests.ContainsKey((object)request.RequestID);
            }

            public void EndRequest(Request request)
            {
                lock (this)
                    m_requests.Remove((object)request.RequestID);
            }

            public event DataChangedEventHandler DataChanged
            {
                add
                {
                    lock (this)
                        m_dataChanged += value;
                }
                remove
                {
                    lock (this)
                        m_dataChanged -= value;
                }
            }

            private event DataChangedEventHandler m_dataChanged;

            public void OnDataChange(
              int dwTransid,
              int hGroup,
              int hrMasterquality,
              int hrMastererror,
              int dwCount,
              int[] phClientItems,
              object[] pvValues,
              short[] pwQualities,
              OpcRcw.Da.FILETIME[] pftTimeStamps,
              int[] pErrors)
            {
                try
                {
                    Request request = (Request)null;
                    lock (this)
                    {
                        if (dwTransid != 0)
                        {
                            request = (Request)m_requests[(object)dwTransid];
                            if (request != null)
                                m_requests.Remove((object)dwTransid);
                        }
                        if (m_dataChanged == null)
                            return;
                        ItemValueResult[] itemValueResultArray = UnmarshalValues(dwCount, phClientItems, pvValues, pwQualities, pftTimeStamps, pErrors);
                        lock (m_items)
                            m_items.ApplyFilters(m_filters | 4, (ItemIdentifier[])itemValueResultArray);
                        m_dataChanged(m_handle, request?.Handle, itemValueResultArray);
                    }
                }
                catch (Exception ex)
                {
                    string stackTrace = ex.StackTrace;
                }
            }

            public void OnReadComplete(
              int dwTransid,
              int hGroup,
              int hrMasterquality,
              int hrMastererror,
              int dwCount,
              int[] phClientItems,
              object[] pvValues,
              short[] pwQualities,
              OpcRcw.Da.FILETIME[] pftTimeStamps,
              int[] pErrors)
            {
                try
                {
                    Request request = (Request)null;
                    ItemValueResult[] results = (ItemValueResult[])null;
                    lock (this)
                    {
                        request = (Request)m_requests[(object)dwTransid];
                        if (request == null)
                            return;
                        m_requests.Remove((object)dwTransid);
                        results = UnmarshalValues(dwCount, phClientItems, pvValues, pwQualities, pftTimeStamps, pErrors);
                        lock (m_items)
                            m_items.ApplyFilters(m_filters | 4, (ItemIdentifier[])results);
                    }
                    lock (request)
                        request.EndRequest(results);
                }
                catch (Exception ex)
                {
                    string stackTrace = ex.StackTrace;
                }
            }

            public void OnWriteComplete(
              int dwTransid,
              int hGroup,
              int hrMastererror,
              int dwCount,
              int[] phClientItems,
              int[] pErrors)
            {
                try
                {
                    Request request = (Request)null;
                    IdentifiedResult[] identifiedResultArray = (IdentifiedResult[])null;
                    lock (this)
                    {
                        request = (Request)m_requests[(object)dwTransid];
                        if (request == null)
                            return;
                        m_requests.Remove((object)dwTransid);
                        identifiedResultArray = new IdentifiedResult[dwCount];
                        for (int index = 0; index < identifiedResultArray.Length; ++index)
                        {
                            ItemIdentifier itemIdentifier = m_items[(object)phClientItems[index]];
                            identifiedResultArray[index] = new IdentifiedResult(itemIdentifier);
                            identifiedResultArray[index].ClientHandle = (object)phClientItems[index];
                            identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(pErrors[index]);
                            identifiedResultArray[index].DiagnosticInfo = (string)null;
                            if (pErrors[index] == -1073479674)
                                identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
                        }
                        lock (m_items)
                            m_items.ApplyFilters(m_filters | 4, (ItemIdentifier[])identifiedResultArray);
                    }
                    lock (request)
                        request.EndRequest(identifiedResultArray);
                }
                catch (Exception ex)
                {
                    string stackTrace = ex.StackTrace;
                }
            }

            public void OnCancelComplete(int dwTransid, int hGroup)
            {
                try
                {
                    Request request = (Request)null;
                    lock (this)
                    {
                        request = (Request)m_requests[(object)dwTransid];
                        if (request == null)
                            return;
                        m_requests.Remove((object)dwTransid);
                    }
                    lock (request)
                        request.EndRequest();
                }
                catch (Exception ex)
                {
                    string stackTrace = ex.StackTrace;
                }
            }

            private ItemValueResult[] UnmarshalValues(
              int dwCount,
              int[] phClientItems,
              object[] pvValues,
              short[] pwQualities,
              OpcRcw.Da.FILETIME[] pftTimeStamps,
              int[] pErrors)
            {
                ItemValueResult[] itemValueResultArray = new ItemValueResult[dwCount];
                for (int index = 0; index < itemValueResultArray.Length; ++index)
                {
                    ItemIdentifier itemIdentifier = m_items[(object)phClientItems[index]];
                    itemValueResultArray[index] = new ItemValueResult(itemIdentifier);
                    itemValueResultArray[index].ClientHandle = (object)phClientItems[index];
                    itemValueResultArray[index].Value = pvValues[index];
                    itemValueResultArray[index].Quality = new Quality(pwQualities[index]);
                    itemValueResultArray[index].QualitySpecified = true;
                    itemValueResultArray[index].Timestamp = OpcCom.Interop.GetFILETIME(Interop.Convert(pftTimeStamps[index]));
                    itemValueResultArray[index].TimestampSpecified = itemValueResultArray[index].Timestamp != DateTime.MinValue;
                    itemValueResultArray[index].ResultID = OpcCom.Interop.GetResultID(pErrors[index]);
                    itemValueResultArray[index].DiagnosticInfo = (string)null;
                    if (pErrors[index] == -1073479674)
                        itemValueResultArray[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
                }
                return itemValueResultArray;
            }
        }
    }
}
