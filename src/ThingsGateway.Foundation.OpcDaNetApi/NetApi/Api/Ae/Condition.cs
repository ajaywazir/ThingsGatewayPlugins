

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
      get => this.m_state;
      set => this.m_state = value;
    }

    public SubCondition ActiveSubCondition
    {
      get => this.m_activeSubcondition;
      set => this.m_activeSubcondition = value;
    }

    public Quality Quality
    {
      get => this.m_quality;
      set => this.m_quality = value;
    }

    public DateTime LastAckTime
    {
      get => this.m_lastAckTime;
      set => this.m_lastAckTime = value;
    }

    public DateTime SubCondLastActive
    {
      get => this.m_subCondLastActive;
      set => this.m_subCondLastActive = value;
    }

    public DateTime CondLastActive
    {
      get => this.m_condLastActive;
      set => this.m_condLastActive = value;
    }

    public DateTime CondLastInactive
    {
      get => this.m_condLastInactive;
      set => this.m_condLastInactive = value;
    }

    public string AcknowledgerID
    {
      get => this.m_acknowledgerID;
      set => this.m_acknowledgerID = value;
    }

    public string Comment
    {
      get => this.m_comment;
      set => this.m_comment = value;
    }

    public Condition.SubConditionCollection SubConditions => this.m_subconditions;

    public Condition.AttributeValueCollection Attributes => this.m_attributes;

    public virtual object Clone()
    {
      Condition condition = (Condition) this.MemberwiseClone();
      condition.m_activeSubcondition = (SubCondition) this.m_activeSubcondition.Clone();
      condition.m_subconditions = (Condition.SubConditionCollection) this.m_subconditions.Clone();
      condition.m_attributes = (Condition.AttributeValueCollection) this.m_attributes.Clone();
      return (object) condition;
    }

    public class AttributeValueCollection : WriteableCollection
    {
      public new AttributeValue this[int index] => (AttributeValue) this.Array[index];

      public new AttributeValue[] ToArray() => (AttributeValue[]) this.Array.ToArray();

      internal AttributeValueCollection()
        : base((ICollection) null, typeof (AttributeValue))
      {
      }
    }

    public class SubConditionCollection : WriteableCollection
    {
      public new SubCondition this[int index] => (SubCondition) this.Array[index];

      public new SubCondition[] ToArray() => (SubCondition[]) this.Array.ToArray();

      internal SubConditionCollection()
        : base((ICollection) null, typeof (SubCondition))
      {
      }
    }
  }
}
