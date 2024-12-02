

using System;
using System.Collections;


namespace Opc.Da
{
    [Serializable]
    public class ItemPropertyCollection : ArrayList, IResult
    {
        private string m_itemName;
        private string m_itemPath;
        private ResultID m_resultID = ResultID.S_OK;
        private string m_diagnosticInfo;

        public string ItemName
        {
            get => m_itemName;
            set => m_itemName = value;
        }

        public string ItemPath
        {
            get => m_itemPath;
            set => m_itemPath = value;
        }

        public new ItemProperty this[int index]
        {
            get => (ItemProperty)base[index];
            set => base[index] = (ItemProperty)value;
        }

        public ItemPropertyCollection()
        {
        }

        public ItemPropertyCollection(ItemIdentifier itemID)
        {
            if (itemID == null)
                return;
            m_itemName = itemID.ItemName;
            m_itemPath = itemID.ItemPath;
        }

        public ItemPropertyCollection(ItemIdentifier itemID, ResultID resultID)
        {
            if (itemID != null)
            {
                m_itemName = itemID.ItemName;
                m_itemPath = itemID.ItemPath;
            }
            ResultID = resultID;
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

        public void CopyTo(ItemProperty[] array, int index) => CopyTo((Array)array, index);

        public void Insert(int index, ItemProperty value) => Insert(index, (object)value);

        public void Remove(ItemProperty value) => Remove((object)value);

        public bool Contains(ItemProperty value) => Contains((object)value);

        public int IndexOf(ItemProperty value) => IndexOf((object)value);

        public int Add(ItemProperty value) => Add((object)value);
    }
}
