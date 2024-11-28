

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc
{
  [Serializable]
  public class ReadOnlyDictionary : IDictionary, ICollection, IEnumerable, ISerializable
  {
    private Hashtable m_dictionary = new Hashtable();
    private const string READ_ONLY_DICTIONARY = "Cannot change the contents of a read-only dictionary";

    protected ReadOnlyDictionary(Hashtable dictionary) => this.Dictionary = dictionary;

    protected virtual Hashtable Dictionary
    {
      get => this.m_dictionary;
      set
      {
        this.m_dictionary = value;
        if (this.m_dictionary != null)
          return;
        this.m_dictionary = new Hashtable();
      }
    }

    protected ReadOnlyDictionary(SerializationInfo info, StreamingContext context)
    {
      int num = (int) info.GetValue("CT", typeof (int));
      this.m_dictionary = new Hashtable();
      for (int index = 0; index < num; ++index)
      {
        object key = info.GetValue("KY" + index.ToString(), typeof (object));
        object obj = info.GetValue("VA" + index.ToString(), typeof (object));
        if (key != null)
          this.m_dictionary[key] = obj;
      }
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("CT", this.m_dictionary.Count);
      int num = 0;
      IDictionaryEnumerator enumerator = this.m_dictionary.GetEnumerator();
      while (enumerator.MoveNext())
      {
        info.AddValue("KY" + num.ToString(), enumerator.Key);
        info.AddValue("VA" + num.ToString(), enumerator.Value);
        ++num;
      }
    }

    public virtual bool IsReadOnly => true;

    public virtual IDictionaryEnumerator GetEnumerator() => this.m_dictionary.GetEnumerator();

    public virtual object this[object key]
    {
      get => this.m_dictionary[key];
      set
      {
        throw new InvalidOperationException("Cannot change the contents of a read-only dictionary");
      }
    }

    public virtual void Remove(object key)
    {
      throw new InvalidOperationException("Cannot change the contents of a read-only dictionary");
    }

    public virtual bool Contains(object key) => this.m_dictionary.Contains(key);

    public virtual void Clear()
    {
      throw new InvalidOperationException("Cannot change the contents of a read-only dictionary");
    }

    public virtual ICollection Values => this.m_dictionary.Values;

    public void Add(object key, object value)
    {
      throw new InvalidOperationException("Cannot change the contents of a read-only dictionary");
    }

    public virtual ICollection Keys => this.m_dictionary.Keys;

    public virtual bool IsFixedSize => false;

    public virtual bool IsSynchronized => false;

    public virtual int Count => this.m_dictionary.Count;

    public virtual void CopyTo(Array array, int index)
    {
      if (this.m_dictionary == null)
        return;
      this.m_dictionary.CopyTo(array, index);
    }

    public virtual object SyncRoot => (object) this;

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public virtual object Clone()
    {
      ReadOnlyDictionary readOnlyDictionary = (ReadOnlyDictionary) this.MemberwiseClone();
      Hashtable hashtable = new Hashtable();
      IDictionaryEnumerator enumerator = this.m_dictionary.GetEnumerator();
      while (enumerator.MoveNext())
        hashtable.Add(Convert.Clone(enumerator.Key), Convert.Clone(enumerator.Value));
      readOnlyDictionary.m_dictionary = hashtable;
      return (object) readOnlyDictionary;
    }

    private class Names
    {
      internal const string COUNT = "CT";
      internal const string KEY = "KY";
      internal const string VALUE = "VA";
    }
  }
}
