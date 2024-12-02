

using System;


namespace Opc.Da
{
    [Serializable]
    public class BrowseElement : ICloneable
    {
        private string m_name;
        private string m_itemName;
        private string m_itemPath;
        private bool m_isItem;
        private bool m_hasChildren;
        private ItemProperty[] m_properties = Array.Empty<ItemProperty>();

        public string Name
        {
            get => m_name;
            set => m_name = value;
        }

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

        public bool IsItem
        {
            get => m_isItem;
            set => m_isItem = value;
        }

        public bool HasChildren
        {
            get => m_hasChildren;
            set => m_hasChildren = value;
        }

        public ItemProperty[] Properties
        {
            get => m_properties;
            set => m_properties = value;
        }

        public virtual object Clone()
        {
            BrowseElement browseElement = (BrowseElement)MemberwiseClone();
            browseElement.m_properties = (ItemProperty[])Opc.Convert.Clone((object)m_properties);
            return (object)browseElement;
        }
    }
}
