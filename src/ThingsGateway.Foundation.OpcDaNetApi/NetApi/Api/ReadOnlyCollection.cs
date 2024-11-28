

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc
{
  [Serializable]
  public class ReadOnlyCollection : ICollection, IEnumerable, ICloneable, ISerializable
  {
    private Array m_array;

    public virtual object this[int index] => this.m_array.GetValue(index);

    public virtual Array ToArray() => (Array) Convert.Clone((object) this.m_array);

    protected ReadOnlyCollection(Array array) => this.Array = array;

    protected virtual Array Array
    {
      get => this.m_array;
      set
      {
        this.m_array = value;
        if (this.m_array != null)
          return;
        this.m_array = (Array) new object[0];
      }
    }

    protected ReadOnlyCollection(SerializationInfo info, StreamingContext context)
    {
      this.m_array = (Array) info.GetValue("AR", typeof (Array));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("AR", (object) this.m_array);
    }

    public virtual bool IsSynchronized => false;

    public virtual int Count => this.m_array.Length;

    public virtual void CopyTo(Array array, int index)
    {
      if (this.m_array == null)
        return;
      this.m_array.CopyTo(array, index);
    }

    public virtual object SyncRoot => (object) this;

    public virtual IEnumerator GetEnumerator() => this.m_array.GetEnumerator();

    public virtual object Clone()
    {
      ReadOnlyCollection readOnlyCollection = (ReadOnlyCollection) this.MemberwiseClone();
      ArrayList arrayList = new ArrayList(this.m_array.Length);
      System.Type type = (System.Type) null;
      for (int index = 0; index < this.m_array.Length; ++index)
      {
        object obj = this.m_array.GetValue(index);
        if (type == (System.Type) null)
          type = obj.GetType();
        else if (type != typeof (object))
        {
          while (!type.IsInstanceOfType(obj))
            type = type.BaseType;
        }
        arrayList.Add(Convert.Clone(obj));
      }
      readOnlyCollection.Array = arrayList.ToArray(type);
      return (object) readOnlyCollection;
    }

    private class Names
    {
      internal const string ARRAY = "AR";
    }
  }
}
