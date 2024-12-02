

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
            get => m_array[index];
            set => m_array[index] = value;
        }

        public virtual System.Array ToArray() => m_array.ToArray(m_elementType);

        public virtual void AddRange(ICollection collection)
        {
            if (collection == null)
                return;
            foreach (object element in (IEnumerable)collection)
                ValidateElement(element);
            m_array.AddRange(collection);
        }

        protected WriteableCollection(ICollection array, System.Type elementType)
        {
            m_array = array == null ? new ArrayList() : new ArrayList(array);
            m_elementType = typeof(object);
            if (!(elementType != (System.Type)null))
                return;
            foreach (object element in m_array)
                ValidateElement(element);
            m_elementType = elementType;
        }

        protected virtual ArrayList Array
        {
            get => m_array;
            set
            {
                m_array = value;
                if (m_array != null)
                    return;
                m_array = new ArrayList();
            }
        }

        protected virtual System.Type ElementType
        {
            get => m_elementType;
            set
            {
                foreach (object element in m_array)
                    ValidateElement(element);
                m_elementType = value;
            }
        }

        protected virtual void ValidateElement(object element)
        {
            if (element == null)
                throw new ArgumentException(string.Format("The value '{0}' cannot be added to the collection.", element));
            if (!m_elementType.IsInstanceOfType(element))
                throw new ArgumentException(string.Format("A value with type '{0}' cannot be added to the collection.", (object)element.GetType()));
        }

        protected WriteableCollection(SerializationInfo info, StreamingContext context)
        {
            m_elementType = (System.Type)info.GetValue("ET", typeof(System.Type));
            int capacity = (int)info.GetValue("CT", typeof(int));
            m_array = new ArrayList(capacity);
            for (int index = 0; index < capacity; ++index)
                m_array.Add(info.GetValue("EL" + index.ToString(), typeof(object)));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ET", (object)m_elementType);
            info.AddValue("CT", m_array.Count);
            for (int index = 0; index < m_array.Count; ++index)
                info.AddValue("EL" + index.ToString(), m_array[index]);
        }

        public virtual bool IsSynchronized => false;

        public virtual int Count => m_array.Count;

        public virtual void CopyTo(System.Array array, int index)
        {
            if (m_array == null)
                return;
            m_array.CopyTo(array, index);
        }

        public virtual object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_array.GetEnumerator();

        public virtual bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }

        public virtual void RemoveAt(int index) => m_array.RemoveAt(index);

        public virtual void Insert(int index, object value)
        {
            ValidateElement(value);
            m_array.Insert(index, value);
        }

        public virtual void Remove(object value) => m_array.Remove(value);

        public virtual bool Contains(object value) => m_array.Contains(value);

        public virtual void Clear() => m_array.Clear();

        public virtual int IndexOf(object value) => m_array.IndexOf(value);

        public virtual int Add(object value)
        {
            ValidateElement(value);
            return m_array.Add(value);
        }

        public virtual bool IsFixedSize => false;

        public virtual object Clone()
        {
            WriteableCollection writeableCollection = (WriteableCollection)MemberwiseClone();
            writeableCollection.m_array = new ArrayList();
            for (int index = 0; index < m_array.Count; ++index)
                writeableCollection.Add(Convert.Clone(m_array[index]));
            return (object)writeableCollection;
        }

        private sealed class Names
        {
            internal const string COUNT = "CT";
            internal const string ELEMENT = "EL";
            internal const string ELEMENT_TYPE = "ET";
        }
    }
}
