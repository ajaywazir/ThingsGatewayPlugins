

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Dx
{
    [Serializable]
    public class DXConnectionCollection : ICollection, IEnumerable, ICloneable, IList, ISerializable
    {
        private ArrayList m_connections = new ArrayList();

        public DXConnection this[int index] => (DXConnection)m_connections[index];

        public DXConnection[] ToArray()
        {
            return (DXConnection[])m_connections.ToArray(typeof(DXConnection));
        }

        internal DXConnectionCollection()
        {
        }

        internal DXConnectionCollection(ICollection connections)
        {
            if (connections == null)
                return;
            foreach (DXConnection connection in (IEnumerable)connections)
                m_connections.Add((object)connection);
        }

        protected DXConnectionCollection(SerializationInfo info, StreamingContext context)
        {
            DXConnection[] dxConnectionArray = (DXConnection[])info.GetValue("Connections", typeof(DXConnection[]));
            if (dxConnectionArray == null)
                return;
            foreach (object obj in dxConnectionArray)
                m_connections.Add(obj);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            DXConnection[] dxConnectionArray = (DXConnection[])null;
            if (m_connections.Count > 0)
            {
                dxConnectionArray = new DXConnection[m_connections.Count];
                for (int index = 0; index < dxConnectionArray.Length; ++index)
                    dxConnectionArray[index] = (DXConnection)m_connections[index];
            }
            info.AddValue("Connections", (object)dxConnectionArray);
        }

        public virtual object Clone()
        {
            DXConnectionCollection connectionCollection = (DXConnectionCollection)MemberwiseClone();
            connectionCollection.m_connections = new ArrayList();
            foreach (DXConnection connection in m_connections)
                connectionCollection.m_connections.Add(connection.Clone());
            return (object)connectionCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_connections == null ? 0 : m_connections.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_connections == null)
                return;
            m_connections.CopyTo(array, index);
        }

        public void CopyTo(DXConnection[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_connections.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_connections[index];
            set => Insert(index, value);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= m_connections.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            Remove(m_connections[index]);
        }

        public void Insert(int index, object value)
        {
            if (!typeof(DXConnection).IsInstanceOfType(value))
                throw new ArgumentException("May only add DXConnection objects into the collection.");
            m_connections.Insert(index, (object)(DXConnection)value);
        }

        public void Remove(object value)
        {
            if (!typeof(ItemIdentifier).IsInstanceOfType(value))
                throw new ArgumentException("May only delete Opc.Dx.ItemIdentifier obejcts from the collection.");
            foreach (ItemIdentifier connection in m_connections)
            {
                if (connection.Equals(value))
                {
                    m_connections.Remove((object)connection);
                    break;
                }
            }
        }

        public bool Contains(object value)
        {
            foreach (ItemIdentifier connection in m_connections)
            {
                if (connection.Equals(value))
                    return true;
            }
            return false;
        }

        public void Clear() => m_connections.Clear();

        public int IndexOf(object value) => m_connections.IndexOf(value);

        public int Add(object value)
        {
            Insert(m_connections.Count, value);
            return m_connections.Count - 1;
        }

        public bool IsFixedSize => false;

        public void Insert(int index, DXConnection value) => Insert(index, (object)value);

        public void Remove(DXConnection value) => Remove((object)value);

        public bool Contains(DXConnection value) => Contains((object)value);

        public int IndexOf(DXConnection value) => IndexOf((object)value);

        public int Add(DXConnection value) => Add((object)value);

        private sealed class Names
        {
            internal const string CONNECTIONS = "Connections";
        }
    }
}
