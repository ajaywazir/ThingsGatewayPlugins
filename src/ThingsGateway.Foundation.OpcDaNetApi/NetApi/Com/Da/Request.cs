

using Opc;
using Opc.Da;

using System;


namespace OpcCom.Da
{
    [Serializable]
    public class Request : Opc.Da.Request
    {
        internal int RequestID;
        internal int CancelID;
        internal Delegate Callback;
        internal int Filters;
        internal ItemIdentifier[] InitialResults;

        public Request(
          ISubscription subscription,
          object clientHandle,
          int filters,
          int requestID,
          Delegate callback)
          : base(subscription, clientHandle)
        {
            Filters = filters;
            RequestID = requestID;
            Callback = callback;
            CancelID = 0;
            InitialResults = (ItemIdentifier[])null;
        }

        public bool BeginRead(int cancelID, IdentifiedResult[] results)
        {
            CancelID = cancelID;
            if (InitialResults != null && InitialResults.GetType() == typeof(ItemValueResult[]))
            {
                ItemValueResult[] initialResults = (ItemValueResult[])InitialResults;
                InitialResults = (ItemIdentifier[])results;
                EndRequest(initialResults);
                return true;
            }
            foreach (IdentifiedResult result in results)
            {
                if (result.ResultID.Succeeded())
                {
                    InitialResults = (ItemIdentifier[])results;
                    return false;
                }
            }
            return true;
        }

        public bool BeginWrite(int cancelID, IdentifiedResult[] results)
        {
            CancelID = cancelID;
            if (InitialResults != null && InitialResults.GetType() == typeof(IdentifiedResult[]))
            {
                IdentifiedResult[] initialResults = (IdentifiedResult[])InitialResults;
                InitialResults = (ItemIdentifier[])results;
                EndRequest(initialResults);
                return true;
            }
            foreach (IdentifiedResult result in results)
            {
                if (result.ResultID.Succeeded())
                {
                    InitialResults = (ItemIdentifier[])results;
                    return false;
                }
            }
            for (int index = 0; index < results.Length; ++index)
            {
                if ((Filters & 1) == 0)
                    results[index].ItemName = (string)null;
                if ((Filters & 2) == 0)
                    results[index].ItemPath = (string)null;
                if ((Filters & 4) == 0)
                    results[index].ClientHandle = (object)null;
            }
          ((WriteCompleteEventHandler)Callback)(Handle, results);
            return true;
        }

        public bool BeginRefresh(int cancelID)
        {
            CancelID = cancelID;
            return false;
        }

        public void EndRequest()
        {
            if (!typeof(CancelCompleteEventHandler).IsInstanceOfType((object)Callback))
                return;
            ((CancelCompleteEventHandler)Callback)(Handle);
        }

        public void EndRequest(ItemValueResult[] results)
        {
            if (InitialResults == null)
                InitialResults = (ItemIdentifier[])results;
            else if (typeof(CancelCompleteEventHandler).IsInstanceOfType((object)Callback))
            {
                ((CancelCompleteEventHandler)Callback)(Handle);
            }
            else
            {
                for (int index = 0; index < results.Length; ++index)
                {
                    if ((Filters & 1) == 0)
                        results[index].ItemName = (string)null;
                    if ((Filters & 2) == 0)
                        results[index].ItemPath = (string)null;
                    if ((Filters & 4) == 0)
                        results[index].ClientHandle = (object)null;
                    if ((Filters & 8) == 0)
                    {
                        results[index].Timestamp = DateTime.MinValue;
                        results[index].TimestampSpecified = false;
                    }
                }
                if (!typeof(ReadCompleteEventHandler).IsInstanceOfType((object)Callback))
                    return;
                ((ReadCompleteEventHandler)Callback)(Handle, results);
            }
        }

        public void EndRequest(IdentifiedResult[] callbackResults)
        {
            if (InitialResults == null)
                InitialResults = (ItemIdentifier[])callbackResults;
            else if ((object)Callback != null && Callback.GetType() == typeof(CancelCompleteEventHandler))
            {
                ((CancelCompleteEventHandler)Callback)(Handle);
            }
            else
            {
                IdentifiedResult[] initialResults = (IdentifiedResult[])InitialResults;
                int index1 = 0;
                for (int index2 = 0; index2 < initialResults.Length; ++index2)
                {
                    for (; index1 < callbackResults.Length; ++index1)
                    {
                        if (callbackResults[index2].ServerHandle.Equals(initialResults[index1].ServerHandle))
                        {
                            initialResults[index1++] = callbackResults[index2];
                            break;
                        }
                    }
                }
                for (int index3 = 0; index3 < initialResults.Length; ++index3)
                {
                    if ((Filters & 1) == 0)
                        initialResults[index3].ItemName = (string)null;
                    if ((Filters & 2) == 0)
                        initialResults[index3].ItemPath = (string)null;
                    if ((Filters & 4) == 0)
                        initialResults[index3].ClientHandle = (object)null;
                }
                if ((object)Callback == null || !(Callback.GetType() == typeof(WriteCompleteEventHandler)))
                    return;
                ((WriteCompleteEventHandler)Callback)(Handle, initialResults);
            }
        }
    }
}
