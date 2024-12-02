

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class AttributeCollection : ICloneable, ICollection, IEnumerable
    {
        private Attribute[] m_attributes = Array.Empty<Attribute>();

        public AttributeCollection()
        {
        }

        public AttributeCollection(ICollection collection) => Init(collection);

        public Attribute this[int index]
        {
            get => m_attributes[index];
            set => m_attributes[index] = value;
        }

        public Attribute Find(int id)
        {
            foreach (Attribute attribute in m_attributes)
            {
                if (attribute.ID == id)
                    return attribute;
            }
            return (Attribute)null;
        }

        public void Init(ICollection collection)
        {
            Clear();
            if (collection == null)
                return;
            ArrayList arrayList = new ArrayList(collection.Count);
            foreach (object source in (IEnumerable)collection)
            {
                if (source.GetType() == typeof(Attribute))
                    arrayList.Add(Opc.Convert.Clone(source));
            }
            m_attributes = (Attribute[])arrayList.ToArray(typeof(Attribute));
        }

        public void Clear() => m_attributes = Array.Empty<Attribute>();

        public virtual object Clone() => (object)new AttributeCollection((ICollection)this);

        public bool IsSynchronized => false;

        public int Count => m_attributes == null ? 0 : m_attributes.Length;

        public void CopyTo(Array array, int index)
        {
            if (m_attributes == null)
                return;
            m_attributes.CopyTo(array, index);
        }

        public void CopyTo(Attribute[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_attributes.GetEnumerator();
    }
}
