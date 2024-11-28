

using System;


namespace Opc.Ae
{
  [Serializable]
  public class BrowsePosition : IBrowsePosition, IDisposable, ICloneable
  {
    private bool m_disposed;
    private string m_areaID;
    private BrowseType m_browseType;
    private string m_browseFilter;

    public BrowsePosition(string areaID, BrowseType browseType, string browseFilter)
    {
      this.m_areaID = areaID;
      this.m_browseType = browseType;
      this.m_browseFilter = browseFilter;
    }

    public string AreaID => this.m_areaID;

    public BrowseType BrowseType => this.m_browseType;

    public string BrowseFilter => this.m_browseFilter;

    ~BrowsePosition() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      int num = disposing ? 1 : 0;
      this.m_disposed = true;
    }

    public virtual object Clone() => (object) (BrowsePosition) this.MemberwiseClone();
  }
}
