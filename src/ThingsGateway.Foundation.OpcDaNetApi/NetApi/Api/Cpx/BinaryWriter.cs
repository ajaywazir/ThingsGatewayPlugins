

using System;


namespace Opc.Cpx
{
    public class BinaryWriter : BinaryStream
    {
        public byte[] Write(ComplexValue namedValue, TypeDictionary dictionary, string typeName)
        {
            if (namedValue == null)
                throw new ArgumentNullException(nameof(namedValue));
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            Context context = typeName != null ? BinaryStream.InitializeContext((byte[])null, dictionary, typeName) : throw new ArgumentNullException(nameof(typeName));
            int length = WriteType(context, namedValue);
            context.Buffer = length != 0 ? new byte[length] : throw new InvalidDataToWriteException("Could not write value into buffer.");
            if (WriteType(context, namedValue) != length)
                throw new InvalidDataToWriteException("Could not write value into buffer.");
            return context.Buffer;
        }

        private int WriteType(Context context, ComplexValue namedValue)
        {
            TypeDescription type = context.Type;
            int index = context.Index;
            ComplexValue[] fieldValues = namedValue.Value != null && !(namedValue.Value.GetType() != typeof(ComplexValue[])) ? (ComplexValue[])namedValue.Value : throw new InvalidDataToWriteException("Type instance does not contain field values.");
            if (fieldValues.Length != type.Field.Length)
                throw new InvalidDataToWriteException("Type instance does not contain the correct number of fields.");
            byte bitOffset = 0;
            for (int fieldIndex = 0; fieldIndex < type.Field.Length; ++fieldIndex)
            {
                FieldType field = type.Field[fieldIndex];
                ComplexValue fieldValue = fieldValues[fieldIndex];
                if (bitOffset != (byte)0 && field.GetType() != typeof(BitString))
                {
                    ++context.Index;
                    bitOffset = (byte)0;
                }
                int num = !BinaryStream.IsArrayField(field) ? (!(field.GetType() == typeof(TypeReference)) ? WriteField(context, field, fieldIndex, fieldValues, fieldValue.Value, ref bitOffset) : WriteField(context, (TypeReference)field, (object)fieldValue)) : WriteArrayField(context, field, fieldIndex, fieldValues, fieldValue.Value);
                if (num == 0 && bitOffset == (byte)0)
                    throw new InvalidDataToWriteException("Could not write field '" + field.Name + "' in type '" + type.TypeID + "'.");
                context.Index += num;
            }
            if (bitOffset != (byte)0)
                ++context.Index;
            return context.Index - index;
        }

        private int WriteField(
          Context context,
          FieldType field,
          int fieldIndex,
          ComplexValue[] fieldValues,
          object fieldValue,
          ref byte bitOffset)
        {
            System.Type type = field.GetType();
            if (type == typeof(Integer) || type.IsSubclassOf(typeof(Integer)))
                return WriteField(context, (Integer)field, fieldValue);
            if (type == typeof(FloatingPoint) || type.IsSubclassOf(typeof(FloatingPoint)))
                return BinaryWriter.WriteField(context, (FloatingPoint)field, fieldValue);
            if (type == typeof(CharString) || type.IsSubclassOf(typeof(CharString)))
                return WriteField(context, (CharString)field, fieldIndex, fieldValues, fieldValue);
            if (type == typeof(BitString) || type.IsSubclassOf(typeof(BitString)))
                return BinaryWriter.WriteField(context, (BitString)field, fieldValue, ref bitOffset);
            if (type == typeof(TypeReference) || type.IsSubclassOf(typeof(TypeReference)))
                return WriteField(context, (TypeReference)field, fieldValue);
            throw new NotImplementedException("Fields of type '" + type.ToString() + "' are not implemented yet.");
        }

        private int WriteField(Context context, TypeReference field, object fieldValue)
        {
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
            return !(fieldValue.GetType() != typeof(ComplexValue)) ? WriteType(context, (ComplexValue)fieldValue) : throw new InvalidDataToWriteException("Instance of type is not the correct type.");
        }

