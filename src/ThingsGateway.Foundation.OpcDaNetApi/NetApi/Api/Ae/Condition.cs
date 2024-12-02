

using Opc.Da;

using System;
using System.Collections;


namespace Opc.Ae
{
    [Serializable]
    public class Condition : ICloneable
    {
        private int m_state;
        private SubCondition m_activeSubcondition = new SubCondition();
        private Quality m_quality = Quality.Bad;
        private DateTime m_lastAckTime = DateTime.MinValue;
        private DateTime m_subCondLastActive = DateTime.MinValue;
        private DateTime m_condLastActive = DateTime.MinValue;
        private DateTime m_condLastInactive = DateTime.MinValue;
        private string m_acknowledgerID;
        private string m_comment;
        private Condition.SubConditionCollection m_subconditions = new Condition.SubConditionCollection();
        private Condition.AttributeValueCollection m_attributes = new Condition.AttributeValueCollection();

        public int State
        {
            get => m_state;
            set => m_state = value;
        }

        public SubCondition ActiveSubCondition
        {
            get => m_activeSubcondition;
            set => m_activeSubcondition = value;
        }

        public Quality Quality
        {
            get => m_quality;
            set => m_quality = value;
        }

        public DateTime LastAckTime
        {
            get => m_lastAckTime;
            set => m_lastAckTime = value;
        }

        public DateTime SubCondLastActive
        {
            get => m_subCondLastActive;
            set => m_subCondLastActive = value;
        }

        public DateTime CondLastActive
        {
            get => m_condLastActive;
            set => m_condLastActive = value;
        }

        public DateTime CondLastInactive
        {
            get => m_condLastInactive;
            set => m_condLastInactive = value;
        }

        public string AcknowledgerID
        {
            get => m_acknowledgerID;
            set => m_acknowledgerID = value;
        }

        public string Comment
        {
            get => m_comment;
            set => m_comment = value;
        }

        public Condition.SubConditionCollection SubConditions => m_subconditions;

        public Condition.AttributeValueCollection Attributes => m_attributes;

        public virtual object Clone()
        {
            Condition condition = (Condition)MemberwiseClone();
            condition.m_activeSubcondition = (SubCondition)m_activeSubcondition.Clone();
            condition.m_subconditions = (Condition.SubConditionCollection)m_subconditions.Clone();
            condition.m_attributes = (Condition.AttributeValueCollection)m_attributes.Clone();
            return (object)condition;
        }

        public class AttributeValueCollection : WriteableCollection
        {
            public new AttributeValue this[int index] => (AttributeValue)Array[index];

            public new AttributeValue[] ToArray() => (AttributeValue[])Array.ToArray();

            internal AttributeValueCollection()
              : base((ICollection)null, typeof(AttributeValue))
            {
            }
        }

        public class SubConditionCollection : WriteableCollection
        {
            public new SubCondition this[int index] => (SubCondition)Array[index];

            public new SubCondition[] ToArray() => (SubCondition[])Array.ToArray();

            internal SubConditionCollection()
              : base((ICollection)null, typeof(SubCondition))
            {
            }
        }
    }
}
