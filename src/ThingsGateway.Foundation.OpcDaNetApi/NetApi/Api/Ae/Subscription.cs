

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Ae
{
    [Serializable]
    public class Subscription : ISubscription, IDisposable, ISerializable, ICloneable
    {
        private bool m_disposed;
        private Server m_server;
        private ISubscription m_subscription;
        private SubscriptionState m_state = new SubscriptionState();
        private string m_name;
        private SubscriptionFilters m_filters = new SubscriptionFilters();
        private Subscription.CategoryCollection m_categories = new Subscription.CategoryCollection();
        private Subscription.StringCollection m_areas = new Subscription.StringCollection();
        private Subscription.StringCollection m_sources = new Subscription.StringCollection();
        private Subscription.AttributeDictionary m_attributes = new Subscription.AttributeDictionary();

        public Subscription(Server server, ISubscription subscription, SubscriptionState state)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            m_server = server;
            m_subscription = subscription;
            m_state = (SubscriptionState)state.Clone();
            m_name = state.Name;
        }

        ~Subscription() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;
            if (disposing && m_subscription != null)
            {
                m_server.SubscriptionDisposed(this);
                m_subscription.Dispose();
            }
            m_disposed = true;
        }

        protected Subscription(SerializationInfo info, StreamingContext context)
        {
            m_state = (SubscriptionState)info.GetValue("ST", typeof(SubscriptionState));
            m_filters = (SubscriptionFilters)info.GetValue("FT", typeof(SubscriptionFilters));
            m_attributes = (Subscription.AttributeDictionary)info.GetValue("AT", typeof(Subscription.AttributeDictionary));
            m_name = m_state.Name;
            m_categories = new Subscription.CategoryCollection(m_filters.Categories.ToArray());
            m_areas = new Subscription.StringCollection(m_filters.Areas.ToArray());
            m_sources = new Subscription.StringCollection(m_filters.Sources.ToArray());
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ST", (object)m_state);
            info.AddValue("FT", (object)m_filters);
            info.AddValue("AT", (object)m_attributes);
        }

        public virtual object Clone() => (object)(Subscription)MemberwiseClone();

        public Server Server => m_server;

        public string Name => m_state.Name;

        public object ClientHandle => m_state.ClientHandle;

        public bool Active => m_state.Active;

        public int BufferTime => m_state.BufferTime;

        public int MaxSize => m_state.MaxSize;

        public int KeepAlive => m_state.KeepAlive;

        public int EventTypes => m_filters.EventTypes;

        public int HighSeverity => m_filters.HighSeverity;

        public int LowSeverity => m_filters.LowSeverity;

        public Subscription.CategoryCollection Categories => m_categories;

        public Subscription.StringCollection Areas => m_areas;

        public Subscription.StringCollection Sources => m_sources;

        public Subscription.AttributeDictionary Attributes => m_attributes;

        public Opc.Ae.AttributeDictionary GetAttributes()
        {
            Opc.Ae.AttributeDictionary attributes = new Opc.Ae.AttributeDictionary();
            IDictionaryEnumerator enumerator = m_attributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int key = (int)enumerator.Key;
                Subscription.AttributeCollection attributeCollection = (Subscription.AttributeCollection)enumerator.Value;
                attributes.Add(key, attributeCollection.ToArray());
            }
            return attributes;
        }

        public event EventChangedEventHandler EventChanged
        {
            add => m_subscription.EventChanged += value;
            remove => m_subscription.EventChanged -= value;
        }

        public SubscriptionState GetState()
        {
            m_state = m_subscription != null ? m_subscription.GetState() : throw new NotConnectedException();
            m_state.Name = m_name;
            return (SubscriptionState)m_state.Clone();
        }

        public SubscriptionState ModifyState(int masks, SubscriptionState state)
        {
            if (m_subscription == null)
                throw new NotConnectedException();
            m_state = m_subscription.ModifyState(masks, state);
            m_state.Name = (masks & 1) == 0 ? m_name : (m_name = state.Name);
            return (SubscriptionState)m_state.Clone();
        }

        public SubscriptionFilters GetFilters()
        {
            m_filters = m_subscription != null ? m_subscription.GetFilters() : throw new NotConnectedException();
            m_categories = new Subscription.CategoryCollection(m_filters.Categories.ToArray());
            m_areas = new Subscription.StringCollection(m_filters.Areas.ToArray());
            m_sources = new Subscription.StringCollection(m_filters.Sources.ToArray());
            return (SubscriptionFilters)m_filters.Clone();
        }

        public void SetFilters(SubscriptionFilters filters)
        {
            if (m_subscription == null)
                throw new NotConnectedException();
            m_subscription.SetFilters(filters);
            GetFilters();
        }

        public int[] GetReturnedAttributes(int eventCategory)
        {
            int[] source = m_subscription != null ? m_subscription.GetReturnedAttributes(eventCategory) : throw new NotConnectedException();
            m_attributes.Update(eventCategory, (int[])Opc.Convert.Clone((object)source));
            return source;
        }

        public void SelectReturnedAttributes(int eventCategory, int[] attributeIDs)
        {
            if (m_subscription == null)
                throw new NotConnectedException();
            m_subscription.SelectReturnedAttributes(eventCategory, attributeIDs);
            m_attributes.Update(eventCategory, (int[])Opc.Convert.Clone((object)attributeIDs));
        }

        public void Refresh()
        {
            if (m_subscription == null)
                throw new NotConnectedException();
            m_subscription.Refresh();
        }

        public void CancelRefresh()
        {
            if (m_subscription == null)
                throw new NotConnectedException();
            m_subscription.CancelRefresh();
        }

        internal SubscriptionState State => m_state;

        internal SubscriptionFilters Filters => m_filters;

        private sealed class Names
        {
            internal const string STATE = "ST";
            internal const string FILTERS = "FT";
            internal const string ATTRIBUTES = "AT";
        }

        public class CategoryCollection : ReadOnlyCollection
        {
            public new int this[int index] => (int)Array.GetValue(index);

            public new int[] ToArray() => (int[])Opc.Convert.Clone((object)Array);

            internal CategoryCollection()
              : base(Array.Empty<int>())
            {
            }

            internal CategoryCollection(int[] categoryIDs)
              : base((Array)categoryIDs)
            {
            }
        }

        public class StringCollection : ReadOnlyCollection
        {
            public new string this[int index] => (string)Array.GetValue(index);

            public new string[] ToArray() => (string[])Opc.Convert.Clone((object)Array);

            internal StringCollection()
              : base(Array.Empty<string>())
            {
            }

            internal StringCollection(string[] strings)
              : base((Array)strings)
            {
            }
        }

        [Serializable]
        public class AttributeDictionary : ReadOnlyDictionary
        {
            public Subscription.AttributeCollection this[int categoryID]
            {
                get => (Subscription.AttributeCollection)this[(object)categoryID];
            }

            internal AttributeDictionary()
              : base((Hashtable)null)
            {
            }

            internal AttributeDictionary(Hashtable dictionary)
              : base(dictionary)
            {
            }

            internal void Update(int categoryID, int[] attributeIDs)
            {
                Dictionary[(object)categoryID] = (object)new Subscription.AttributeCollection(attributeIDs);
            }

            protected AttributeDictionary(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
        }

        [Serializable]
        public class AttributeCollection : ReadOnlyCollection
        {
            public new int this[int index] => (int)Array.GetValue(index);

            public new int[] ToArray() => (int[])Opc.Convert.Clone((object)Array);

            internal AttributeCollection()
              : base(Array.Empty<int>())
            {
            }

            internal AttributeCollection(int[] attributeIDs)
              : base((Array)attributeIDs)
            {
            }

            protected AttributeCollection(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
        }
    }
}
