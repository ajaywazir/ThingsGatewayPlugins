

using System;
using System.Collections;


namespace Opc.Da
{
  [Serializable]
  public class ItemCollection : ICollection, IEnumerable, ICloneable, IList
  {
    private ArrayList m_items = new ArrayList();

    public Item this[int index]
    {
      get => (Item) this.m_items[index];
      set => this.m_items[index] = (object) value;
    }

    public Item this[ItemIdentifier itemID]
    {
      get
      {
        foreach (Item obj in this.m_items)
        {
          if (itemID.Key == obj.Key)
            return obj;
        }
        return (Item) null;
      }
    }

    public ItemCollection()
    {
    }

    public ItemCollection(ItemCollection items)
    {
      if (items == null)
        return;
      foreach (Item obj in items)
        this.Add(obj);
    }

    public virtual object Clone()
    {
      ItemCollection itemCollection = (ItemCollection) this.MemberwiseClone();
      itemCollection.m_items = new ArrayList();
      foreach (Item obj in this.m_items)
        itemCollection.m_items.Add(obj.Clone());
      return (object) itemCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_items == null ? 0 : this.m_items.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_items == null)
        return;
      this.m_items.CopyTo(array, index);
    }

    public void CopyTo(Item[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_items.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_items[index];
      set
      {
        this.m_items[index] = typeof (Item).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add Item objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_items.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (Item).IsInstanceOfType(value))
        throw new ArgumentException("May only add Item objects into the collection.");
      this.m_items.Insert(index, value);
    }

    public void Remove(object value) => this.m_items.Remove(value);

    public bool Contains(object value) => this.m_items.Contains(value);

    public void Clear() => this.m_items.Clear();

    public int IndexOf(object value) => this.m_items.IndexOf(value);

    public int Add(object value)
    {
      return typeof (Item).IsInstanceOfType(value) ? this.m_items.Add(value) : throw new ArgumentException("May only add Item objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, Item value) => this.Insert(index, (object) value);

    public void Remove(Item value) => this.Remove((object) value);

    public bool Contains(Item value) => this.Contains((object) value);

    public int IndexOf(Item value) => this.IndexOf((object) value);

    public int Add(Item value) => this.Add((object) value);
  }
}
