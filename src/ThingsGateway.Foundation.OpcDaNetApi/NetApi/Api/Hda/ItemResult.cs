

using System;


namespace Opc.Hda
{
    [Serializable]
    public class ItemResult : Item, IResult
    {
        private ResultID m_resultID = ResultID.S_OK;
        private string m_diagnosticInfo;

        public ItemResult()
        {
        }

        public ItemResult(ItemIdentifier item)
          : base(item)
        {
        }

        public ItemResult(Item item)
          : base(item)
        {
        }

        public ItemResult(ItemResult item)
          : base((Item)item)
        {
            if (item == null)
                return;
            ResultID = item.ResultID;
            DiagnosticInfo = item.DiagnosticInfo;
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
