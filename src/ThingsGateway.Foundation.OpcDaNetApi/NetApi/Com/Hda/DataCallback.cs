

using Opc;
using Opc.Hda;

using OpcRcw.Hda;

using System;
using System.Collections;


namespace OpcCom.Hda
{
    internal sealed class DataCallback : IOPCHDA_DataCallback
    {
        private int m_nextID;
        private Hashtable m_requests = new Hashtable();
        private CallbackExceptionEventHandler m_callbackException;

        public event CallbackExceptionEventHandler CallbackException
        {
            add
            {
                lock (this)
                    m_callbackException += value;
            }
            remove
            {
                lock (this)
                    m_callbackException -= value;
            }
        }

        public Request CreateRequest(object requestHandle, Delegate callback)
        {
            lock (this)
            {
                Request request = new Request(requestHandle, callback, ++m_nextID);
                m_requests[(object)request.RequestID] = (object)request;
                return request;
            }
        }

        public bool CancelRequest(Request request, CancelCompleteEventHandler callback)
        {
            lock (this)
            {
                if (!m_requests.Contains((object)request.RequestID))
                    return false;
                if (callback != null)
                    request.CancelComplete += callback;
                else
                    m_requests.Remove((object)request.RequestID);
                return true;
            }
        }

        public void OnDataChange(
          int dwTransactionID,
          int hrStatus,
          int dwNumItems,
          OPCHDA_ITEM[] pItemValues,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ItemValueCollection[] results = new ItemValueCollection[pItemValues.Length];
                    for (int index = 0; index < pItemValues.Length; ++index)
                    {
                        results[index] = Interop.GetItemValueCollection(pItemValues[index], false);
                        results[index].ServerHandle = results[index].ClientHandle;
                        results[index].ClientHandle = (object)null;
                        results[index].ResultID = OpcCom.Interop.GetResultID(phrErrors[index]);
                    }
                    if (!request.InvokeCallback((object)results))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnReadComplete(
          int dwTransactionID,
          int hrStatus,
          int dwNumItems,
          OPCHDA_ITEM[] pItemValues,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ItemValueCollection[] results = new ItemValueCollection[pItemValues.Length];
                    for (int index = 0; index < pItemValues.Length; ++index)
                    {
                        results[index] = Interop.GetItemValueCollection(pItemValues[index], false);
                        results[index].ServerHandle = (object)pItemValues[index].hClient;
                        results[index].ResultID = OpcCom.Interop.GetResultID(phrErrors[index]);
                    }
                    if (!request.InvokeCallback((object)results))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnReadModifiedComplete(
          int dwTransactionID,
          int hrStatus,
          int dwNumItems,
          OPCHDA_MODIFIEDITEM[] pItemValues,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ModifiedValueCollection[] results = new ModifiedValueCollection[pItemValues.Length];
                    for (int index = 0; index < pItemValues.Length; ++index)
                    {
                        results[index] = Interop.GetModifiedValueCollection(pItemValues[index], false);
                        results[index].ServerHandle = (object)pItemValues[index].hClient;
                        results[index].ResultID = OpcCom.Interop.GetResultID(phrErrors[index]);
                    }
                    if (!request.InvokeCallback((object)results))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnReadAttributeComplete(
          int dwTransactionID,
          int hrStatus,
          int hClient,
          int dwNumItems,
          OPCHDA_ATTRIBUTE[] pAttributeValues,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ItemAttributeCollection results = new ItemAttributeCollection();
                    results.ServerHandle = (object)hClient;
                    AttributeValueCollection[] attributeValueCollectionArray = new AttributeValueCollection[pAttributeValues.Length];
                    for (int index = 0; index < pAttributeValues.Length; ++index)
                    {
                        attributeValueCollectionArray[index] = Interop.GetAttributeValueCollection(pAttributeValues[index], false);
                        attributeValueCollectionArray[index].ResultID = OpcCom.Interop.GetResultID(phrErrors[index]);
                        results.Add(attributeValueCollectionArray[index]);
                    }
                    if (!request.InvokeCallback((object)results))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnReadAnnotations(
          int dwTransactionID,
          int hrStatus,
          int dwNumItems,
          OPCHDA_ANNOTATION[] pAnnotationValues,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    AnnotationValueCollection[] results = new AnnotationValueCollection[pAnnotationValues.Length];
                    for (int index = 0; index < pAnnotationValues.Length; ++index)
                    {
                        results[index] = Interop.GetAnnotationValueCollection(pAnnotationValues[index], false);
                        results[index].ServerHandle = (object)pAnnotationValues[index].hClient;
                        results[index].ResultID = OpcCom.Interop.GetResultID(phrErrors[index]);
                    }
                    if (!request.InvokeCallback((object)results))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnInsertAnnotations(
          int dwTransactionID,
          int hrStatus,
          int dwCount,
          int[] phClients,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ArrayList arrayList = new ArrayList();
                    if (dwCount > 0)
                    {
                        int phClient = phClients[0];
                        ResultCollection resultCollection = new ResultCollection();
                        for (int index = 0; index < dwCount; ++index)
                        {
                            if (phClients[index] != phClient)
                            {
                                resultCollection.ServerHandle = (object)phClient;
                                arrayList.Add((object)resultCollection);
                                phClient = phClients[index];
                                resultCollection = new ResultCollection();
                            }
                            Result result = new Result(OpcCom.Interop.GetResultID(phrErrors[index]));
                            resultCollection.Add(result);
                        }
                        resultCollection.ServerHandle = (object)phClient;
                        arrayList.Add((object)resultCollection);
                    }
                    if (!request.InvokeCallback((object)(ResultCollection[])arrayList.ToArray(typeof(ResultCollection))))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnPlayback(
          int dwTransactionID,
          int hrStatus,
          int dwNumItems,
          IntPtr ppItemValues,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ItemValueCollection[] results = new ItemValueCollection[dwNumItems];
                    int[] int32s = OpcCom.Interop.GetInt32s(ref ppItemValues, dwNumItems, false);
                    for (int index = 0; index < dwNumItems; ++index)
                    {
                        IntPtr pInput = (nint)int32s[index];
                        ItemValueCollection[] valueCollections = Interop.GetItemValueCollections(ref pInput, 1, false);
                        if (valueCollections != null && valueCollections.Length == 1)
                        {
                            results[index] = valueCollections[0];
                            results[index].ServerHandle = results[index].ClientHandle;
                            results[index].ClientHandle = (object)null;
                            results[index].ResultID = OpcCom.Interop.GetResultID(phrErrors[index]);
                        }
                    }
                    if (!request.InvokeCallback((object)results))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnUpdateComplete(
          int dwTransactionID,
          int hrStatus,
          int dwCount,
          int[] phClients,
          int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwTransactionID];
                    if (request == null)
                        return;
                    ArrayList arrayList = new ArrayList();
                    if (dwCount > 0)
                    {
                        int phClient = phClients[0];
                        ResultCollection resultCollection = new ResultCollection();
                        for (int index = 0; index < dwCount; ++index)
                        {
                            if (phClients[index] != phClient)
                            {
                                resultCollection.ServerHandle = (object)phClient;
                                arrayList.Add((object)resultCollection);
                                phClient = phClients[index];
                                resultCollection = new ResultCollection();
                            }
                            Result result = new Result(OpcCom.Interop.GetResultID(phrErrors[index]));
                            resultCollection.Add(result);
                        }
                        resultCollection.ServerHandle = (object)phClient;
                        arrayList.Add((object)resultCollection);
                    }
                    if (!request.InvokeCallback((object)(ResultCollection[])arrayList.ToArray(typeof(ResultCollection))))
                        return;
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwTransactionID, ex);
            }
        }

        public void OnCancelComplete(int dwCancelID)
        {
            try
            {
                lock (this)
                {
                    Request request = (Request)m_requests[(object)dwCancelID];
                    if (request == null)
                        return;
                    request.OnCancelComplete();
                    m_requests.Remove((object)request.RequestID);
                }
            }
            catch (Exception ex)
            {
                HandleException(dwCancelID, ex);
            }
        }

        private void HandleException(int requestID, Exception exception)
        {
            lock (this)
            {
                Request request = (Request)m_requests[(object)requestID];
                if (request == null || m_callbackException == null)
                    return;
                m_callbackException((IRequest)request, exception);
            }
        }
    }
}
