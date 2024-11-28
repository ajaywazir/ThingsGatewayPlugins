

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class ItemTimeCollection : ItemIdentifier, ICollection, IEnumerable, ICloneable, IList
  {
    private ArrayList m_times = new ArrayList();

    public DateTime this[int index]
    {
      get => (DateTime) this.m_times[index];
      set => this.m_times[index] = (object) value;
    }

    public ItemTimeCollection()
    {
    }

    public ItemTimeCollection(ItemIdentifier item)
      : base(item)
    {
    }

    public ItemTimeCollection(ItemTimeCollection item)
      : base((ItemIdentifier) item)
    {
      this.m_times = new ArrayList(item.m_times.Count);
      foreach (DateTime time in item.m_times)
        this.m_times.Add((object) time);
    }

    public override object Clone()
    {
      ItemTimeCollection itemTimeCollection = (ItemTimeCollection) base.Clone();
      itemTimeCollection.m_times = new ArrayList(this.m_times.Count);
      foreach (DateTime time in this.m_times)
        itemTimeCollection.m_times.Add((object) time);
      return (object) itemTimeCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_times == null ? 0 : this.m_times.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_times == null)
        return;
      this.m_times.CopyTo(array, index);
    }

    public void CopyTo(DateTime[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_times.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_times[index];
      set
      {
        this.m_times[index] = typeof (DateTime).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add DateTime objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_times.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (DateTime).IsInstanceOfType(value))
        throw new ArgumentException("May only add DateTime objects into the collection.");
      this.m_times.Insert(index, value);
    }

    public void Remove(object value) => this.m_times.Remove(value);

    public bool Contains(object value) => this.m_times.Contains(value);

    public void Clear() => this.m_times.Clear();

    public int IndexOf(object value) => this.m_times.IndexOf(value);

    public int Add(object value)
    {
      return typeof (DateTime).IsInstanceOfType(value) ? this.m_times.Add(value) : throw new ArgumentException("May only add DateTime objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, DateTime value) => this.Insert(index, (object) value);

    public void Remove(DateTime value) => this.Remove((object) value);

    public bool Contains(DateTime value) => this.Contains((object) value);

    public int IndexOf(DateTime value) => this.IndexOf((object) value);

    public int Add(DateTime value) => this.Add((object) value);
  }
}
