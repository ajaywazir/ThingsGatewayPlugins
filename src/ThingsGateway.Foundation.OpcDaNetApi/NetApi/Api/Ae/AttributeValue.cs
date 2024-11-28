

using System;


namespace Opc.Ae
{
  [Serializable]
  public class AttributeValue : ICloneable, IResult
  {
    private int m_id;
    private object m_value;
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public int ID
    {
      get => this.m_id;
      set => this.m_id = value;
    }

    public object Value
    {
      get => this.m_value;
      set => this.m_value = value;
    }

    public ResultID ResultID
    {
      get => this.m_resultID;
      set => this.m_resultID = value;
    }

    public string DiagnosticInfo
    {
      get => this.m_diagnosticInfo;
      set => this.m_diagnosticInfo = value;
    }

    public virtual object Clone()
    {
      AttributeValue attributeValue = (AttributeValue) this.MemberwiseClone();
      attributeValue.Value = Opc.Convert.Clone(this.Value);
      return (object) attributeValue;
    }
  }
}
