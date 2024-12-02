

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Ae
{
    [Serializable]
    public class SubscriptionFilters : ICloneable, ISerializable
    {
        private int m_eventTypes = (int)ushort.MaxValue;
        private SubscriptionFilters.CategoryCollection m_categories = new SubscriptionFilters.CategoryCollection();
        private int m_highSeverity = 1000;
        private int m_lowSeverity = 1;
        private SubscriptionFilters.StringCollection m_areas = new SubscriptionFilters.StringCollection();
        private SubscriptionFilters.StringCollection m_sources = new SubscriptionFilters.StringCollection();

        public int EventTypes
        {
            get => m_eventTypes;
            set => m_eventTypes = value;
        }

        public int HighSeverity
        {
            get => m_highSeverity;
            set => m_highSeverity = value;
        }

        public int LowSeverity
        {
            get => m_lowSeverity;
            set => m_lowSeverity = value;
        }

        public SubscriptionFilters.CategoryCollection Categories => m_categories;

        public SubscriptionFilters.StringCollection Areas => m_areas;

        public SubscriptionFilters.StringCollection Sources => m_sources;

        public SubscriptionFilters()
        {
        }

        protected SubscriptionFilters(SerializationInfo info, StreamingContext context)
        {
            m_eventTypes = (int)info.GetValue("ET", typeof(int));
            m_categories = (SubscriptionFilters.CategoryCollection)info.GetValue("CT", typeof(SubscriptionFilters.CategoryCollection));
            m_highSeverity = (int)info.GetValue("HS", typeof(int));
            m_lowSeverity = (int)info.GetValue("LS", typeof(int));
            m_areas = (SubscriptionFilters.StringCollection)info.GetValue("AR", typeof(SubscriptionFilters.StringCollection));
            m_sources = (SubscriptionFilters.StringCollection)info.GetValue("SR", typeof(SubscriptionFilters.StringCollection));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ET", m_eventTypes);
            info.AddValue("CT", (object)m_categories);
            info.AddValue("HS", m_highSeverity);
            info.AddValue("LS", m_lowSeverity);
            info.AddValue("AR", (object)m_areas);
            info.AddValue("SR", (object)m_sources);
        }

        public virtual object Clone()
        {
            SubscriptionFilters subscriptionFilters = (SubscriptionFilters)MemberwiseClone();
            subscriptionFilters.m_categories = (SubscriptionFilters.CategoryCollection)m_categories.Clone();
            subscriptionFilters.m_areas = (SubscriptionFilters.StringCollection)m_areas.Clone();
            subscriptionFilters.m_sources = (SubscriptionFilters.StringCollection)m_sources.Clone();
            return (object)subscriptionFilters;
        }

        [Serializable]
        public class CategoryCollection : WriteableCollection
        {
            public new int this[int index] => (int)Array[index];

            public new int[] ToArray() => (int[])Array.ToArray(typeof(int));

            internal CategoryCollection()
              : base((ICollection)null, typeof(int))
            {
            }

            protected CategoryCollection(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
        }

        [Serializable]
        public class StringCollection : WriteableCollection
        {
            public new string this[int index] => (string)Array[index];

            public new string[] ToArray() => (string[])Array.ToArray(typeof(string));

            internal StringCollection()
              : base((ICollection)null, typeof(string))
            {
            }

            protected StringCollection(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
        }

        private sealed class Names
        {
            internal const string EVENT_TYPES = "ET";
            internal const string CATEGORIES = "CT";
            internal const string HIGH_SEVERITY = "HS";
            internal const string LOW_SEVERITY = "LS";
            internal const string AREAS = "AR";
            internal const string SOURCES = "SR";
        }
    }
}