        private static int WriteField(Context context, Integer field, object fieldValue)
        {
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
            if (buffer != null)
            {
                if (buffer.Length - context.Index < length)
                    throw new InvalidDataToWriteException("Unexpected end of buffer.");
                byte[] bytes;
                if (flag)
                {
                    switch (length)
                    {
                        case 1:
                            bytes = new byte[1];
                            sbyte num = System.Convert.ToSByte(fieldValue);
                            bytes[0] = num >= (sbyte)0 ? (byte)num : (byte)((int)byte.MaxValue + (int)num + 1);
                            break;
                        case 2:
                            bytes = BitConverter.GetBytes(System.Convert.ToInt16(fieldValue));
                            break;
                        case 4:
                            bytes = BitConverter.GetBytes(System.Convert.ToInt32(fieldValue));
                            break;
                        case 8:
                            bytes = BitConverter.GetBytes(System.Convert.ToInt64(fieldValue));
                            break;
                        default:
                            bytes = (byte[])fieldValue;
                            break;
                    }
                }
                else
                {
                    switch (length)
                    {
                        case 1:
                            bytes = new byte[1]
                            {
                System.Convert.ToByte(fieldValue)
                            };
                            break;
                        case 2:
                            bytes = BitConverter.GetBytes(System.Convert.ToUInt16(fieldValue));
                            break;
                        case 4:
                            bytes = BitConverter.GetBytes(System.Convert.ToUInt32(fieldValue));
                            break;
                        case 8:
                            bytes = BitConverter.GetBytes(System.Convert.ToUInt64(fieldValue));
                            break;
                        default:
                            bytes = (byte[])fieldValue;
                            break;
                    }
                }
                if (context.BigEndian)
                    BinaryStream.SwapBytes(bytes, 0, length);
                for (int index = 0; index < bytes.Length; ++index)
                    buffer[context.Index + index] = bytes[index];
            }
            return length;
        }

        private static int WriteField(Context context, FloatingPoint field, object fieldValue)
        {
            byte[] buffer = context.Buffer;
            int num = field.LengthSpecified ? field.Length : 4;
            string str = field.FloatFormat != null ? field.FloatFormat : context.FloatFormat;
            if (field.GetType() == typeof(Single))
            {
                num = 4;
                str = "IEEE-754";
            }
            else if (field.GetType() == typeof(Double))
            {
                num = 8;
                str = "IEEE-754";
            }
            if (buffer != null)
            {
                if (buffer.Length - context.Index < num)
                    throw new InvalidDataToWriteException("Unexpected end of buffer.");
                byte[] numArray;
                if (str == "IEEE-754")
                {
                    switch (num)
                    {
                        case 4:
                            numArray = BitConverter.GetBytes(System.Convert.ToSingle(fieldValue));
                            break;
                        case 8:
                            numArray = BitConverter.GetBytes(System.Convert.ToDouble(fieldValue));
                            break;
                        default:
                            numArray = (byte[])fieldValue;
                            break;
                    }
                }
                else
                    numArray = (byte[])fieldValue;
                for (int index = 0; index < numArray.Length; ++index)
                    buffer[context.Index + index] = numArray[index];
            }
            return num;
        }

        private static int WriteField(
          Context context,
          CharString field,
          int fieldIndex,
          ComplexValue[] fieldValues,
          object fieldValue)
        {
            byte[] buffer = context.Buffer;
            int length = field.CharWidthSpecified ? field.CharWidth : context.CharWidth;
            int count = field.LengthSpecified ? field.Length : -1;
            if (field.GetType() == typeof(Ascii))
                length = 1;
            else if (field.GetType() == typeof(Unicode))
                length = 2;
            byte[] numArray = (byte[])null;
            if (count == -1)
            {
                if (length > 2)
                {
                    numArray = !(fieldValue.GetType() != typeof(byte[])) ? (byte[])fieldValue : throw new InvalidDataToWriteException("Field value is not a byte array.");
                    count = numArray.Length / length;
                }
                else
                {
                    string str = !(fieldValue.GetType() != typeof(string)) ? (string)fieldValue : throw new InvalidDataToWriteException("Field value is not a string.");
                    count = str.Length + 1;
                    if (length == 1)
                    {
                        count = 1;
                        foreach (int num in str)
                        {
                            ++count;
                            if (BitConverter.GetBytes((char)num)[1] != (byte)0)
                                ++count;
                        }
                    }
                }
            }
            if (field.CharCountRef != null)
                BinaryWriter.WriteReference(context, (FieldType)field, fieldIndex, fieldValues, field.CharCountRef, count);
            if (buffer != null)
            {
                if (numArray == null)
                {
                    string str = (string)fieldValue;
                    numArray = new byte[length * count];
                    int num = 0;
                    for (int index = 0; index < str.Length && num < numArray.Length; ++index)
                    {
                        byte[] bytes = BitConverter.GetBytes(str[index]);
                        numArray[num++] = bytes[0];
                        if (length == 2 || bytes[1] != (byte)0)
                            numArray[num++] = bytes[1];
                    }
                }
                if (buffer.Length - context.Index < numArray.Length)
                    throw new InvalidDataToWriteException("Unexpected end of buffer.");
                for (int index = 0; index < numArray.Length; ++index)
                    buffer[context.Index + index] = numArray[index];
                if (context.BigEndian && length > 1)
                {
                    for (int index = 0; index < numArray.Length; index += length)
                        BinaryStream.SwapBytes(buffer, context.Index + index, length);
                }
            }
            return count * length;
        }

