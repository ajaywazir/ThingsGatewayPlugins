

using Opc;
using Opc.Hda;
using System;
using System.Collections;


namespace OpcCom.Hda
{
  internal class Request : IRequest, IActualTime
  {
    private object m_requestHandle;
    private Delegate m_callback;
    private int m_requestID;
    private int m_cancelID;
    private DateTime m_startTime = DateTime.MinValue;
    private DateTime m_endTime = DateTime.MinValue;
    private Hashtable m_items;
    private ArrayList m_results;

    public int RequestID => this.m_requestID;

    public int CancelID => this.m_cancelID;

    public event CancelCompleteEventHandler CancelComplete
    {
      add
      {
        lock (this)
          this.m_cancelComplete += value;
      }
      remove
      {
        lock (this)
          this.m_cancelComplete -= value;
      }
    }

    public Request(object requestHandle, Delegate callback, int requestID)
    {
      this.m_requestHandle = requestHandle;
      this.m_callback = callback;
      this.m_requestID = requestID;
    }

    public bool Update(int cancelID, ItemIdentifier[] results)
    {
      lock (this)
      {
        this.m_cancelID = cancelID;
        this.m_items = new Hashtable();
        foreach (ItemIdentifier result in results)
        {
          if (!typeof (IResult).IsInstanceOfType((object) result) || ((IResult) result).ResultID.Succeeded())
            this.m_items[result.ServerHandle] = (object) new ItemIdentifier(result);
        }
        if (this.m_items.Count == 0)
          return true;
        bool flag = false;
        if (this.m_results != null)
        {
          foreach (object result in this.m_results)
            flag = this.InvokeCallback(result);
        }
        return flag;
      }
    }

    public bool InvokeCallback(object results)
    {
      lock (this)
      {
        if (this.m_items == null)
        {
          if (this.m_results == null)
            this.m_results = new ArrayList();
          this.m_results.Add(results);
          return false;
        }
        if (typeof (DataUpdateEventHandler).IsInstanceOfType((object) this.m_callback))
          return this.InvokeCallback((DataUpdateEventHandler) this.m_callback, results);
        if (typeof (ReadValuesEventHandler).IsInstanceOfType((object) this.m_callback))
          return this.InvokeCallback((ReadValuesEventHandler) this.m_callback, results);
        if (typeof (ReadAttributesEventHandler).IsInstanceOfType((object) this.m_callback))
          return this.InvokeCallback((ReadAttributesEventHandler) this.m_callback, results);
        if (typeof (ReadAnnotationsEventHandler).IsInstanceOfType((object) this.m_callback))
          return this.InvokeCallback((ReadAnnotationsEventHandler) this.m_callback, results);
        return !typeof (UpdateCompleteEventHandler).IsInstanceOfType((object) this.m_callback) || this.InvokeCallback((UpdateCompleteEventHandler) this.m_callback, results);
      }
    }

    public void OnCancelComplete()
    {
      lock (this)
      {
        if (this.m_cancelComplete == null)
          return;
        this.m_cancelComplete((IRequest) this);
      }
    }

    public object Handle => this.m_requestHandle;

    public DateTime StartTime
    {
      get => this.m_startTime;
      set => this.m_startTime = value;
    }

    public DateTime EndTime
    {
      get => this.m_endTime;
      set => this.m_endTime = value;
    }

    private bool InvokeCallback(DataUpdateEventHandler callback, object results)
    {
      if (!typeof (ItemValueCollection[]).IsInstanceOfType(results))
        return false;
      ItemValueCollection[] results1 = (ItemValueCollection[]) results;
      this.UpdateResults((ItemIdentifier[]) results1);
      try
      {
        callback((IRequest) this, results1);
      }
      catch
      {
      }
      return false;
    }

    private bool InvokeCallback(ReadValuesEventHandler callback, object results)
    {
      if (!typeof (ItemValueCollection[]).IsInstanceOfType(results))
        return false;
      ItemValueCollection[] results1 = (ItemValueCollection[]) results;
      this.UpdateResults((ItemIdentifier[]) results1);
      try
      {
        callback((IRequest) this, results1);
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
      if (!typeof (ItemAttributeCollection).IsInstanceOfType(results))
        return false;
      ItemAttributeCollection results1 = (ItemAttributeCollection) results;
      this.UpdateResults((ItemIdentifier[]) new ItemAttributeCollection[1]
      {
        results1
      });
      try
      {
        callback((IRequest) this, results1);
      }
      catch
      {
      }
      return true;
    }

    private bool InvokeCallback(ReadAnnotationsEventHandler callback, object results)
    {
      if (!typeof (AnnotationValueCollection[]).IsInstanceOfType(results))
        return false;
      AnnotationValueCollection[] results1 = (AnnotationValueCollection[]) results;
      this.UpdateResults((ItemIdentifier[]) results1);
      try
      {
        callback((IRequest) this, results1);
      }
      catch
      {
      }
      return true;
    }

    private bool InvokeCallback(UpdateCompleteEventHandler callback, object results)
    {
      if (!typeof (ResultCollection[]).IsInstanceOfType(results))
        return false;
      ResultCollection[] results1 = (ResultCollection[]) results;
      this.UpdateResults((ItemIdentifier[]) results1);
      try
      {
        callback((IRequest) this, results1);
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
        if (typeof (IActualTime).IsInstanceOfType((object) result))
        {
          ((IActualTime) result).StartTime = this.StartTime;
          ((IActualTime) result).EndTime = this.EndTime;
        }
        ItemIdentifier itemIdentifier = (ItemIdentifier) this.m_items[result.ServerHandle];
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
