

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class AggregateCollection : ICloneable, ICollection, IEnumerable
    {
        private Aggregate[] m_aggregates = Array.Empty<Aggregate>();

        public AggregateCollection()
        {
        }

        public AggregateCollection(ICollection collection) => Init(collection);

        public Aggregate this[int index]
        {
            get => m_aggregates[index];
            set => m_aggregates[index] = value;
        }

        public Aggregate Find(int id)
        {
            foreach (Aggregate aggregate in m_aggregates)
            {
                if (aggregate.ID == id)
                    return aggregate;
            }
            return (Aggregate)null;
        }

        public void Init(ICollection collection)
        {
            Clear();
            if (collection == null)
                return;
            ArrayList arrayList = new ArrayList(collection.Count);
            foreach (object source in (IEnumerable)collection)
            {
                if (source.GetType() == typeof(Aggregate))
                    arrayList.Add(Opc.Convert.Clone(source));
            }
            m_aggregates = (Aggregate[])arrayList.ToArray(typeof(Aggregate));
        }

        public void Clear() => m_aggregates = Array.Empty<Aggregate>();

        public virtual object Clone() => (object)new AggregateCollection((ICollection)this);

        public bool IsSynchronized => false;

        public int Count => m_aggregates == null ? 0 : m_aggregates.Length;

        public void CopyTo(Array array, int index)
        {
            if (m_aggregates == null)
                return;
            m_aggregates.CopyTo(array, index);
        }

        public void CopyTo(Aggregate[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_aggregates.GetEnumerator();
    }
}
