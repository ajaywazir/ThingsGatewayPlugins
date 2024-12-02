

using System;


namespace Opc
{
    [Serializable]
    public class IdentifiedResult : ItemIdentifier, IResult
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
          : base((ItemIdentifier)item)
        {
            if (item == null)
                return;
            ResultID = item.ResultID;
            DiagnosticInfo = item.DiagnosticInfo;
        }

        public IdentifiedResult(string itemName, ResultID resultID)
          : base(itemName)
        {
            ResultID = resultID;
        }

        public IdentifiedResult(string itemName, ResultID resultID, string diagnosticInfo)
          : base(itemName)
        {
            ResultID = resultID;
            DiagnosticInfo = diagnosticInfo;
        }

        public IdentifiedResult(ItemIdentifier item, ResultID resultID)
          : base(item)
        {
            ResultID = resultID;
        }

        public IdentifiedResult(ItemIdentifier item, ResultID resultID, string diagnosticInfo)
          : base(item)
        {
            ResultID = resultID;
            DiagnosticInfo = diagnosticInfo;
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
    }
}
