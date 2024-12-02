

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class BrowseFilterCollection : ItemIdentifier, ICollection, IEnumerable
    {
        private BrowseFilter[] m_filters = Array.Empty<BrowseFilter>();

        public BrowseFilterCollection()
        {
        }

        public BrowseFilterCollection(ICollection collection) => Init(collection);

        public BrowseFilter this[int index]
        {
            get => m_filters[index];
            set => m_filters[index] = value;
        }

        public BrowseFilter Find(int id)
        {
            foreach (BrowseFilter filter in m_filters)
            {
                if (filter.AttributeID == id)
                    return filter;
            }
            return (BrowseFilter)null;
        }

        public void Init(ICollection collection)
        {
            Clear();
            if (collection == null)
                return;
            ArrayList arrayList = new ArrayList(collection.Count);
            foreach (object source in (IEnumerable)collection)
            {
                if (source.GetType() == typeof(BrowseFilter))
                    arrayList.Add(Opc.Convert.Clone(source));
            }
            m_filters = (BrowseFilter[])arrayList.ToArray(typeof(BrowseFilter));
        }

        public void Clear() => m_filters = Array.Empty<BrowseFilter>();

        public override object Clone() => (object)new BrowseFilterCollection((ICollection)this);

        public bool IsSynchronized => false;

        public int Count => m_filters == null ? 0 : m_filters.Length;

        public void CopyTo(Array array, int index)
        {
            if (m_filters == null)
                return;
            m_filters.CopyTo(array, index);
        }

        public void CopyTo(BrowseFilter[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_filters.GetEnumerator();
    }
}
