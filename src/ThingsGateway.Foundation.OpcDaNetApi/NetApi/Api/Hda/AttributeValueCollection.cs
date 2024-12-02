

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class AttributeValueCollection : IResult, ICollection, IEnumerable, ICloneable, IList
    {
        private int m_attributeID;
        private ResultID m_resultID = ResultID.S_OK;
        private string m_diagnosticInfo;
        private ArrayList m_values = new ArrayList();

        public int AttributeID
        {
            get => m_attributeID;
            set => m_attributeID = value;
        }

        public AttributeValue this[int index]
        {
            get => (AttributeValue)m_values[index];
            set => m_values[index] = (object)value;
        }

        public AttributeValueCollection()
        {
        }

        public AttributeValueCollection(Attribute attribute) => m_attributeID = attribute.ID;

        public AttributeValueCollection(AttributeValueCollection collection)
        {
            m_values = new ArrayList(collection.m_values.Count);
            foreach (AttributeValue attributeValue in collection.m_values)
                m_values.Add(attributeValue.Clone());
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

        public virtual object Clone()
        {
            AttributeValueCollection attributeValueCollection = (AttributeValueCollection)MemberwiseClone();
            attributeValueCollection.m_values = new ArrayList(m_values.Count);
            foreach (AttributeValue attributeValue in m_values)
                attributeValueCollection.m_values.Add(attributeValue.Clone());
            return (object)attributeValueCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_values == null ? 0 : m_values.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_values == null)
                return;
            m_values.CopyTo(array, index);
        }

        public void CopyTo(AttributeValue[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_values.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_values[index];
            set
            {
                m_values[index] = typeof(AttributeValue).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add AttributeValue objects into the collection.");
            }
        }

        public void RemoveAt(int index) => m_values.RemoveAt(index);

        public void Insert(int index, object value)
        {
            if (!typeof(AttributeValue).IsInstanceOfType(value))
                throw new ArgumentException("May only add AttributeValue objects into the collection.");
            m_values.Insert(index, value);
        }

        public void Remove(object value) => m_values.Remove(value);

        public bool Contains(object value) => m_values.Contains(value);

        public void Clear() => m_values.Clear();

        public int IndexOf(object value) => m_values.IndexOf(value);

        public int Add(object value)
        {
            return typeof(AttributeValue).IsInstanceOfType(value) ? m_values.Add(value) : throw new ArgumentException("May only add AttributeValue objects into the collection.");
        }

        public bool IsFixedSize => false;

        public void Insert(int index, AttributeValue value) => Insert(index, (object)value);

        public void Remove(AttributeValue value) => Remove((object)value);

        public bool Contains(AttributeValue value) => Contains((object)value);

        public int IndexOf(AttributeValue value) => IndexOf((object)value);

        public int Add(AttributeValue value) => Add((object)value);
    }
}
