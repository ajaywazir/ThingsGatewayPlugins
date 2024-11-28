

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
      get => (Subscription) this.m_subscriptions[index];
      set => this.m_subscriptions[index] = (object) value;
    }

    public SubscriptionCollection()
    {
    }

    public SubscriptionCollection(SubscriptionCollection subscriptions)
    {
      if (subscriptions == null)
        return;
      foreach (Subscription subscription in subscriptions)
        this.Add(subscription);
    }

    public virtual object Clone()
    {
      SubscriptionCollection subscriptionCollection = (SubscriptionCollection) this.MemberwiseClone();
      subscriptionCollection.m_subscriptions = new ArrayList();
      foreach (Subscription subscription in this.m_subscriptions)
        subscriptionCollection.m_subscriptions.Add(subscription.Clone());
      return (object) subscriptionCollection;
    }

    public bool IsSynchronized => false;

    public int Count => this.m_subscriptions == null ? 0 : this.m_subscriptions.Count;

    public void CopyTo(Array array, int index)
    {
      if (this.m_subscriptions == null)
        return;
      this.m_subscriptions.CopyTo(array, index);
    }

    public void CopyTo(Subscription[] array, int index) => this.CopyTo((Array) array, index);

    public object SyncRoot => (object) this;

    public IEnumerator GetEnumerator() => this.m_subscriptions.GetEnumerator();

    public bool IsReadOnly => false;

    object IList.this[int index]
    {
      get => this.m_subscriptions[index];
      set
      {
        this.m_subscriptions[index] = typeof (Subscription).IsInstanceOfType(value) ? value : throw new ArgumentException("May only add Subscription objects into the collection.");
      }
    }

    public void RemoveAt(int index) => this.m_subscriptions.RemoveAt(index);

    public void Insert(int index, object value)
    {
      if (!typeof (Subscription).IsInstanceOfType(value))
        throw new ArgumentException("May only add Subscription objects into the collection.");
      this.m_subscriptions.Insert(index, value);
    }

    public void Remove(object value) => this.m_subscriptions.Remove(value);

    public bool Contains(object value) => this.m_subscriptions.Contains(value);

    public void Clear() => this.m_subscriptions.Clear();

    public int IndexOf(object value) => this.m_subscriptions.IndexOf(value);

    public int Add(object value)
    {
      return typeof (Subscription).IsInstanceOfType(value) ? this.m_subscriptions.Add(value) : throw new ArgumentException("May only add Subscription objects into the collection.");
    }

    public bool IsFixedSize => false;

    public void Insert(int index, Subscription value) => this.Insert(index, (object) value);

    public void Remove(Subscription value) => this.Remove((object) value);

    public bool Contains(Subscription value) => this.Contains((object) value);

    public int IndexOf(Subscription value) => this.IndexOf((object) value);

    public int Add(Subscription value) => this.Add((object) value);
  }
}
