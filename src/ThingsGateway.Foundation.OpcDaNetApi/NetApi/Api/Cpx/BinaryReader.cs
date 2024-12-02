

using System;
using System.Collections;


namespace Opc.Cpx
{
    public class BinaryReader : BinaryStream
    {
        public ComplexValue Read(byte[] buffer, TypeDictionary dictionary, string typeName)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            Context context = BinaryStream.InitializeContext(buffer, dictionary, typeName);
            ComplexValue complexValue = (ComplexValue)null;
            if (ReadType(context, out complexValue) == 0)
                throw new InvalidSchemaException("Type '" + typeName + "' not found in dictionary.");
            return complexValue;
        }

        private int ReadType(Context context, out ComplexValue complexValue)
        {
            complexValue = (ComplexValue)null;
            TypeDescription type = context.Type;
            int index = context.Index;
            byte bitOffset = 0;
            ArrayList fieldValues = new ArrayList();
            for (int fieldIndex = 0; fieldIndex < type.Field.Length; ++fieldIndex)
            {
                FieldType field = type.Field[fieldIndex];
                ComplexValue complexValue1 = new ComplexValue();
                complexValue1.Name = field.Name == null || field.Name.Length == 0 ? "[" + fieldIndex.ToString() + "]" : field.Name;
                complexValue1.Type = (string)null;
                complexValue1.Value = (object)null;
                if (bitOffset != (byte)0 && field.GetType() != typeof(BitString))
                {
                    ++context.Index;
                    bitOffset = (byte)0;
                }
                int num;
                if (BinaryStream.IsArrayField(field))
                    num = ReadArrayField(context, field, fieldIndex, fieldValues, out complexValue1.Value);
                else if (field.GetType() == typeof(TypeReference))
                {
                    object fieldValue = (object)null;
                    num = ReadField(context, (TypeReference)field, out fieldValue);
                    complexValue1.Name = field.Name;
                    complexValue1.Type = ((ComplexValue)fieldValue).Type;
                    complexValue1.Value = ((ComplexValue)fieldValue).Value;
                }
                else
                    num = ReadField(context, field, fieldIndex, fieldValues, out complexValue1.Value, ref bitOffset);
                if (num == 0 && bitOffset == (byte)0)
                    throw new InvalidDataInBufferException("Could not read field '" + field.Name + "' in type '" + type.TypeID + "'.");
                context.Index += num;
                if (complexValue1.Type == null)
                    complexValue1.Type = Opc.Convert.ToString((object)complexValue1.Value.GetType());
                fieldValues.Add((object)complexValue1);
            }
            if (bitOffset != (byte)0)
                ++context.Index;
            complexValue = new ComplexValue();
            complexValue.Name = type.TypeID;
            complexValue.Type = type.TypeID;
            complexValue.Value = (object)(ComplexValue[])fieldValues.ToArray(typeof(ComplexValue));
            return context.Index - index;
        }

        private int ReadField(
          Context context,
          FieldType field,
          int fieldIndex,
          ArrayList fieldValues,
          out object fieldValue,
          ref byte bitOffset)
        {
            fieldValue = (object)null;
            System.Type type = field.GetType();
            if (type == typeof(Integer) || type.IsSubclassOf(typeof(Integer)))
                return ReadField(context, (Integer)field, out fieldValue);
            if (type == typeof(FloatingPoint) || type.IsSubclassOf(typeof(FloatingPoint)))
                return BinaryReader.ReadField(context, (FloatingPoint)field, out fieldValue);
            if (type == typeof(CharString) || type.IsSubclassOf(typeof(CharString)))
                return ReadField(context, (CharString)field, fieldIndex, fieldValues, out fieldValue);
            if (type == typeof(BitString) || type.IsSubclassOf(typeof(BitString)))
                return BinaryReader.ReadField(context, (BitString)field, out fieldValue, ref bitOffset);
            if (type == typeof(TypeReference))
                return ReadField(context, (TypeReference)field, out fieldValue);
            throw new NotImplementedException("Fields of type '" + type.ToString() + "' are not implemented yet.");
        }

