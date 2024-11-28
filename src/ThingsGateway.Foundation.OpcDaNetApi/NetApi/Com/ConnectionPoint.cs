

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
            ((IConnectionPointContainer)server).FindConnectionPoint(ref iid, out this.m_server);
        }

        public void Dispose()
        {
            if (this.m_server == null)
                return;

            while (this.Unadvise() > 0)
            {

            }
            Interop.ReleaseServer((object)this.m_server);
            this.m_server = (IConnectionPoint)null;
        }

        public int Cookie => this.m_cookie;

        public int Advise(object callback)
        {
            if (this.m_refs++ == 0)
                this.m_server.Advise(callback, out this.m_cookie);
            return this.m_refs;
        }

        public int Unadvise()
        {
            if (--this.m_refs == 0)
                this.m_server.Unadvise(this.m_cookie);
            return this.m_refs;
        }
    }
}
