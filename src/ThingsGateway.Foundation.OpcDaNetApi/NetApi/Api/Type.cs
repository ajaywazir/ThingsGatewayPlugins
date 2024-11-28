

using System;
using System.Collections;
using System.Reflection;


namespace Opc
{
  public class Type
  {
    public static System.Type SBYTE = typeof (sbyte);
    public static System.Type BYTE = typeof (byte);
    public static System.Type SHORT = typeof (short);
    public static System.Type USHORT = typeof (ushort);
    public static System.Type INT = typeof (int);
    public static System.Type UINT = typeof (uint);
    public static System.Type LONG = typeof (long);
    public static System.Type ULONG = typeof (ulong);
    public static System.Type FLOAT = typeof (float);
    public static System.Type DOUBLE = typeof (double);
    public static System.Type DECIMAL = typeof (Decimal);
    public static System.Type BOOLEAN = typeof (bool);
    public static System.Type DATETIME = typeof (DateTime);
    public static System.Type DURATION = typeof (TimeSpan);
    public static System.Type STRING = typeof (string);
    public static System.Type ANY_TYPE = typeof (object);
    public static System.Type BINARY = typeof (byte[]);
    public static System.Type ARRAY_SHORT = typeof (short[]);
    public static System.Type ARRAY_USHORT = typeof (ushort[]);
    public static System.Type ARRAY_INT = typeof (int[]);
    public static System.Type ARRAY_UINT = typeof (uint[]);
    public static System.Type ARRAY_LONG = typeof (long[]);
    public static System.Type ARRAY_ULONG = typeof (ulong[]);
    public static System.Type ARRAY_FLOAT = typeof (float[]);
    public static System.Type ARRAY_DOUBLE = typeof (double[]);
    public static System.Type ARRAY_DECIMAL = typeof (Decimal[]);
    public static System.Type ARRAY_BOOLEAN = typeof (bool[]);
    public static System.Type ARRAY_DATETIME = typeof (DateTime[]);
    public static System.Type ARRAY_STRING = typeof (string[]);
    public static System.Type ARRAY_ANY_TYPE = typeof (object[]);
    public static System.Type ILLEGAL_TYPE = typeof (Type);

    public static System.Type[] Enumerate()
    {
      ArrayList arrayList = new ArrayList();
      foreach (FieldInfo field in typeof (Type).GetFields(BindingFlags.Public | BindingFlags.Static))
        arrayList.Add(field.GetValue((object) typeof (System.Type)));
      return (System.Type[]) arrayList.ToArray(typeof (System.Type));
    }
  }
}
