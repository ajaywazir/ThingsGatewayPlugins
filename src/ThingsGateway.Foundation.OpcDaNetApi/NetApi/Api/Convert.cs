

using System;
using System.Collections;
using System.Text;
using System.Xml;


namespace Opc
{
    public class Convert
    {
        public static bool IsValid(Array array) => array != null && array.Length > 0;

        public static bool IsEmpty(Array array) => array == null || array.Length == 0;

        public static bool IsValid(string target) => target != null && target.Length > 0;

        public static bool IsEmpty(string target) => target == null || target.Length == 0;

        public static object Clone(object source)
        {
            if (source == null)
                return (object)null;
            if (source.GetType().IsValueType)
                return source;
            if (!source.GetType().IsArray)
            {
                if (!(source.GetType() == typeof(Array)))
                {
                    try
                    {
                        return ((ICloneable)source).Clone();
                    }
                    catch
                    {
                        throw new NotSupportedException("Object cannot be cloned.");
                    }
                }
            }
            Array array = (Array)((Array)source).Clone();
            for (int index = 0; index < array.Length; ++index)
                array.SetValue(Convert.Clone(array.GetValue(index)), index);
            return (object)array;
        }

        public static bool Compare(object a, object b)
        {
            if (a == null || b == null)
                return a == null && b == null;
            System.Type type1 = a.GetType();
            System.Type type2 = b.GetType();
            if (type1 != type2)
                return false;
            if (!type1.IsArray || !type2.IsArray)
                return a.Equals(b);
            Array array1 = (Array)a;
            Array array2 = (Array)b;
            if (array1.Length != array2.Length)
                return false;
            for (int index = 0; index < array1.Length; ++index)
            {
                if (!Convert.Compare(array1.GetValue(index), array2.GetValue(index)))
                    return false;
            }
            return true;
        }

        public static object ChangeType(object source, System.Type newType)
        {
            if (source == null)
                return newType != (System.Type)null && newType.IsValueType ? Activator.CreateInstance(newType) : (object)null;
            if (newType == (System.Type)null || newType == typeof(object) || newType == source.GetType())
                return Convert.Clone(source);
            System.Type type = source.GetType();
            if (type.IsArray && newType.IsArray)
            {
                ArrayList arrayList = new ArrayList(((Array)source).Length);
                foreach (object source1 in (Array)source)
                    arrayList.Add(Convert.ChangeType(source1, newType.GetElementType()));
                return (object)arrayList.ToArray(newType.GetElementType());
            }
            if (!type.IsArray && newType.IsArray)
                return (object)new ArrayList(1)
        {
          Convert.ChangeType(source, newType.GetElementType())
        }.ToArray(newType.GetElementType());
            if (type.IsArray && !newType.IsArray && ((Array)source).Length == 1)
                return System.Convert.ChangeType(((Array)source).GetValue(0), newType);
            if (type.IsArray && newType == typeof(string))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{ ");
                int num = 0;
                foreach (object source2 in (Array)source)
                {
                    stringBuilder.AppendFormat("{0}", Convert.ChangeType(source2, typeof(string)));
                    ++num;
                    if (num < ((Array)source).Length)
                        stringBuilder.Append(" | ");
                }
                stringBuilder.Append(" }");
                return (object)stringBuilder.ToString();
            }
            if (newType.IsEnum)
            {
                if (!(type == typeof(string)))
                    return Enum.ToObject(newType, source);
                return ((string)source).Length > 0 && char.IsDigit((string)source, 0) ? Enum.ToObject(newType, System.Convert.ToInt32(source)) : Enum.Parse(newType, (string)source);
            }
            if (!(newType == typeof(bool)))
                return System.Convert.ChangeType(source, newType);
            if (typeof(string).IsInstanceOfType(source))
            {
                string s = (string)source;
                if (s.Length > 0 && (s[0] == '+' || s[0] == '-' || char.IsDigit(s, 0)))
                    return (object)System.Convert.ToBoolean(System.Convert.ToInt32(source));
            }
            return (object)System.Convert.ToBoolean(source);
        }

