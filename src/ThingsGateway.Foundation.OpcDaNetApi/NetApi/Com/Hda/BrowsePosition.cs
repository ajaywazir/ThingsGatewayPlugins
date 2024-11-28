


namespace OpcCom.Hda
{
  internal class BrowsePosition : Opc.Hda.BrowsePosition
  {
    private bool m_disposed;
    private string m_branchPath;
    private EnumString m_enumerator;
    private bool m_fetchingItems;

    internal BrowsePosition(string branchPath, EnumString enumerator, bool fetchingItems)
    {
      this.m_branchPath = branchPath;
      this.m_enumerator = enumerator;
      this.m_fetchingItems = fetchingItems;
    }

    internal string BranchPath
    {
      get => this.m_branchPath;
      set => this.m_branchPath = value;
    }

    internal EnumString Enumerator
    {
      get => this.m_enumerator;
      set => this.m_enumerator = value;
    }

    internal bool FetchingItems
    {
      get => this.m_fetchingItems;
      set => this.m_fetchingItems = value;
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        if (disposing && this.m_enumerator != null)
        {
          this.m_enumerator.Dispose();
          this.m_enumerator = (EnumString) null;
        }
        this.m_disposed = true;
      }
      base.Dispose(disposing);
    }
  }
}
