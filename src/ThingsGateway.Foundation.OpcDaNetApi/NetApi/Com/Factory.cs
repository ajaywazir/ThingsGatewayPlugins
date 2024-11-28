

using Opc;

using OpcRcw.Ae;
using OpcRcw.Da;
using OpcRcw.Dx;
using OpcRcw.Hda;

using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;


namespace OpcCom
{
    [Serializable]
    public class Factory : Opc.Factory
    {
        public Factory()
          : base((System.Type)null, false)
        {
        }

        public Factory(bool useRemoting)
          : base((System.Type)null, useRemoting)
        {
        }

        protected Factory(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        public override IServer CreateInstance(URL url, ConnectData connectData)
        {
            object obj = Factory.Connect(url, connectData);
            if (obj == null)
                return (IServer)null;
            Server server1 = (Server)null;
            System.Type type = (System.Type)null;
            Server instance;
            try
            {
                if (url.Scheme == "opcda")
                {
                    if (!typeof(IOPCServer).IsInstanceOfType(obj))
                    {
                        type = typeof(IOPCServer);
                        throw new NotSupportedException();
                    }
                    if (typeof(IOPCBrowse).IsInstanceOfType(obj) && typeof(IOPCItemIO).IsInstanceOfType(obj))
                        instance = (Server)new OpcCom.Da.Server(url, obj);
                    else if (typeof(IOPCItemProperties).IsInstanceOfType(obj))
                    {
                        instance = (Server)new OpcCom.Da20.Server(url, obj);
                    }
                    else
                    {
                        type = typeof(IOPCItemProperties);
                        throw new NotSupportedException();
                    }
                }
                else if (url.Scheme == "opcae")
                {
                    if (!typeof(IOPCEventServer).IsInstanceOfType(obj))
                    {
                        type = typeof(IOPCEventServer);
                        throw new NotSupportedException();
                    }
                    instance = (Server)new OpcCom.Ae.Server(url, obj);
                }
                else if (url.Scheme == "opchda")
                {
                    if (!typeof(IOPCHDA_Server).IsInstanceOfType(obj))
                    {
                        type = typeof(IOPCHDA_Server);
                        throw new NotSupportedException();
                    }
                    instance = (Server)new OpcCom.Hda.Server(url, obj);
                }
                else
                {
                    if (!(url.Scheme == "opcdx"))
                        throw new NotSupportedException(string.Format("The URL scheme '{0}' is not supported.", (object)url.Scheme));
                    if (!typeof(IOPCConfiguration).IsInstanceOfType(obj))
                    {
                        type = typeof(IOPCConfiguration);
                        throw new NotSupportedException();
                    }
                    instance = (Server)new OpcCom.Dx.Server(url, obj);
                }
            }
            catch (NotSupportedException ex)
            {
                Interop.ReleaseServer((object)server1);
                if (type != (System.Type)null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("The COM server does not support the interface ");
                    stringBuilder.AppendFormat("'{0}'.", (object)type.FullName);
                    stringBuilder.Append("\r\n\r\nThis problem could be caused by:\r\n");
                    stringBuilder.Append("- incorrectly installed proxy/stubs.\r\n");
                    stringBuilder.Append("- problems with the DCOM security settings.\r\n");
                    stringBuilder.Append("- a personal firewall (sometimes activated by default).\r\n");
                    throw new NotSupportedException(stringBuilder.ToString());
                }
                throw ex;
            }
            catch (Exception ex)
            {
                Interop.ReleaseServer((object)server1);
                throw ex;
            }
            instance?.Initialize(url, connectData);
            return (IServer)instance;
        }

        public static object Connect(URL url, ConnectData connectData)
        {
            string str1 = url.Path;
            string str2 = (string)null;
            int length = url.Path.LastIndexOf('/');
            if (length >= 0)
            {
                str1 = url.Path.Substring(0, length);
                str2 = url.Path.Substring(length + 1);
            }
            Guid clsid;
            if (str2 == null)
            {
                clsid = new ServerEnumerator2().CLSIDFromProgID(str1, url.HostName, connectData);
                if (clsid == Guid.Empty)
                {
                    try
                    {
                        clsid = new ServerEnumerator1().CLSIDFromProgID(str1, url.HostName, connectData);

                    }
                    catch (Exception)
                    {


                        try
                        {
                            clsid = new Guid(str1);
                        }
                        catch
                        {
                            throw new ConnectFailedException(str1);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    clsid = new Guid(str2);
                }
                catch
                {
                    throw new ConnectFailedException(str2);
                }
            }
            NetworkCredential credential = connectData?.GetCredential((Uri)null, (string)null);
            if (connectData != null)
            {
                if (connectData.LicenseKey != null)
                {
                    try
                    {
                        return Interop.CreateInstanceWithLicenseKey(clsid, url.HostName, credential, connectData.LicenseKey);
                    }
                    catch (Exception ex)
                    {
                        throw new ConnectFailedException(ex);
                    }
                }
            }
            try
            {
                return Interop.CreateInstance(clsid, url.HostName, credential);
            }
            catch (Exception ex)
            {
                throw new ConnectFailedException(ex);
            }
        }
    }
}
