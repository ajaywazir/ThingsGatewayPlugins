

using System;
using System.Collections;


namespace Opc.Dx
{
  [Serializable]
  public class DXConnectionQuery
  {
    private string m_name;
    private string m_browsePath;
    private DXConnectionCollection m_masks = new DXConnectionCollection();
    private bool m_recursive;

    public string Name
    {
      get => this.m_name;
      set => this.m_name = value;
    }

    public string BrowsePath
    {
      get => this.m_browsePath;
      set => this.m_browsePath = value;
    }

    public bool Recursive
    {
      get => this.m_recursive;
      set => this.m_recursive = value;
    }

    public DXConnectionCollection Masks => this.m_masks;

    public DXConnection[] Query(Server server, out ResultID[] errors)
    {
      if (server == null)
        throw new ArgumentNullException(nameof (server));
      return server.QueryDXConnections(this.BrowsePath, this.Masks.ToArray(), this.Recursive, out errors);
    }

    public GeneralResponse Update(
      Server server,
      DXConnection connectionDefinition,
      out ResultID[] errors)
    {
      if (server == null)
        throw new ArgumentNullException(nameof (server));
      return server.UpdateDXConnections(this.BrowsePath, this.Masks.ToArray(), this.Recursive, connectionDefinition, out errors);
    }

    public GeneralResponse Delete(Server server, out ResultID[] errors)
    {
      if (server == null)
        throw new ArgumentNullException(nameof (server));
      return server.DeleteDXConnections(this.BrowsePath, this.Masks.ToArray(), this.Recursive, out errors);
    }

    public GeneralResponse CopyDefaultAttributes(
      Server server,
      bool configToStatus,
      out ResultID[] errors)
    {
      if (server == null)
        throw new ArgumentNullException(nameof (server));
      return server.CopyDXConnectionDefaultAttributes(configToStatus, this.BrowsePath, this.Masks.ToArray(), this.Recursive, out errors);
    }

    public DXConnectionQuery()
    {
    }

    public DXConnectionQuery(DXConnectionQuery query)
    {
      if (query == null)
        return;
      this.Name = query.Name;
      this.BrowsePath = query.BrowsePath;
      this.Recursive = query.Recursive;
      this.m_masks = new DXConnectionCollection((ICollection) query.Masks);
    }

    public virtual object Clone() => (object) new DXConnectionQuery(this);
  }
}
