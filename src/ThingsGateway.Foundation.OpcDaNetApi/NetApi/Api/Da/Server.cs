

using System;
using System.Runtime.Serialization;


namespace Opc.Da
{
    [Serializable]
    public class Server : Opc.Server, IServer, Opc.IServer, IDisposable
    {
        private SubscriptionCollection m_subscriptions = new SubscriptionCollection();
        private int m_filters = 9;

        public Server(Factory factory, URL url)
          : base(factory, url)
        {
        }

        protected Server(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            m_filters = (int)info.GetValue(nameof(Filters), typeof(int));
            Subscription[] subscriptionArray = (Subscription[])info.GetValue("Subscription", typeof(Subscription[]));
            if (subscriptionArray == null)
                return;
            foreach (Subscription subscription in subscriptionArray)
                m_subscriptions.Add(subscription);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Filters", m_filters);
            Subscription[] subscriptionArray = (Subscription[])null;
            if (m_subscriptions.Count > 0)
            {
                subscriptionArray = new Subscription[m_subscriptions.Count];
                for (int index = 0; index < subscriptionArray.Length; ++index)
                    subscriptionArray[index] = m_subscriptions[index];
            }
            info.AddValue("Subscription", (object)subscriptionArray);
        }

        public override object Clone()
        {
            Server server = (Server)base.Clone();
            if (server.m_subscriptions != null)
            {
                SubscriptionCollection subscriptionCollection = new SubscriptionCollection();
                foreach (Subscription subscription in server.m_subscriptions)
                    subscriptionCollection.Add(subscription.Clone());
                server.m_subscriptions = subscriptionCollection;
            }
            return (object)server;
        }

        public SubscriptionCollection Subscriptions => m_subscriptions;

        public int Filters => m_filters;

        public override void Connect(URL url, ConnectData connectData)
        {
            base.Connect(url, connectData);
            if (m_subscriptions == null)
                return;
            SubscriptionCollection subscriptionCollection = new SubscriptionCollection();
            foreach (Subscription subscription in m_subscriptions)
            {
                try
                {
                    subscriptionCollection.Add(EstablishSubscription(subscription));
                }
                catch
                {
                }
            }
            m_subscriptions = subscriptionCollection;
        }

        public override void Disconnect()
        {
            if (m_server == null)
                throw new NotConnectedException();
            if (m_subscriptions != null)
            {
                foreach (Subscription subscription in m_subscriptions)
                    subscription.Dispose();
                m_subscriptions = (SubscriptionCollection)null;
            }
            base.Disconnect();
        }

        public int GetResultFilters()
        {
            m_filters = m_server != null ? ((IServer)m_server).GetResultFilters() : throw new NotConnectedException();
            return m_filters;
        }

        public void SetResultFilters(int filters)
        {
            if (m_server == null)
                throw new NotConnectedException();
            ((IServer)m_server).SetResultFilters(filters);
            m_filters = filters;
        }

        public ServerStatus GetStatus()
        {
            ServerStatus status = m_server != null ? ((IServer)m_server).GetStatus() : throw new NotConnectedException();
            if (status.StatusInfo == null)
                status.StatusInfo = GetString("serverState." + status.ServerState.ToString());
            return status;
        }

        public ItemValueResult[] Read(Item[] items)
        {
            return m_server != null ? ((IServer)m_server).Read(items) : throw new NotConnectedException();
        }

        public IdentifiedResult[] Write(ItemValue[] items)
        {
            return m_server != null ? ((IServer)m_server).Write(items) : throw new NotConnectedException();
        }

        public virtual ISubscription CreateSubscription(SubscriptionState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            ISubscription subscription1 = m_server != null ? ((IServer)m_server).CreateSubscription(state) : throw new NotConnectedException();
            subscription1.SetResultFilters(m_filters);
            SubscriptionCollection subscriptionCollection = new SubscriptionCollection();
            if (m_subscriptions != null)
            {
                foreach (Subscription subscription2 in m_subscriptions)
                    subscriptionCollection.Add(subscription2);
            }
            subscriptionCollection.Add(CreateSubscription(subscription1));
            m_subscriptions = subscriptionCollection;
            return (ISubscription)m_subscriptions[m_subscriptions.Count - 1];
        }

        protected virtual Subscription CreateSubscription(ISubscription subscription)
        {
            return new Subscription(this, subscription);
        }

        public virtual void CancelSubscription(ISubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            if (m_server == null)
                throw new NotConnectedException();
            if (!typeof(Subscription).IsInstanceOfType((object)subscription))
                throw new ArgumentException("Incorrect object type.", nameof(subscription));
            if (!Equals((object)((Subscription)subscription).Server))
                throw new ArgumentException("Unknown subscription.", nameof(subscription));
            SubscriptionCollection subscriptionCollection = new SubscriptionCollection();
            foreach (Subscription subscription1 in m_subscriptions)
            {
                if (!subscription.Equals((object)subscription1))
                    subscriptionCollection.Add(subscription1);
            }
            if (subscriptionCollection.Count == m_subscriptions.Count)
                throw new ArgumentException("Subscription not found.", nameof(subscription));
            m_subscriptions = subscriptionCollection;
            ((IServer)m_server).CancelSubscription(((Subscription)subscription).m_subscription);
        }

        public BrowseElement[] Browse(
          ItemIdentifier itemID,
          BrowseFilters filters,
          out BrowsePosition position)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Browse(itemID, filters, out position);
        }

        public BrowseElement[] BrowseNext(ref BrowsePosition position)
        {
            return m_server != null ? ((IServer)m_server).BrowseNext(ref position) : throw new NotConnectedException();
        }

        public ItemPropertyCollection[] GetProperties(
          ItemIdentifier[] itemIDs,
          PropertyID[] propertyIDs,
          bool returnValues)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).GetProperties(itemIDs, propertyIDs, returnValues);
        }

        private Subscription EstablishSubscription(Subscription template)
        {
            Subscription subscription = new Subscription(this, ((IServer)m_server).CreateSubscription(template.State));
            subscription.SetResultFilters(template.Filters);
            try
            {
                subscription.AddItems(template.Items);
            }
            catch
            {
                subscription.Dispose();
                subscription = (Subscription)null;
            }
            return subscription;
        }

        private class Names
        {
            internal const string FILTERS = "Filters";
            internal const string SUBSCRIPTIONS = "Subscription";
        }
    }
}
