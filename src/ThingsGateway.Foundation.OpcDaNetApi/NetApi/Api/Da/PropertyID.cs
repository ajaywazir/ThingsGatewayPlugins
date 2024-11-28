

using System;
using System.Runtime.Serialization;
using System.Xml;


namespace Opc.Da
{
    [Serializable]
    public struct PropertyID : ISerializable
    {
        private int m_code;
        private XmlQualifiedName m_name;

        private PropertyID(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            string name = "";
            string ns = "";
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Name.Equals("NA"))
                    name = (string)enumerator.Current.Value;
                else if (enumerator.Current.Name.Equals("NS"))
                    ns = (string)enumerator.Current.Value;
            }
            m_name = new XmlQualifiedName(name, ns);
            m_code = (int)info.GetValue("CO", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (m_name != (XmlQualifiedName)null)
            {
                info.AddValue("NA", (object)m_name.Name);
                info.AddValue("NS", (object)m_name.Namespace);
            }
            info.AddValue("CO", m_code);
        }

        public XmlQualifiedName Name => m_name;

        public int Code => m_code;

        public static bool operator ==(PropertyID a, PropertyID b) => a.Equals((object)b);

        public static bool operator !=(PropertyID a, PropertyID b) => !a.Equals((object)b);

        public PropertyID(XmlQualifiedName name)
        {
            m_name = name;
            m_code = 0;
        }

        public PropertyID(int code)
        {
            m_name = (XmlQualifiedName)null;
            m_code = code;
        }

        public PropertyID(string name, int code, string ns)
        {
            m_name = new XmlQualifiedName(name, ns);
            m_code = code;
        }

        public override bool Equals(object target)
        {
            if (target != null && target.GetType() == typeof(PropertyID))
            {
                PropertyID propertyId = (PropertyID)target;
                if (propertyId.Code != 0 && Code != 0)
                    return propertyId.Code == Code;
                if (propertyId.Name != (XmlQualifiedName)null && Name != (XmlQualifiedName)null)
                    return propertyId.Name == Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (Code != 0)
                return Code.GetHashCode();
            return Name != (XmlQualifiedName)null ? Name.GetHashCode() : base.GetHashCode();
        }

        public override string ToString()
        {
            if (Name != (XmlQualifiedName)null && Code != 0)
                return string.Format("{0} ({1})", (object)Name.Name, (object)Code);
            if (Name != (XmlQualifiedName)null)
                return Name.Name;
            return Code != 0 ? string.Format("{0}", (object)Code) : "";
        }

        private class Names
        {
            internal const string NAME = "NA";
            internal const string NAMESPACE = "NS";
            internal const string CODE = "CO";
        }
    }
}
