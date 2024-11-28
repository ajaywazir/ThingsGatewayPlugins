

using System;


namespace Opc.Da
{
  [Serializable]
  public class ItemProperty : ICloneable, IResult
  {
    private PropertyID m_id;
    private string m_description;
    private System.Type m_datatype;
    private object m_value;
    private string m_itemName;
    private string m_itemPath;
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public PropertyID ID
    {
      get => this.m_id;
      set => this.m_id = value;
    }

    public string Description
    {
      get => this.m_description;
      set => this.m_description = value;
    }

    public System.Type DataType
    {
      get => this.m_datatype;
      set => this.m_datatype = value;
    }

    public object Value
    {
      get => this.m_value;
      set => this.m_value = value;
    }

    public string ItemName
    {
      get => this.m_itemName;
      set => this.m_itemName = value;
    }

    public string ItemPath
    {
      get => this.m_itemPath;
      set => this.m_itemPath = value;
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
      ItemProperty itemProperty = (ItemProperty) this.MemberwiseClone();
      itemProperty.Value = Opc.Convert.Clone(this.Value);
      return (object) itemProperty;
    }
  }
}
