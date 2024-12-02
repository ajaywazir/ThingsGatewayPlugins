

using Opc;
using Opc.Da;

using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml;


namespace OpcCom
{
    public class Interop
    {
        private const uint LEVEL_SERVER_INFO_100 = 100;
        private const uint LEVEL_SERVER_INFO_101 = 101;
        private const int MAX_PREFERRED_LENGTH = -1;
        private const uint SV_TYPE_WORKSTATION = 1;
        private const uint SV_TYPE_SERVER = 2;
        private const int MAX_MESSAGE_LENGTH = 1024;
        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 512;
        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 4096;
        private const int MAX_COMPUTERNAME_LENGTH = 31;
        private const uint RPC_C_AUTHN_NONE = 0;
        private const uint RPC_C_AUTHN_DCE_PRIVATE = 1;
        private const uint RPC_C_AUTHN_DCE_PUBLIC = 2;
        private const uint RPC_C_AUTHN_DEC_PUBLIC = 4;
        private const uint RPC_C_AUTHN_GSS_NEGOTIATE = 9;
        private const uint RPC_C_AUTHN_WINNT = 10;
        private const uint RPC_C_AUTHN_GSS_SCHANNEL = 14;
        private const uint RPC_C_AUTHN_GSS_KERBEROS = 16;
        private const uint RPC_C_AUTHN_DPA = 17;
        private const uint RPC_C_AUTHN_MSN = 18;
        private const uint RPC_C_AUTHN_DIGEST = 21;
        private const uint RPC_C_AUTHN_MQ = 100;
        private const uint RPC_C_AUTHN_DEFAULT = 4294967295;
        private const uint RPC_C_AUTHZ_NONE = 0;
        private const uint RPC_C_AUTHZ_NAME = 1;
        private const uint RPC_C_AUTHZ_DCE = 2;
        private const uint RPC_C_AUTHZ_DEFAULT = 4294967295;
        private const uint RPC_C_AUTHN_LEVEL_DEFAULT = 0;
        private const uint RPC_C_AUTHN_LEVEL_NONE = 1;
        private const uint RPC_C_AUTHN_LEVEL_CONNECT = 2;
        private const uint RPC_C_AUTHN_LEVEL_CALL = 3;
        private const uint RPC_C_AUTHN_LEVEL_PKT = 4;
        private const uint RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5;
        private const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;
        private const uint RPC_C_IMP_LEVEL_ANONYMOUS = 1;
        private const uint RPC_C_IMP_LEVEL_IDENTIFY = 2;
        private const uint RPC_C_IMP_LEVEL_IMPERSONATE = 3;
        private const uint RPC_C_IMP_LEVEL_DELEGATE = 4;
        private const uint EOAC_NONE = 0;
        private const uint EOAC_MUTUAL_AUTH = 1;
        private const uint EOAC_CLOAKING = 16;
        private const uint EOAC_SECURE_REFS = 2;
        private const uint EOAC_ACCESS_CONTROL = 4;
        private const uint EOAC_APPID = 8;
        private const uint CLSCTX_INPROC_SERVER = 1;
        private const uint CLSCTX_INPROC_HANDLER = 2;
        private const uint CLSCTX_LOCAL_SERVER = 4;
        private const uint CLSCTX_REMOTE_SERVER = 16;
        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private const uint SEC_WINNT_AUTH_IDENTITY_ANSI = 1;
        private const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 2;
        private static bool m_preserveUTC = false;
        private static readonly DateTime FILETIME_BaseTime = new DateTime(1601, 1, 1);
        internal const int LOCALE_SYSTEM_DEFAULT = 2048;
        internal const int LOCALE_USER_DEFAULT = 1024;

        [DllImport("Netapi32.dll")]
        private static extern int NetServerEnum(
          IntPtr servername,
          uint level,
          out IntPtr bufptr,
          int prefmaxlen,
          out int entriesread,
          out int totalentries,
          uint servertype,
          IntPtr domain,
          IntPtr resume_handle);

        [DllImport("Netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);

        public static string[] EnumComputers()
        {
            int entriesread = 0;
            int totalentries = 0;
            IntPtr bufptr;
            int num = Interop.NetServerEnum(IntPtr.Zero, 100U, out bufptr, -1, out entriesread, out totalentries, 3U, IntPtr.Zero, IntPtr.Zero);
            if (num != 0)
                throw new ApplicationException("NetApi Error = " + string.Format("0x{0,0:X}", (object)num));
            string[] strArray = new string[entriesread];
            IntPtr ptr = bufptr;
            for (int index = 0; index < entriesread; ++index)
            {
                Interop.SERVER_INFO_100 structure = (Interop.SERVER_INFO_100)Marshal.PtrToStructure(ptr, typeof(Interop.SERVER_INFO_100));
                strArray[index] = structure.sv100_name;
                ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(Interop.SERVER_INFO_100)));
            }
            _ = Interop.NetApiBufferFree(bufptr);
            return strArray;
        }

        [DllImport("Kernel32.dll")]
        private static extern int FormatMessageW(
          int dwFlags,
          IntPtr lpSource,
          int dwMessageId,
          int dwLanguageId,
          IntPtr lpBuffer,
          int nSize,
          IntPtr Arguments);

        public static string GetSystemMessage(int error)
        {
            IntPtr num = Marshal.AllocCoTaskMem(1024);
            _ = Interop.FormatMessageW(4096, IntPtr.Zero, error, 0, num, 1023, IntPtr.Zero);
            string stringUni = Marshal.PtrToStringUni(num);
            Marshal.FreeCoTaskMem(num);
            return stringUni != null && stringUni.Length > 0 ? stringUni : string.Format("0x{0,0:X}", (object)error);
        }

        [DllImport("Kernel32.dll")]
        private static extern int GetComputerNameW(IntPtr lpBuffer, ref int lpnSize);

