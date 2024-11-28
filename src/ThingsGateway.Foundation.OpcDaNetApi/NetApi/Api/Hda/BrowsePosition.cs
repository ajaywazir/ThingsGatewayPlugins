

using System;


namespace Opc.Hda
{
  [Serializable]
  public class BrowsePosition : IBrowsePosition, IDisposable, ICloneable
  {
    private bool m_disposed;

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
