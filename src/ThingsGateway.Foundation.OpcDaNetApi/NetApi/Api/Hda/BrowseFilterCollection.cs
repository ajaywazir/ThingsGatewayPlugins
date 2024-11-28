

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class BrowseFilterCollection : ItemIdentifier, ICollection, IEnumerable
  {
    private BrowseFilter[] m_filters = new BrowseFilter[0];

    public BrowseFilterCollection()
    {
    }

    public BrowseFilterCollection(ICollection collection) => this.Init(collection);

    public BrowseFilter this[int index]
    {
      get => this.m_filters[index];
      set => this.m_filters[index] = value;
    }

    public BrowseFilter Find(int id)
    {
      foreach (BrowseFilter filter in this.m_filters)
      {
        if (filter.AttributeID == id)
          return filter;
      }
      return (BrowseFilter) null;
    }

    public void Init(ICollection collection)
    {
      this.Clear();
      if (collection == null)
        return;
      ArrayList arrayList = new ArrayList(collection.Count);
      foreach (object source in (IEnumerable) collection)
      {
        if (source.GetType() == typeof (BrowseFilter))
          arrayList.Add(Opc.Convert.Clone(source));
      }
      this.m_filters = (BrowseFilter[]) arrayList.ToArray(typeof (BrowseFilter));
    }

    public void Clear() => this.m_filters = new BrowseFilter[0];

    public override object Clone() => (object) new BrowseFilterCollection((ICollection) this);

    public bool IsSynchronized => false;

    public int Count => this.m_filters == null ? 0 : this.m_filters.Length;

    public void CopyTo(Array array, int index)
    {
      if (this.m_filters == null)
        return;
      this.m_filters.CopyTo(array, index);
    }

    public void CopyTo(BrowseFilter[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_filters.GetEnumerator();
  }
}
