

using Opc.Ae;
using System;
using System.Runtime.InteropServices.ComTypes;


namespace OpcCom.Ae
{
  [Serializable]
  public class BrowsePosition : Opc.Ae.BrowsePosition
  {
    private bool m_disposed;
    private IEnumString m_enumerator;

    public BrowsePosition(
      string areaID,
      BrowseType browseType,
      string browseFilter,
      IEnumString enumerator)
      : base(areaID, browseType, browseFilter)
    {
      this.m_enumerator = enumerator;
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        int num = disposing ? 1 : 0;
        if (this.m_enumerator != null)
        {
          OpcCom.Interop.ReleaseServer((object) this.m_enumerator);
          this.m_enumerator = (IEnumString) null;
        }
        this.m_disposed = true;
      }
      base.Dispose(disposing);
    }

    public IEnumString Enumerator => this.m_enumerator;
  }
}
