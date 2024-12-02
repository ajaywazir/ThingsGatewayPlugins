

using System;
using System.Collections;


namespace Opc.Dx
{
    [Serializable]
    public class GeneralResponse : ICloneable, ICollection, IEnumerable
    {
        private string m_version;
        private IdentifiedResult[] m_results = Array.Empty<IdentifiedResult>();

        public string Version
        {
            get => m_version;
            set => m_version = value;
        }

        public GeneralResponse()
        {
        }

        public GeneralResponse(string version, ICollection results)
        {
            Version = version;
            Init(results);
        }

        public IdentifiedResult this[int index]
        {
            get => m_results[index];
            set => m_results[index] = value;
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

        private void Init(ICollection collection)
        {
            Clear();
            if (collection == null)
                return;
            ArrayList arrayList = new ArrayList(collection.Count);
            foreach (object o in (IEnumerable)collection)
            {
                if (typeof(IdentifiedResult).IsInstanceOfType(o))
                    arrayList.Add(((Opc.ItemIdentifier)o).Clone());
            }
            m_results = (IdentifiedResult[])arrayList.ToArray(typeof(IdentifiedResult));
        }
    }
}
