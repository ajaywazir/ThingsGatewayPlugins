

using System;
using System.Collections;


namespace Opc.Dx
{
  [Serializable]
  public class GeneralResponse : ICloneable, ICollection, IEnumerable
  {
    private string m_version;
    private IdentifiedResult[] m_results = new IdentifiedResult[0];

    public string Version
    {
      get => this.m_version;
      set => this.m_version = value;
    }

    public GeneralResponse()
    {
    }

    public GeneralResponse(string version, ICollection results)
    {
      this.Version = version;
      this.Init(results);
    }

    public IdentifiedResult this[int index]
    {
      get => this.m_results[index];
      set => this.m_results[index] = value;
    }

    public void Clear() => this.m_results = new IdentifiedResult[0];

    public virtual object Clone() => (object) new IdentifiedResultCollection((ICollection) this);

    public bool IsSynchronized => false;

    public int Count => this.m_results == null ? 0 : this.m_results.Length;

    public void CopyTo(Array array, int index)
    {
      if (this.m_results == null)
        return;
      this.m_results.CopyTo(array, index);
    }

    public void CopyTo(IdentifiedResult[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_results.GetEnumerator();

    private void Init(ICollection collection)
    {
      this.Clear();
      if (collection == null)
        return;
      ArrayList arrayList = new ArrayList(collection.Count);
      foreach (object o in (IEnumerable) collection)
      {
        if (typeof (IdentifiedResult).IsInstanceOfType(o))
          arrayList.Add(((Opc.ItemIdentifier) o).Clone());
      }
      this.m_results = (IdentifiedResult[]) arrayList.ToArray(typeof (IdentifiedResult));
    }
  }
}
