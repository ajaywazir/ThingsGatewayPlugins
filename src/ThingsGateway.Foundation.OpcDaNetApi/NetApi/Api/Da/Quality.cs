

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
      get => this.m_qualityBits;
      set => this.m_qualityBits = value;
    }

    public limitBits LimitBits
    {
      get => this.m_limitBits;
      set => this.m_limitBits = value;
    }

    public byte VendorBits
    {
      get => this.m_vendorBits;
      set => this.m_vendorBits = value;
    }

    public short GetCode()
    {
      ushort num = (ushort) ((uint) (ushort) ((uint) (ushort) (0U | (uint) (ushort) this.QualityBits) | (uint) (ushort) this.LimitBits) | (uint) (ushort) ((uint) this.VendorBits << 8));
      return num > (ushort) short.MaxValue ? (short) -(65536 - (int) num) : (short) num;
    }

    public void SetCode(short code)
    {
      this.m_qualityBits = (qualityBits) ((int) code & 252);
      this.m_limitBits = (limitBits) ((int) code & 3);
      this.m_vendorBits = (byte) (((int) code & -253) >> 8);
    }

    public static bool operator ==(Quality a, Quality b) => a.Equals((object) b);

    public static bool operator !=(Quality a, Quality b) => !a.Equals((object) b);

    public Quality(qualityBits quality)
    {
      this.m_qualityBits = quality;
      this.m_limitBits = limitBits.none;
      this.m_vendorBits = (byte) 0;
    }

    public Quality(short code)
    {
      this.m_qualityBits = (qualityBits) ((int) code & 252);
      this.m_limitBits = (limitBits) ((int) code & 3);
      this.m_vendorBits = (byte) (((int) code & -253) >> 8);
    }

    public override string ToString()
    {
      string str = this.QualityBits.ToString();
      if (this.LimitBits != limitBits.none)
        str += string.Format("[{0}]", (object) this.LimitBits.ToString());
      if (this.VendorBits != (byte) 0)
        str += string.Format(":{0,0:X}", (object) this.VendorBits);
      return str;
    }

    public override bool Equals(object target)
    {
      if (target == null || target.GetType() != typeof (Quality))
        return false;
      Quality quality = (Quality) target;
      return this.QualityBits == quality.QualityBits && this.LimitBits == quality.LimitBits && (int) this.VendorBits == (int) quality.VendorBits;
    }

    public override int GetHashCode() => (int) this.GetCode();
  }
}
