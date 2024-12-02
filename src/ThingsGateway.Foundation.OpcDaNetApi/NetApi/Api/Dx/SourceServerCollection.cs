

using System;
using System.Collections;


namespace Opc.Dx
{
    public class SourceServerCollection : ICollection, IEnumerable, ICloneable
    {
        private ArrayList m_servers = new ArrayList();

        public SourceServer this[int index] => (SourceServer)m_servers[index];

        public SourceServer this[string name]
        {
            get
            {
                foreach (SourceServer server in m_servers)
                {
                    if (server.Name == name)
                        return server;
                }
                return (SourceServer)null;
            }
        }

        internal SourceServerCollection()
        {
        }

        internal void Initialize(ICollection sourceServers)
        {
            m_servers.Clear();
            if (sourceServers == null)
                return;
            foreach (SourceServer sourceServer in (IEnumerable)sourceServers)
                m_servers.Add((object)sourceServer);
        }

        public virtual object Clone()
        {
            SourceServerCollection serverCollection = (SourceServerCollection)MemberwiseClone();
            serverCollection.m_servers = new ArrayList();
            foreach (SourceServer server in m_servers)
                serverCollection.m_servers.Add(server.Clone());
            return (object)serverCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_servers == null ? 0 : m_servers.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_servers == null)
                return;
            m_servers.CopyTo(array, index);
        }

        public void CopyTo(SourceServer[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_servers.GetEnumerator();
    }
}
