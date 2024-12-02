

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class ItemTimeCollection : ItemIdentifier, ICollection, IEnumerable, ICloneable, IList
    {
        private ArrayList m_times = new ArrayList();

        public DateTime this[int index]
        {
            get => (DateTime)m_times[index];
            set => m_times[index] = (object)value;
        }

        public ItemTimeCollection()
        {
        }

        public ItemTimeCollection(ItemIdentifier item)
          : base(item)
        {
        }

        public ItemTimeCollection(ItemTimeCollection item)
          : base((ItemIdentifier)item)
        {
            m_times = new ArrayList(item.m_times.Count);
            foreach (DateTime time in item.m_times)
                m_times.Add((object)time);
        }

        public override object Clone()
        {
            ItemTimeCollection itemTimeCollection = (ItemTimeCollection)base.Clone();
            itemTimeCollection.m_times = new ArrayList(m_times.Count);
            foreach (DateTime time in m_times)
                itemTimeCollection.m_times.Add((object)time);
            return (object)itemTimeCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_times == null ? 0 : m_times.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_times == null)
                return;
            m_times.CopyTo(array, index);
        }

        public void CopyTo(DateTime[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_times.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_times[index];
            set
            {
                m_times[index] = typeof(DateTime).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add DateTime objects into the collection.");
            }
        }

        public void RemoveAt(int index) => m_times.RemoveAt(index);

        public void Insert(int index, object value)
        {
            if (!typeof(DateTime).IsInstanceOfType(value))
                throw new ArgumentException("May only add DateTime objects into the collection.");
            m_times.Insert(index, value);
        }

        public void Remove(object value) => m_times.Remove(value);

        public bool Contains(object value) => m_times.Contains(value);

        public void Clear() => m_times.Clear();

        public int IndexOf(object value) => m_times.IndexOf(value);

        public int Add(object value)
        {
            return typeof(DateTime).IsInstanceOfType(value) ? m_times.Add(value) : throw new ArgumentException("May only add DateTime objects into the collection.");
        }

        public bool IsFixedSize => false;

        public void Insert(int index, DateTime value) => Insert(index, (object)value);

        public void Remove(DateTime value) => Remove((object)value);

        public bool Contains(DateTime value) => Contains((object)value);

        public int IndexOf(DateTime value) => IndexOf((object)value);

        public int Add(DateTime value) => Add((object)value);
    }
}
