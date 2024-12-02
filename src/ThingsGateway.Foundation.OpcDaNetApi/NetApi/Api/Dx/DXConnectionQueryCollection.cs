

using System;
using System.Collections;


namespace Opc.Dx
{
    public class DXConnectionQueryCollection : ICollection, IEnumerable, ICloneable, IList
    {
        private ArrayList m_queries = new ArrayList();

        public DXConnectionQuery this[int index] => (DXConnectionQuery)m_queries[index];

        public DXConnectionQuery this[string name]
        {
            get
            {
                foreach (DXConnectionQuery query in m_queries)
                {
                    if (query.Name == name)
                        return query;
                }
                return (DXConnectionQuery)null;
            }
        }

        internal DXConnectionQueryCollection()
        {
        }

        internal void Initialize(ICollection queries)
        {
            m_queries.Clear();
            if (queries == null)
                return;
            foreach (DXConnectionQuery query in (IEnumerable)queries)
                m_queries.Add((object)query);
        }

        public virtual object Clone()
        {
            DXConnectionQueryCollection connectionQueryCollection = (DXConnectionQueryCollection)MemberwiseClone();
            connectionQueryCollection.m_queries = new ArrayList();
            foreach (DXConnectionQuery query in m_queries)
                connectionQueryCollection.m_queries.Add(query.Clone());
            return (object)connectionQueryCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_queries == null ? 0 : m_queries.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_queries == null)
                return;
            m_queries.CopyTo(array, index);
        }

        public void CopyTo(DXConnectionQuery[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_queries.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_queries[index];
            set => Insert(index, value);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= m_queries.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            Remove(m_queries[index]);
        }

        public void Insert(int index, object value)
        {
            if (!typeof(DXConnectionQuery).IsInstanceOfType(value))
                throw new ArgumentException("May only add DXConnectionQuery objects into the collection.");
            m_queries.Insert(index, value);
        }

        public void Remove(object value)
        {
            if (!typeof(DXConnectionQuery).IsInstanceOfType(value))
                throw new ArgumentException("May only delete DXConnectionQuery obejcts from the collection.");
            m_queries.Remove(value);
        }

        public bool Contains(object value)
        {
            foreach (ItemIdentifier query in m_queries)
            {
                if (query.Equals(value))
                    return true;
            }
            return false;
        }

        public void Clear() => m_queries.Clear();

        public int IndexOf(object value) => m_queries.IndexOf(value);

        public int Add(object value)
        {
            Insert(m_queries.Count, value);
            return m_queries.Count - 1;
        }

        public bool IsFixedSize => false;

        public void Insert(int index, DXConnectionQuery value) => Insert(index, (object)value);

        public void Remove(DXConnectionQuery value) => Remove((object)value);

        public bool Contains(DXConnectionQuery value) => Contains((object)value);

        public int IndexOf(DXConnectionQuery value) => IndexOf((object)value);

        public int Add(DXConnectionQuery value) => Add((object)value);
    }
}
