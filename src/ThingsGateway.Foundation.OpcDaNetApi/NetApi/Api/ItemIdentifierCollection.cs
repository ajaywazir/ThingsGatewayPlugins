

using System;
using System.Collections;


namespace Opc
{
  [Serializable]
  public class ItemIdentifierCollection : ICloneable, ICollection, IEnumerable
  {
    private ItemIdentifier[] m_itemIDs = new ItemIdentifier[0];

    public ItemIdentifierCollection()
    {
    }

    public ItemIdentifierCollection(ICollection collection) => this.Init(collection);

    public ItemIdentifier this[int index]
    {
      get => this.m_itemIDs[index];
      set => this.m_itemIDs[index] = value;
    }

    public void Init(ICollection collection)
    {
      this.Clear();
      if (collection == null)
        return;
      ArrayList arrayList = new ArrayList(collection.Count);
      foreach (object o in (IEnumerable) collection)
      {
        if (typeof (ItemIdentifier).IsInstanceOfType(o))
          arrayList.Add(((ItemIdentifier) o).Clone());
      }
      this.m_itemIDs = (ItemIdentifier[]) arrayList.ToArray(typeof (ItemIdentifier));
    }

    public void Clear() => this.m_itemIDs = new ItemIdentifier[0];

    public virtual object Clone() => (object) new ItemIdentifierCollection((ICollection) this);

    public bool IsSynchronized => false;

    public int Count => this.m_itemIDs == null ? 0 : this.m_itemIDs.Length;

    public void CopyTo(Array array, int index)
    {
      if (this.m_itemIDs == null)
        return;
      this.m_itemIDs.CopyTo(array, index);
    }

    public void CopyTo(ItemIdentifier[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_itemIDs.GetEnumerator();
  }
}
