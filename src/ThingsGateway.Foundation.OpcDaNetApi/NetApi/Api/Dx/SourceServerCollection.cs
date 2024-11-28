

using System;
using System.Collections;


namespace Opc.Dx
{
  public class SourceServerCollection : ICollection, IEnumerable, ICloneable
  {
    private ArrayList m_servers = new ArrayList();

    public SourceServer this[int index] => (SourceServer) this.m_servers[index];

    public SourceServer this[string name]
    {
      get
      {
        foreach (SourceServer server in this.m_servers)
        {
          if (server.Name == name)
            return server;
        }
        return (SourceServer) null;
      }
    }

    internal SourceServerCollection()
    {
    }

    internal void Initialize(ICollection sourceServers)
    {
      this.m_servers.Clear();
      if (sourceServers == null)
        return;
      foreach (SourceServer sourceServer in (IEnumerable) sourceServers)
        this.m_servers.Add((object) sourceServer);
    }

    public virtual object Clone()
    {
      SourceServerCollection serverCollection = (SourceServerCollection) this.MemberwiseClone();
      serverCollection.m_servers = new ArrayList();
      foreach (SourceServer server in this.m_servers)
        serverCollection.m_servers.Add(server.Clone());
      return (object) serverCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_servers == null ? 0 : this.m_servers.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_servers == null)
        return;
      this.m_servers.CopyTo(array, index);
    }

    public void CopyTo(SourceServer[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_servers.GetEnumerator();
  }
}
