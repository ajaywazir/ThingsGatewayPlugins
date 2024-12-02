

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;


namespace OpcRcw
{
    public static class Utils
    {
        public static readonly Guid CATID_OPCDAServer20 = typeof(OpcRcw.Da.CATID_OPCDAServer20).GUID;
        public static readonly Guid CATID_OPCDAServer30 = typeof(OpcRcw.Da.CATID_OPCDAServer30).GUID;
        public static readonly Guid CATID_OPCAEServer10 = typeof(OpcRcw.Ae.CATID_OPCAEServer10).GUID;
        public static readonly Guid CATID_OPCHDAServer10 = typeof(OpcRcw.Hda.CATID_OPCHDAServer10).GUID;
        private const uint CLSCTX_INPROC_SERVER = 1;
        private const uint CLSCTX_INPROC_HANDLER = 2;
        private const uint CLSCTX_LOCAL_SERVER = 4;
        private const uint CLSCTX_REMOTE_SERVER = 16;
        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private static readonly Guid CLSID_StdComponentCategoriesMgr = new Guid("0002E005-0000-0000-C000-000000000046");
        private const string CATID_OPCDAServer20_Description = "OPC Data Access Servers Version 2.0";
        private const string CATID_OPCDAServer30_Description = "OPC Data Access Servers Version 3.0";
        private const string CATID_OPCAEServer10_Description = "OPC Alarm & Event Server Version 1.0";
        private const string CATID_OPCHDAServer10_Description = "OPC History Data Access Servers Version 1.0";
        private const int MAX_MESSAGE_LENGTH = 1024;
        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 512;
        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 4096;
        public const int LOCALE_SYSTEM_DEFAULT = 2048;
        public const int LOCALE_USER_DEFAULT = 1024;
        private static readonly DateTime FILETIME_BaseTime = new DateTime(1601, 1, 1);
        private const int VARIANT_SIZE = 16;
        private const int DISP_E_TYPEMISMATCH = -2147352571;
        private const int DISP_E_OVERFLOW = -2147352566;
        private const int VARIANT_NOVALUEPROP = 1;
        private const int VARIANT_ALPHABOOL = 2;
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
        private const uint EOAC_STATIC_CLOAKING = 32;
        private const uint EOAC_DYNAMIC_CLOAKING = 64;
        private const uint EOAC_SECURE_REFS = 2;
        private const uint EOAC_ACCESS_CONTROL = 4;
        private const uint EOAC_APPID = 8;
        private static readonly IntPtr COLE_DEFAULT_PRINCIPAL = new IntPtr(-1);
        private static readonly IntPtr COLE_DEFAULT_AUTHINFO = new IntPtr(-1);
        private const uint SEC_WINNT_AUTH_IDENTITY_ANSI = 1;
        private const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int SECURITY_ANONYMOUS = 0;
        private const int SECURITY_IDENTIFICATION = 1;
        private const int SECURITY_IMPERSONATION = 2;
        private const int SECURITY_DELEGATION = 3;

        public static List<Type> RegisterComTypes(string filePath)
        {
            Utils.VerifyCodebase(Assembly.LoadFrom(filePath), filePath);
            throw new NotSupportedException(".NET does not supoprt registration of COM servers.");
        }

        private static void VerifyCodebase(Assembly assembly, string filepath)
        {
            string lower = assembly.CodeBase.ToLower();
            string str = filepath.Replace('\\', '/').Replace("//", "/").ToLower();
            if (!str.StartsWith("file:///"))
                str = "file:///" + str;
            if (lower != str)
                throw new ApplicationException(string.Format("Duplicate assembly loaded. You need to restart the application.\r\n{0}\r\n{1}", (object)lower, (object)str));
        }

        public static List<Type> UnregisterComTypes(string filePath)
        {
            Utils.VerifyCodebase(Assembly.LoadFrom(filePath), filePath);
            throw new NotSupportedException(".NET does not supoprt registration of COM servers.");
        }

