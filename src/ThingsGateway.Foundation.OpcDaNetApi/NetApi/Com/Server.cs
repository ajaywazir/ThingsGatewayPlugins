

using Opc;
using OpcRcw.Comn;
using System;
using System.Collections;


namespace OpcCom
{
  public class Server : IServer, IDisposable
  {
    private bool m_disposed;
    protected object m_server;
    protected URL m_url;
    private ConnectionPoint m_connection;
    private Server.Callback m_callback;

    internal Server()
    {
      this.m_url = (URL) null;
      this.m_server = (object) null;
      this.m_callback = new Server.Callback(this);
    }

    internal Server(URL url, object server)
    {
      this.m_url = url != null ? (URL) url.Clone() : throw new ArgumentNullException(nameof (url));
      this.m_server = server;
      this.m_callback = new Server.Callback(this);
    }

    ~Server() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      lock (this)
      {
        if (disposing && this.m_connection != null)
        {
          this.m_connection.Dispose();
          this.m_connection = (ConnectionPoint) null;
        }
        Interop.ReleaseServer(this.m_server);
        this.m_server = (object) null;
      }
      this.m_disposed = true;
    }

    public virtual void Initialize(URL url, ConnectData connectData)
    {
      if (url == null)
        throw new ArgumentNullException(nameof (url));
      lock (this)
      {
        if (this.m_url == null || !this.m_url.Equals((object) url))
        {
          if (this.m_server != null)
            this.Uninitialize();
          this.m_server = (object) (IOPCCommon) Factory.Connect(url, connectData);
        }
        this.m_url = (URL) url.Clone();
      }
    }

    public virtual void Uninitialize()
    {
      lock (this)
        this.Dispose();
    }

    public virtual event ServerShutdownEventHandler ServerShutdown
    {
      add
      {
        lock (this)
        {
          try
          {
            this.Advise();
            this.m_callback.ServerShutdown += value;
          }
          catch
          {
          }
        }
      }
      remove
      {
        lock (this)
        {
          this.m_callback.ServerShutdown -= value;
          this.Unadvise();
        }
      }
    }

    public virtual string GetLocale()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        try
        {
          int pdwLcid = 0;
          ((IOPCCommon) this.m_server).GetLocaleID(out pdwLcid);
          return Interop.GetLocale(pdwLcid);
        }
        catch (Exception ex)
        {
          throw Interop.CreateException("IOPCCommon.GetLocaleID", ex);
        }
      }
    }

    public virtual string SetLocale(string locale)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        int locale1 = Interop.GetLocale(locale);
        try
        {
          ((IOPCCommon) this.m_server).SetLocaleID(locale1);
        }
        catch (Exception ex)
        {
          if (locale1 != 0)
            throw Interop.CreateException("IOPCCommon.SetLocaleID", ex);
          try
          {
            ((IOPCCommon) this.m_server).SetLocaleID(2048);
          }
          catch
          {
          }
        }
        return this.GetLocale();
      }
    }

    public virtual string[] GetSupportedLocales()
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        try
        {
          int pdwCount = 0;
          IntPtr pdwLcid = IntPtr.Zero;
          ((IOPCCommon) this.m_server).QueryAvailableLocaleIDs(out pdwCount, out pdwLcid);
          int[] int32s = Interop.GetInt32s(ref pdwLcid, pdwCount, true);
          if (int32s == null)
            return (string[]) null;
          ArrayList arrayList = new ArrayList();
          foreach (int input in int32s)
          {
            try
            {
              arrayList.Add((object) Interop.GetLocale(input));
            }
            catch
            {
            }
          }
          return (string[]) arrayList.ToArray(typeof (string));
        }
        catch (Exception ex)
        {
          throw Interop.CreateException("IOPCCommon.QueryAvailableLocaleIDs", ex);
        }
      }
    }

    public virtual string GetErrorText(string locale, ResultID resultID)
    {
      lock (this)
      {
        if (this.m_server == null)
          throw new NotConnectedException();
        try
        {
          string locale1 = this.GetLocale();
          if (locale1 != locale)
            this.SetLocale(locale);
          string ppString = (string) null;
          ((IOPCCommon) this.m_server).GetErrorString(resultID.Code, out ppString);
          if (locale1 != locale)
            this.SetLocale(locale1);
          return ppString;
        }
        catch (Exception ex)
        {
          throw Interop.CreateException("IOPCServer.GetErrorString", ex);
        }
      }
    }

    private void Advise()
    {
      if (this.m_connection != null)
        return;
      this.m_connection = new ConnectionPoint(this.m_server, typeof (IOPCShutdown).GUID);
      this.m_connection.Advise((object) this.m_callback);
    }

    private void Unadvise()
    {
      if (this.m_connection == null || this.m_connection.Unadvise() != 0)
        return;
      this.m_connection.Dispose();
      this.m_connection = (ConnectionPoint) null;
    }

    private class Callback : IOPCShutdown
    {
      private Server m_server;

      public Callback(Server server) => this.m_server = server;

      public event ServerShutdownEventHandler ServerShutdown
      {
        add
        {
          lock (this)
            this.m_serverShutdown += value;
        }
        remove
        {
          lock (this)
            this.m_serverShutdown -= value;
        }
      }

      private event ServerShutdownEventHandler m_serverShutdown;

      public void ShutdownRequest(string reason)
      {
        try
        {
          lock (this)
          {
            if (this.m_serverShutdown == null)
              return;
            this.m_serverShutdown(reason);
          }
        }
        catch (Exception ex)
        {
          string stackTrace = ex.StackTrace;
        }
      }
    }
  }
}
