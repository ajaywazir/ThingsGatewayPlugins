


namespace Opc.Cpx
{
    public class BinaryStream
    {
        protected BinaryStream()
        {
        }

        internal static bool IsArrayField(FieldType field)
        {
            if (field.ElementCountSpecified)
            {
                if (field.ElementCountRef != null || field.FieldTerminator != null)
                    throw new InvalidSchemaException("Multiple array size attributes specified for field '" + field.Name + " '.");
                return true;
            }
            if (field.ElementCountRef != null)
            {
                if (field.FieldTerminator != null)
                    throw new InvalidSchemaException("Multiple array size attributes specified for field '" + field.Name + " '.");
                return true;
            }
            return field.FieldTerminator != null;
        }

        internal static byte[] GetTerminator(Context context, FieldType field)
        {
            string str = field.FieldTerminator != null ? System.Convert.ToString(field.FieldTerminator).ToUpper() : throw new InvalidSchemaException(field.Name + " is not a terminated group.");
            byte[] terminator = new byte[str.Length / 2];
            for (int index = 0; index < terminator.Length; ++index)
                terminator[index] = System.Convert.ToByte(str.Substring(index * 2, 2), 16);
            return terminator;
        }

        internal static Context InitializeContext(byte[] buffer, TypeDictionary dictionary, string typeName)
        {
            Context context = new Context(buffer);
            context.Dictionary = dictionary;
            context.Type = (TypeDescription)null;
            context.BigEndian = dictionary.DefaultBigEndian;
            context.CharWidth = dictionary.DefaultCharWidth;
            context.StringEncoding = dictionary.DefaultStringEncoding;
            context.FloatFormat = dictionary.DefaultFloatFormat;
            foreach (TypeDescription typeDescription in dictionary.TypeDescription)
            {
                if (typeDescription.TypeID == typeName)
                {
                    context.Type = typeDescription;
                    if (typeDescription.DefaultBigEndianSpecified)
                        context.BigEndian = typeDescription.DefaultBigEndian;
                    if (typeDescription.DefaultCharWidthSpecified)
                        context.CharWidth = typeDescription.DefaultCharWidth;
                    if (typeDescription.DefaultStringEncoding != null)
                        context.StringEncoding = typeDescription.DefaultStringEncoding;
                    if (typeDescription.DefaultFloatFormat != null)
                    {
                        context.FloatFormat = typeDescription.DefaultFloatFormat;
                        break;
                    }
                    break;
                }
            }
            return context.Type != null ? context : throw new InvalidSchemaException("Type '" + typeName + "' not found in dictionary.");
        }

        internal static void SwapBytes(byte[] bytes, int index, int length)
        {
            for (int index1 = 0; index1 < length / 2; ++index1)
            {
                byte num = bytes[index + length - 1 - index1];
                bytes[index + length - 1 - index1] = bytes[index + index1];
                bytes[index + index1] = num;
            }
        }
    }
}
