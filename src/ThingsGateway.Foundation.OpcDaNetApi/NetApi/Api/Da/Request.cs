

using System;


namespace Opc.Da
{
  [Serializable]
  public class Request : IRequest
  {
    private ISubscription m_subscription;
    private object m_handle;

    public ISubscription Subscription => this.m_subscription;

    public object Handle => this.m_handle;

    public void Cancel(CancelCompleteEventHandler callback)
    {
      this.m_subscription.Cancel((IRequest) this, callback);
    }

    public Request(ISubscription subscription, object handle)
    {
      this.m_subscription = subscription;
      this.m_handle = handle;
    }
  }
}
