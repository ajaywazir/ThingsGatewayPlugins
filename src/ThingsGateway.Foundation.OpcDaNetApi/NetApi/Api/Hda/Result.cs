

using System;


namespace Opc.Hda
{
  [Serializable]
  public class Result : ICloneable, IResult
  {
    private ResultID m_resultID = ResultID.S_OK;
    private string m_diagnosticInfo;

    public Result()
    {
    }

    public Result(ResultID resultID)
    {
      this.ResultID = resultID;
      this.DiagnosticInfo = (string) null;
    }

    public Result(IResult result)
    {
      this.ResultID = result.ResultID;
      this.DiagnosticInfo = result.DiagnosticInfo;
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

    public object Clone() => this.MemberwiseClone();
  }
}
