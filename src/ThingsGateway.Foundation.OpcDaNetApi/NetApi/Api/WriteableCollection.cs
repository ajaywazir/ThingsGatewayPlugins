

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc
{
  [Serializable]
  public class WriteableCollection : ICollection, IEnumerable, IList, ICloneable, ISerializable
  {
    protected const string INVALID_VALUE = "The value '{0}' cannot be added to the collection.";
    protected const string INVALID_TYPE = "A value with type '{0}' cannot be added to the collection.";
    private ArrayList m_array;
    private System.Type m_elementType;

    public virtual object this[int index]
    {
      get => this.m_array[index];
      set => this.m_array[index] = value;
    }

    public virtual System.Array ToArray() => this.m_array.ToArray(this.m_elementType);

    public virtual void AddRange(ICollection collection)
    {
      if (collection == null)
        return;
      foreach (object element in (IEnumerable) collection)
        this.ValidateElement(element);
      this.m_array.AddRange(collection);
    }

    protected WriteableCollection(ICollection array, System.Type elementType)
    {
      this.m_array = array == null ? new ArrayList() : new ArrayList(array);
      this.m_elementType = typeof (object);
      if (!(elementType != (System.Type) null))
        return;
      foreach (object element in this.m_array)
        this.ValidateElement(element);
      this.m_elementType = elementType;
    }

    protected virtual ArrayList Array
    {
      get => this.m_array;
      set
      {
        this.m_array = value;
        if (this.m_array != null)
          return;
        this.m_array = new ArrayList();
      }
    }

    protected virtual System.Type ElementType
    {
      get => this.m_elementType;
      set
      {
        foreach (object element in this.m_array)
          this.ValidateElement(element);
        this.m_elementType = value;
      }
    }

    protected virtual void ValidateElement(object element)
    {
      if (element == null)
        throw new ArgumentException(string.Format("The value '{0}' cannot be added to the collection.", element));
      if (!this.m_elementType.IsInstanceOfType(element))
        throw new ArgumentException(string.Format("A value with type '{0}' cannot be added to the collection.", (object) element.GetType()));
    }

    protected WriteableCollection(SerializationInfo info, StreamingContext context)
    {
      this.m_elementType = (System.Type) info.GetValue("ET", typeof (System.Type));
      int capacity = (int) info.GetValue("CT", typeof (int));
      this.m_array = new ArrayList(capacity);
      for (int index = 0; index < capacity; ++index)
        this.m_array.Add(info.GetValue("EL" + index.ToString(), typeof (object)));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("ET", (object) this.m_elementType);
      info.AddValue("CT", this.m_array.Count);
      for (int index = 0; index < this.m_array.Count; ++index)
        info.AddValue("EL" + index.ToString(), this.m_array[index]);
    }

    public virtual bool IsSynchronized => false;

    public virtual int Count => this.m_array.Count;

    public virtual void CopyTo(System.Array array, int index)
    {
      if (this.m_array == null)
        return;
      this.m_array.CopyTo(array, index);
    }

    public virtual object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_array.GetEnumerator();

    public virtual bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this[index];
      set => this[index] = value;
    }

    public virtual void RemoveAt(int index) => this.m_array.RemoveAt(index);

    public virtual void Insert(int index, object value)
    {
      this.ValidateElement(value);
      this.m_array.Insert(index, value);
    }

    public virtual void Remove(object value) => this.m_array.Remove(value);

    public virtual bool Contains(object value) => this.m_array.Contains(value);

    public virtual void Clear() => this.m_array.Clear();

    public virtual int IndexOf(object value) => this.m_array.IndexOf(value);

    public virtual int Add(object value)
    {
      this.ValidateElement(value);
      return this.m_array.Add(value);
    }

    public virtual bool IsFixedSize => false;

    public virtual object Clone()
    {
      WriteableCollection writeableCollection = (WriteableCollection) this.MemberwiseClone();
      writeableCollection.m_array = new ArrayList();
      for (int index = 0; index < this.m_array.Count; ++index)
        writeableCollection.Add(Convert.Clone(this.m_array[index]));
      return (object) writeableCollection;
    }

    private class Names
    {
      internal const string COUNT = "CT";
      internal const string ELEMENT = "EL";
      internal const string ELEMENT_TYPE = "ET";
    }
  }
}
