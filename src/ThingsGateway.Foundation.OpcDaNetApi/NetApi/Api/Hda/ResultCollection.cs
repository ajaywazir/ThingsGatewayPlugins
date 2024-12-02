

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class ResultCollection : ItemIdentifier, ICollection, IEnumerable, ICloneable, IList
    {
        private ArrayList m_results = new ArrayList();

        public Result this[int index]
        {
            get => (Result)m_results[index];
            set => m_results[index] = (object)value;
        }

        public ResultCollection()
        {
        }

        public ResultCollection(ItemIdentifier item)
          : base(item)
        {
        }

        public ResultCollection(ResultCollection item)
          : base((ItemIdentifier)item)
        {
            m_results = new ArrayList(item.m_results.Count);
            foreach (Result result in item.m_results)
                m_results.Add(result.Clone());
        }

        public override object Clone()
        {
            ResultCollection resultCollection = (ResultCollection)base.Clone();
            resultCollection.m_results = new ArrayList(m_results.Count);
            foreach (ResultCollection result in m_results)
                resultCollection.m_results.Add(result.Clone());
            return (object)resultCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_results == null ? 0 : m_results.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_results == null)
                return;
            m_results.CopyTo(array, index);
        }

        public void CopyTo(Result[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_results.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_results[index];
            set
            {
                m_results[index] = typeof(Result).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add Result objects into the collection.");
            }
        }

        public void RemoveAt(int index) => m_results.RemoveAt(index);

        public void Insert(int index, object value)
        {
            if (!typeof(Result).IsInstanceOfType(value))
                throw new ArgumentException("May only add Result objects into the collection.");
            m_results.Insert(index, value);
        }

        public void Remove(object value) => m_results.Remove(value);

        public bool Contains(object value) => m_results.Contains(value);

        public void Clear() => m_results.Clear();

        public int IndexOf(object value) => m_results.IndexOf(value);

        public int Add(object value)
        {
            return typeof(Result).IsInstanceOfType(value) ? m_results.Add(value) : throw new ArgumentException("May only add Result objects into the collection.");
        }

        public bool IsFixedSize => false;

        public void Insert(int index, Result value) => Insert(index, (object)value);

        public void Remove(Result value) => Remove((object)value);

        public bool Contains(Result value) => Contains((object)value);

        public int IndexOf(Result value) => IndexOf((object)value);

        public int Add(Result value) => Add((object)value);
    }
}
