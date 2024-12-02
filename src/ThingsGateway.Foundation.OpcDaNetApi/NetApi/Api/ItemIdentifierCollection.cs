

using System;
using System.Collections;


namespace Opc
{
    [Serializable]
    public class ItemIdentifierCollection : ICloneable, ICollection, IEnumerable
    {
        private ItemIdentifier[] m_itemIDs = Array.Empty<ItemIdentifier>();

        public ItemIdentifierCollection()
        {
        }

        public ItemIdentifierCollection(ICollection collection) => Init(collection);

        public ItemIdentifier this[int index]
        {
            get => m_itemIDs[index];
            set => m_itemIDs[index] = value;
        }

        public void Init(ICollection collection)
        {
            Clear();
            if (collection == null)
                return;
            ArrayList arrayList = new ArrayList(collection.Count);
            foreach (object o in (IEnumerable)collection)
            {
                if (typeof(ItemIdentifier).IsInstanceOfType(o))
                    arrayList.Add(((ItemIdentifier)o).Clone());
            }
            m_itemIDs = (ItemIdentifier[])arrayList.ToArray(typeof(ItemIdentifier));
        }

        public void Clear() => m_itemIDs = Array.Empty<ItemIdentifier>();

        public virtual object Clone() => (object)new ItemIdentifierCollection((ICollection)this);

        public bool IsSynchronized => false;

        public int Count => m_itemIDs == null ? 0 : m_itemIDs.Length;

        public void CopyTo(Array array, int index)
        {
            if (m_itemIDs == null)
                return;
            m_itemIDs.CopyTo(array, index);
        }

        public void CopyTo(ItemIdentifier[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_itemIDs.GetEnumerator();
    }
}
