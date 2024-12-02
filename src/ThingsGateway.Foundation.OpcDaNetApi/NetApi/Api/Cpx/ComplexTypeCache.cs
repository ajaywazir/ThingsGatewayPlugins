

using Opc.Da;

using System.Collections;


namespace Opc.Cpx
{
    public class ComplexTypeCache
    {
        private static Opc.Da.Server m_server = (Opc.Da.Server)null;
        private static Hashtable m_items = new Hashtable();
        private static Hashtable m_dictionaries = new Hashtable();
        private static Hashtable m_descriptions = new Hashtable();

        public static Opc.Da.Server Server
        {
            get
            {
                lock (typeof(ComplexTypeCache))
                    return ComplexTypeCache.m_server;
            }
            set
            {
                lock (typeof(ComplexTypeCache))
                {
                    ComplexTypeCache.m_server = value;
                    ComplexTypeCache.m_items.Clear();
                    ComplexTypeCache.m_dictionaries.Clear();
                    ComplexTypeCache.m_descriptions.Clear();
                }
            }
        }

        public static ComplexItem GetComplexItem(ItemIdentifier itemID)
        {
            if (itemID == null)
                return (ComplexItem)null;
            lock (typeof(ComplexTypeCache))
            {
                ComplexItem complexItem = new ComplexItem(itemID);
                try
                {
                    complexItem.Update(ComplexTypeCache.m_server);
                }
                catch
                {
                    complexItem = (ComplexItem)null;
                }
                ComplexTypeCache.m_items[(object)itemID.Key] = (object)complexItem;
                return complexItem;
            }
        }

        public static ComplexItem GetComplexItem(BrowseElement element)
        {
            if (element == null)
                return (ComplexItem)null;
            lock (typeof(ComplexTypeCache))
                return ComplexTypeCache.GetComplexItem(new ItemIdentifier(element.ItemPath, element.ItemName));
        }

        public static string GetTypeDictionary(ItemIdentifier itemID)
        {
            if (itemID == null)
                return (string)null;
            lock (typeof(ComplexTypeCache))
            {
                string typeDictionary = (string)ComplexTypeCache.m_dictionaries[(object)itemID.Key];
                if (typeDictionary != null)
                    return typeDictionary;
                ComplexItem complexItem = ComplexTypeCache.GetComplexItem(itemID);
                if (complexItem != null)
                    typeDictionary = complexItem.GetTypeDictionary(ComplexTypeCache.m_server);
                return typeDictionary;
            }
        }

        public static string GetTypeDescription(ItemIdentifier itemID)
        {
            if (itemID == null)
                return (string)null;
            lock (typeof(ComplexTypeCache))
            {
                string typeDescription = (string)null;
                ComplexItem complexItem = ComplexTypeCache.GetComplexItem(itemID);
                if (complexItem != null)
                {
                    string description = (string)ComplexTypeCache.m_descriptions[(object)complexItem.TypeItemID.Key];
                    if (description != null)
                        return description;
                    ComplexTypeCache.m_descriptions[(object)complexItem.TypeItemID.Key] = (object)(typeDescription = complexItem.GetTypeDescription(ComplexTypeCache.m_server));
                }
                return typeDescription;
            }
        }
    }
}
