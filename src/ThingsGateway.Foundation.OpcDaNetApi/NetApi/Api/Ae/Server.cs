

using System;
using System.Runtime.Serialization;


namespace Opc.Ae
{
    [Serializable]
    public class Server : Opc.Server, IServer, Opc.IServer, IDisposable, ISerializable
    {
        private int m_filters;
        private bool m_disposing;
        private Server.SubscriptionCollection m_subscriptions = new Server.SubscriptionCollection();

        public Server(Factory factory, URL url)
          : base(factory, url)
        {
        }

        protected Server(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            int num = (int)info.GetValue("CT", typeof(int));
            m_subscriptions = new Server.SubscriptionCollection();
            for (int index = 0; index < num; ++index)
                m_subscriptions.Add((Subscription)info.GetValue("SU" + index.ToString(), typeof(Subscription)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CT", m_subscriptions.Count);
            for (int index = 0; index < m_subscriptions.Count; ++index)
                info.AddValue("SU" + index.ToString(), (object)m_subscriptions[index]);
        }

        public int AvailableFilters => m_filters;

        public Server.SubscriptionCollection Subscriptions => m_subscriptions;

        public override void Connect(URL url, ConnectData connectData)
        {
            base.Connect(url, connectData);
            if (m_subscriptions.Count == 0)
                return;
            Server.SubscriptionCollection subscriptionCollection = new Server.SubscriptionCollection();
            foreach (Subscription subscription in (ReadOnlyCollection)m_subscriptions)
            {
                try
                {
                    //subscriptionCollection.Add(EstablishSubscription(subscription));
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
            m_disposing = true;
            foreach (Subscription subscription in (ReadOnlyCollection)m_subscriptions)
                subscription.Dispose();
            m_disposing = false;
            base.Disconnect();
        }

        public ServerStatus GetStatus()
        {
            ServerStatus status = m_server != null ? ((IServer)m_server).GetStatus() : throw new NotConnectedException();
            if (status.StatusInfo == null)
                status.StatusInfo = GetString("serverState." + status.ServerState.ToString());
            return status;
        }

        public ISubscription CreateSubscription(SubscriptionState state)
        {
            ISubscription subscription1 = m_server != null ? ((IServer)m_server).CreateSubscription(state) : throw new NotConnectedException();
            if (subscription1 == null)
                return (ISubscription)null;
            Subscription subscription2 = new Subscription(this, subscription1, state);
            m_subscriptions.Add(subscription2);
            return (ISubscription)subscription2;
        }

        public int QueryAvailableFilters()
        {
            m_filters = m_server != null ? ((IServer)m_server).QueryAvailableFilters() : throw new NotConnectedException();
            return m_filters;
        }

        public Category[] QueryEventCategories(int eventType)
        {
            return m_server != null ? ((IServer)m_server).QueryEventCategories(eventType) : throw new NotConnectedException();
        }

        public string[] QueryConditionNames(int eventCategory)
        {
            return m_server != null ? ((IServer)m_server).QueryConditionNames(eventCategory) : throw new NotConnectedException();
        }

        public string[] QuerySubConditionNames(string conditionName)
        {
            return m_server != null ? ((IServer)m_server).QuerySubConditionNames(conditionName) : throw new NotConnectedException();
        }

        public string[] QueryConditionNames(string sourceName)
        {
            return m_server != null ? ((IServer)m_server).QueryConditionNames(sourceName) : throw new NotConnectedException();
        }

        public Attribute[] QueryEventAttributes(int eventCategory)
        {
            return m_server != null ? ((IServer)m_server).QueryEventAttributes(eventCategory) : throw new NotConnectedException();
        }

        public ItemUrl[] TranslateToItemIDs(
          string sourceName,
          int eventCategory,
          string conditionName,
          string subConditionName,
          int[] attributeIDs)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).TranslateToItemIDs(sourceName, eventCategory, conditionName, subConditionName, attributeIDs);
        }

        public Condition GetConditionState(string sourceName, string conditionName, int[] attributeIDs)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).GetConditionState(sourceName, conditionName, attributeIDs);
        }

        public ResultID[] EnableConditionByArea(string[] areas)
        {
            return m_server != null ? ((IServer)m_server).EnableConditionByArea(areas) : throw new NotConnectedException();
        }

        public ResultID[] DisableConditionByArea(string[] areas)
        {
            return m_server != null ? ((IServer)m_server).DisableConditionByArea(areas) : throw new NotConnectedException();
        }

        public ResultID[] EnableConditionBySource(string[] sources)
        {
            return m_server != null ? ((IServer)m_server).EnableConditionBySource(sources) : throw new NotConnectedException();
        }

        public ResultID[] DisableConditionBySource(string[] sources)
        {
            return m_server != null ? ((IServer)m_server).DisableConditionBySource(sources) : throw new NotConnectedException();
        }

        public EnabledStateResult[] GetEnableStateByArea(string[] areas)
        {
            return m_server != null ? ((IServer)m_server).GetEnableStateByArea(areas) : throw new NotConnectedException();
        }

        public EnabledStateResult[] GetEnableStateBySource(string[] sources)
        {
            return m_server != null ? ((IServer)m_server).GetEnableStateBySource(sources) : throw new NotConnectedException();
        }

        public ResultID[] AcknowledgeCondition(
          string acknowledgerID,
          string comment,
          EventAcknowledgement[] conditions)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).AcknowledgeCondition(acknowledgerID, comment, conditions);
        }

        public BrowseElement[] Browse(string areaID, BrowseType browseType, string browseFilter)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Browse(areaID, browseType, browseFilter);
        }

        public BrowseElement[] Browse(
          string areaID,
          BrowseType browseType,
          string browseFilter,
          int maxElements,
          out IBrowsePosition position)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Browse(areaID, browseType, browseFilter, maxElements, out position);
        }

        public BrowseElement[] BrowseNext(int maxElements, ref IBrowsePosition position)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).BrowseNext(maxElements, ref position);
        }

        internal void SubscriptionDisposed(Subscription subscription)
        {
            if (m_disposing)
                return;
            m_subscriptions.Remove(subscription);
        }

        //private Subscription EstablishSubscription(Subscription template)
        //{
        //}

        private  class Names
        {
            internal const string COUNT = "CT";
            internal const string SUBSCRIPTION = "SU";
        }

        public class SubscriptionCollection : ReadOnlyCollection
        {
            public new Subscription this[int index] => (Subscription)Array.GetValue(index);

            public new Subscription[] ToArray() => (Subscription[])Array;

            internal void Add(Subscription subscription)
            {
                Subscription[] subscriptionArray = new Subscription[Count + 1];
                Array.CopyTo((Array)subscriptionArray, 0);
                subscriptionArray[Count] = subscription;
                Array = (Array)subscriptionArray;
            }

            internal void Remove(Subscription subscription)
            {
                Subscription[] subscriptionArray = new Subscription[Count - 1];
                int num = 0;
                for (int index = 0; index < Array.Length; ++index)
                {
                    Subscription subscription1 = (Subscription)Array.GetValue(index);
                    if (subscription != subscription1)
                        subscriptionArray[num++] = subscription1;
                }
                Array = (Array)subscriptionArray;
            }

            internal SubscriptionCollection()
              : base((Array)new Subscription[0])
            {
            }
        }
    }
}
