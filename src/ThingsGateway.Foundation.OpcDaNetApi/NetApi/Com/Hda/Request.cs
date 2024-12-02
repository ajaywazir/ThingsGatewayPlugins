

using Opc;
using Opc.Hda;

using System;
using System.Collections;


namespace OpcCom.Hda
{
    internal sealed class Request : IRequest, IActualTime
    {
        private object m_requestHandle;
        private Delegate m_callback;
        private int m_requestID;
        private int m_cancelID;
        private DateTime m_startTime = DateTime.MinValue;
        private DateTime m_endTime = DateTime.MinValue;
        private Hashtable m_items;
        private ArrayList m_results;

        public int RequestID => m_requestID;

        public int CancelID => m_cancelID;

        public event CancelCompleteEventHandler CancelComplete
        {
            add
            {
                lock (this)
                    m_cancelComplete += value;
            }
            remove
            {
                lock (this)
                    m_cancelComplete -= value;
            }
        }

        public Request(object requestHandle, Delegate callback, int requestID)
        {
            m_requestHandle = requestHandle;
            m_callback = callback;
            m_requestID = requestID;
        }

        public bool Update(int cancelID, ItemIdentifier[] results)
        {
            lock (this)
            {
                m_cancelID = cancelID;
                m_items = new Hashtable();
                foreach (ItemIdentifier result in results)
                {
                    if (!typeof(IResult).IsInstanceOfType((object)result) || ((IResult)result).ResultID.Succeeded())
                        m_items[result.ServerHandle] = (object)new ItemIdentifier(result);
                }
                if (m_items.Count == 0)
                    return true;
                bool flag = false;
                if (m_results != null)
                {
                    foreach (object result in m_results)
                        flag = InvokeCallback(result);
                }
                return flag;
            }
        }

        public bool InvokeCallback(object results)
        {
            lock (this)
            {
                if (m_items == null)
                {
                    if (m_results == null)
                        m_results = new ArrayList();
                    m_results.Add(results);
                    return false;
                }
                if (typeof(DataUpdateEventHandler).IsInstanceOfType((object)m_callback))
                    return InvokeCallback((DataUpdateEventHandler)m_callback, results);
                if (typeof(ReadValuesEventHandler).IsInstanceOfType((object)m_callback))
                    return InvokeCallback((ReadValuesEventHandler)m_callback, results);
                if (typeof(ReadAttributesEventHandler).IsInstanceOfType((object)m_callback))
                    return InvokeCallback((ReadAttributesEventHandler)m_callback, results);
                if (typeof(ReadAnnotationsEventHandler).IsInstanceOfType((object)m_callback))
                    return InvokeCallback((ReadAnnotationsEventHandler)m_callback, results);
                return !typeof(UpdateCompleteEventHandler).IsInstanceOfType((object)m_callback) || InvokeCallback((UpdateCompleteEventHandler)m_callback, results);
            }
        }

        public void OnCancelComplete()
        {
            lock (this)
            {
                if (m_cancelComplete == null)
                    return;
                m_cancelComplete((IRequest)this);
            }
        }

        public object Handle => m_requestHandle;

        public DateTime StartTime
        {
            get => m_startTime;
            set => m_startTime = value;
        }

        public DateTime EndTime
        {
            get => m_endTime;
            set => m_endTime = value;
        }

        private bool InvokeCallback(DataUpdateEventHandler callback, object results)
        {
            if (!typeof(ItemValueCollection[]).IsInstanceOfType(results))
                return false;
            ItemValueCollection[] results1 = (ItemValueCollection[])results;
            UpdateResults((ItemIdentifier[])results1);
            try
            {
                callback((IRequest)this, results1);
            }
            catch
            {
            }
            return false;
        }

        private bool InvokeCallback(ReadValuesEventHandler callback, object results)
        {
            if (!typeof(ItemValueCollection[]).IsInstanceOfType(results))
                return false;
            ItemValueCollection[] results1 = (ItemValueCollection[])results;
            UpdateResults((ItemIdentifier[])results1);
            try
            {
                callback((IRequest)this, results1);
            }
            catch
            {
            }
            foreach (ItemValueCollection itemValueCollection in results1)
            {
                if (itemValueCollection.ResultID == ResultID.Hda.S_MOREDATA)
                    return false;
            }
            return true;
        }

        private bool InvokeCallback(ReadAttributesEventHandler callback, object results)
        {
            if (!typeof(ItemAttributeCollection).IsInstanceOfType(results))
                return false;
            ItemAttributeCollection results1 = (ItemAttributeCollection)results;
            UpdateResults((ItemIdentifier[])new ItemAttributeCollection[1]
            {
        results1
            });
            try
            {
                callback((IRequest)this, results1);
            }
            catch
            {
            }
            return true;
        }

        private bool InvokeCallback(ReadAnnotationsEventHandler callback, object results)
        {
            if (!typeof(AnnotationValueCollection[]).IsInstanceOfType(results))
                return false;
            AnnotationValueCollection[] results1 = (AnnotationValueCollection[])results;
            UpdateResults((ItemIdentifier[])results1);
            try
            {
                callback((IRequest)this, results1);
            }
            catch
            {
            }
            return true;
        }

        private bool InvokeCallback(UpdateCompleteEventHandler callback, object results)
        {
            if (!typeof(ResultCollection[]).IsInstanceOfType(results))
                return false;
            ResultCollection[] results1 = (ResultCollection[])results;
            UpdateResults((ItemIdentifier[])results1);
            try
            {
                callback((IRequest)this, results1);
            }
            catch
            {
            }
            return true;
        }

        private void UpdateResults(ItemIdentifier[] results)
        {
            foreach (ItemIdentifier result in results)
            {
                if (typeof(IActualTime).IsInstanceOfType((object)result))
                {
                    ((IActualTime)result).StartTime = StartTime;
                    ((IActualTime)result).EndTime = EndTime;
                }
                ItemIdentifier itemIdentifier = (ItemIdentifier)m_items[result.ServerHandle];
                if (itemIdentifier != null)
                {
                    result.ItemName = itemIdentifier.ItemName;
                    result.ItemPath = itemIdentifier.ItemPath;
                    result.ServerHandle = itemIdentifier.ServerHandle;
                    result.ClientHandle = itemIdentifier.ClientHandle;
                }
            }
        }

        private event CancelCompleteEventHandler m_cancelComplete;
    }
}
