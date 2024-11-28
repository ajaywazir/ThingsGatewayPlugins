

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class ResultCollection : ItemIdentifier, ICollection, IEnumerable, ICloneable, IList
  {
    private ArrayList m_results = new ArrayList();

    public Result this[int index]
    {
      get => (Result) this.m_results[index];
      set => this.m_results[index] = (object) value;
    }

    public ResultCollection()
    {
    }

    public ResultCollection(ItemIdentifier item)
      : base(item)
    {
    }

    public ResultCollection(ResultCollection item)
      : base((ItemIdentifier) item)
    {
      this.m_results = new ArrayList(item.m_results.Count);
      foreach (Result result in item.m_results)
        this.m_results.Add(result.Clone());
    }

    public override object Clone()
    {
      ResultCollection resultCollection = (ResultCollection) base.Clone();
      resultCollection.m_results = new ArrayList(this.m_results.Count);
      foreach (ResultCollection result in this.m_results)
        resultCollection.m_results.Add(result.Clone());
      return (object) resultCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_results == null ? 0 : this.m_results.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_results == null)
        return;
      this.m_results.CopyTo(array, index);
    }

    public void CopyTo(Result[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_results.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_results[index];
      set
      {
        this.m_results[index] = typeof (Result).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add Result objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_results.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (Result).IsInstanceOfType(value))
        throw new ArgumentException("May only add Result objects into the collection.");
      this.m_results.Insert(index, value);
    }

    public void Remove(object value) => this.m_results.Remove(value);

    public bool Contains(object value) => this.m_results.Contains(value);

    public void Clear() => this.m_results.Clear();

    public int IndexOf(object value) => this.m_results.IndexOf(value);

    public int Add(object value)
    {
      return typeof (Result).IsInstanceOfType(value) ? this.m_results.Add(value) : throw new ArgumentException("May only add Result objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, Result value) => this.Insert(index, (object) value);

    public void Remove(Result value) => this.Remove((object) value);

    public bool Contains(Result value) => this.Contains((object) value);

    public int IndexOf(Result value) => this.IndexOf((object) value);

    public int Add(Result value) => this.Add((object) value);
  }
}
