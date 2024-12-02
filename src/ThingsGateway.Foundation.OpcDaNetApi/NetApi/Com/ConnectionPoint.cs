

using OpcRcw.Comn;

using System;


namespace OpcCom
{
    public class ConnectionPoint : IDisposable
    {
        private IConnectionPoint m_server;
        private int m_cookie;
        private int m_refs;

        public ConnectionPoint(object server, Guid iid)
        {
            ((IConnectionPointContainer)server).FindConnectionPoint(ref iid, out m_server);
        }

        public void Dispose()
        {
            if (m_server == null)
                return;

            while (Unadvise() > 0)
            {

            }
            Interop.ReleaseServer((object)m_server);
            m_server = (IConnectionPoint)null;
        }

        public int Cookie => m_cookie;

        public int Advise(object callback)
        {
            if (m_refs++ == 0)
                m_server.Advise(callback, out m_cookie);
            return m_refs;
        }

        public int Unadvise()
        {
            if (--m_refs == 0)
                m_server.Unadvise(m_cookie);
            return m_refs;
        }
    }
}
