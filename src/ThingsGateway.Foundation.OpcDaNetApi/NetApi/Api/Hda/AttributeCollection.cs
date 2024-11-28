

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class AttributeCollection : ICloneable, ICollection, IEnumerable
  {
    private Attribute[] m_attributes = new Attribute[0];

    public AttributeCollection()
    {
    }

    public AttributeCollection(ICollection collection) => this.Init(collection);

    public Attribute this[int index]
    {
      get => this.m_attributes[index];
      set => this.m_attributes[index] = value;
    }

    public Attribute Find(int id)
    {
      foreach (Attribute attribute in this.m_attributes)
      {
        if (attribute.ID == id)
          return attribute;
      }
      return (Attribute) null;
    }

    public void Init(ICollection collection)
    {
      this.Clear();
      if (collection == null)
        return;
      ArrayList arrayList = new ArrayList(collection.Count);
      foreach (object source in (IEnumerable) collection)
      {
        if (source.GetType() == typeof (Attribute))
          arrayList.Add(Opc.Convert.Clone(source));
      }
      this.m_attributes = (Attribute[]) arrayList.ToArray(typeof (Attribute));
    }

    public void Clear() => this.m_attributes = new Attribute[0];

    public virtual object Clone() => (object) new AttributeCollection((ICollection) this);

    public bool IsSynchronized => false;

    public int Count => this.m_attributes == null ? 0 : this.m_attributes.Length;

    public void CopyTo(Array array, int index)
    {
      if (this.m_attributes == null)
        return;
      this.m_attributes.CopyTo(array, index);
    }

    public void CopyTo(Attribute[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_attributes.GetEnumerator();
  }
}
