

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
            ResultID = resultID;
            DiagnosticInfo = (string)null;
        }

        public Result(IResult result)
        {
            ResultID = result.ResultID;
            DiagnosticInfo = result.DiagnosticInfo;
        }

        public ResultID ResultID
        {
            get => m_resultID;
            set => m_resultID = value;
        }

        public string DiagnosticInfo
        {
            get => m_diagnosticInfo;
            set => m_diagnosticInfo = value;
        }

        public object Clone() => MemberwiseClone();
    }
}
