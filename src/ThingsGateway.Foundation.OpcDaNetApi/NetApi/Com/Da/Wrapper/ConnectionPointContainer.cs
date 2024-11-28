

using OpcRcw.Comn;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
  public class ConnectionPointContainer : IConnectionPointContainer
  {
    private Hashtable m_connectionPoints = new Hashtable();

    public virtual void OnAdvise(Guid riid)
    {
    }

    public virtual void OnUnadvise(Guid riid)
    {
    }

    protected ConnectionPointContainer()
    {
    }

    protected void RegisterInterface(Guid iid)
    {
      this.m_connectionPoints[(object) iid] = (object) new ConnectionPoint(iid, this);
    }

    protected void UnregisterInterface(Guid iid) => this.m_connectionPoints.Remove((object) iid);

    protected object GetCallback(Guid iid)
    {
      return ((ConnectionPoint) this.m_connectionPoints[(object) iid])?.Callback;
    }

    protected bool IsConnected(Guid iid)
    {
      ConnectionPoint connectionPoint = (ConnectionPoint) this.m_connectionPoints[(object) iid];
      return connectionPoint != null && connectionPoint.IsConnected;
    }

    public void EnumConnectionPoints(out IEnumConnectionPoints ppenum)
    {
      lock (this)
      {
        try
        {
          ppenum = (IEnumConnectionPoints) new OpcCom.Da.Wrapper.EnumConnectionPoints(this.m_connectionPoints.Values);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
    {
      lock (this)
      {
        try
        {
          ppCP = (IConnectionPoint) null;
          ppCP = (IConnectionPoint) ((ConnectionPoint) this.m_connectionPoints[(object) riid] ?? throw new ExternalException("CONNECT_E_NOCONNECTION", -2147220992));
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }
  }
}
