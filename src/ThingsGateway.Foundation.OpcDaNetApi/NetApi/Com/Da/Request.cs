

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
      this.Filters = filters;
      this.RequestID = requestID;
      this.Callback = callback;
      this.CancelID = 0;
      this.InitialResults = (ItemIdentifier[]) null;
    }

    public bool BeginRead(int cancelID, IdentifiedResult[] results)
    {
      this.CancelID = cancelID;
      if (this.InitialResults != null && this.InitialResults.GetType() == typeof (ItemValueResult[]))
      {
        ItemValueResult[] initialResults = (ItemValueResult[]) this.InitialResults;
        this.InitialResults = (ItemIdentifier[]) results;
        this.EndRequest(initialResults);
        return true;
      }
      foreach (IdentifiedResult result in results)
      {
        if (result.ResultID.Succeeded())
        {
          this.InitialResults = (ItemIdentifier[]) results;
          return false;
        }
      }
      return true;
    }

    public bool BeginWrite(int cancelID, IdentifiedResult[] results)
    {
      this.CancelID = cancelID;
      if (this.InitialResults != null && this.InitialResults.GetType() == typeof (IdentifiedResult[]))
      {
        IdentifiedResult[] initialResults = (IdentifiedResult[]) this.InitialResults;
        this.InitialResults = (ItemIdentifier[]) results;
        this.EndRequest(initialResults);
        return true;
      }
      foreach (IdentifiedResult result in results)
      {
        if (result.ResultID.Succeeded())
        {
          this.InitialResults = (ItemIdentifier[]) results;
          return false;
        }
      }
      for (int index = 0; index < results.Length; ++index)
      {
        if ((this.Filters & 1) == 0)
          results[index].ItemName = (string) null;
        if ((this.Filters & 2) == 0)
          results[index].ItemPath = (string) null;
        if ((this.Filters & 4) == 0)
          results[index].ClientHandle = (object) null;
      }
      ((WriteCompleteEventHandler) this.Callback)(this.Handle, results);
      return true;
    }

    public bool BeginRefresh(int cancelID)
    {
      this.CancelID = cancelID;
      return false;
    }

    public void EndRequest()
    {
      if (!typeof (CancelCompleteEventHandler).IsInstanceOfType((object) this.Callback))
        return;
      ((CancelCompleteEventHandler) this.Callback)(this.Handle);
    }

    public void EndRequest(ItemValueResult[] results)
    {
      if (this.InitialResults == null)
        this.InitialResults = (ItemIdentifier[]) results;
      else if (typeof (CancelCompleteEventHandler).IsInstanceOfType((object) this.Callback))
      {
        ((CancelCompleteEventHandler) this.Callback)(this.Handle);
      }
      else
      {
        for (int index = 0; index < results.Length; ++index)
        {
          if ((this.Filters & 1) == 0)
            results[index].ItemName = (string) null;
          if ((this.Filters & 2) == 0)
            results[index].ItemPath = (string) null;
          if ((this.Filters & 4) == 0)
            results[index].ClientHandle = (object) null;
          if ((this.Filters & 8) == 0)
          {
            results[index].Timestamp = DateTime.MinValue;
            results[index].TimestampSpecified = false;
          }
        }
        if (!typeof (ReadCompleteEventHandler).IsInstanceOfType((object) this.Callback))
          return;
        ((ReadCompleteEventHandler) this.Callback)(this.Handle, results);
      }
    }

    public void EndRequest(IdentifiedResult[] callbackResults)
    {
      if (this.InitialResults == null)
        this.InitialResults = (ItemIdentifier[]) callbackResults;
      else if ((object) this.Callback != null && this.Callback.GetType() == typeof (CancelCompleteEventHandler))
      {
        ((CancelCompleteEventHandler) this.Callback)(this.Handle);
      }
      else
      {
        IdentifiedResult[] initialResults = (IdentifiedResult[]) this.InitialResults;
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
          if ((this.Filters & 1) == 0)
            initialResults[index3].ItemName = (string) null;
          if ((this.Filters & 2) == 0)
            initialResults[index3].ItemPath = (string) null;
          if ((this.Filters & 4) == 0)
            initialResults[index3].ClientHandle = (object) null;
        }
        if ((object) this.Callback == null || !(this.Callback.GetType() == typeof (WriteCompleteEventHandler)))
          return;
        ((WriteCompleteEventHandler) this.Callback)(this.Handle, initialResults);
      }
    }
  }
}