        public static string GetComputerName()
        {
            string computerName = (string)null;
            int lpnSize = 32;
            IntPtr num = Marshal.AllocCoTaskMem(lpnSize * 2);
            if (Interop.GetComputerNameW(num, ref lpnSize) != 0)
                computerName = Marshal.PtrToStringUni(num, lpnSize);
            Marshal.FreeCoTaskMem(num);
            return computerName;
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeSecurity(
          IntPtr pSecDesc,
          int cAuthSvc,
          Interop.SOLE_AUTHENTICATION_SERVICE[] asAuthSvc,
          IntPtr pReserved1,
          uint dwAuthnLevel,
          uint dwImpLevel,
          IntPtr pAuthList,
          uint dwCapabilities,
          IntPtr pReserved3);

        [DllImport("ole32.dll")]
        private static extern void CoCreateInstanceEx(
          ref Guid clsid,
          [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
          uint dwClsCtx,
          [In] ref Interop.COSERVERINFO pServerInfo,
          uint dwCount,
          [In, Out] Interop.MULTI_QI[] pResults);

        [DllImport("ole32.dll")]
        private static extern void CoGetClassObject(
          [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
          uint dwClsContext,
          [In] ref Interop.COSERVERINFO pServerInfo,
          [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
          [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        public static void InitializeSecurity()
        {
            int num = Interop.CoInitializeSecurity(IntPtr.Zero, -1, (Interop.SOLE_AUTHENTICATION_SERVICE[])null, IntPtr.Zero, 1U, 2U, IntPtr.Zero, 0U, IntPtr.Zero);
            if (num != 0)
                throw new ExternalException("CoInitializeSecurity: " + Interop.GetSystemMessage(num), num);
        }

        public static object CreateInstance(Guid clsid, string hostName, NetworkCredential credential)
        {
            Interop.ServerInfo serverInfo = new Interop.ServerInfo();
            Interop.COSERVERINFO pServerInfo = serverInfo.Allocate(hostName, credential);
            GCHandle gcHandle = GCHandle.Alloc((object)Interop.IID_IUnknown, GCHandleType.Pinned);
            Interop.MULTI_QI[] pResults = new Interop.MULTI_QI[1];
            pResults[0].iid = gcHandle.AddrOfPinnedObject();
            pResults[0].pItf = (object)null;
            pResults[0].hr = 0U;
            try
            {
                uint dwClsCtx = 5;
                if (hostName != null && hostName.Length > 0 && hostName != "localhost")
                    dwClsCtx = 20U;
                Interop.CoCreateInstanceEx(ref clsid, (object)null, dwClsCtx, ref pServerInfo, 1U, pResults);
            }
            finally
            {
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
                serverInfo.Deallocate();
            }
            return pResults[0].hr == 0U ? pResults[0].pItf : throw new ExternalException("CoCreateInstanceEx: " + Interop.GetSystemMessage((int)pResults[0].hr));
        }

        public static object CreateInstanceWithLicenseKey(
          Guid clsid,
          string hostName,
          NetworkCredential credential,
          string licenseKey)
        {
            Interop.ServerInfo serverInfo = new Interop.ServerInfo();
            Interop.COSERVERINFO pServerInfo = serverInfo.Allocate(hostName, credential);
            object ppvObject = (object)null;
            try
            {
                uint dwClsContext = 5;
                if (hostName != null && hostName.Length > 0)
                    dwClsContext = 20U;
                object ppv = (object)null;
                Interop.CoGetClassObject(clsid, dwClsContext, ref pServerInfo, typeof(Interop.IClassFactory2).GUID, out ppv);
                Interop.IClassFactory2 pProxy = (Interop.IClassFactory2)ppv;
                Interop.IClientSecurity clientSecurity = (Interop.IClientSecurity)pProxy;
                uint pAuthnSvc = 0;
                uint pAuthzSvc = 0;
                string pServerPrincName = "";
                uint pAuthnLevel = 0;
                uint pImpLevel = 0;
                IntPtr zero = IntPtr.Zero;
                uint pCapabilities = 0;
                clientSecurity.QueryBlanket((object)pProxy, ref pAuthnSvc, ref pAuthzSvc, ref pServerPrincName, ref pAuthnLevel, ref pImpLevel, ref zero, ref pCapabilities);
                uint maxValue = uint.MaxValue;
                pAuthnLevel = 2U;
                clientSecurity.SetBlanket((object)pProxy, maxValue, pAuthzSvc, pServerPrincName, pAuthnLevel, pImpLevel, zero, pCapabilities);
                pProxy.CreateInstanceLic((object)null, (object)null, Interop.IID_IUnknown, licenseKey, out ppvObject);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                serverInfo.Deallocate();
            }
            return ppvObject;
        }

        public static int[] GetInt32s(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
                return (int[])null;
            int[] destination = new int[size];
            Marshal.Copy(pArray, destination, 0, size);
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }
            return destination;
        }

        public static IntPtr GetInt32s(int[] input)
        {
            IntPtr destination = IntPtr.Zero;
            if (input != null)
            {
                destination = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * input.Length);
                Marshal.Copy(input, 0, destination, input.Length);
            }
            return destination;
        }

        public static short[] GetInt16s(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
                return (short[])null;
            short[] destination = new short[size];
            Marshal.Copy(pArray, destination, 0, size);
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }
            return destination;
        }

        public static IntPtr GetInt16s(short[] input)
        {
            IntPtr destination = IntPtr.Zero;
            if (input != null)
            {
                destination = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(short)) * input.Length);
                Marshal.Copy(input, 0, destination, input.Length);
            }
            return destination;
        }

        public static IntPtr GetUnicodeStrings(string[] values)
        {
            int length = values != null ? values.Length : 0;
            if (length <= 0)
                return IntPtr.Zero;
            IntPtr zero = IntPtr.Zero;
            IntPtr[] source = new IntPtr[length];
            for (int index = 0; index < length; ++index)
                source[index] = Marshal.StringToCoTaskMemUni(values[index]);
            IntPtr destination = Marshal.AllocCoTaskMem(values.Length * Marshal.SizeOf(typeof(nint)));
            Marshal.Copy(source, 0, destination, length);
            return destination;
        }

        public static string[] GetUnicodeStrings(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
                return (string[])null;
            IntPtr[] destination = new IntPtr[size];
            Marshal.Copy(pArray, destination, 0, size);
            string[] unicodeStrings = new string[size];
            for (int index = 0; index < size; ++index)
            {
                IntPtr ptr = destination[index];
                unicodeStrings[index] = Marshal.PtrToStringUni(ptr);
                if (deallocate)
                    Marshal.FreeCoTaskMem(ptr);
            }
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }
            return unicodeStrings;
        }

        public static bool PreserveUTC
        {
            get
            {
                lock (typeof(Interop))
                    return Interop.m_preserveUTC;
            }
            set
            {
                lock (typeof(Interop))
                    Interop.m_preserveUTC = value;
            }
        }

