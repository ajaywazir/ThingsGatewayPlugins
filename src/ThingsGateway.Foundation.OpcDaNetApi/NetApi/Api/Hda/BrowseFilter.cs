

using System;


namespace Opc.Hda
{
  [Serializable]
  public class BrowseFilter : ICloneable
  {
    private int m_attributeID;
    private Operator m_operator = Operator.Equal;
    private object m_filterValue;

    public int AttributeID
    {
      get => this.m_attributeID;
      set => this.m_attributeID = value;
    }

    public Operator Operator
    {
      get => this.m_operator;
      set => this.m_operator = value;
    }

    public object FilterValue
    {
      get => this.m_filterValue;
      set => this.m_filterValue = value;
    }

    public virtual object Clone()
    {
      BrowseFilter browseFilter = (BrowseFilter) this.MemberwiseClone();
      browseFilter.FilterValue = Opc.Convert.Clone(this.FilterValue);
      return (object) browseFilter;
    }
  }
}
