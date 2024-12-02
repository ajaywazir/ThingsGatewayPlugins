

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
            m_url = (URL)null;
            m_server = (object)null;
            m_callback = new Server.Callback(this);
        }

        internal Server(URL url, object server)
        {
            m_url = url != null ? (URL)url.Clone() : throw new ArgumentNullException(nameof(url));
            m_server = server;
            m_callback = new Server.Callback(this);
        }

        ~Server() => Dispose(false);

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
                Interop.ReleaseServer(m_server);
                m_server = (object)null;
            }
            m_disposed = true;
        }

        public virtual void Initialize(URL url, ConnectData connectData)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            lock (this)
            {
                if (m_url == null || !m_url.Equals((object)url))
                {
                    if (m_server != null)
                        Uninitialize();
                    m_server = (object)(IOPCCommon)Factory.Connect(url, connectData);
                }
                m_url = (URL)url.Clone();
            }
        }

        public virtual void Uninitialize()
        {
            lock (this)
                Dispose();
        }

        public virtual event ServerShutdownEventHandler ServerShutdown
        {
            add
            {
                lock (this)
                {
                    try
                    {
                        Advise();
                        m_callback.ServerShutdown += value;
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
                    m_callback.ServerShutdown -= value;
                    Unadvise();
                }
            }
        }

        public virtual string GetLocale()
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    int pdwLcid = 0;
                    ((IOPCCommon)m_server).GetLocaleID(out pdwLcid);
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
                if (m_server == null)
                    throw new NotConnectedException();
                int locale1 = Interop.GetLocale(locale);
                try
                {
                    ((IOPCCommon)m_server).SetLocaleID(locale1);
                }
                catch (Exception ex)
                {
                    if (locale1 != 0)
                        throw Interop.CreateException("IOPCCommon.SetLocaleID", ex);
                    try
                    {
                        ((IOPCCommon)m_server).SetLocaleID(2048);
                    }
                    catch
                    {
                    }
                }
                return GetLocale();
            }
        }

        public virtual string[] GetSupportedLocales()
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    int pdwCount = 0;
                    IntPtr pdwLcid = IntPtr.Zero;
                    ((IOPCCommon)m_server).QueryAvailableLocaleIDs(out pdwCount, out pdwLcid);
                    int[] int32s = Interop.GetInt32s(ref pdwLcid, pdwCount, true);
                    if (int32s == null)
                        return (string[])null;
                    ArrayList arrayList = new ArrayList();
                    foreach (int input in int32s)
                    {
                        try
                        {
                            arrayList.Add((object)Interop.GetLocale(input));
                        }
                        catch
                        {
                        }
                    }
                    return (string[])arrayList.ToArray(typeof(string));
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
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    string locale1 = GetLocale();
                    if (locale1 != locale)
                        SetLocale(locale);
                    string ppString = (string)null;
                    ((IOPCCommon)m_server).GetErrorString(resultID.Code, out ppString);
                    if (locale1 != locale)
                        SetLocale(locale1);
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
            if (m_connection != null)
                return;
            m_connection = new ConnectionPoint(m_server, typeof(IOPCShutdown).GUID);
            m_connection.Advise((object)m_callback);
        }

        private void Unadvise()
        {
            if (m_connection == null || m_connection.Unadvise() != 0)
                return;
            m_connection.Dispose();
            m_connection = (ConnectionPoint)null;
        }

        private sealed class Callback : IOPCShutdown
        {
            private Server m_server;

            public Callback(Server server) => m_server = server;

            public event ServerShutdownEventHandler ServerShutdown
            {
                add
                {
                    lock (this)
                        m_serverShutdown += value;
                }
                remove
                {
                    lock (this)
                        m_serverShutdown -= value;
                }
            }

            private event ServerShutdownEventHandler m_serverShutdown;

            public void ShutdownRequest(string reason)
            {
                try
                {
                    lock (this)
                    {
                        if (m_serverShutdown == null)
                            return;
                        m_serverShutdown(reason);
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