        public static FILETIME GetFILETIME(DateTime datetime)
        {
            if (datetime <= Interop.FILETIME_BaseTime)
            {
                FILETIME filetime;
                filetime.dwHighDateTime = 0;
                filetime.dwLowDateTime = 0;
                return filetime;
            }
            long ticks;
            if (Interop.m_preserveUTC)
            {
                ticks = datetime.Subtract(new TimeSpan(Interop.FILETIME_BaseTime.Ticks)).Ticks;
            }
            else
            {
                DateTime dateTime = datetime.ToUniversalTime();
                dateTime = dateTime.Subtract(new TimeSpan(Interop.FILETIME_BaseTime.Ticks));
                ticks = dateTime.Ticks;
            }
            FILETIME filetime1;
            filetime1.dwHighDateTime = (int)(ticks >> 32 & (long)uint.MaxValue);
            filetime1.dwLowDateTime = (int)(ticks & (long)uint.MaxValue);
            return filetime1;
        }

        public static DateTime GetFILETIME(IntPtr pFiletime)
        {
            return pFiletime == IntPtr.Zero ? DateTime.MinValue : Interop.GetFILETIME((FILETIME)Marshal.PtrToStructure(pFiletime, typeof(FILETIME)));
        }

        public static DateTime GetFILETIME(FILETIME filetime)
        {
            long dwHighDateTime = (long)filetime.dwHighDateTime;
            if (dwHighDateTime < 0L)
                dwHighDateTime += 4294967296L;
            long num = dwHighDateTime << 32;
            long dwLowDateTime = (long)filetime.dwLowDateTime;
            if (dwLowDateTime < 0L)
                dwLowDateTime += 4294967296L;
            long ticks = num + dwLowDateTime;
            if (ticks == 0L)
                return DateTime.MinValue;
            return Interop.m_preserveUTC ? Interop.FILETIME_BaseTime.Add(new TimeSpan(ticks)) : Interop.FILETIME_BaseTime.Add(new TimeSpan(ticks)).ToLocalTime();
        }

