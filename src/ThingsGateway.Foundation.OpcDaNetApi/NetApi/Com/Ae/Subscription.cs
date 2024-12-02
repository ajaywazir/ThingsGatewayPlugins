

using Opc;
using Opc.Ae;

using OpcRcw.Ae;

using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Ae
{
    [Serializable]
    public class Subscription : ISubscription, IDisposable
    {
        private bool m_disposed;
        private object m_subscription;
        private object m_clientHandle;
        private bool m_supportsAE11 = true;
        private ConnectionPoint m_connection;
        private Subscription.Callback m_callback;

        internal Subscription(SubscriptionState state, object subscription)
        {
            m_subscription = subscription;
            m_clientHandle = Opc.Convert.Clone(state.ClientHandle);
            m_supportsAE11 = true;
            m_callback = new Subscription.Callback(state.ClientHandle);
            try
            {
                IOPCEventSubscriptionMgt2 subscription1 = (IOPCEventSubscriptionMgt2)m_subscription;
            }
            catch
            {
                m_supportsAE11 = false;
            }
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
                if (disposing && m_connection != null)
                {
                    m_connection.Dispose();
                    m_connection = (ConnectionPoint)null;
                }
                if (m_subscription != null)
                {
                    OpcCom.Interop.ReleaseServer(m_subscription);
                    m_subscription = (object)null;
                }
            }
            m_disposed = true;
        }

        public event EventChangedEventHandler EventChanged
        {
            add
            {
                lock (this)
                {
                    Advise();
                    m_callback.EventChanged += value;
                }
            }
            remove
            {
                lock (this)
                {
                    m_callback.EventChanged -= value;
                    Unadvise();
                }
            }
        }

        public SubscriptionState GetState()
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                int pbActive = 0;
                int pdwBufferTime = 0;
                int pdwMaxSize = 0;
                int phClientSubscription = 0;
                int pdwKeepAliveTime = 0;
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).GetState(out pbActive, out pdwBufferTime, out pdwMaxSize, out phClientSubscription);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.GetState", ex);
                }
                if (m_supportsAE11)
                {
                    try
                    {
                        ((IOPCEventSubscriptionMgt2)m_subscription).GetKeepAlive(out pdwKeepAliveTime);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt2.GetKeepAlive", ex);
                    }
                }
                return new SubscriptionState()
                {
                    Active = pbActive != 0,
                    ClientHandle = m_clientHandle,
                    BufferTime = pdwBufferTime,
                    MaxSize = pdwMaxSize,
                    KeepAlive = pdwKeepAliveTime
                };
            }
        }

        public SubscriptionState ModifyState(int masks, SubscriptionState state)
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                GCHandle gcHandle1 = GCHandle.Alloc((object)(state.Active ? 1 : 0), GCHandleType.Pinned);
                GCHandle gcHandle2 = GCHandle.Alloc((object)state.BufferTime, GCHandleType.Pinned);
                GCHandle gcHandle3 = GCHandle.Alloc((object)state.MaxSize, GCHandleType.Pinned);
                IntPtr pbActive = (masks & 4) != 0 ? gcHandle1.AddrOfPinnedObject() : IntPtr.Zero;
                IntPtr pdwBufferTime = (masks & 8) != 0 ? gcHandle2.AddrOfPinnedObject() : IntPtr.Zero;
                IntPtr pdwMaxSize = (masks & 16) != 0 ? gcHandle3.AddrOfPinnedObject() : IntPtr.Zero;
                int hClientSubscription = 0;
                int pdwRevisedBufferTime = 0;
                int pdwRevisedMaxSize = 0;
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).SetState(pbActive, pdwBufferTime, pdwMaxSize, hClientSubscription, out pdwRevisedBufferTime, out pdwRevisedMaxSize);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.SetState", ex);
                }
                finally
                {
                    if (gcHandle1.IsAllocated)
                        gcHandle1.Free();
                    if (gcHandle2.IsAllocated)
                        gcHandle2.Free();
                    if (gcHandle3.IsAllocated)
                        gcHandle3.Free();
                }
                if ((masks & 32) != 0 && m_supportsAE11)
                {
                    int pdwRevisedKeepAliveTime = 0;
                    try
                    {
                        ((IOPCEventSubscriptionMgt2)m_subscription).SetKeepAlive(state.KeepAlive, out pdwRevisedKeepAliveTime);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt2.SetKeepAlive", ex);
                    }
                }
                return GetState();
            }
        }

        public SubscriptionFilters GetFilters()
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                int pdwEventType = 0;
                int pdwNumCategories = 0;
                IntPtr ppdwEventCategories = IntPtr.Zero;
                int pdwLowSeverity = 0;
                int pdwHighSeverity = 0;
                int pdwNumAreas = 0;
                IntPtr ppszAreaList = IntPtr.Zero;
                int pdwNumSources = 0;
                IntPtr ppszSourceList = IntPtr.Zero;
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).GetFilter(out pdwEventType, out pdwNumCategories, out ppdwEventCategories, out pdwLowSeverity, out pdwHighSeverity, out pdwNumAreas, out ppszAreaList, out pdwNumSources, out ppszSourceList);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.GetFilter", ex);
                }
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppdwEventCategories, pdwNumCategories, true);
                string[] unicodeStrings1 = OpcCom.Interop.GetUnicodeStrings(ref ppszAreaList, pdwNumAreas, true);
                string[] unicodeStrings2 = OpcCom.Interop.GetUnicodeStrings(ref ppszSourceList, pdwNumSources, true);
                SubscriptionFilters filters = new SubscriptionFilters();
                filters.EventTypes = pdwEventType;
                filters.LowSeverity = pdwLowSeverity;
                filters.HighSeverity = pdwHighSeverity;
                filters.Categories.AddRange((ICollection)int32s);
                filters.Areas.AddRange((ICollection)unicodeStrings1);
                filters.Sources.AddRange((ICollection)unicodeStrings2);
                return filters;
            }
        }

        public void SetFilters(SubscriptionFilters filters)
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).SetFilter(filters.EventTypes, filters.Categories.Count, filters.Categories.ToArray(), filters.LowSeverity, filters.HighSeverity, filters.Areas.Count, filters.Areas.ToArray(), filters.Sources.Count, filters.Sources.ToArray());
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.SetFilter", ex);
                }
            }
        }

        public int[] GetReturnedAttributes(int eventCategory)
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                int pdwCount = 0;
                IntPtr ppdwAttributeIDs = IntPtr.Zero;
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).GetReturnedAttributes(eventCategory, out pdwCount, out ppdwAttributeIDs);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.GetReturnedAttributes", ex);
                }
                return OpcCom.Interop.GetInt32s(ref ppdwAttributeIDs, pdwCount, true);
            }
        }

        public void SelectReturnedAttributes(int eventCategory, int[] attributeIDs)
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).SelectReturnedAttributes(eventCategory, attributeIDs != null ? attributeIDs.Length : 0, attributeIDs != null ? attributeIDs : Array.Empty<int>());
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.SelectReturnedAttributes", ex);
                }
            }
        }

        public void Refresh()
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).Refresh(m_connection.Cookie);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.Refresh", ex);
                }
            }
        }

        public void CancelRefresh()
        {
            lock (this)
            {
                if (m_subscription == null)
                    throw new NotConnectedException();
                try
                {
                    ((IOPCEventSubscriptionMgt)m_subscription).CancelRefresh(m_connection.Cookie);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventSubscriptionMgt.Refresh", ex);
                }
            }
        }

        private void Advise()
        {
            if (m_connection != null)
                return;
            m_connection = new ConnectionPoint(m_subscription, typeof(IOPCEventSink).GUID);
            m_connection.Advise((object)m_callback);
        }

        private void Unadvise()
        {
            if (m_connection == null || m_connection.Unadvise() != 0)
                return;
            m_connection.Dispose();
            m_connection = (ConnectionPoint)null;
        }

        private sealed class Callback : IOPCEventSink
        {
            private object m_clientHandle;

            public Callback(object clientHandle) => m_clientHandle = clientHandle;

            public event EventChangedEventHandler EventChanged
            {
                add
                {
                    lock (this)
                        m_EventChanged += value;
                }
                remove
                {
                    lock (this)
                        m_EventChanged -= value;
                }
            }

            public void OnEvent(
              int hClientSubscription,
              int bRefresh,
              int bLastRefresh,
              int dwCount,
              ONEVENTSTRUCT[] pEvents)
            {
                try
                {
                    lock (this)
                    {
                        if (m_EventChanged == null)
                            return;
                        EventNotification[] eventNotifications = Interop.GetEventNotifications(pEvents);
                        for (int index = 0; index < eventNotifications.Length; ++index)
                            eventNotifications[index].ClientHandle = m_clientHandle;
                        m_EventChanged(eventNotifications, bRefresh != 0, bLastRefresh != 0);
                    }
                }
                catch (Exception ex)
                {
                    string stackTrace = ex.StackTrace;
                }
            }

            private event EventChangedEventHandler m_EventChanged;
        }
    }
}
