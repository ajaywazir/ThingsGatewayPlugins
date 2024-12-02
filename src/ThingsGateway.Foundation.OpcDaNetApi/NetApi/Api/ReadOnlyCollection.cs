

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc
{
    [Serializable]
    public class ReadOnlyCollection : ICollection, IEnumerable, ICloneable, ISerializable
    {
        private Array m_array;

        public virtual object this[int index] => m_array.GetValue(index);

        public virtual Array ToArray() => (Array)Convert.Clone((object)m_array);

        protected ReadOnlyCollection(Array array) => Array = array;

        protected virtual Array Array
        {
            get => m_array;
            set
            {
                m_array = value;
                if (m_array != null)
                    return;
                m_array = Array.Empty<object>();
            }
        }

        protected ReadOnlyCollection(SerializationInfo info, StreamingContext context)
        {
            m_array = (Array)info.GetValue("AR", typeof(Array));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AR", (object)m_array);
        }

        public virtual bool IsSynchronized => false;

        public virtual int Count => m_array.Length;

        public virtual void CopyTo(Array array, int index)
        {
            if (m_array == null)
                return;
            m_array.CopyTo(array, index);
        }

        public virtual object SyncRoot => (object)this;

        public virtual IEnumerator GetEnumerator() => m_array.GetEnumerator();

        public virtual object Clone()
        {
            ReadOnlyCollection readOnlyCollection = (ReadOnlyCollection)MemberwiseClone();
            ArrayList arrayList = new ArrayList(m_array.Length);
            System.Type type = (System.Type)null;
            for (int index = 0; index < m_array.Length; ++index)
            {
                object obj = m_array.GetValue(index);
                if (type == (System.Type)null)
                    type = obj.GetType();
                else if (type != typeof(object))
                {
                    while (!type.IsInstanceOfType(obj))
                        type = type.BaseType;
                }
                arrayList.Add(Convert.Clone(obj));
            }
            readOnlyCollection.Array = arrayList.ToArray(type);
            return (object)readOnlyCollection;
        }

        private sealed class Names
        {
            internal const string ARRAY = "AR";
        }
    }
}