        public static string GetSystemMessage(int error, int localeId)
        {
            int dwLanguageId;
            switch (localeId)
            {
                case 1024:
                    dwLanguageId = Utils.GetUserDefaultLangID();
                    break;
                case 2048:
                    dwLanguageId = Utils.GetSystemDefaultLangID();
                    break;
                default:
                    dwLanguageId = (int)ushort.MaxValue & localeId;
                    break;
            }
            IntPtr num = Marshal.AllocCoTaskMem(1024);
            if (Utils.FormatMessageW(4096, IntPtr.Zero, error, dwLanguageId, num, 1023, IntPtr.Zero) > 0)
            {
                string stringUni = Marshal.PtrToStringUni(num);
                Marshal.FreeCoTaskMem(num);
                if (stringUni != null && stringUni.Length > 0)
                    return stringUni.Trim();
            }
            return string.Format("0x{0:X8}", (object)error);
        }

        public static string ProgIDFromCLSID(Guid clsid)
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(string.Format("CLSID\\{{{0}}}\\ProgId", (object)clsid));
            if (registryKey == null)
                return (string)null;
            try
            {
                return registryKey.GetValue("") as string;
            }
            finally
            {
                registryKey.Close();
            }
        }

        public static Guid CLSIDFromProgID(string progID)
        {
            if (string.IsNullOrEmpty(progID))
                return Guid.Empty;
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(string.Format("{0}\\CLSID", (object)progID));
            if (registryKey != null)
            {
                try
                {
                    if (registryKey.GetValue((string)null) is string str)
                        return new Guid(str.Substring(1, str.Length - 2));
                }
                finally
                {
                    registryKey.Close();
                }
            }
            return Guid.Empty;
        }

