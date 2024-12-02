

using System.Xml.Serialization;


namespace Opc.Cpx
{
    [XmlType(Namespace = "http://opcfoundation.org/OPCBinary/1.0/")]
    [XmlInclude(typeof(Double))]
    [XmlInclude(typeof(Single))]
    public class FloatingPoint : FieldType
    {
        [XmlAttribute]
        public string FloatFormat;
    }
}
