

using System;


namespace Opc.Dx
{
  [Serializable]
  public class IdentifiedResult : ItemIdentifier
  {
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public IdentifiedResult()
    {
    }

    public IdentifiedResult(ItemIdentifier item)
      : base(item)
    {
    }

    public IdentifiedResult(IdentifiedResult item)
      : base((ItemIdentifier) item)
    {
      if (item == null)
        return;
      this.ResultID = item.ResultID;
      this.DiagnosticInfo = item.DiagnosticInfo;
    }

    public IdentifiedResult(string itemName, ResultID resultID)
      : base(itemName)
    {
      this.ResultID = resultID;
    }

    public IdentifiedResult(string itemName, ResultID resultID, string diagnosticInfo)
      : base(itemName)
    {
      this.ResultID = resultID;
      this.DiagnosticInfo = diagnosticInfo;
    }

    public IdentifiedResult(ItemIdentifier item, ResultID resultID)
      : base(item)
    {
      this.ResultID = resultID;
    }

    public IdentifiedResult(ItemIdentifier item, ResultID resultID, string diagnosticInfo)
      : base(item)
    {
      this.ResultID = resultID;
      this.DiagnosticInfo = diagnosticInfo;
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
  }
}
