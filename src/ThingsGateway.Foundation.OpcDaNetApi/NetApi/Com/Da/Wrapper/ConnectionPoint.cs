

using OpcRcw.Comn;
using System;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
  public class ConnectionPoint : IConnectionPoint
  {
    private Guid m_interface = Guid.Empty;
    private ConnectionPointContainer m_container;
    private object m_callback;
    private int m_cookie;

    public ConnectionPoint(Guid iid, ConnectionPointContainer container)
    {
      this.m_interface = iid;
      this.m_container = container;
    }

    public object Callback => this.m_callback;

    public bool IsConnected => this.m_callback != null;

    public void Advise(object pUnkSink, out int pdwCookie)
    {
      lock (this)
      {
        try
        {
          if (pUnkSink == null)
            throw new ExternalException("E_POINTER", -2147467261);
          pdwCookie = 0;
          this.m_callback = this.m_callback == null ? pUnkSink : throw new ExternalException("CONNECT_E_ADVISELIMIT", -2147220991);
          pdwCookie = ++this.m_cookie;
          this.m_container.OnAdvise(this.m_interface);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Unadvise(int dwCookie)
    {
      lock (this)
      {
        try
        {
          if (this.m_cookie != dwCookie || this.m_callback == null)
            throw new ExternalException("CONNECT_E_NOCONNECTION", -2147220992);
          this.m_callback = (object) null;
          this.m_container.OnUnadvise(this.m_interface);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void GetConnectionInterface(out Guid pIID)
    {
      lock (this)
      {
        try
        {
          pIID = this.m_interface;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void EnumConnections(out IEnumConnections ppenum)
    {
      throw new ExternalException("E_NOTIMPL", -2147467263);
    }

    public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
    {
      lock (this)
      {
        try
        {
          ppCPC = (IConnectionPointContainer) this.m_container;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }
  }
}