        public static IntPtr GetFILETIMEs(DateTime[] datetimes)
        {
            int length = datetimes != null ? datetimes.Length : 0;
            if (length <= 0)
                return IntPtr.Zero;
            IntPtr filetimEs = Marshal.AllocCoTaskMem(length * Marshal.SizeOf(typeof(FILETIME)));
            IntPtr ptr = filetimEs;
            for (int index = 0; index < length; ++index)
            {
                Marshal.StructureToPtr<FILETIME>(Interop.GetFILETIME(datetimes[index]), ptr, false);
                ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(FILETIME)));
            }
            return filetimEs;
        }

        public static DateTime[] GetFILETIMEs(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
                return (DateTime[])null;
            DateTime[] filetimEs = new DateTime[size];
            IntPtr pFiletime = pArray;
            for (int index = 0; index < size; ++index)
            {
                filetimEs[index] = Interop.GetFILETIME(pFiletime);
                pFiletime = (nint)(pFiletime.ToInt64() + (long)Marshal.SizeOf(typeof(FILETIME)));
            }
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }
            return filetimEs;
        }

        public static Guid[] GetGUIDs(ref IntPtr pInput, int size, bool deallocate)
        {
            if (pInput == IntPtr.Zero || size <= 0)
                return (Guid[])null;
            Guid[] guiDs = new Guid[size];
            IntPtr num = pInput;
            for (int index = 0; index < size; ++index)
            {
                Interop.GUID structure = (Interop.GUID)Marshal.PtrToStructure(pInput, typeof(Interop.GUID));
                guiDs[index] = new Guid(structure.Data1, structure.Data2, structure.Data3, structure.Data4);
                num = (nint)(num.ToInt64() + (long)Marshal.SizeOf(typeof(Interop.GUID)));
            }
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pInput);
                pInput = IntPtr.Zero;
            }
            return guiDs;
        }

        private static int VARIANT_SIZE => IntPtr.Size <= 4 ? 16 : 24;

        [DllImport("oleaut32.dll")]
        private static extern void VariantClear(IntPtr pVariant);

        public static object GetVARIANT(object source)
        {
            if (source == null || source.GetType() == (System.Type)null)
                return (object)null;
            if (!(source.GetType() == typeof(Decimal[])))
                return source;
            Decimal[] numArray = (Decimal[])source;
            object[] variant = new object[numArray.Length];
            for (int index = 0; index < numArray.Length; ++index)
            {
                try
                {
                    variant[index] = (object)numArray[index];
                }
                catch
                {
                    variant[index] = (object)double.NaN;
                }
            }
            return (object)variant;
        }

        public static IntPtr GetVARIANTs(object[] values, bool preprocess)
        {
            int length = values != null ? values.Length : 0;
            if (length <= 0)
                return IntPtr.Zero;
            IntPtr varianTs = Marshal.AllocCoTaskMem(length * Interop.VARIANT_SIZE);
            IntPtr pDstNativeVariant = varianTs;
            for (int index = 0; index < length; ++index)
            {
                if (preprocess)
                    Marshal.GetNativeVariantForObject(Interop.GetVARIANT(values[index]), pDstNativeVariant);
                else
                    Marshal.GetNativeVariantForObject(values[index], pDstNativeVariant);
                pDstNativeVariant = (nint)(pDstNativeVariant.ToInt64() + (long)Interop.VARIANT_SIZE);
            }
            return varianTs;
        }

        public static object[] GetVARIANTs(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
                return (object[])null;
            object[] varianTs = new object[size];
            IntPtr num = pArray;
            byte[] destination = new byte[size * Interop.VARIANT_SIZE];
            Marshal.Copy(num, destination, 0, destination.Length);
            for (int index = 0; index < size; ++index)
            {
                try
                {
                    varianTs[index] = Marshal.GetObjectForNativeVariant(num);
                    if (deallocate)
                        Interop.VariantClear(num);
                }
                catch
                {
                    varianTs[index] = (object)null;
                }
                num = (nint)(num.ToInt64() + (long)Interop.VARIANT_SIZE);
            }
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }
            return varianTs;
        }

        internal static string GetLocale(int input)
        {
            try
            {
                return input == 2048 || input == 1024 || input == 0 ? CultureInfo.InvariantCulture.Name : new CultureInfo(input).Name;
            }
            catch
            {
                throw new ExternalException("Invalid LCID", -2147024809);
            }
        }

        internal static int GetLocale(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            CultureInfo cultureInfo;
            try
            {
                cultureInfo = new CultureInfo(input);
            }
            catch
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }
            return cultureInfo.LCID;
        }

        internal static System.Type GetType(VarEnum input)
        {
            switch (input)
            {
                case VarEnum.VT_EMPTY:
                    return (System.Type)null;
                case VarEnum.VT_I2:
                    return typeof(short);
                case VarEnum.VT_I4:
                    return typeof(int);
                case VarEnum.VT_R4:
                    return typeof(float);
                case VarEnum.VT_R8:
                    return typeof(double);
                case VarEnum.VT_CY:
                    return typeof(Decimal);
                case VarEnum.VT_DATE:
                    return typeof(DateTime);
                case VarEnum.VT_BSTR:
                    return typeof(string);
                case VarEnum.VT_BOOL:
                    return typeof(bool);
                case VarEnum.VT_I1:
                    return typeof(sbyte);
                case VarEnum.VT_UI1:
                    return typeof(byte);
                case VarEnum.VT_UI2:
                    return typeof(ushort);
                case VarEnum.VT_UI4:
                    return typeof(uint);
                case VarEnum.VT_I8:
                    return typeof(long);
                case VarEnum.VT_UI8:
                    return typeof(ulong);
                case VarEnum.VT_ARRAY | VarEnum.VT_I2:
                    return typeof(short[]);
                case VarEnum.VT_I4 | VarEnum.VT_ARRAY:
                    return typeof(int[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_R4:
                    return typeof(float[]);
                case VarEnum.VT_R8 | VarEnum.VT_ARRAY:
                    return typeof(double[]);
                case VarEnum.VT_CY | VarEnum.VT_ARRAY:
                    return typeof(Decimal[]);
                case VarEnum.VT_DATE | VarEnum.VT_ARRAY:
                    return typeof(DateTime[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_BSTR:
                    return typeof(string[]);
                case VarEnum.VT_BOOL | VarEnum.VT_ARRAY:
                    return typeof(bool[]);
                case VarEnum.VT_VARIANT | VarEnum.VT_ARRAY:
                    return typeof(object[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_I1:
                    return typeof(sbyte[]);
                case VarEnum.VT_UI1 | VarEnum.VT_ARRAY:
                    return typeof(byte[]);
                case VarEnum.VT_UI2 | VarEnum.VT_ARRAY:
                    return typeof(ushort[]);
                case VarEnum.VT_UI4 | VarEnum.VT_ARRAY:
                    return typeof(uint[]);
                case VarEnum.VT_I8 | VarEnum.VT_ARRAY:
                    return typeof(long[]);
                case VarEnum.VT_UI8 | VarEnum.VT_ARRAY:
                    return typeof(ulong[]);
                default:
                    return Opc.Type.ILLEGAL_TYPE;
            }
        }

        internal static VarEnum GetType(System.Type input)
        {
            if (input == (System.Type)null)
                return VarEnum.VT_EMPTY;
            if (input == typeof(sbyte))
                return VarEnum.VT_I1;
            if (input == typeof(byte))
                return VarEnum.VT_UI1;
            if (input == typeof(short))
                return VarEnum.VT_I2;
            if (input == typeof(ushort))
                return VarEnum.VT_UI2;
            if (input == typeof(int))
                return VarEnum.VT_I4;
            if (input == typeof(uint))
                return VarEnum.VT_UI4;
            if (input == typeof(long))
                return VarEnum.VT_I8;
            if (input == typeof(ulong))
                return VarEnum.VT_UI8;
            if (input == typeof(float))
                return VarEnum.VT_R4;
            if (input == typeof(double))
                return VarEnum.VT_R8;
            if (input == typeof(Decimal))
                return VarEnum.VT_CY;
            if (input == typeof(bool))
                return VarEnum.VT_BOOL;
            if (input == typeof(DateTime))
                return VarEnum.VT_DATE;
            if (input == typeof(string))
                return VarEnum.VT_BSTR;
            if (input == typeof(object))
                return VarEnum.VT_EMPTY;
            if (input == typeof(sbyte[]))
                return VarEnum.VT_ARRAY | VarEnum.VT_I1;
            if (input == typeof(byte[]))
                return VarEnum.VT_UI1 | VarEnum.VT_ARRAY;
            if (input == typeof(short[]))
                return VarEnum.VT_ARRAY | VarEnum.VT_I2;
            if (input == typeof(ushort[]))
                return VarEnum.VT_UI2 | VarEnum.VT_ARRAY;
            if (input == typeof(int[]))
                return VarEnum.VT_I4 | VarEnum.VT_ARRAY;
            if (input == typeof(uint[]))
                return VarEnum.VT_UI4 | VarEnum.VT_ARRAY;
            if (input == typeof(long[]))
                return VarEnum.VT_I8 | VarEnum.VT_ARRAY;
            if (input == typeof(ulong[]))
                return VarEnum.VT_UI8 | VarEnum.VT_ARRAY;
            if (input == typeof(float[]))
                return VarEnum.VT_ARRAY | VarEnum.VT_R4;
            if (input == typeof(double[]))
                return VarEnum.VT_R8 | VarEnum.VT_ARRAY;
            if (input == typeof(Decimal[]))
                return VarEnum.VT_CY | VarEnum.VT_ARRAY;
            if (input == typeof(bool[]))
                return VarEnum.VT_BOOL | VarEnum.VT_ARRAY;
            if (input == typeof(DateTime[]))
                return VarEnum.VT_DATE | VarEnum.VT_ARRAY;
            if (input == typeof(string[]))
                return VarEnum.VT_ARRAY | VarEnum.VT_BSTR;
            if (input == typeof(object[]))
                return VarEnum.VT_VARIANT | VarEnum.VT_ARRAY;
            if (input == Opc.Type.ILLEGAL_TYPE)
                return (VarEnum)Enum.ToObject(typeof(VarEnum), (int)short.MaxValue);
            if (input == typeof(System.Type) || input == typeof(Quality))
                return VarEnum.VT_I2;
            return input == typeof(accessRights) || input == typeof(euType) ? VarEnum.VT_I4 : VarEnum.VT_EMPTY;
        }

        internal static ResultID GetResultID(int input)
        {
            switch (input)
            {
                case -2147467262:
                    return new ResultID(ResultID.E_NOTSUPPORTED, (long)input);
                case -2147467259:
                    return new ResultID(ResultID.E_FAIL, (long)input);
                case -2147352571:
                    return new ResultID(ResultID.Da.E_BADTYPE, (long)input);
                case -2147352566:
                    return new ResultID(ResultID.Da.E_RANGE, (long)input);
                case -2147217401:
                    return new ResultID(ResultID.Hda.W_NOFILTER, (long)input);
                case -2147024882:
                    return new ResultID(ResultID.E_OUTOFMEMORY, (long)input);
                case -2147024809:
                    return new ResultID(ResultID.E_INVALIDARG, (long)input);
                case -1073479679:
                    return new ResultID(ResultID.Da.E_INVALIDHANDLE, (long)input);
                case -1073479676:
                    return new ResultID(ResultID.Da.E_BADTYPE, (long)input);
                case -1073479673:
                    return new ResultID(ResultID.Da.E_UNKNOWN_ITEM_NAME, (long)input);
                case -1073479672:
                    return new ResultID(ResultID.Da.E_INVALID_ITEM_NAME, (long)input);
                case -1073479671:
                    return new ResultID(ResultID.Da.E_INVALID_FILTER, (long)input);
                case -1073479670:
                    return new ResultID(ResultID.Da.E_UNKNOWN_ITEM_PATH, (long)input);
                case -1073479669:
                    return new ResultID(ResultID.Da.E_RANGE, (long)input);
                case -1073479165:
                    return new ResultID(ResultID.Da.E_INVALID_PID, (long)input);
                case -1073479164:
                    return new ResultID(ResultID.Ae.E_INVALIDTIME, (long)input);
                case -1073479163:
                    return new ResultID(ResultID.Ae.E_BUSY, (long)input);
                case -1073479162:
                    return new ResultID(ResultID.Ae.E_NOINFO, (long)input);
                case -1073478655:
                    return new ResultID(ResultID.Da.E_NO_ITEM_DEADBAND, (long)input);
                case -1073478654:
                    return new ResultID(ResultID.Da.E_NO_ITEM_BUFFERING, (long)input);
                case -1073478653:
                    return new ResultID(ResultID.Da.E_INVALIDCONTINUATIONPOINT, (long)input);
                case -1073478650:
                    return new ResultID(ResultID.Da.E_NO_WRITEQT, (long)input);
                case -1073478649:
                    return new ResultID(ResultID.Cpx.E_TYPE_CHANGED, (long)input);
                case -1073478648:
                    return new ResultID(ResultID.Cpx.E_FILTER_DUPLICATE, (long)input);
                case -1073478647:
                    return new ResultID(ResultID.Cpx.E_FILTER_INVALID, (long)input);
                case -1073478646:
                    return new ResultID(ResultID.Cpx.E_FILTER_ERROR, (long)input);
                case -1073477888:
                    return new ResultID(ResultID.Dx.E_PERSISTING, (long)input);
                case -1073477887:
                    return new ResultID(ResultID.Dx.E_NOITEMLIST, (long)input);
                case -1073477886:
                    return new ResultID(ResultID.Dx.E_VERSION_MISMATCH, (long)input);
                case -1073477885:
                    return new ResultID(ResultID.Dx.E_VERSION_MISMATCH, (long)input);
                case -1073477884:
                    return new ResultID(ResultID.Dx.E_UNKNOWN_ITEM_PATH, (long)input);
                case -1073477883:
                    return new ResultID(ResultID.Dx.E_UNKNOWN_ITEM_NAME, (long)input);
                case -1073477882:
                    return new ResultID(ResultID.Dx.E_INVALID_ITEM_PATH, (long)input);
                case -1073477881:
                    return new ResultID(ResultID.Dx.E_INVALID_ITEM_NAME, (long)input);
                case -1073477880:
                    return new ResultID(ResultID.Dx.E_INVALID_NAME, (long)input);
                case -1073477879:
                    return new ResultID(ResultID.Dx.E_DUPLICATE_NAME, (long)input);
                case -1073477878:
                    return new ResultID(ResultID.Dx.E_INVALID_BROWSE_PATH, (long)input);
                case -1073477877:
                    return new ResultID(ResultID.Dx.E_INVALID_SERVER_URL, (long)input);
                case -1073477876:
                    return new ResultID(ResultID.Dx.E_INVALID_SERVER_TYPE, (long)input);
                case -1073477875:
                    return new ResultID(ResultID.Dx.E_UNSUPPORTED_SERVER_TYPE, (long)input);
                case -1073477874:
                    return new ResultID(ResultID.Dx.E_CONNECTIONS_EXIST, (long)input);
                case -1073477873:
                    return new ResultID(ResultID.Dx.E_TOO_MANY_CONNECTIONS, (long)input);
                case -1073477872:
                    return new ResultID(ResultID.Dx.E_OVERRIDE_BADTYPE, (long)input);
                case -1073477871:
                    return new ResultID(ResultID.Dx.E_OVERRIDE_RANGE, (long)input);
                case -1073477870:
                    return new ResultID(ResultID.Dx.E_SUBSTITUTE_BADTYPE, (long)input);
                case -1073477869:
                    return new ResultID(ResultID.Dx.E_SUBSTITUTE_RANGE, (long)input);
                case -1073477868:
                    return new ResultID(ResultID.Dx.E_INVALID_TARGET_ITEM, (long)input);
                case -1073477867:
                    return new ResultID(ResultID.Dx.E_UNKNOWN_TARGET_ITEM, (long)input);
                case -1073477866:
                    return new ResultID(ResultID.Dx.E_TARGET_ALREADY_CONNECTED, (long)input);
                case -1073477865:
                    return new ResultID(ResultID.Dx.E_UNKNOWN_SERVER_NAME, (long)input);
                case -1073477864:
                    return new ResultID(ResultID.Dx.E_UNKNOWN_SOURCE_ITEM, (long)input);
                case -1073477863:
                    return new ResultID(ResultID.Dx.E_INVALID_SOURCE_ITEM, (long)input);
                case -1073477862:
                    return new ResultID(ResultID.Dx.E_INVALID_QUEUE_SIZE, (long)input);
                case -1073477861:
                    return new ResultID(ResultID.Dx.E_INVALID_DEADBAND, (long)input);
                case -1073477860:
                    return new ResultID(ResultID.Dx.E_INVALID_CONFIG_FILE, (long)input);
                case -1073477859:
                    return new ResultID(ResultID.Dx.E_PERSIST_FAILED, (long)input);
                case -1073477858:
                    return new ResultID(ResultID.Dx.E_TARGET_FAULT, (long)input);
                case -1073477857:
                    return new ResultID(ResultID.Dx.E_TARGET_NO_ACCESSS, (long)input);
                case -1073477856:
                    return new ResultID(ResultID.Dx.E_SOURCE_SERVER_FAULT, (long)input);
                case -1073477855:
                    return new ResultID(ResultID.Dx.E_SOURCE_SERVER_NO_ACCESSS, (long)input);
                case -1073477854:
                    return new ResultID(ResultID.Dx.E_SUBSCRIPTION_FAULT, (long)input);
                case -1073477853:
                    return new ResultID(ResultID.Dx.E_SOURCE_ITEM_BADRIGHTS, (long)input);
                case -1073477852:
                    return new ResultID(ResultID.Dx.E_SOURCE_ITEM_BAD_QUALITY, (long)input);
                case -1073477851:
                    return new ResultID(ResultID.Dx.E_SOURCE_ITEM_BADTYPE, (long)input);
                case -1073477850:
                    return new ResultID(ResultID.Dx.E_SOURCE_ITEM_RANGE, (long)input);
                case -1073477849:
                    return new ResultID(ResultID.Dx.E_SOURCE_SERVER_NOT_CONNECTED, (long)input);
                case -1073477848:
                    return new ResultID(ResultID.Dx.E_SOURCE_SERVER_TIMEOUT, (long)input);
                case -1073477847:
                    return new ResultID(ResultID.Dx.E_TARGET_ITEM_DISCONNECTED, (long)input);
                case -1073477846:
                    return new ResultID(ResultID.Dx.E_TARGET_NO_WRITES_ATTEMPTED, (long)input);
                case -1073477845:
                    return new ResultID(ResultID.Dx.E_TARGET_ITEM_BADTYPE, (long)input);
                case -1073477844:
                    return new ResultID(ResultID.Dx.E_TARGET_ITEM_RANGE, (long)input);
                case -1073475583:
                    return new ResultID(ResultID.Hda.E_MAXEXCEEDED, (long)input);
                case -1073475580:
                    return new ResultID(ResultID.Hda.E_INVALIDAGGREGATE, (long)input);
                case -1073475576:
                    return new ResultID(ResultID.Hda.E_UNKNOWNATTRID, (long)input);
                case -1073475575:
                    return new ResultID(ResultID.Hda.E_NOT_AVAIL, (long)input);
                case -1073475574:
                    return new ResultID(ResultID.Hda.E_INVALIDDATATYPE, (long)input);
                case -1073475573:
                    return new ResultID(ResultID.Hda.E_DATAEXISTS, (long)input);
                case -1073475572:
                    return new ResultID(ResultID.Hda.E_INVALIDATTRID, (long)input);
                case -1073475571:
                    return new ResultID(ResultID.Hda.E_NODATAEXISTS, (long)input);
                case 0:
                    return new ResultID(ResultID.S_OK, (long)input);
                case 262157:
                    return new ResultID(ResultID.Da.S_UNSUPPORTEDRATE, (long)input);
                case 262158:
                    return new ResultID(ResultID.Da.S_CLAMP, (long)input);
                case 262656:
                    return new ResultID(ResultID.Ae.S_ALREADYACKED, (long)input);
                case 262657:
                    return new ResultID(ResultID.Ae.S_INVALIDBUFFERTIME, (long)input);
                case 262658:
                    return new ResultID(ResultID.Ae.S_INVALIDMAXSIZE, (long)input);
                case 262659:
                    return new ResultID(ResultID.Ae.S_INVALIDKEEPALIVETIME, (long)input);
                case 263172:
                    return new ResultID(ResultID.Da.S_DATAQUEUEOVERFLOW, (long)input);
                case 263179:
                    return new ResultID(ResultID.Cpx.S_FILTER_NO_DATA, (long)input);
                case 264064:
                    return new ResultID(ResultID.Dx.S_TARGET_SUBSTITUTED, (long)input);
                case 264065:
                    return new ResultID(ResultID.Dx.S_TARGET_OVERRIDEN, (long)input);
                case 264066:
                    return new ResultID(ResultID.Dx.S_CLAMP, (long)input);
                case 1074008066:
                    return new ResultID(ResultID.Hda.S_NODATA, (long)input);
                case 1074008067:
                    return new ResultID(ResultID.Hda.S_MOREDATA, (long)input);
                case 1074008069:
                    return new ResultID(ResultID.Hda.S_CURRENTVALUE, (long)input);
                case 1074008070:
                    return new ResultID(ResultID.Hda.S_EXTRADATA, (long)input);
                case 1074008078:
                    return new ResultID(ResultID.Hda.S_INSERTED, (long)input);
                case 1074008079:
                    return new ResultID(ResultID.Hda.S_REPLACED, (long)input);
                default:
                    if ((input & 2147418112) == 65536)
                        return new ResultID(ResultID.E_NETWORK_ERROR, (long)input);
                    return input >= 0 ? new ResultID(ResultID.S_FALSE, (long)input) : new ResultID(ResultID.E_FAIL, (long)input);
            }
        }

        internal static int GetResultID(ResultID input)
        {
            if (input.Name != (XmlQualifiedName)null && input.Name.Namespace == "http://opcfoundation.org/DataAccess/")
            {
                if (input == ResultID.S_OK)
                    return 0;
                if (input == ResultID.E_FAIL)
                    return -2147467259;
                if (input == ResultID.E_INVALIDARG)
                    return -2147024809;
                if (input == ResultID.Da.E_BADTYPE)
                    return -1073479676;
                if (input == ResultID.Da.E_READONLY || input == ResultID.Da.E_WRITEONLY)
                    return -1073479674;
                if (input == ResultID.Da.E_RANGE)
                    return -1073479669;
                if (input == ResultID.E_OUTOFMEMORY)
                    return -2147024882;
                if (input == ResultID.E_NOTSUPPORTED)
                    return -2147467262;
                if (input == ResultID.Da.E_INVALIDHANDLE)
                    return -1073479679;
                if (input == ResultID.Da.E_UNKNOWN_ITEM_NAME)
                    return -1073479673;
                if (input == ResultID.Da.E_INVALID_ITEM_NAME || input == ResultID.Da.E_INVALID_ITEM_PATH)
                    return -1073479672;
                if (input == ResultID.Da.E_UNKNOWN_ITEM_PATH)
                    return -1073479670;
                if (input == ResultID.Da.E_INVALID_FILTER)
                    return -1073479671;
                if (input == ResultID.Da.S_UNSUPPORTEDRATE)
                    return 262157;
                if (input == ResultID.Da.S_CLAMP)
                    return 262158;
                if (input == ResultID.Da.E_INVALID_PID)
                    return -1073479165;
                if (input == ResultID.Da.E_NO_ITEM_DEADBAND)
                    return -1073478655;
                if (input == ResultID.Da.E_NO_ITEM_BUFFERING)
                    return -1073478654;
                if (input == ResultID.Da.E_NO_WRITEQT)
                    return -1073478650;
                if (input == ResultID.Da.E_INVALIDCONTINUATIONPOINT)
                    return -1073478653;
                if (input == ResultID.Da.S_DATAQUEUEOVERFLOW)
                    return 263172;
            }
            else if (input.Name != (XmlQualifiedName)null && input.Name.Namespace == "http://opcfoundation.org/ComplexData/")
            {
                if (input == ResultID.Cpx.E_TYPE_CHANGED)
                    return -1073478649;
                if (input == ResultID.Cpx.E_FILTER_DUPLICATE)
                    return -1073478648;
                if (input == ResultID.Cpx.E_FILTER_INVALID)
                    return -1073478647;
                if (input == ResultID.Cpx.E_FILTER_ERROR)
                    return -1073478646;
                if (input == ResultID.Cpx.S_FILTER_NO_DATA)
                    return 263179;
            }
            else if (input.Name != (XmlQualifiedName)null && input.Name.Namespace == "http://opcfoundation.org/HistoricalDataAccess/")
            {
                if (input == ResultID.Hda.E_MAXEXCEEDED)
                    return -1073475583;
                if (input == ResultID.Hda.S_NODATA)
                    return 1074008066;
                if (input == ResultID.Hda.S_MOREDATA)
                    return 1074008067;
                if (input == ResultID.Hda.E_INVALIDAGGREGATE)
                    return -1073475580;
                if (input == ResultID.Hda.S_CURRENTVALUE)
                    return 1074008069;
                if (input == ResultID.Hda.S_EXTRADATA)
                    return 1074008070;
                if (input == ResultID.Hda.E_UNKNOWNATTRID)
                    return -1073475576;
                if (input == ResultID.Hda.E_NOT_AVAIL)
                    return -1073475575;
                if (input == ResultID.Hda.E_INVALIDDATATYPE)
                    return -1073475574;
                if (input == ResultID.Hda.E_DATAEXISTS)
                    return -1073475573;
                if (input == ResultID.Hda.E_INVALIDATTRID)
                    return -1073475572;
                if (input == ResultID.Hda.E_NODATAEXISTS)
                    return -1073475571;
                if (input == ResultID.Hda.S_INSERTED)
                    return 1074008078;
                if (input == ResultID.Hda.S_REPLACED)
                    return 1074008079;
            }
            if (input.Name != (XmlQualifiedName)null && input.Name.Namespace == "http://opcfoundation.org/DataExchange/")
            {
                if (input == ResultID.Dx.E_PERSISTING)
                    return -1073477888;
                if (input == ResultID.Dx.E_NOITEMLIST)
                    return -1073477887;
                if (input == ResultID.Dx.E_SERVER_STATE || input == ResultID.Dx.E_VERSION_MISMATCH)
                    return -1073477885;
                if (input == ResultID.Dx.E_UNKNOWN_ITEM_PATH)
                    return -1073477884;
                if (input == ResultID.Dx.E_UNKNOWN_ITEM_NAME)
                    return -1073477883;
                if (input == ResultID.Dx.E_INVALID_ITEM_PATH)
                    return -1073477882;
                if (input == ResultID.Dx.E_INVALID_ITEM_NAME)
                    return -1073477881;
                if (input == ResultID.Dx.E_INVALID_NAME)
                    return -1073477880;
                if (input == ResultID.Dx.E_DUPLICATE_NAME)
                    return -1073477879;
                if (input == ResultID.Dx.E_INVALID_BROWSE_PATH)
                    return -1073477878;
                if (input == ResultID.Dx.E_INVALID_SERVER_URL)
                    return -1073477877;
                if (input == ResultID.Dx.E_INVALID_SERVER_TYPE)
                    return -1073477876;
                if (input == ResultID.Dx.E_UNSUPPORTED_SERVER_TYPE)
                    return -1073477875;
                if (input == ResultID.Dx.E_CONNECTIONS_EXIST)
                    return -1073477874;
                if (input == ResultID.Dx.E_TOO_MANY_CONNECTIONS)
                    return -1073477873;
                if (input == ResultID.Dx.E_OVERRIDE_BADTYPE)
                    return -1073477872;
                if (input == ResultID.Dx.E_OVERRIDE_RANGE)
                    return -1073477871;
                if (input == ResultID.Dx.E_SUBSTITUTE_BADTYPE)
                    return -1073477870;
                if (input == ResultID.Dx.E_SUBSTITUTE_RANGE)
                    return -1073477869;
                if (input == ResultID.Dx.E_INVALID_TARGET_ITEM)
                    return -1073477868;
                if (input == ResultID.Dx.E_UNKNOWN_TARGET_ITEM)
                    return -1073477867;
                if (input == ResultID.Dx.E_TARGET_ALREADY_CONNECTED)
                    return -1073477866;
                if (input == ResultID.Dx.E_UNKNOWN_SERVER_NAME)
                    return -1073477865;
                if (input == ResultID.Dx.E_UNKNOWN_SOURCE_ITEM)
                    return -1073477864;
                if (input == ResultID.Dx.E_INVALID_SOURCE_ITEM)
                    return -1073477863;
                if (input == ResultID.Dx.E_INVALID_QUEUE_SIZE)
                    return -1073477862;
                if (input == ResultID.Dx.E_INVALID_DEADBAND)
                    return -1073477861;
                if (input == ResultID.Dx.E_INVALID_CONFIG_FILE)
                    return -1073477860;
                if (input == ResultID.Dx.E_PERSIST_FAILED)
                    return -1073477859;
                if (input == ResultID.Dx.E_TARGET_FAULT)
                    return -1073477858;
                if (input == ResultID.Dx.E_TARGET_NO_ACCESSS)
                    return -1073477857;
                if (input == ResultID.Dx.E_SOURCE_SERVER_FAULT)
                    return -1073477856;
                if (input == ResultID.Dx.E_SOURCE_SERVER_NO_ACCESSS)
                    return -1073477855;
                if (input == ResultID.Dx.E_SUBSCRIPTION_FAULT)
                    return -1073477854;
                if (input == ResultID.Dx.E_SOURCE_ITEM_BADRIGHTS)
                    return -1073477853;
                if (input == ResultID.Dx.E_SOURCE_ITEM_BAD_QUALITY)
                    return -1073477852;
                if (input == ResultID.Dx.E_SOURCE_ITEM_BADTYPE)
                    return -1073477851;
                if (input == ResultID.Dx.E_SOURCE_ITEM_RANGE)
                    return -1073477850;
                if (input == ResultID.Dx.E_SOURCE_SERVER_NOT_CONNECTED)
                    return -1073477849;
                if (input == ResultID.Dx.E_SOURCE_SERVER_TIMEOUT)
                    return -1073477848;
                if (input == ResultID.Dx.E_TARGET_ITEM_DISCONNECTED)
                    return -1073477847;
                if (input == ResultID.Dx.E_TARGET_NO_WRITES_ATTEMPTED)
                    return -1073477846;
                if (input == ResultID.Dx.E_TARGET_ITEM_BADTYPE)
                    return -1073477845;
                if (input == ResultID.Dx.E_TARGET_ITEM_RANGE)
                    return -1073477844;
                if (input == ResultID.Dx.S_TARGET_SUBSTITUTED)
                    return 264064;
                if (input == ResultID.Dx.S_TARGET_OVERRIDEN)
                    return 264065;
                if (input == ResultID.Dx.S_CLAMP)
                    return 264066;
            }
            else if (input.Code == -1)
                return input.Succeeded() ? 1 : -2147467259;
            return input.Code;
        }

        public static Exception CreateException(string message, Exception e)
        {
            return Interop.CreateException(message, Marshal.GetHRForException(e));
        }

        public static Exception CreateException(string message, int code)
        {
            return (Exception)new ResultIDException(Interop.GetResultID(code), message);
        }

        public static void ReleaseServer(object server)
        {
            if (server == null || !server.GetType().IsCOMObject)
                return;
            Marshal.ReleaseComObject(server);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SERVER_INFO_100
        {
            public uint sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sv100_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SOLE_AUTHENTICATION_SERVICE
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pPrincipalName;
            public int hr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct COSERVERINFO
        {
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;
            public IntPtr pAuthInfo;
            public uint dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct COAUTHINFO
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            public IntPtr pwszServerPrincName;
            public uint dwAuthnLevel;
            public uint dwImpersonationLevel;
            public IntPtr pAuthIdentityData;
            public uint dwCapabilities;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct COAUTHIDENTITY
        {
            public IntPtr User;
            public uint UserLength;
            public IntPtr Domain;
            public uint DomainLength;
            public IntPtr Password;
            public uint PasswordLength;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MULTI_QI
        {
            public IntPtr iid;
            [MarshalAs(UnmanagedType.IUnknown)]
            public object pItf;
            public uint hr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct LICINFO
        {
            public int cbLicInfo;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fRuntimeKeyAvail;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fLicVerified;
        }

        [Guid("B196B28F-BAB4-101A-B69C-00AA00341D07")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IClassFactory2
        {
            void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object punkOuter, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

            void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);

            void GetLicInfo([In, Out] ref Interop.LICINFO pLicInfo);

            void RequestLicKey(int dwReserved, [MarshalAs(UnmanagedType.BStr)] string pbstrKey);

            void CreateInstanceLic(
              [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
              [MarshalAs(UnmanagedType.IUnknown)] object punkReserved,
              [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
              [MarshalAs(UnmanagedType.BStr)] string bstrKey,
              [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
        }

        [Guid("0000013D-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IClientSecurity
        {
            void QueryBlanket(
              [MarshalAs(UnmanagedType.IUnknown)] object pProxy,
              ref uint pAuthnSvc,
              ref uint pAuthzSvc,
              [MarshalAs(UnmanagedType.LPWStr)] ref string pServerPrincName,
              ref uint pAuthnLevel,
              ref uint pImpLevel,
              ref IntPtr pAuthInfo,
              ref uint pCapabilities);

            void SetBlanket(
              [MarshalAs(UnmanagedType.IUnknown)] object pProxy,
              uint pAuthnSvc,
              uint pAuthzSvc,
              [MarshalAs(UnmanagedType.LPWStr)] string pServerPrincName,
              uint pAuthnLevel,
              uint pImpLevel,
              IntPtr pAuthInfo,
              uint pCapabilities);

            void CopyProxy([MarshalAs(UnmanagedType.IUnknown)] object pProxy, [MarshalAs(UnmanagedType.IUnknown)] out object ppCopy);
        }

        private sealed class ServerInfo
        {
            private GCHandle m_hUserName;
            private GCHandle m_hPassword;
            private GCHandle m_hDomain;
            private GCHandle m_hIdentity;
            private GCHandle m_hAuthInfo;

            public Interop.COSERVERINFO Allocate(string hostName, NetworkCredential credential)
            {
                string str1 = (string)null;
                string str2 = (string)null;
                string str3 = (string)null;
                if (credential != null)
                {
                    str1 = credential.UserName;
                    str2 = credential.Password;
                    str3 = credential.Domain;
                }
                m_hUserName = GCHandle.Alloc((object)str1, GCHandleType.Pinned);
                m_hPassword = GCHandle.Alloc((object)str2, GCHandleType.Pinned);
                m_hDomain = GCHandle.Alloc((object)str3, GCHandleType.Pinned);
                m_hIdentity = new GCHandle();
                if (!string.IsNullOrEmpty(str1))
                    m_hIdentity = GCHandle.Alloc((object)new Interop.COAUTHIDENTITY()
                    {
                        User = m_hUserName.AddrOfPinnedObject(),
                        UserLength = (str1 != null ? (uint)str1.Length : 0U),
                        Password = m_hPassword.AddrOfPinnedObject(),
                        PasswordLength = (str2 != null ? (uint)str2.Length : 0U),
                        Domain = m_hDomain.AddrOfPinnedObject(),
                        DomainLength = (str3 != null ? (uint)str3.Length : 0U),
                        Flags = 2U
                    }, GCHandleType.Pinned);
                m_hAuthInfo = GCHandle.Alloc((object)new Interop.COAUTHINFO()
                {
                    dwAuthnSvc = 10U,
                    dwAuthzSvc = 0U,
                    pwszServerPrincName = IntPtr.Zero,
                    dwAuthnLevel = 2U,
                    dwImpersonationLevel = 3U,
                    pAuthIdentityData = (m_hIdentity.IsAllocated ? m_hIdentity.AddrOfPinnedObject() : IntPtr.Zero),
                    dwCapabilities = 0U
                }, GCHandleType.Pinned);
                return new Interop.COSERVERINFO()
                {
                    pwszName = hostName,
                    pAuthInfo = credential != null ? m_hAuthInfo.AddrOfPinnedObject() : IntPtr.Zero,
                    dwReserved1 = 0,
                    dwReserved2 = 0
                };
            }

            public void Deallocate()
            {
                if (m_hUserName.IsAllocated)
                    m_hUserName.Free();
                if (m_hPassword.IsAllocated)
                    m_hPassword.Free();
                if (m_hDomain.IsAllocated)
                    m_hDomain.Free();
                if (m_hIdentity.IsAllocated)
                    m_hIdentity.Free();
                if (!m_hAuthInfo.IsAllocated)
                    return;
                m_hAuthInfo.Free();
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct GUID
        {
            public int Data1;
            public short Data2;
            public short Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }
    }
}