        private int ReadField(Context context, TypeReference field, out object fieldValue)
        {
            fieldValue = (object)null;
            foreach (TypeDescription typeDescription in context.Dictionary.TypeDescription)
            {
                if (typeDescription.TypeID == field.TypeID)
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
            if (context.Type == null)
                throw new InvalidSchemaException("Reference type '" + field.TypeID + "' not found.");
            ComplexValue complexValue = (ComplexValue)null;
            int num = ReadType(context, out complexValue);
            if (num == 0)
                fieldValue = (object)null;
            fieldValue = (object)complexValue;
            return num;
        }

        private static int ReadField(Context context, Integer field, out object fieldValue)
        {
            fieldValue = (object)null;
            byte[] buffer = context.Buffer;
            int length = field.LengthSpecified ? field.Length : 4;
            bool flag = field.Signed;
            if (field.GetType() == typeof(Int8))
            {
                length = 1;
                flag = true;
            }
            else if (field.GetType() == typeof(Int16))
            {
                length = 2;
                flag = true;
            }
            else if (field.GetType() == typeof(Int32))
            {
                length = 4;
                flag = true;
            }
            else if (field.GetType() == typeof(Int64))
            {
                length = 8;
                flag = true;
            }
            else if (field.GetType() == typeof(UInt8))
            {
                length = 1;
                flag = false;
            }
            else if (field.GetType() == typeof(UInt16))
            {
                length = 2;
                flag = false;
            }
            else if (field.GetType() == typeof(UInt32))
            {
                length = 4;
                flag = false;
            }
            else if (field.GetType() == typeof(UInt64))
            {
                length = 8;
                flag = false;
            }
            if (buffer.Length - context.Index < length)
                throw new InvalidDataInBufferException("Unexpected end of buffer.");
            byte[] bytes = new byte[length];
            for (int index = 0; index < length; ++index)
                bytes[index] = buffer[context.Index + index];
            if (context.BigEndian)
                BinaryStream.SwapBytes(bytes, 0, length);
            if (flag)
            {
                switch (length)
                {
                    case 1:
                        fieldValue = bytes[0] >= (byte)128 ? (object)(sbyte)-bytes[0] : (object)(sbyte)bytes[0];
                        break;
                    case 2:
                        fieldValue = (object)BitConverter.ToInt16(bytes, 0);
                        break;
                    case 4:
                        fieldValue = (object)BitConverter.ToInt32(bytes, 0);
                        break;
                    case 8:
                        fieldValue = (object)BitConverter.ToInt64(bytes, 0);
                        break;
                    default:
                        fieldValue = (object)bytes;
                        break;
                }
            }
            else
            {
                switch (length)
                {
                    case 1:
                        fieldValue = (object)bytes[0];
                        break;
                    case 2:
                        fieldValue = (object)BitConverter.ToUInt16(bytes, 0);
                        break;
                    case 4:
                        fieldValue = (object)BitConverter.ToUInt32(bytes, 0);
                        break;
                    case 8:
                        fieldValue = (object)BitConverter.ToUInt64(bytes, 0);
                        break;
                    default:
                        fieldValue = (object)bytes;
                        break;
                }
            }
            return length;
        }

        private static int ReadField(Context context, FloatingPoint field, out object fieldValue)
        {
            fieldValue = (object)null;
            byte[] buffer = context.Buffer;
            int length = field.LengthSpecified ? field.Length : 4;
            string str = field.FloatFormat != null ? field.FloatFormat : context.FloatFormat;
            if (field.GetType() == typeof(Single))
            {
                length = 4;
                str = "IEEE-754";
            }
            else if (field.GetType() == typeof(Double))
            {
                length = 8;
                str = "IEEE-754";
            }
            if (buffer.Length - context.Index < length)
                throw new InvalidDataInBufferException("Unexpected end of buffer.");
            byte[] numArray = new byte[length];
            for (int index = 0; index < length; ++index)
                numArray[index] = buffer[context.Index + index];
            if (str == "IEEE-754")
            {
                switch (length)
                {
                    case 4:
                        fieldValue = (object)BitConverter.ToSingle(numArray, 0);
                        break;
                    case 8:
                        fieldValue = (object)BitConverter.ToDouble(numArray, 0);
                        break;
                    default:
                        fieldValue = (object)numArray;
                        break;
                }
            }
            else
                fieldValue = (object)numArray;
            return length;
        }

        private static int ReadField(
          Context context,
          CharString field,
          int fieldIndex,
          ArrayList fieldValues,
          out object fieldValue)
        {
            fieldValue = (object)null;
            byte[] buffer = context.Buffer;
            int length1 = field.CharWidthSpecified ? field.CharWidth : context.CharWidth;
            int length2 = field.LengthSpecified ? field.Length : -1;
            if (field.GetType() == typeof(Ascii))
                length1 = 1;
            else if (field.GetType() == typeof(Unicode))
                length1 = 2;
            if (field.CharCountRef != null)

                length2 = BinaryReader.ReadReference(context, (FieldType)field, fieldIndex, fieldValues, field.CharCountRef);
            if (length2 == -1)
            {
                length2 = 0;
                for (int index1 = context.Index; index1 < context.Buffer.Length - length1 + 1; index1 += length1)
                {
                    ++length2;
                    bool flag = true;
                    for (int index2 = 0; index2 < length1; ++index2)
                    {
                        if (context.Buffer[index1 + index2] != (byte)0)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                        break;
                }
            }
            if (buffer.Length - context.Index < length1 * length2)
                throw new InvalidDataInBufferException("Unexpected end of buffer.");
            if (length1 > 2)
            {
                byte[] bytes = new byte[length2 * length1];
                for (int index = 0; index < length2 * length1; ++index)
                    bytes[index] = buffer[context.Index + index];
                if (context.BigEndian)
                {
                    for (int index = 0; index < bytes.Length; index += length1)
                        BinaryStream.SwapBytes(bytes, 0, length1);
                }
                fieldValue = (object)bytes;
            }
            else
            {
                char[] chArray = new char[length2];
                for (int index = 0; index < length2; ++index)
                {
                    if (length1 == 1)
                    {
                        chArray[index] = System.Convert.ToChar(buffer[context.Index + index]);
                    }
                    else
                    {
                        byte[] bytes = new byte[2]
                        {
              buffer[context.Index + 2 * index],
              buffer[context.Index + 2 * index + 1]
                        };
                        if (context.BigEndian)
                            BinaryStream.SwapBytes(bytes, 0, 2);
                        chArray[index] = BitConverter.ToChar(bytes, 0);
                    }
                }
                fieldValue = (object)new string(chArray).TrimEnd(new char[1]);
            }
            return length2 * length1;
        }

        private static int ReadField(
          Context context,
          BitString field,
          out object fieldValue,
          ref byte bitOffset)
        {
            fieldValue = (object)null;
            byte[] buffer = context.Buffer;
            int num1 = field.LengthSpecified ? field.Length : 8;
            int length = num1 % 8 == 0 ? num1 / 8 : num1 / 8 + 1;
            if (buffer.Length - context.Index < length)
                throw new InvalidDataInBufferException("Unexpected end of buffer.");
            byte[] numArray = new byte[length];
            int num2 = num1;
            byte num3 = (byte)~((1 << (int)bitOffset) - 1);
            for (int index = 0; num2 >= 0 && index < length; ++index)
            {
                numArray[index] = (byte)(((uint)num3 & (uint)buffer[context.Index + index]) >> (int)bitOffset);
                if (num2 + (int)bitOffset <= 8)
                {
                    numArray[index] &= (byte)((1 << num2) - 1);
                    break;
                }
                if (context.Index + index + 1 >= buffer.Length)
                    throw new InvalidDataInBufferException("Unexpected end of buffer.");
                numArray[index] += (byte)(((int)~num3 & (int)buffer[context.Index + index + 1]) << 8 - (int)bitOffset);
                if (num2 <= 8)
                {
                    numArray[index] &= (byte)((1 << num2) - 1);
                    break;
                }
                num2 -= 8;
            }
            fieldValue = (object)numArray;
            int num4 = (num1 + (int)bitOffset) / 8;
            bitOffset = (byte)((num1 + (int)bitOffset) % 8);
            return num4;
        }

        private int ReadArrayField(
          Context context,
          FieldType field,
          int fieldIndex,
          ArrayList fieldValues,
          out object fieldValue)
        {
            fieldValue = (object)null;
            int index1 = context.Index;
            ArrayList arrayList = new ArrayList();
            object fieldValue1 = (object)null;
            byte bitOffset = 0;
            if (field.ElementCountSpecified)
            {
                for (int index2 = 0; index2 < field.ElementCount; ++index2)
                {
                    int num = ReadField(context, field, fieldIndex, fieldValues, out fieldValue1, ref bitOffset);
                    if (num != 0 || bitOffset != (byte)0)
                    {
                        arrayList.Add(fieldValue1);
                        context.Index += num;
                    }
                    else
                        break;
                }
            }
            else if (field.ElementCountRef != null)
            {
                int num1 = BinaryReader.ReadReference(context, field, fieldIndex, fieldValues, field.ElementCountRef);
                for (int index3 = 0; index3 < num1; ++index3)
                {
                    int num2 = ReadField(context, field, fieldIndex, fieldValues, out fieldValue1, ref bitOffset);
                    if (num2 != 0 || bitOffset != (byte)0)
                    {
                        arrayList.Add(fieldValue1);
                        context.Index += num2;
                    }
                    else
                        break;
                }
            }
            else if (field.FieldTerminator != null)
            {
                byte[] terminator = BinaryStream.GetTerminator(context, field);
                int num;
                for (; context.Index < context.Buffer.Length; context.Index += num)
                {
                    bool flag = true;
                    for (int index4 = 0; index4 < terminator.Length; ++index4)
                    {
                        if ((int)terminator[index4] != (int)context.Buffer[context.Index + index4])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        context.Index += terminator.Length;
                        break;
                    }
                    num = ReadField(context, field, fieldIndex, fieldValues, out fieldValue1, ref bitOffset);
                    if (num != 0 || bitOffset != (byte)0)
                        arrayList.Add(fieldValue1);
                    else
                        break;
                }
            }
            if (bitOffset != (byte)0)
                ++context.Index;
            System.Type type = (System.Type)null;
            foreach (object obj in arrayList)
            {
                if (type == (System.Type)null)
                    type = obj.GetType();
                else if (type != obj.GetType())
                {
                    type = typeof(object);
                    break;
                }
            }
            fieldValue = (object)arrayList.ToArray(type);
            return context.Index - index1;
        }

        private static int ReadReference(
          Context context,
          FieldType field,
          int fieldIndex,
          ArrayList fieldValues,
          string fieldName)
        {
            ComplexValue complexValue = (ComplexValue)null;
            if (fieldName.Length == 0)
            {
                if (fieldIndex > 0 && fieldIndex - 1 < fieldValues.Count)
                    complexValue = (ComplexValue)fieldValues[fieldIndex - 1];
            }
            else
            {
                for (int index = 0; index < fieldIndex; ++index)
                {
                    complexValue = (ComplexValue)fieldValues[index];
                    if (!(complexValue.Name == fieldName))
                        complexValue = (ComplexValue)null;
                    else
                        break;
                }
            }
            return complexValue != null ? System.Convert.ToInt32(complexValue.Value) : throw new InvalidSchemaException("Referenced field not found (" + fieldName + ").");
        }
    }
}
