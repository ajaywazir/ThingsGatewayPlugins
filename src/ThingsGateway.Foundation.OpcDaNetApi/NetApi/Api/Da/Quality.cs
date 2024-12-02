

using System;


namespace Opc.Da
{
    [Serializable]
    public struct Quality
    {
        private qualityBits m_qualityBits;
        private limitBits m_limitBits;
        private byte m_vendorBits;
        public static readonly Quality Good = new Quality(qualityBits.good);
        public static readonly Quality Bad = new Quality(qualityBits.bad);

        public qualityBits QualityBits
        {
            get => m_qualityBits;
            set => m_qualityBits = value;
        }

        public limitBits LimitBits
        {
            get => m_limitBits;
            set => m_limitBits = value;
        }

        public byte VendorBits
        {
            get => m_vendorBits;
            set => m_vendorBits = value;
        }

        public short GetCode()
        {
            ushort num = (ushort)((uint)(ushort)((uint)(ushort)(0U | (uint)(ushort)QualityBits) | (uint)(ushort)LimitBits) | (uint)(ushort)((uint)VendorBits << 8));
            return num > (ushort)short.MaxValue ? (short)-(65536 - (int)num) : (short)num;
        }

        public void SetCode(short code)
        {
            m_qualityBits = (qualityBits)((int)code & 252);
            m_limitBits = (limitBits)((int)code & 3);
            m_vendorBits = (byte)(((int)code & -253) >> 8);
        }

        public static bool operator ==(Quality a, Quality b) => a.Equals((object)b);

        public static bool operator !=(Quality a, Quality b) => !a.Equals((object)b);

        public Quality(qualityBits quality)
        {
            m_qualityBits = quality;
            m_limitBits = limitBits.none;
            m_vendorBits = (byte)0;
        }

        public Quality(short code)
        {
            m_qualityBits = (qualityBits)((int)code & 252);
            m_limitBits = (limitBits)((int)code & 3);
            m_vendorBits = (byte)(((int)code & -253) >> 8);
        }

        public override string ToString()
        {
            string str = QualityBits.ToString();
            if (LimitBits != limitBits.none)
                str += string.Format("[{0}]", (object)LimitBits.ToString());
            if (VendorBits != (byte)0)
                str += string.Format(":{0,0:X}", (object)VendorBits);
            return str;
        }

        public override bool Equals(object target)
        {
            if (target == null || target.GetType() != typeof(Quality))
                return false;
            Quality quality = (Quality)target;
            return QualityBits == quality.QualityBits && LimitBits == quality.LimitBits && (int)VendorBits == (int)quality.VendorBits;
        }

        public override int GetHashCode() => (int)GetCode();
    }
}
