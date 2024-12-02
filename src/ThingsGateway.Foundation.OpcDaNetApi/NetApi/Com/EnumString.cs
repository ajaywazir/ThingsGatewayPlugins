

using OpcRcw.Comn;

using System;
using System.Runtime.InteropServices;


namespace OpcCom
{
    public class EnumString : IDisposable
    {
        private IEnumString m_enumerator;

        public EnumString(object enumerator) => m_enumerator = (IEnumString)enumerator;

        public void Dispose()
        {
            Interop.ReleaseServer((object)m_enumerator);
            m_enumerator = (IEnumString)null;
        }

        public string[] Next(int count)
        {
            try
            {
                IntPtr pArray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(nint)) * count);
                try
                {
                    int pceltFetched = 0;
                    m_enumerator.RemoteNext(count, pArray, out pceltFetched);
                    return pceltFetched == 0 ? Array.Empty<string>() : Interop.GetUnicodeStrings(ref pArray, pceltFetched, true);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pArray);
                }
            }
            catch
            {
                return (string[])null;
            }
        }

        public void Skip(int count) => m_enumerator.Skip(count);

        public void Reset() => m_enumerator.Reset();

        public EnumString Clone()
        {
            IEnumString ppenum = (IEnumString)null;
            m_enumerator.Clone(out ppenum);
            return new EnumString((object)ppenum);
        }
    }
}
