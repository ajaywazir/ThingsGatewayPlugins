

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class AggregateCollection : ICloneable, ICollection, IEnumerable
  {
    private Aggregate[] m_aggregates = new Aggregate[0];

    public AggregateCollection()
    {
    }

    public AggregateCollection(ICollection collection) => this.Init(collection);

    public Aggregate this[int index]
    {
      get => this.m_aggregates[index];
      set => this.m_aggregates[index] = value;
    }

    public Aggregate Find(int id)
    {
      foreach (Aggregate aggregate in this.m_aggregates)
      {
        if (aggregate.ID == id)
          return aggregate;
      }
      return (Aggregate) null;
    }

    public void Init(ICollection collection)
    {
      this.Clear();
      if (collection == null)
        return;
      ArrayList arrayList = new ArrayList(collection.Count);
      foreach (object source in (IEnumerable) collection)
      {
        if (source.GetType() == typeof (Aggregate))
          arrayList.Add(Opc.Convert.Clone(source));
      }
      this.m_aggregates = (Aggregate[]) arrayList.ToArray(typeof (Aggregate));
    }

    public void Clear() => this.m_aggregates = new Aggregate[0];

    public virtual object Clone() => (object) new AggregateCollection((ICollection) this);

    public bool IsSynchronized => false;

    public int Count => this.m_aggregates == null ? 0 : this.m_aggregates.Length;

    public void CopyTo(Array array, int index)
    {
      if (this.m_aggregates == null)
        return;
      this.m_aggregates.CopyTo(array, index);
    }

    public void CopyTo(Aggregate[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_aggregates.GetEnumerator();
  }
}