        public static string ToString(object source)
        {
            if (source == null)
                return "";
            System.Type type = source.GetType();
            if (type == typeof(DateTime))
            {
                if ((DateTime)source == DateTime.MinValue)
                    return string.Empty;
                DateTime dateTime = (DateTime)source;
                return dateTime.Millisecond > 0 ? dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") : dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (type == typeof(XmlQualifiedName))
                return ((XmlQualifiedName)source).Name;
            if (type.FullName == "System.RuntimeType")
                return ((System.Type)source).Name;
            if (type == typeof(byte[]))
            {
                byte[] numArray = (byte[])source;
                StringBuilder stringBuilder = new StringBuilder(numArray.Length * 3);
                for (int index = 0; index < numArray.Length; ++index)
                {
                    stringBuilder.Append(numArray[index].ToString("X2"));
                    stringBuilder.Append(' ');
                }
                return stringBuilder.ToString();
            }
            if (type.IsArray)
                return string.Format("{0}[{1}]", (object)type.GetElementType().Name, (object)((Array)source).Length);
            return type == typeof(Array) ? string.Format("Object[{0}]", (object)((Array)source).Length) : source.ToString();
        }

        public static bool Match(string target, string pattern, bool caseSensitive)
        {
            if (pattern == null || pattern.Length == 0)
                return true;
            if (target == null || target.Length == 0)
                return false;
            if (caseSensitive)
            {
                if (target == pattern)
                    return true;
            }
            else if (string.Equals(target, pattern, StringComparison.OrdinalIgnoreCase))
                return true;
            int num1 = 0;
            int num2 = 0;
        label_46:
            while (num2 < target.Length && num1 < pattern.Length)
            {
                char ch1 = Convert.ConvertCase(pattern[num1++], caseSensitive);
                if (num1 > pattern.Length)
                    return num2 >= target.Length;
                switch (ch1)
                {
                    case '#':
                        if (!char.IsDigit(target[num2++]))
                            return false;
                        continue;
                    case '*':
                        while (num2 < target.Length)
                        {
                            if (Convert.Match(target.Substring(num2++), pattern.Substring(num1), caseSensitive))
                                return true;
                        }
                        return Convert.Match(target, pattern.Substring(num1), caseSensitive);
                    case '?':
                        if (num2 >= target.Length || num1 >= pattern.Length && num2 < target.Length - 1)
                            return false;
                        ++num2;
                        continue;
                    case '[':
                        char ch2 = Convert.ConvertCase(target[num2++], caseSensitive);
                        if (num2 > target.Length)
                            return false;
                        char ch3 = char.MinValue;
                        if (pattern[num1] == '!')
                        {
                            int num3 = num1 + 1;
                            string str = pattern;
                            int index = num3;
                            num1 = index + 1;
                            char ch4 = Convert.ConvertCase(str[index], caseSensitive);
                            for (; num1 < pattern.Length; ch4 = Convert.ConvertCase(pattern[num1++], caseSensitive))
                            {
                                switch (ch4)
                                {
                                    case '-':
                                        ch4 = Convert.ConvertCase(pattern[num1], caseSensitive);
                                        if (num1 > pattern.Length || ch4 == ']' || (int)ch2 >= (int)ch3 && (int)ch2 <= (int)ch4)
                                            return false;
                                        break;
                                    case ']':
                                        goto label_46;
                                }
                                ch3 = ch4;
                                if ((int)ch2 == (int)ch4)
                                    return false;
                            }
                            continue;
                        }
                        char ch5;
                        for (ch5 = Convert.ConvertCase(pattern[num1++], caseSensitive); num1 < pattern.Length; ch5 = Convert.ConvertCase(pattern[num1++], caseSensitive))
                        {
                            switch (ch5)
                            {
                                case '-':
                                    ch5 = Convert.ConvertCase(pattern[num1], caseSensitive);
                                    if (num1 > pattern.Length || ch5 == ']')
                                        return false;
                                    if ((int)ch2 < (int)ch3 || (int)ch2 > (int)ch5)
                                        break;
                                    goto label_41;
                                case ']':
                                    return false;
                            }
                            ch3 = ch5;
                            if ((int)ch2 == (int)ch5)
                                break;
                        }
                    label_41:
                        while (true)
                        {
                            if (num1 < pattern.Length && ch5 != ']')
                                ch5 = pattern[num1++];
                            else
                                goto label_46;
                        }
                    default:
                        if ((int)Convert.ConvertCase(target[num2++], caseSensitive) != (int)ch1 || num1 >= pattern.Length && num2 < target.Length - 1)
                            return false;
                        continue;
                }
            }
            return true;
        }

        private static char ConvertCase(char c, bool caseSensitive)
        {
            return !caseSensitive ? char.ToUpper(c) : c;
        }
    }
}
