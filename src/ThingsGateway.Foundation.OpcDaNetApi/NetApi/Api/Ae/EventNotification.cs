

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
      get => this.m_clientHandle;
      set => this.m_clientHandle = value;
    }

    public string SourceID
    {
      get => this.m_sourceID;
      set => this.m_sourceID = value;
    }

    public DateTime Time
    {
      get => this.m_time;
      set => this.m_time = value;
    }

    public string Message
    {
      get => this.m_message;
      set => this.m_message = value;
    }

    public EventType EventType
    {
      get => this.m_eventType;
      set => this.m_eventType = value;
    }

    public int EventCategory
    {
      get => this.m_eventCategory;
      set => this.m_eventCategory = value;
    }

    public int Severity
    {
      get => this.m_severity;
      set => this.m_severity = value;
    }

    public string ConditionName
    {
      get => this.m_conditionName;
      set => this.m_conditionName = value;
    }

    public string SubConditionName
    {
      get => this.m_subConditionName;
      set => this.m_subConditionName = value;
    }

    public EventNotification.AttributeCollection Attributes => this.m_attributes;

    public int ChangeMask
    {
      get => this.m_changeMask;
      set => this.m_changeMask = value;
    }

    public int NewState
    {
      get => this.m_newState;
      set => this.m_newState = value;
    }

    public Quality Quality
    {
      get => this.m_quality;
      set => this.m_quality = value;
    }

    public bool AckRequired
    {
      get => this.m_ackRequired;
      set => this.m_ackRequired = value;
    }

    public DateTime ActiveTime
    {
      get => this.m_activeTime;
      set => this.m_activeTime = value;
    }

    public int Cookie
    {
      get => this.m_cookie;
      set => this.m_cookie = value;
    }

    public string ActorID
    {
      get => this.m_actorID;
      set => this.m_actorID = value;
    }

    public void SetAttributes(object[] attributes)
    {
      if (attributes == null)
        this.m_attributes = new EventNotification.AttributeCollection();
      else
        this.m_attributes = new EventNotification.AttributeCollection(attributes);
    }

    public virtual object Clone()
    {
      EventNotification eventNotification = (EventNotification) this.MemberwiseClone();
      eventNotification.m_attributes = (EventNotification.AttributeCollection) this.m_attributes.Clone();
      return (object) eventNotification;
    }

    [Serializable]
    public class AttributeCollection : ReadOnlyCollection
    {
      internal AttributeCollection()
        : base((Array) new object[0])
      {
      }

      internal AttributeCollection(object[] attributes)
        : base((Array) attributes)
      {
      }
    }
  }
}
