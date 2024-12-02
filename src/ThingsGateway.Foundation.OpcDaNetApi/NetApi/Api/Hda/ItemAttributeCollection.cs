

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class ItemAttributeCollection :
      ItemIdentifier,
      IResult,
      IActualTime,
      ICollection,
      IEnumerable,
      IList
    {
        private DateTime m_startTime = DateTime.MinValue;
        private DateTime m_endTime = DateTime.MinValue;
        private ArrayList m_attributes = new ArrayList();
        private ResultID m_resultID = ResultID.S_OK;
        private string m_diagnosticInfo;

        public AttributeValueCollection this[int index]
        {
            get => (AttributeValueCollection)m_attributes[index];
            set => m_attributes[index] = (object)value;
        }

        public ItemAttributeCollection()
        {
        }

        public ItemAttributeCollection(ItemIdentifier item)
          : base(item)
        {
        }

        public ItemAttributeCollection(ItemAttributeCollection item)
          : base((ItemIdentifier)item)
        {
            m_attributes = new ArrayList(item.m_attributes.Count);
            foreach (AttributeValueCollection attribute in item.m_attributes)
            {
                if (attribute != null)
                    m_attributes.Add(attribute.Clone());
            }
        }

        public ResultID ResultID
        {
            get => m_resultID;
            set => m_resultID = value;
        }

        public string DiagnosticInfo
        {
            get => m_diagnosticInfo;
            set => m_diagnosticInfo = value;
        }

        public DateTime StartTime
        {
            get => m_startTime;
            set => m_startTime = value;
        }

        public DateTime EndTime
        {
            get => m_endTime;
            set => m_endTime = value;
        }

        public override object Clone()
        {
            ItemAttributeCollection attributeCollection = (ItemAttributeCollection)base.Clone();
            attributeCollection.m_attributes = new ArrayList(m_attributes.Count);
            foreach (AttributeValueCollection attribute in m_attributes)
                attributeCollection.m_attributes.Add(attribute.Clone());
            return (object)attributeCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_attributes == null ? 0 : m_attributes.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_attributes == null)
                return;
            m_attributes.CopyTo(array, index);
        }

        public void CopyTo(AttributeValueCollection[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_attributes.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_attributes[index];
            set
            {
                m_attributes[index] = typeof(AttributeValueCollection).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
            }
        }

        public void RemoveAt(int index) => m_attributes.RemoveAt(index);

        public void Insert(int index, object value)
        {
            if (!typeof(AttributeValueCollection).IsInstanceOfType(value))
                throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
            m_attributes.Insert(index, value);
        }

        public void Remove(object value) => m_attributes.Remove(value);

        public bool Contains(object value) => m_attributes.Contains(value);

        public void Clear() => m_attributes.Clear();

        public int IndexOf(object value) => m_attributes.IndexOf(value);

        public int Add(object value)
        {
            return typeof(AttributeValueCollection).IsInstanceOfType(value) ? m_attributes.Add(value) : throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
        }

        public bool IsFixedSize => false;

        public void Insert(int index, AttributeValueCollection value)
        {
            Insert(index, (object)value);
        }

        public void Remove(AttributeValueCollection value) => Remove((object)value);

        public bool Contains(AttributeValueCollection value) => Contains((object)value);

        public int IndexOf(AttributeValueCollection value) => IndexOf((object)value);

        public int Add(AttributeValueCollection value) => Add((object)value);
    }
}
