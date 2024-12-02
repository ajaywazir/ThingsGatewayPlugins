

using System;


namespace Opc.Hda
{
    [Serializable]
    public struct TimeOffset
    {
        private int m_value;
        private RelativeTime m_type;

        public int Value
        {
            get => m_value;
            set => m_value = value;
        }

        public RelativeTime Type
        {
            get => m_type;
            set => m_type = value;
        }

        internal static string OffsetTypeToString(RelativeTime offsetType)
        {
            switch (offsetType)
            {
                case RelativeTime.Second:
                    return "S";
                case RelativeTime.Minute:
                    return "M";
                case RelativeTime.Hour:
                    return "H";
                case RelativeTime.Day:
                    return "D";
                case RelativeTime.Week:
                    return "W";
                case RelativeTime.Month:
                    return "MO";
                case RelativeTime.Year:
                    return "Y";
                default:
                    throw new ArgumentOutOfRangeException(nameof(offsetType), (object)offsetType.ToString(), "Invalid value for relative time offset type.");
            }
        }
    }
}
