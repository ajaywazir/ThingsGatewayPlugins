

using System;
using System.Collections;


namespace Opc
{
    [Serializable]
    public class IdentifiedResultCollection : ICloneable, ICollection, IEnumerable
    {
        private IdentifiedResult[] m_results = Array.Empty<IdentifiedResult>();

        public IdentifiedResult this[int index]
        {
            get => m_results[index];
            set => m_results[index] = value;
        }

        public IdentifiedResultCollection()
        {
        }

        public IdentifiedResultCollection(ICollection collection) => Init(collection);

        public void Init(ICollection collection)
        {
            Clear();
            if (collection == null)
                return;
            ArrayList arrayList = new ArrayList(collection.Count);
            foreach (object o in (IEnumerable)collection)
            {
                if (typeof(IdentifiedResult).IsInstanceOfType(o))
                    arrayList.Add(((ItemIdentifier)o).Clone());
            }
            m_results = (IdentifiedResult[])arrayList.ToArray(typeof(IdentifiedResult));
        }

        public void Clear() => m_results = Array.Empty<IdentifiedResult>();

        public virtual object Clone() => (object)new IdentifiedResultCollection((ICollection)this);

        public bool IsSynchronized => false;

        public int Count => m_results == null ? 0 : m_results.Length;

        public void CopyTo(Array array, int index)
        {
            if (m_results == null)
                return;
            m_results.CopyTo(array, index);
        }

        public void CopyTo(IdentifiedResult[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_results.GetEnumerator();
    }
}