        private static int WriteField(
          Context context,
          BitString field,
          object fieldValue,
          ref byte bitOffset)
        {
            byte[] buffer = context.Buffer;
            int num1 = field.LengthSpecified ? field.Length : 8;
            int num2 = num1 % 8 == 0 ? num1 / 8 : num1 / 8 + 1;
            byte[] numArray = !(fieldValue.GetType() != typeof(byte[])) ? (byte[])fieldValue : throw new InvalidDataToWriteException("Wrong data type to write.");
            if (buffer != null)
            {
                if (buffer.Length - context.Index < num2)
                    throw new InvalidDataToWriteException("Unexpected end of buffer.");
                int num3 = num1;
                byte num4 = bitOffset == (byte)0 ? byte.MaxValue : (byte)((128 >> (int)bitOffset - 1) - 1);
                for (int index = 0; num3 >= 0 && index < num2; ++index)
                {
                    buffer[context.Index + index] += (byte)(((uint)num4 & (uint)((1 << num3) - 1) & (uint)numArray[index]) << (int)bitOffset);
                    if (num3 + (int)bitOffset > 8)
                    {
                        if (context.Index + index + 1 >= buffer.Length)
                            throw new InvalidDataToWriteException("Unexpected end of buffer.");
                        buffer[context.Index + index + 1] += (byte)(((int)~num4 & (1 << num3) - 1 & (int)numArray[index]) >> 8 - (int)bitOffset);
                        if (num3 > 8)
                            num3 -= 8;
                        else
                            break;
                    }
                    else
                        break;
                }
            }
            int num5 = (num1 + (int)bitOffset) / 8;
            bitOffset = (byte)((num1 + (int)bitOffset) % 8);
            return num5;
        }

        private int WriteArrayField(
          Context context,
          FieldType field,
          int fieldIndex,
          ComplexValue[] fieldValues,
          object fieldValue)
        {
            int index1 = context.Index;
            Array array = fieldValue.GetType().IsArray ? (Array)fieldValue : throw new InvalidDataToWriteException("Array field value is not an array type.");
            byte bitOffset = 0;
            if (field.ElementCountSpecified)
            {
                int num1 = 0;
                foreach (object fieldValue1 in array)
                {
                    if (num1 != field.ElementCount)
                    {
                        int num2 = WriteField(context, field, fieldIndex, fieldValues, fieldValue1, ref bitOffset);
                        if (num2 == 0)
                        {
                            if (bitOffset == (byte)0)
                                break;
                        }
                        context.Index += num2;
                        ++num1;
                    }
                    else
                        break;
                }
                for (; num1 < field.ElementCount; ++num1)
                {
                    int num3 = WriteField(context, field, fieldIndex, fieldValues, (object)null, ref bitOffset);
                    if (num3 != 0 || bitOffset != (byte)0)
                        context.Index += num3;
                    else
                        break;
                }
            }
            else if (field.ElementCountRef != null)
            {
                int count = 0;
                foreach (object fieldValue2 in array)
                {
                    int num = WriteField(context, field, fieldIndex, fieldValues, fieldValue2, ref bitOffset);
                    if (num == 0)
                    {
                        if (bitOffset == (byte)0)
                            break;
                    }
                    context.Index += num;
                    ++count;
                }

                BinaryWriter.WriteReference(context, field, fieldIndex, fieldValues, field.ElementCountRef, count);
            }
            else if (field.FieldTerminator != null)
            {
                foreach (object fieldValue3 in array)
                {
                    int num = WriteField(context, field, fieldIndex, fieldValues, fieldValue3, ref bitOffset);
                    if (num == 0)
                    {
                        if (bitOffset == (byte)0)
                            break;
                    }
                    context.Index += num;
                }

                byte[] terminator = BinaryStream.GetTerminator(context, field);
                if (context.Buffer != null)
                {
                    for (int index2 = 0; index2 < terminator.Length; ++index2)
                        context.Buffer[context.Index + index2] = terminator[index2];
                }
                context.Index += terminator.Length;
            }
            if (bitOffset != (byte)0)
                ++context.Index;
            return context.Index - index1;
        }

        private static void WriteReference(
          Context context,
          FieldType field,
          int fieldIndex,
          ComplexValue[] fieldValues,
          string fieldName,
          int count)
        {
            ComplexValue complexValue = (ComplexValue)null;
            if (fieldName.Length == 0)
            {
                if (fieldIndex > 0 && fieldIndex - 1 < fieldValues.Length)
                    complexValue = fieldValues[fieldIndex - 1];
            }
            else
            {
                for (int index = 0; index < fieldIndex; ++index)
                {
                    complexValue = fieldValues[index];
                    if (!(complexValue.Name == fieldName))
                        complexValue = (ComplexValue)null;
                    else
                        break;
                }
            }
            if (complexValue == null)
                throw new InvalidSchemaException("Referenced field not found (" + fieldName + ").");
            if (context.Buffer == null)
                complexValue.Value = (object)count;
            if (!count.Equals(complexValue.Value))
                throw new InvalidDataToWriteException("Reference field value and the actual array length are not equal.");
        }
    }
}
