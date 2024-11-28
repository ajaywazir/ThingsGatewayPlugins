

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class AttributeValueCollection : IResult, ICollection, IEnumerable, ICloneable, IList
  {
    private int m_attributeID;
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;
    private ArrayList m_values = new ArrayList();

    public int AttributeID
    {
      get => this.m_attributeID;
      set => this.m_attributeID = value;
    }

    public AttributeValue this[int index]
    {
      get => (AttributeValue) this.m_values[index];
      set => this.m_values[index] = (object) value;
    }

    public AttributeValueCollection()
    {
    }

    public AttributeValueCollection(Attribute attribute) => this.m_attributeID = attribute.ID;

    public AttributeValueCollection(AttributeValueCollection collection)
    {
      this.m_values = new ArrayList(collection.m_values.Count);
      foreach (AttributeValue attributeValue in collection.m_values)
        this.m_values.Add(attributeValue.Clone());
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

    public virtual object Clone()
    {
      AttributeValueCollection attributeValueCollection = (AttributeValueCollection) this.MemberwiseClone();
      attributeValueCollection.m_values = new ArrayList(this.m_values.Count);
      foreach (AttributeValue attributeValue in this.m_values)
        attributeValueCollection.m_values.Add(attributeValue.Clone());
      return (object) attributeValueCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_values == null ? 0 : this.m_values.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_values == null)
        return;
      this.m_values.CopyTo(array, index);
    }

    public void CopyTo(AttributeValue[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_values.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_values[index];
      set
      {
        this.m_values[index] = typeof (AttributeValue).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add AttributeValue objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_values.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (AttributeValue).IsInstanceOfType(value))
        throw new ArgumentException("May only add AttributeValue objects into the collection.");
      this.m_values.Insert(index, value);
    }

    public void Remove(object value) => this.m_values.Remove(value);

    public bool Contains(object value) => this.m_values.Contains(value);

    public void Clear() => this.m_values.Clear();

    public int IndexOf(object value) => this.m_values.IndexOf(value);

    public int Add(object value)
    {
      return typeof (AttributeValue).IsInstanceOfType(value) ? this.m_values.Add(value) : throw new ArgumentException("May only add AttributeValue objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, AttributeValue value) => this.Insert(index, (object) value);

    public void Remove(AttributeValue value) => this.Remove((object) value);

    public bool Contains(AttributeValue value) => this.Contains((object) value);

    public int IndexOf(AttributeValue value) => this.IndexOf((object) value);

    public int Add(AttributeValue value) => this.Add((object) value);
  }
}
