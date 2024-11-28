

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class ItemAttributeCollection : 
    ItemIdentifier,
    IResult,
    IActualTime,
    ICollection,
    IEnumerable,
    IList
  {
    private DateTime m_startTime = DateTime.MinValue;
    private DateTime m_endTime = DateTime.MinValue;
    private ArrayList m_attributes = new ArrayList();
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public AttributeValueCollection this[int index]
    {
      get => (AttributeValueCollection) this.m_attributes[index];
      set => this.m_attributes[index] = (object) value;
    }

    public ItemAttributeCollection()
    {
    }

    public ItemAttributeCollection(ItemIdentifier item)
      : base(item)
    {
    }

    public ItemAttributeCollection(ItemAttributeCollection item)
      : base((ItemIdentifier) item)
    {
      this.m_attributes = new ArrayList(item.m_attributes.Count);
      foreach (AttributeValueCollection attribute in item.m_attributes)
      {
        if (attribute != null)
          this.m_attributes.Add(attribute.Clone());
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
      ItemAttributeCollection attributeCollection = (ItemAttributeCollection) base.Clone();
      attributeCollection.m_attributes = new ArrayList(this.m_attributes.Count);
      foreach (AttributeValueCollection attribute in this.m_attributes)
        attributeCollection.m_attributes.Add(attribute.Clone());
      return (object) attributeCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_attributes == null ? 0 : this.m_attributes.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_attributes == null)
        return;
      this.m_attributes.CopyTo(array, index);
    }

    public void CopyTo(AttributeValueCollection[] array, int index)
    {
      this.CopyTo((Array) array, index);
    }

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_attributes.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_attributes[index];
      set
      {
        this.m_attributes[index] = typeof (AttributeValueCollection).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_attributes.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (AttributeValueCollection).IsInstanceOfType(value))
        throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
      this.m_attributes.Insert(index, value);
    }

    public void Remove(object value) => this.m_attributes.Remove(value);

    public bool Contains(object value) => this.m_attributes.Contains(value);

    public void Clear() => this.m_attributes.Clear();

    public int IndexOf(object value) => this.m_attributes.IndexOf(value);

    public int Add(object value)
    {
      return typeof (AttributeValueCollection).IsInstanceOfType(value) ? this.m_attributes.Add(value) : throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, AttributeValueCollection value)
    {
      this.Insert(index, (object) value);
    }

    public void Remove(AttributeValueCollection value) => this.Remove((object) value);

    public bool Contains(AttributeValueCollection value) => this.Contains((object) value);

    public int IndexOf(AttributeValueCollection value) => this.IndexOf((object) value);

    public int Add(AttributeValueCollection value) => this.Add((object) value);
  }
}
