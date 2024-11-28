

using System.Collections;
using System.Reflection;


namespace Opc.Dx
{
  public class ServerType
  {
    public const string COM_DA10 = "COM-DA1.0";
    public const string COM_DA204 = "COM-DA2.04";
    public const string COM_DA205 = "COM-DA2.05";
    public const string COM_DA30 = "COM-DA3.0";
    public const string XML_DA10 = "XML-DA1.0";

    public static string[] Enumerate()
    {
      ArrayList arrayList = new ArrayList();
      foreach (FieldInfo field in typeof (ServerType).GetFields(BindingFlags.Public | BindingFlags.Static))
        arrayList.Add(field.GetValue((object) typeof (string)));
      return (string[]) arrayList.ToArray(typeof (string));
    }
  }
}
