

using System;
using System.Collections;


namespace Opc.Hda
{
  [Serializable]
  public class AnnotationValueCollection : 
    Opc.Hda.Item,
    IResult,
    IActualTime,
    ICollection,
    IEnumerable,
    ICloneable,
    IList
  {
    private ArrayList m_values = new ArrayList();
    private DateTime m_startTime = DateTime.MinValue;
    private DateTime m_endTime = DateTime.MinValue;
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public AnnotationValue this[int index]
    {
      get => (AnnotationValue) this.m_values[index];
      set => this.m_values[index] = (object) value;
    }

    public AnnotationValueCollection()
    {
    }

    public AnnotationValueCollection(ItemIdentifier item)
      : base(item)
    {
    }

    public AnnotationValueCollection(Opc.Hda.Item item)
      : base(item)
    {
    }

    public AnnotationValueCollection(AnnotationValueCollection item)
      : base((Opc.Hda.Item) item)
    {
      this.m_values = new ArrayList(item.m_values.Count);
      foreach (ItemValue itemValue in item.m_values)
        this.m_values.Add(itemValue.Clone());
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
      AnnotationValueCollection annotationValueCollection = (AnnotationValueCollection) base.Clone();
      annotationValueCollection.m_values = new ArrayList(this.m_values.Count);
      foreach (AnnotationValue annotationValue in this.m_values)
        annotationValueCollection.m_values.Add(annotationValue.Clone());
      return (object) annotationValueCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_values == null ? 0 : this.m_values.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_values == null)
        return;
      this.m_values.CopyTo(array, index);
    }

    public void CopyTo(AnnotationValue[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_values.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_values[index];
      set
      {
        this.m_values[index] = typeof (AnnotationValue).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add AnnotationValue objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_values.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (AnnotationValue).IsInstanceOfType(value))
        throw new ArgumentException("May only add AnnotationValue objects into the collection.");
      this.m_values.Insert(index, value);
    }

    public void Remove(object value) => this.m_values.Remove(value);

    public bool Contains(object value) => this.m_values.Contains(value);

    public void Clear() => this.m_values.Clear();

    public int IndexOf(object value) => this.m_values.IndexOf(value);

    public int Add(object value)
    {
      return typeof (AnnotationValue).IsInstanceOfType(value) ? this.m_values.Add(value) : throw new ArgumentException("May only add AnnotationValue objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, AnnotationValue value) => this.Insert(index, (object) value);

    public void Remove(AnnotationValue value) => this.Remove((object) value);

    public bool Contains(AnnotationValue value) => this.Contains((object) value);

    public int IndexOf(AnnotationValue value) => this.IndexOf((object) value);

    public int Add(AnnotationValue value) => this.Add((object) value);
  }
}
