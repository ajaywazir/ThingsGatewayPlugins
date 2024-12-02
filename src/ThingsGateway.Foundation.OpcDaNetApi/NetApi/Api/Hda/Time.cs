

using System;
using System.Collections;
using System.Text;


namespace Opc.Hda
{
    [Serializable]
    public class Time
    {
        private DateTime m_absoluteTime = DateTime.MinValue;
        private RelativeTime m_baseTime;
        private TimeOffsetCollection m_offsets = new TimeOffsetCollection();

        public Time()
        {
        }

        public Time(DateTime time) => AbsoluteTime = time;

        public Time(string time)
        {
            Time time1 = Time.Parse(time);
            m_absoluteTime = DateTime.MinValue;
            m_baseTime = time1.m_baseTime;
            m_offsets = time1.m_offsets;
        }

        public bool IsRelative
        {
            get => m_absoluteTime == DateTime.MinValue;
            set => m_absoluteTime = DateTime.MinValue;
        }

        public DateTime AbsoluteTime
        {
            get => m_absoluteTime;
            set => m_absoluteTime = value;
        }

        public RelativeTime BaseTime
        {
            get => m_baseTime;
            set => m_baseTime = value;
        }

        public TimeOffsetCollection Offsets => m_offsets;

        public DateTime ResolveTime()
        {
            if (!IsRelative)
                return m_absoluteTime;
            DateTime dateTime = DateTime.UtcNow;
            int year = dateTime.Year;
            int month = dateTime.Month;
            int day = dateTime.Day;
            int hour = dateTime.Hour;
            int minute = dateTime.Minute;
            int second = dateTime.Second;
            int millisecond = dateTime.Millisecond;
            switch (BaseTime)
            {
                case RelativeTime.Second:
                    millisecond = 0;
                    break;
                case RelativeTime.Minute:
                    second = 0;
                    millisecond = 0;
                    break;
                case RelativeTime.Hour:
                    minute = 0;
                    second = 0;
                    millisecond = 0;
                    break;
                case RelativeTime.Day:
                case RelativeTime.Week:
                    hour = 0;
                    minute = 0;
                    second = 0;
                    millisecond = 0;
                    break;
                case RelativeTime.Month:
                    day = 0;
                    hour = 0;
                    minute = 0;
                    second = 0;
                    millisecond = 0;
                    break;
                case RelativeTime.Year:
                    month = 0;
                    day = 0;
                    hour = 0;
                    minute = 0;
                    second = 0;
                    millisecond = 0;
                    break;
            }
            dateTime = new DateTime(year, month, day, hour, minute, second, millisecond);
            if (BaseTime == RelativeTime.Week && dateTime.DayOfWeek != DayOfWeek.Sunday)
                dateTime = dateTime.AddDays((double)-(int)dateTime.DayOfWeek);
            foreach (TimeOffset offset in (ArrayList)Offsets)
            {
                switch (offset.Type)
                {
                    case RelativeTime.Second:
                        dateTime = dateTime.AddSeconds((double)offset.Value);
                        continue;
                    case RelativeTime.Minute:
                        dateTime = dateTime.AddMinutes((double)offset.Value);
                        continue;
                    case RelativeTime.Hour:
                        dateTime = dateTime.AddHours((double)offset.Value);
                        continue;
                    case RelativeTime.Day:
                        dateTime = dateTime.AddDays((double)offset.Value);
                        continue;
                    case RelativeTime.Week:
                        dateTime = dateTime.AddDays((double)(offset.Value * 7));
                        continue;
                    case RelativeTime.Month:
                        dateTime = dateTime.AddMonths(offset.Value);
                        continue;
                    case RelativeTime.Year:
                        dateTime = dateTime.AddYears(offset.Value);
                        continue;
                    default:
                        continue;
                }
            }
            return dateTime;
        }

        public override string ToString()
        {
            if (!IsRelative)
                return Opc.Convert.ToString((object)m_absoluteTime);
            StringBuilder stringBuilder = new StringBuilder(256);
            stringBuilder.Append(Time.BaseTypeToString(BaseTime));
            stringBuilder.Append(Offsets.ToString());
            return stringBuilder.ToString();
        }

        public static Time Parse(string buffer)
        {
            buffer = buffer.Trim();
            Time time = new Time();
            bool flag = false;
            foreach (RelativeTime baseTime in Enum.GetValues(typeof(RelativeTime)))
            {
                string str = Time.BaseTypeToString(baseTime);
                if (buffer.StartsWith(str))
                {
                    buffer = buffer.Substring(str.Length).Trim();
                    time.BaseTime = baseTime;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                time.AbsoluteTime = System.Convert.ToDateTime(buffer).ToUniversalTime();
                return time;
            }
            if (buffer.Length > 0)
                time.Offsets.Parse(buffer);
            return time;
        }

        private static string BaseTypeToString(RelativeTime baseTime)
        {
            switch (baseTime)
            {
                case RelativeTime.Now:
                    return "NOW";
                case RelativeTime.Second:
                    return "SECOND";
                case RelativeTime.Minute:
                    return "MINUTE";
                case RelativeTime.Hour:
                    return "HOUR";
                case RelativeTime.Day:
                    return "DAY";
                case RelativeTime.Week:
                    return "WEEK";
                case RelativeTime.Month:
                    return "MONTH";
                case RelativeTime.Year:
                    return "YEAR";
                default:
                    throw new ArgumentOutOfRangeException(nameof(baseTime), (object)baseTime.ToString(), "Invalid value for relative base time.");
            }
        }
    }
}
