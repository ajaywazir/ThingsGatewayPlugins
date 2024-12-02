

using Opc.Da;

using System;


namespace Opc.Ae
{
    [Serializable]
    public class EventNotification : ICloneable
    {
        private object m_clientHandle;
        private string m_sourceID;
        private DateTime m_time = DateTime.MinValue;
        private string m_message;
        private EventType m_eventType = EventType.Condition;
        private int m_eventCategory;
        private int m_severity = 1;
        private string m_conditionName;
        private string m_subConditionName;
        private EventNotification.AttributeCollection m_attributes = new EventNotification.AttributeCollection();
        private int m_changeMask;
        private int m_newState;
        private Quality m_quality = Quality.Bad;
        private bool m_ackRequired;
        private DateTime m_activeTime = DateTime.MinValue;
        private int m_cookie;
        private string m_actorID;

        public object ClientHandle
        {
            get => m_clientHandle;
            set => m_clientHandle = value;
        }

        public string SourceID
        {
            get => m_sourceID;
            set => m_sourceID = value;
        }

        public DateTime Time
        {
            get => m_time;
            set => m_time = value;
        }

        public string Message
        {
            get => m_message;
            set => m_message = value;
        }

        public EventType EventType
        {
            get => m_eventType;
            set => m_eventType = value;
        }

        public int EventCategory
        {
            get => m_eventCategory;
            set => m_eventCategory = value;
        }

        public int Severity
        {
            get => m_severity;
            set => m_severity = value;
        }

        public string ConditionName
        {
            get => m_conditionName;
            set => m_conditionName = value;
        }

        public string SubConditionName
        {
            get => m_subConditionName;
            set => m_subConditionName = value;
        }

        public EventNotification.AttributeCollection Attributes => m_attributes;

        public int ChangeMask
        {
            get => m_changeMask;
            set => m_changeMask = value;
        }

        public int NewState
        {
            get => m_newState;
            set => m_newState = value;
        }

        public Quality Quality
        {
            get => m_quality;
            set => m_quality = value;
        }

        public bool AckRequired
        {
            get => m_ackRequired;
            set => m_ackRequired = value;
        }

        public DateTime ActiveTime
        {
            get => m_activeTime;
            set => m_activeTime = value;
        }

        public int Cookie
        {
            get => m_cookie;
            set => m_cookie = value;
        }

        public string ActorID
        {
            get => m_actorID;
            set => m_actorID = value;
        }

        public void SetAttributes(object[] attributes)
        {
            if (attributes == null)
                m_attributes = new EventNotification.AttributeCollection();
            else
                m_attributes = new EventNotification.AttributeCollection(attributes);
        }

        public virtual object Clone()
        {
            EventNotification eventNotification = (EventNotification)MemberwiseClone();
            eventNotification.m_attributes = (EventNotification.AttributeCollection)m_attributes.Clone();
            return (object)eventNotification;
        }

        [Serializable]
        public class AttributeCollection : ReadOnlyCollection
        {
            internal AttributeCollection()
                : base(Array.Empty<object>())
            {
            }

            internal AttributeCollection(object[] attributes)
              : base((Array)attributes)
            {
            }
        }
    }
}
