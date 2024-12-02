

using System;
using System.Collections;


namespace Opc.Hda
{
    [Serializable]
    public class ItemValueCollection :
      Opc.Hda.Item,
      IResult,
      IActualTime,
      ICollection,
      IEnumerable,
      ICloneable,
      IList
    {
        private DateTime m_startTime = DateTime.MinValue;
        private DateTime m_endTime = DateTime.MinValue;
        private ArrayList m_values = new ArrayList();
        private ResultID m_resultID = ResultID.S_OK;
        private string m_diagnosticInfo;

        public ItemValue this[int index]
        {
            get => (ItemValue)m_values[index];
            set => m_values[index] = (object)value;
        }

        public ItemValueCollection()
        {
        }

        public ItemValueCollection(ItemIdentifier item)
          : base(item)
        {
        }

        public ItemValueCollection(Opc.Hda.Item item)
          : base(item)
        {
        }

        public ItemValueCollection(ItemValueCollection item)
          : base((Opc.Hda.Item)item)
        {
            m_values = new ArrayList(item.m_values.Count);
            foreach (ItemValue itemValue in item.m_values)
            {
                if (itemValue != null)
                    m_values.Add(itemValue.Clone());
            }
        }

        public void AddRange(ItemValueCollection collection)
        {
            if (collection == null)
                return;
            foreach (ItemValue itemValue in collection)
            {
                if (itemValue != null)
                    m_values.Add(itemValue.Clone());
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
            ItemValueCollection itemValueCollection = (ItemValueCollection)base.Clone();
            itemValueCollection.m_values = new ArrayList(m_values.Count);
            foreach (ItemValue itemValue in m_values)
                itemValueCollection.m_values.Add(itemValue.Clone());
            return (object)itemValueCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_values == null ? 0 : m_values.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_values == null)
                return;
            m_values.CopyTo(array, index);
        }

        public void CopyTo(ItemValue[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_values.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_values[index];
            set
            {
                m_values[index] = typeof(ItemValue).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add ItemValue objects into the collection.");
            }
        }

        public void RemoveAt(int index) => m_values.RemoveAt(index);

        public void Insert(int index, object value)
        {
            if (!typeof(ItemValue).IsInstanceOfType(value))
                throw new ArgumentException("May only add ItemValue objects into the collection.");
            m_values.Insert(index, value);
        }

        public void Remove(object value) => m_values.Remove(value);

        public bool Contains(object value) => m_values.Contains(value);

        public void Clear() => m_values.Clear();

        public int IndexOf(object value) => m_values.IndexOf(value);

        public int Add(object value)
        {
            return typeof(ItemValue).IsInstanceOfType(value) ? m_values.Add(value) : throw new ArgumentException("May only add ItemValue objects into the collection.");
        }

        public bool IsFixedSize => false;

        public void Insert(int index, ItemValue value) => Insert(index, (object)value);

        public void Remove(ItemValue value) => Remove((object)value);

        public bool Contains(ItemValue value) => Contains((object)value);

        public int IndexOf(ItemValue value) => IndexOf((object)value);

        public int Add(ItemValue value) => Add((object)value);
    }
}
