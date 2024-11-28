

using System.Xml.Serialization;


namespace Opc.Cpx
{
  [XmlType(Namespace = "http://opcfoundation.org/OPCBinary/1.0/")]
  [XmlInclude(typeof (TypeReference))]
  [XmlInclude(typeof (CharString))]
  [XmlInclude(typeof (Unicode))]
  [XmlInclude(typeof (Ascii))]
  [XmlInclude(typeof (FloatingPoint))]
  [XmlInclude(typeof (Double))]
  [XmlInclude(typeof (Single))]
  [XmlInclude(typeof (Integer))]
  [XmlInclude(typeof (UInt64))]
  [XmlInclude(typeof (UInt32))]
  [XmlInclude(typeof (UInt16))]
  [XmlInclude(typeof (UInt8))]
  [XmlInclude(typeof (Int64))]
  [XmlInclude(typeof (Int32))]
  [XmlInclude(typeof (Int16))]
  [XmlInclude(typeof (Int8))]
  [XmlInclude(typeof (BitString))]
  public class FieldType
  {
    [XmlAttribute]
    public string Name;
    [XmlAttribute]
    public string Format;
    [XmlAttribute]
    public int Length;
    [XmlIgnore]
    public bool LengthSpecified;
    [XmlAttribute]
    public int ElementCount;
    [XmlIgnore]
    public bool ElementCountSpecified;
    [XmlAttribute]
    public string ElementCountRef;
    [XmlAttribute]
    public string FieldTerminator;
  }
}
