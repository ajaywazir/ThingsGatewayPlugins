

using System;


namespace Opc.Hda
{
  [Serializable]
  public class ModifiedValue : ItemValue
  {
    private DateTime m_modificationTime = DateTime.MinValue;
    private EditType m_editType = EditType.Insert;
    private string m_user;

    public DateTime ModificationTime
    {
      get => this.m_modificationTime;
      set => this.m_modificationTime = value;
    }

    public EditType EditType
    {
      get => this.m_editType;
      set => this.m_editType = value;
    }

    public string User
    {
      get => this.m_user;
      set => this.m_user = value;
    }
  }
}
