

using System;
using System.Collections;


namespace Opc.Da
{
    [Serializable]
    public class SubscriptionCollection : ICollection, IEnumerable, ICloneable, IList
    {
        private ArrayList m_subscriptions = new ArrayList();

        public Subscription this[int index]
        {
            get => (Subscription)m_subscriptions[index];
            set => m_subscriptions[index] = (object)value;
        }

        public SubscriptionCollection()
        {
        }

        public SubscriptionCollection(SubscriptionCollection subscriptions)
        {
            if (subscriptions == null)
                return;
            foreach (Subscription subscription in subscriptions)
                Add(subscription);
        }

        public virtual object Clone()
        {
            SubscriptionCollection subscriptionCollection = (SubscriptionCollection)MemberwiseClone();
            subscriptionCollection.m_subscriptions = new ArrayList();
            foreach (Subscription subscription in m_subscriptions)
                subscriptionCollection.m_subscriptions.Add(subscription.Clone());
            return (object)subscriptionCollection;
        }

        public bool IsSynchronized => false;

        public int Count => m_subscriptions == null ? 0 : m_subscriptions.Count;

        public void CopyTo(Array array, int index)
        {
            if (m_subscriptions == null)
                return;
            m_subscriptions.CopyTo(array, index);
        }

        public void CopyTo(Subscription[] array, int index) => CopyTo((Array)array, index);

        public object SyncRoot => (object)this;

        public IEnumerator GetEnumerator() => m_subscriptions.GetEnumerator();

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => m_subscriptions[index];
            set
            {
                m_subscriptions[index] = typeof(Subscription).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add Subscription objects into the collection.");
            }
        }

        public void RemoveAt(int index) => m_subscriptions.RemoveAt(index);

        public void Insert(int index, object value)
        {
            if (!typeof(Subscription).IsInstanceOfType(value))
                throw new ArgumentException("May only add Subscription objects into the collection.");
            m_subscriptions.Insert(index, value);
        }

        public void Remove(object value) => m_subscriptions.Remove(value);

        public bool Contains(object value) => m_subscriptions.Contains(value);

        public void Clear() => m_subscriptions.Clear();

        public int IndexOf(object value) => m_subscriptions.IndexOf(value);

        public int Add(object value)
        {
            return typeof(Subscription).IsInstanceOfType(value) ? m_subscriptions.Add(value) : throw new ArgumentException("May only add Subscription objects into the collection.");
        }

        public bool IsFixedSize => false;

        public void Insert(int index, Subscription value) => Insert(index, (object)value);

        public void Remove(Subscription value) => Remove((object)value);

        public bool Contains(Subscription value) => Contains((object)value);

        public int IndexOf(Subscription value) => IndexOf((object)value);

        public int Add(Subscription value) => Add((object)value);
    }
}
