

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Da
{
    [Serializable]
    public class Subscription : ISubscription, IDisposable, ISerializable, ICloneable
    {
        internal Server m_server;
        internal ISubscription m_subscription;
        private SubscriptionState m_state = new SubscriptionState();
        private Item[] m_items;
        private bool m_enabled = true;
        private int m_filters = 9;

        public Subscription(Server server, ISubscription subscription)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            m_server = server;
            m_subscription = subscription;
            GetResultFilters();
            GetState();
        }

        public void Dispose()
        {
            if (m_subscription == null)
                return;
            m_subscription.Dispose();
            m_server = (Server)null;
            m_subscription = (ISubscription)null;
            m_items = (Item[])null;
        }

        protected Subscription(SerializationInfo info, StreamingContext context)
        {
            m_state = (SubscriptionState)info.GetValue(nameof(State), typeof(SubscriptionState));
            m_filters = (int)info.GetValue(nameof(Filters), typeof(int));
            m_items = (Item[])info.GetValue(nameof(Items), typeof(Item[]));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("State", (object)m_state);
            info.AddValue("Filters", m_filters);
            info.AddValue("Items", (object)m_items);
        }

        public virtual object Clone()
        {
            Subscription subscription = (Subscription)MemberwiseClone();
            subscription.m_server = (Server)null;
            subscription.m_subscription = (ISubscription)null;
            subscription.m_state = (SubscriptionState)m_state.Clone();
            subscription.m_state.ServerHandle = (object)null;
            subscription.m_state.Active = false;
            if (subscription.m_items != null)
            {
                ArrayList arrayList = new ArrayList();
                foreach (Item obj in subscription.m_items)
                    arrayList.Add(obj.Clone());
                subscription.m_items = (Item[])arrayList.ToArray(typeof(Item));
            }
            return (object)subscription;
        }

        public Server Server => m_server;

        public string Name => m_state.Name;

        public object ClientHandle => m_state.ClientHandle;

        public object ServerHandle => m_state.ServerHandle;

        public bool Active => m_state.Active;

        public bool Enabled => m_enabled;

        public string Locale => m_state.Locale;

        public int Filters => m_filters;

        public SubscriptionState State => (SubscriptionState)m_state.Clone();

        public Item[] Items
        {
            get
            {
                if (m_items == null)
                    return Array.Empty<Item>();
                Item[] items = new Item[m_items.Length];
                for (int index = 0; index < m_items.Length; ++index)
                    items[index] = (Item)m_items[index].Clone();
                return items;
            }
        }

        public event DataChangedEventHandler DataChanged
        {
            add => m_subscription.DataChanged += value;
            remove => m_subscription.DataChanged -= value;
        }

        public int GetResultFilters()
        {
            m_filters = m_subscription.GetResultFilters();
            return m_filters;
        }

        public void SetResultFilters(int filters)
        {
            m_subscription.SetResultFilters(filters);
            m_filters = filters;
        }

        public SubscriptionState GetState()
        {
            m_state = m_subscription.GetState();
            return m_state;
        }

        public SubscriptionState ModifyState(int masks, SubscriptionState state)
        {
            m_state = m_subscription.ModifyState(masks, state);
            return m_state;
        }

        public virtual ItemResult[] AddItems(Item[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<ItemResult>();
            ItemResult[] itemResultArray = m_subscription.AddItems(items);
            if (itemResultArray == null || itemResultArray.Length == 0)
                throw new InvalidResponseException();
            ArrayList arrayList = new ArrayList();
            if (m_items != null)
                arrayList.AddRange((ICollection)m_items);
            for (int index = 0; index < itemResultArray.Length; ++index)
            {
                if (!itemResultArray[index].ResultID.Failed())
                {
                    Item obj = new Item((Item)itemResultArray[index]);
                    obj.ItemName = items[index].ItemName;
                    obj.ItemPath = items[index].ItemPath;
                    obj.ClientHandle = items[index].ClientHandle;
                    arrayList.Add((object)obj);
                }
            }
            m_items = (Item[])arrayList.ToArray(typeof(Item));
            GetState();
            return itemResultArray;
        }

        public virtual ItemResult[] ModifyItems(int masks, Item[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<ItemResult>();
            ItemResult[] itemResultArray = m_subscription.ModifyItems(masks, items);
            if (itemResultArray == null || itemResultArray.Length == 0)
                throw new InvalidResponseException();
            for (int index1 = 0; index1 < itemResultArray.Length; ++index1)
            {
                if (!itemResultArray[index1].ResultID.Failed())
                {
                    for (int index2 = 0; index2 < m_items.Length; ++index2)
                    {
                        if (m_items[index2].ServerHandle.Equals(items[index1].ServerHandle))
                        {
                            Item obj = new Item((Item)itemResultArray[index1]);
                            obj.ItemName = m_items[index2].ItemName;
                            obj.ItemPath = m_items[index2].ItemPath;
                            obj.ClientHandle = m_items[index2].ClientHandle;
                            m_items[index2] = obj;
                            break;
                        }
                    }
                }
            }
            GetState();
            return itemResultArray;
        }

        public virtual IdentifiedResult[] RemoveItems(ItemIdentifier[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                return Array.Empty<IdentifiedResult>();
            IdentifiedResult[] identifiedResultArray = m_subscription.RemoveItems(items);
            if (identifiedResultArray == null || identifiedResultArray.Length == 0)
                throw new InvalidResponseException();
            ArrayList arrayList = new ArrayList();
            foreach (Item obj in m_items)
            {
                bool flag = false;
                for (int index = 0; index < identifiedResultArray.Length; ++index)
                {
                    if (obj.ServerHandle.Equals(items[index].ServerHandle))
                    {
                        flag = identifiedResultArray[index].ResultID.Succeeded();
                        break;
                    }
                }
                if (!flag)
                    arrayList.Add((object)obj);
            }
            m_items = (Item[])arrayList.ToArray(typeof(Item));
            GetState();
            return identifiedResultArray;
        }

        public ItemValueResult[] Read(Item[] items) => m_subscription.Read(items);

        public IdentifiedResult[] Write(ItemValue[] items) => m_subscription.Write(items);

        public IdentifiedResult[] Read(
          Item[] items,
          object requestHandle,
          ReadCompleteEventHandler callback,
          out IRequest request)
        {
            return m_subscription.Read(items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] Write(
          ItemValue[] items,
          object requestHandle,
          WriteCompleteEventHandler callback,
          out IRequest request)
        {
            return m_subscription.Write(items, requestHandle, callback, out request);
        }

        public void Cancel(IRequest request, CancelCompleteEventHandler callback)
        {
            m_subscription.Cancel(request, callback);
        }

        public void Refresh() => m_subscription.Refresh();

        public void Refresh(object requestHandle, out IRequest request)
        {
            m_subscription.Refresh(requestHandle, out request);
        }

        public void SetEnabled(bool enabled)
        {
            m_subscription.SetEnabled(enabled);
            m_enabled = enabled;
        }

        public bool GetEnabled()
        {
            m_enabled = m_subscription.GetEnabled();
            return m_enabled;
        }

        private sealed class Names
        {
            internal const string STATE = "State";
            internal const string FILTERS = "Filters";
            internal const string ITEMS = "Items";
        }
    }
}
