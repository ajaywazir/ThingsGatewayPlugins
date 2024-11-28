

using System;


namespace Opc.Da
{
  [Serializable]
  public class BrowsePosition : IBrowsePosition, IDisposable, ICloneable
  {
    private bool m_disposed;
    private BrowseFilters m_filters;
    private ItemIdentifier m_itemID;

    public ItemIdentifier ItemID => this.m_itemID;

    public BrowseFilters Filters => (BrowseFilters) this.m_filters.Clone();

    public int MaxElementsReturned
    {
      get => this.m_filters.MaxElementsReturned;
      set => this.m_filters.MaxElementsReturned = value;
    }

    public BrowsePosition(ItemIdentifier itemID, BrowseFilters filters)
    {
      if (filters == null)
        throw new ArgumentNullException(nameof (filters));
      this.m_itemID = itemID != null ? (ItemIdentifier) itemID.Clone() : (ItemIdentifier) null;
      this.m_filters = (BrowseFilters) filters.Clone();
    }

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
