

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class ItemValueCollection : 
    Opc.Hda.Item,
    IResult,
    IActualTime,
    ICollection,
    IEnumerable,
    ICloneable,
    IList
  {
    private DateTime m_startTime = DateTime.MinValue;
    private DateTime m_endTime = DateTime.MinValue;
    private ArrayList m_values = new ArrayList();
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public ItemValue this[int index]
    {
      get => (ItemValue) this.m_values[index];
      set => this.m_values[index] = (object) value;
    }

    public ItemValueCollection()
    {
    }

    public ItemValueCollection(ItemIdentifier item)
      : base(item)
    {
    }

    public ItemValueCollection(Opc.Hda.Item item)
      : base(item)
    {
    }

    public ItemValueCollection(ItemValueCollection item)
      : base((Opc.Hda.Item) item)
    {
      this.m_values = new ArrayList(item.m_values.Count);
      foreach (ItemValue itemValue in item.m_values)
      {
        if (itemValue != null)
          this.m_values.Add(itemValue.Clone());
      }
    }

    public void AddRange(ItemValueCollection collection)
    {
      if (collection == null)
        return;
      foreach (ItemValue itemValue in collection)
      {
        if (itemValue != null)
          this.m_values.Add(itemValue.Clone());
      }
    }

    public ResultID ResultID
    {
      get => this.m_resultID;
      set => this.m_resultID = value;
    }

    public string DiagnosticInfo
    {
      get => this.m_diagnosticInfo;
      set => this.m_diagnosticInfo = value;
    }

    public DateTime StartTime
    {
      get => this.m_startTime;
      set => this.m_startTime = value;
    }

    public DateTime EndTime
    {
      get => this.m_endTime;
      set => this.m_endTime = value;
    }

    public override object Clone()
    {
      ItemValueCollection itemValueCollection = (ItemValueCollection) base.Clone();
      itemValueCollection.m_values = new ArrayList(this.m_values.Count);
      foreach (ItemValue itemValue in this.m_values)
        itemValueCollection.m_values.Add(itemValue.Clone());
      return (object) itemValueCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_values == null ? 0 : this.m_values.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_values == null)
        return;
      this.m_values.CopyTo(array, index);
    }

    public void CopyTo(ItemValue[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_values.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_values[index];
      set
      {
        this.m_values[index] = typeof (ItemValue).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add ItemValue objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_values.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (ItemValue).IsInstanceOfType(value))
        throw new ArgumentException("May only add ItemValue objects into the collection.");
      this.m_values.Insert(index, value);
    }

    public void Remove(object value) => this.m_values.Remove(value);

    public bool Contains(object value) => this.m_values.Contains(value);

    public void Clear() => this.m_values.Clear();

    public int IndexOf(object value) => this.m_values.IndexOf(value);

    public int Add(object value)
    {
      return typeof (ItemValue).IsInstanceOfType(value) ? this.m_values.Add(value) : throw new ArgumentException("May only add ItemValue objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, ItemValue value) => this.Insert(index, (object) value);

    public void Remove(ItemValue value) => this.Remove((object) value);

    public bool Contains(ItemValue value) => this.Contains((object) value);

    public int IndexOf(ItemValue value) => this.IndexOf((object) value);

    public int Add(ItemValue value) => this.Add((object) value);
  }
}
