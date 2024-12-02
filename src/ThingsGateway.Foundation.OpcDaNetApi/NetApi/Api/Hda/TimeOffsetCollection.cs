

using System;
using System.Collections;
using System.Text;


namespace Opc.Hda
{
    [Serializable]
    public class TimeOffsetCollection : ArrayList
    {
        public new TimeOffset this[int index]
        {
            get => (TimeOffset)base[index];
            set => base[index] = value;
        }

        public int Add(int value, RelativeTime type)
        {
            return Add((object)new TimeOffset()
            {
                Value = value,
                Type = type
            });
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            foreach (TimeOffset timeOffset in (IEnumerable)this)
            {
                if (timeOffset.Value >= 0)
                    stringBuilder.Append('+');
                stringBuilder.AppendFormat("{0}", (object)timeOffset.Value);
                stringBuilder.Append(TimeOffset.OffsetTypeToString(timeOffset.Type));
            }
            return stringBuilder.ToString();
        }

        public void Parse(string buffer)
        {
            Clear();
            bool positive = true;
            int magnitude = 0;
            string units = "";
            int num = 0;
            for (int index = 0; index < buffer.Length; ++index)
            {
                if (buffer[index] == '+' || buffer[index] == '-')
                {
                    if (num == 3)
                    {
                        Add(TimeOffsetCollection.CreateOffset(positive, magnitude, units));
                        magnitude = 0;
                        units = "";
                        num = 0;
                    }
                    if (num != 0)
                        throw new FormatException("Unexpected token encountered while parsing relative time string.");
                    positive = buffer[index] == '+';
                    num = 1;
                }
                else if (char.IsDigit(buffer, index))
                {
                    if (num == 3)
                    {
                        Add(TimeOffsetCollection.CreateOffset(positive, magnitude, units));
                        magnitude = 0;
                        units = "";
                        num = 0;
                    }
                    if (num != 0 && num != 1 && num != 2)
                        throw new FormatException("Unexpected token encountered while parsing relative time string.");
                    magnitude = magnitude * 10 + System.Convert.ToInt32((int)buffer[index] - 48);
                    num = 2;
                }
                else if (!char.IsWhiteSpace(buffer, index))
                {
                    if (num != 2 && num != 3)
                        throw new FormatException("Unexpected token encountered while parsing relative time string.");
                    units += buffer[index].ToString();
                    num = 3;
                }
            }
            if (num == 3)
            {
                Add(TimeOffsetCollection.CreateOffset(positive, magnitude, units));
                num = 0;
            }
            if (num != 0)
                throw new FormatException("Unexpected end of string encountered while parsing relative time string.");
        }

        public void CopyTo(TimeOffset[] array, int index) => CopyTo((Array)array, index);

        public void Insert(int index, TimeOffset value) => Insert(index, (object)value);

        public void Remove(TimeOffset value) => Remove((object)value);

        public bool Contains(TimeOffset value) => Contains((object)value);

        public int IndexOf(TimeOffset value) => IndexOf((object)value);

        public int Add(TimeOffset value) => Add((object)value);

        private static TimeOffset CreateOffset(bool positive, int magnitude, string units)
        {
            foreach (RelativeTime offsetType in Enum.GetValues(typeof(RelativeTime)))
            {
                if (offsetType != RelativeTime.Now && units == TimeOffset.OffsetTypeToString(offsetType))
                    return new TimeOffset()
                    {
                        Value = positive ? magnitude : -magnitude,
                        Type = offsetType
                    };
            }
            throw new ArgumentOutOfRangeException(nameof(units), (object)units, "String is not a valid offset time type.");
        }
    }
}