        public static List<Guid> GetImplementedCategories(Guid clsid)
        {
            List<Guid> implementedCategories = new List<Guid>();
            string name = string.Format("CLSID\\{{{0}}}\\Implemented Categories", (object)clsid);
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(name);
            if (registryKey != null)
            {
                try
                {
                    foreach (string subKeyName in registryKey.GetSubKeyNames())
                    {
                        try
                        {
                            Guid guid = new Guid(subKeyName.Substring(1, subKeyName.Length - 2));
                            implementedCategories.Add(guid);
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    registryKey.Close();
                }
            }
            return implementedCategories;
        }

        public static List<Guid> EnumClassesInCategories(params Guid[] categories)
        {
            Utils.ICatInformation localServer = (Utils.ICatInformation)Utils.CreateLocalServer(Utils.CLSID_StdComponentCategoriesMgr);
            object ppenumClsid = (object)null;
            try
            {
                localServer.EnumClassesOfCategories(1, categories, 0, (Guid[])null, out ppenumClsid);
                Utils.IEnumGUID enumGuid = (Utils.IEnumGUID)ppenumClsid;
                List<Guid> guidList = new List<Guid>();
                Guid[] guidArray = new Guid[10];
            label_2:
                int pceltFetched = 0;
                IntPtr num = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)) * guidArray.Length);
                try
                {
                    enumGuid.Next(guidArray.Length, num, out pceltFetched);
                    if (pceltFetched != 0)
                    {
                        IntPtr ptr = num;
                        for (int index = 0; index < pceltFetched; ++index)
                        {
                            guidArray[index] = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
                            ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(Guid)));
                        }
                    }
                    else
                        goto label_11;
                }
                finally
                {
                    Marshal.FreeCoTaskMem(num);
                }
                for (int index = 0; index < pceltFetched; ++index)
                    guidList.Add(guidArray[index]);
                goto label_2;
            label_11:
                return guidList;
            }
            finally
            {
                Utils.ReleaseServer(ppenumClsid);
                Utils.ReleaseServer((object)localServer);
            }
        }

        public static string GetExecutablePath(Guid clsid)
        {
            using RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(string.Format("CLSID\\{{{0}}}\\LocalServer32", (object)clsid)) ?? Registry.ClassesRoot.OpenSubKey(string.Format("CLSID\\{{{0}}}\\InprocServer32", (object)clsid));
            if (registryKey == null)
                return (string)null;
            try
            {
                return !(registryKey.GetValue("Codebase") is string str) ? registryKey.GetValue((string)null) as string : str;
            }
            finally
            {
                registryKey.Close();
            }
        }

        public static object CreateLocalServer(Guid clsid)
        {
            Utils.COSERVERINFO pServerInfo = new Utils.COSERVERINFO();
            pServerInfo.pwszName = (string)null;
            pServerInfo.pAuthInfo = IntPtr.Zero;
            pServerInfo.dwReserved1 = 0U;
            pServerInfo.dwReserved2 = 0U;
            GCHandle gcHandle = GCHandle.Alloc((object)Utils.IID_IUnknown, GCHandleType.Pinned);
            Utils.MULTI_QI[] pResults = new Utils.MULTI_QI[1];
            pResults[0].iid = gcHandle.AddrOfPinnedObject();
            pResults[0].pItf = (object)null;
            pResults[0].hr = 0U;
            try
            {
                Utils.CoCreateInstanceEx(ref clsid, (object)null, 5U, ref pServerInfo, 1U, pResults);
            }
            finally
            {
                gcHandle.Free();
            }
            return pResults[0].hr == 0U ? pResults[0].pItf : throw new ExternalException("CoCreateInstanceEx: 0x{0:X8}" + pResults[0].hr.ToString());
        }

        public static object CreateInstance(
          Guid clsid,
          string hostName,
          string username,
          string password,
          string domain,
          bool useConnectSecurity = false)
        {
            Utils.ServerInfo serverInfo = new Utils.ServerInfo();
            Utils.COSERVERINFO pServerInfo = serverInfo.Allocate(hostName, username, password, domain, useConnectSecurity);
            GCHandle gcHandle = GCHandle.Alloc((object)Utils.IID_IUnknown, GCHandleType.Pinned);
            Utils.MULTI_QI[] pResults = new Utils.MULTI_QI[1];
            pResults[0].iid = gcHandle.AddrOfPinnedObject();
            pResults[0].pItf = (object)null;
            pResults[0].hr = 0U;
            try
            {
                uint dwClsCtx = 5;
                if (!string.IsNullOrEmpty(hostName) && hostName != "localhost")
                    dwClsCtx = 20U;
                Utils.CoCreateInstanceEx(ref clsid, (object)null, dwClsCtx, ref pServerInfo, 1U, pResults);
            }
            finally
            {
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
                serverInfo.Deallocate();
            }
            return pResults[0].hr == 0U ? pResults[0].pItf : throw Utils.CreateComException(-2147467259, "Could not create COM server '{0}' on host '{1}'. Reason: {2}.", (object)clsid, (object)hostName, (object)Utils.GetSystemMessage((int)pResults[0].hr, 2048));
        }

        public static void ReleaseServer(object server)
        {
            if (server == null || !server.GetType().IsCOMObject)
                return;
            Marshal.ReleaseComObject(server);
        }

        public static void RegisterClassInCategory(Guid clsid, Guid catid)
        {
            Utils.RegisterClassInCategory(clsid, catid, (string)null);
        }

        public static void RegisterClassInCategory(Guid clsid, Guid catid, string description)
        {
            Utils.ICatRegister localServer = (Utils.ICatRegister)Utils.CreateLocalServer(Utils.CLSID_StdComponentCategoriesMgr);
            try
            {
                string pszDesc = (string)null;
                try
                {
                    ((Utils.ICatInformation)localServer).GetCategoryDesc(catid, 0, out pszDesc);
                }
                catch (Exception ex)
                {
                    string str = description;
                    if (string.IsNullOrEmpty(str))
                    {
                        if (catid == Utils.CATID_OPCDAServer20)
                            str = "OPC Data Access Servers Version 2.0";
                        else if (catid == Utils.CATID_OPCDAServer30)
                            str = "OPC Data Access Servers Version 3.0";
                        else if (catid == Utils.CATID_OPCAEServer10)
                        {
                            str = "OPC Alarm & Event Server Version 1.0";
                        }
                        else
                        {
                            if (!(catid == Utils.CATID_OPCHDAServer10))
                                throw new ApplicationException("No description for category available", ex);
                            str = "OPC History Data Access Servers Version 1.0";
                        }
                    }
                    Utils.CATEGORYINFO categoryinfo;
                    categoryinfo.catid = catid;
                    categoryinfo.lcid = 0;
                    categoryinfo.szDescription = str;
                    localServer.RegisterCategories(1, new Utils.CATEGORYINFO[1]
                    {
            categoryinfo
                    });
                }
                localServer.RegisterClassImplCategories(clsid, 1, new Guid[1]
                {
          catid
                });
            }
            finally
            {
                Utils.ReleaseServer((object)localServer);
            }
        }

        public static void UnregisterComServer(Guid clsid)
        {
            string name1 = string.Format("CLSID\\{{{0}}}\\Implemented Categories", (object)clsid);
            RegistryKey registryKey1 = Registry.ClassesRoot.OpenSubKey(name1);
            if (registryKey1 != null)
            {
                try
                {
                    foreach (string subKeyName in registryKey1.GetSubKeyNames())
                    {
                        try
                        {
                            Utils.UnregisterClassInCategory(clsid, new Guid(subKeyName.Substring(1, subKeyName.Length - 2)));
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    registryKey1.Close();
                }
            }
            string name2 = string.Format("CLSID\\{{{0}}}\\ProgId", (object)clsid);
            RegistryKey registryKey2 = Registry.ClassesRoot.OpenSubKey(name2);
            if (registryKey2 != null)
            {
                string subkey = registryKey2.GetValue((string)null) as string;
                registryKey2.Close();
                if (!string.IsNullOrEmpty(subkey))
                {
                    try
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree(subkey);
                    }
                    catch
                    {
                    }
                }
            }
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(string.Format("CLSID\\{{{0}}}", (object)clsid));
            }
            catch
            {
            }
        }

        public static void UnregisterClassInCategory(Guid clsid, Guid catid)
        {
            Utils.ICatRegister localServer = (Utils.ICatRegister)Utils.CreateLocalServer(Utils.CLSID_StdComponentCategoriesMgr);
            try
            {
                localServer.UnRegisterClassImplCategories(clsid, 1, new Guid[1]
                {
          catid
                });
            }
            finally
            {
                Utils.ReleaseServer((object)localServer);
            }
        }

        public static Exception CreateComException(Exception e)
        {
            return Utils.CreateComException(e, 0, (string)null);
        }

        public static Exception CreateComException(int code, string message, params object[] args)
        {
            return Utils.CreateComException((Exception)null, code, message, args);
        }

        public static Exception CreateComException(
          Exception e,
          int code,
          string message,
          params object[] args)
        {
            if (code == 0)
                code = !(e is COMException) ? -2147467259 : ((ExternalException)e).ErrorCode;
            if (!string.IsNullOrEmpty(message))
            {
                if (args != null && args.Length != 0)
                    message = string.Format((IFormatProvider)CultureInfo.CurrentUICulture, message, args);
            }
            else
                message = e == null ? Utils.GetSystemMessage(code, CultureInfo.CurrentUICulture.LCID) : e.Message;
            return (Exception)new COMException(message, code);
        }

        [DllImport("ole32.dll")]
        private static extern void CoCreateInstanceEx(
          ref Guid clsid,
          [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
          uint dwClsCtx,
          [In] ref Utils.COSERVERINFO pServerInfo,
          uint dwCount,
          [In, Out] Utils.MULTI_QI[] pResults);

        [DllImport("Kernel32.dll")]
        private static extern int FormatMessageW(
          int dwFlags,
          IntPtr lpSource,
          int dwMessageId,
          int dwLanguageId,
          IntPtr lpBuffer,
          int nSize,
          IntPtr Arguments);

        [DllImport("Kernel32.dll")]
        private static extern int GetSystemDefaultLangID();

        [DllImport("Kernel32.dll")]
        private static extern int GetUserDefaultLangID();

        [DllImport("OleAut32.dll")]
        private static extern int VariantChangeTypeEx(
          IntPtr pvargDest,
          IntPtr pvarSrc,
          int lcid,
          ushort wFlags,
          short vt);

        [DllImport("oleaut32.dll")]
        private static extern void VariantInit(IntPtr pVariant);

        [DllImport("oleaut32.dll")]
        private static extern void VariantClear(IntPtr pVariant);

        [DllImport("ole32.dll")]
        private static extern int CoInitializeSecurity(
          IntPtr pSecDesc,
          int cAuthSvc,
          Utils.SOLE_AUTHENTICATION_SERVICE[] asAuthSvc,
          IntPtr pReserved1,
          uint dwAuthnLevel,
          uint dwImpLevel,
          IntPtr pAuthList,
          uint dwCapabilities,
          IntPtr pReserved3);

        [DllImport("ole32.dll")]
        private static extern int CoQueryProxyBlanket(
          [MarshalAs(UnmanagedType.IUnknown)] object pProxy,
          ref uint pAuthnSvc,
          ref uint pAuthzSvc,
          [MarshalAs(UnmanagedType.LPWStr)] ref string pServerPrincName,
          ref uint pAuthnLevel,
          ref uint pImpLevel,
          ref IntPtr pAuthInfo,
          ref uint pCapabilities);

        [DllImport("ole32.dll")]
        private static extern int CoSetProxyBlanket(
          [MarshalAs(UnmanagedType.IUnknown)] object pProxy,
          uint pAuthnSvc,
          uint pAuthzSvc,
          IntPtr pServerPrincName,
          uint pAuthnLevel,
          uint pImpLevel,
          IntPtr pAuthInfo,
          uint pCapabilities);

        [DllImport("ole32.dll")]
        private static extern void CoGetClassObject(
          [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
          uint dwClsContext,
          [In] ref Utils.COSERVERINFO pServerInfo,
          [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
          [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(
          string lpszUsername,
          string lpszDomain,
          string lpszPassword,
          int dwLogonType,
          int dwLogonProvider,
          ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DuplicateToken(
          IntPtr ExistingTokenHandle,
          int SECURITY_IMPERSONATION_LEVEL,
          ref IntPtr DuplicateTokenHandle);

        private sealed class ServerInfo
        {
            private GCHandle m_hUserName;
            private GCHandle m_hPassword;
            private GCHandle m_hDomain;
            private GCHandle m_hIdentity;
            private GCHandle m_hAuthInfo;

            public Utils.COSERVERINFO Allocate(
              string hostName,
              string username,
              string password,
              string domain,
              bool useConnectSecurity = false)
            {
                Utils.COSERVERINFO coserverinfo = new Utils.COSERVERINFO();
                coserverinfo.pwszName = hostName;
                coserverinfo.pAuthInfo = IntPtr.Zero;
                coserverinfo.dwReserved1 = 0U;
                coserverinfo.dwReserved2 = 0U;
                if (string.IsNullOrEmpty(username))
                    return coserverinfo;
                m_hUserName = GCHandle.Alloc((object)username, GCHandleType.Pinned);
                m_hPassword = GCHandle.Alloc((object)password, GCHandleType.Pinned);
                m_hDomain = GCHandle.Alloc((object)domain, GCHandleType.Pinned);
                m_hIdentity = new GCHandle();
                m_hIdentity = GCHandle.Alloc((object)new Utils.COAUTHIDENTITY()
                {
                    User = m_hUserName.AddrOfPinnedObject(),
                    UserLength = (username != null ? (uint)username.Length : 0U),
                    Password = m_hPassword.AddrOfPinnedObject(),
                    PasswordLength = (password != null ? (uint)password.Length : 0U),
                    Domain = m_hDomain.AddrOfPinnedObject(),
                    DomainLength = (domain != null ? (uint)domain.Length : 0U),
                    Flags = 2U
                }, GCHandleType.Pinned);
                m_hAuthInfo = GCHandle.Alloc((object)new Utils.COAUTHINFO()
                {
                    dwAuthnSvc = 10U,
                    dwAuthzSvc = 0U,
                    pwszServerPrincName = IntPtr.Zero,
                    dwAuthnLevel = (useConnectSecurity ? 2U : 5U),
                    pAuthIdentityData = m_hIdentity.AddrOfPinnedObject(),
                    dwCapabilities = 0U
                }, GCHandleType.Pinned);
                coserverinfo.pAuthInfo = m_hAuthInfo.AddrOfPinnedObject();
                return coserverinfo;
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
        private struct MULTI_QI
        {
            public IntPtr iid;
            [MarshalAs(UnmanagedType.IUnknown)]
            public object pItf;
            public uint hr;
        }

        [Guid("0002E000-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IEnumGUID
        {
            void Next([MarshalAs(UnmanagedType.I4)] int celt, [Out] IntPtr rgelt, [MarshalAs(UnmanagedType.I4)] out int pceltFetched);

            void Skip([MarshalAs(UnmanagedType.I4)] int celt);

            void Reset();

            void Clone(out Utils.IEnumGUID ppenum);
        }

        [Guid("0002E013-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface ICatInformation
        {
            void EnumCategories(int lcid, [MarshalAs(UnmanagedType.Interface)] out object ppenumCategoryInfo);

            void GetCategoryDesc([MarshalAs(UnmanagedType.LPStruct)] Guid rcatid, int lcid, [MarshalAs(UnmanagedType.LPWStr)] out string pszDesc);

            void EnumClassesOfCategories(
              int cImplemented,
              [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatidImpl,
              int cRequired,
              [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatidReq,
              [MarshalAs(UnmanagedType.Interface)] out object ppenumClsid);

            void IsClassOfCategories(
              [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
              int cImplemented,
              [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatidImpl,
              int cRequired,
              [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatidReq);

            void EnumImplCategoriesOfClass([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [MarshalAs(UnmanagedType.Interface)] out object ppenumCatid);

            void EnumReqCategoriesOfClass([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [MarshalAs(UnmanagedType.Interface)] out object ppenumCatid);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CATEGORYINFO
        {
            public Guid catid;
            public int lcid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 127)]
            public string szDescription;
        }

        [Guid("0002E012-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface ICatRegister
        {
            void RegisterCategories(int cCategories, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] Utils.CATEGORYINFO[] rgCategoryInfo);

            void UnRegisterCategories(int cCategories, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatid);

            void RegisterClassImplCategories([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, int cCategories, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatid);

            void UnRegisterClassImplCategories([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, int cCategories, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatid);

            void RegisterClassReqCategories([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, int cCategories, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatid);

            void UnRegisterClassReqCategories([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, int cCategories, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.LPStruct)] Guid[] rgcatid);
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
        private struct LICINFO
        {
            public int cbLicInfo;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fRuntimeKeyAvail;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fLicVerified;
        }

        [Guid("00000001-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IClassFactory
        {
            void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object punkOuter, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

            void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
        }

        [Guid("B196B28F-BAB4-101A-B69C-00AA00341D07")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IClassFactory2
        {
            void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object punkOuter, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

            void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);

            void GetLicInfo([In, Out] ref Utils.LICINFO pLicInfo);

            void RequestLicKey(int dwReserved, [MarshalAs(UnmanagedType.BStr)] string pbstrKey);

            void CreateInstanceLic(
              [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
              [MarshalAs(UnmanagedType.IUnknown)] object punkReserved,
              [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
              [MarshalAs(UnmanagedType.BStr)] string bstrKey,
              [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
        }
    }
}
