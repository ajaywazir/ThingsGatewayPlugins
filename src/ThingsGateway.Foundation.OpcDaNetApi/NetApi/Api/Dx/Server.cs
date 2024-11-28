

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Dx
{
    [Serializable]
    public class Server : Opc.Da.Server, IServer, Opc.Da.IServer, Opc.IServer, IDisposable, ISerializable
    {
        private string m_version;
        private SourceServerCollection m_sourceServers = new SourceServerCollection();
        private DXConnectionQueryCollection m_connectionQueries = new DXConnectionQueryCollection();

        public Server(Factory factory, URL url)
          : base(factory, url)
        {
        }

        public string Version => m_version;

        public SourceServerCollection SourceServers => m_sourceServers;

        public DXConnectionQueryCollection Queries => m_connectionQueries;

        public SourceServer AddSourceServer(SourceServer server)
        {
            GeneralResponse generalResponse = AddSourceServers(new SourceServer[1]
            {
        server
            });
            if (generalResponse == null || generalResponse.Count != 1)
                throw new InvalidResponseException();
            if (generalResponse[0].ResultID.Failed())
                throw new ResultIDException(generalResponse[0].ResultID);
            SourceServer sourceServer = new SourceServer(server);
            sourceServer.ItemName = generalResponse[0].ItemName;
            sourceServer.ItemPath = generalResponse[0].ItemPath;
            sourceServer.Version = generalResponse[0].Version;
            return sourceServer;
        }

        public SourceServer ModifySourceServer(SourceServer server)
        {
            GeneralResponse generalResponse = ModifySourceServers(new SourceServer[1]
            {
        server
            });
            if (generalResponse == null || generalResponse.Count != 1)
                throw new InvalidResponseException();
            if (generalResponse[0].ResultID.Failed())
                throw new ResultIDException(generalResponse[0].ResultID);
            SourceServer sourceServer = new SourceServer(server);
            sourceServer.ItemName = generalResponse[0].ItemName;
            sourceServer.ItemPath = generalResponse[0].ItemPath;
            sourceServer.Version = generalResponse[0].Version;
            return sourceServer;
        }

        public void DeleteSourceServer(SourceServer server)
        {
            GeneralResponse generalResponse = DeleteSourceServers(new ItemIdentifier[1]
            {
        (ItemIdentifier) server
            });
            if (generalResponse == null || generalResponse.Count != 1)
                throw new InvalidResponseException();
            if (generalResponse[0].ResultID.Failed())
                throw new ResultIDException(generalResponse[0].ResultID);
        }

        public DXConnection AddDXConnection(DXConnection connection)
        {
            GeneralResponse generalResponse = AddDXConnections(new DXConnection[1]
            {
        connection
            });
            if (generalResponse == null || generalResponse.Count != 1)
                throw new InvalidResponseException();
            if (generalResponse[0].ResultID.Failed())
                throw new ResultIDException(generalResponse[0].ResultID);
            DXConnection dxConnection = new DXConnection(connection);
            dxConnection.ItemName = generalResponse[0].ItemName;
            dxConnection.ItemPath = generalResponse[0].ItemPath;
            dxConnection.Version = generalResponse[0].Version;
            return dxConnection;
        }

        public DXConnection ModifyDXConnection(DXConnection connection)
        {
            GeneralResponse generalResponse = ModifyDXConnections(new DXConnection[1]
            {
        connection
            });
            if (generalResponse == null || generalResponse.Count != 1)
                throw new InvalidResponseException();
            if (generalResponse[0].ResultID.Failed())
                throw new ResultIDException(generalResponse[0].ResultID);
            DXConnection dxConnection = new DXConnection(connection);
            dxConnection.ItemName = generalResponse[0].ItemName;
            dxConnection.ItemPath = generalResponse[0].ItemPath;
            dxConnection.Version = generalResponse[0].Version;
            return dxConnection;
        }

        public void DeleteDXConnections(DXConnection connection)
        {
            ResultID[] errors = (ResultID[])null;
            GeneralResponse generalResponse = DeleteDXConnections((string)null, new DXConnection[1]
            {
        connection
            }, true, out errors);
            if (errors != null && errors.Length != 0 && errors[0].Failed())
                throw new ResultIDException(errors[0]);
            if (generalResponse == null || generalResponse.Count != 1)
                throw new InvalidResponseException();
            if (generalResponse[0].ResultID.Failed())
                throw new ResultIDException(generalResponse[0].ResultID);
        }

        protected Server(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            DXConnectionQuery[] dxConnectionQueryArray = (DXConnectionQuery[])info.GetValue(nameof(Queries), typeof(DXConnectionQuery[]));
            if (dxConnectionQueryArray == null)
                return;
            foreach (DXConnectionQuery dxConnectionQuery in dxConnectionQueryArray)
                m_connectionQueries.Add(dxConnectionQuery);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            DXConnectionQuery[] dxConnectionQueryArray = (DXConnectionQuery[])null;
            if (m_connectionQueries.Count > 0)
            {
                dxConnectionQueryArray = new DXConnectionQuery[m_connectionQueries.Count];
                for (int index = 0; index < dxConnectionQueryArray.Length; ++index)
                    dxConnectionQueryArray[index] = m_connectionQueries[index];
            }
            info.AddValue("Queries", (object)dxConnectionQueryArray);
        }

        public SourceServer[] GetSourceServers()
        {
            SourceServer[] sourceServers = m_server != null ? ((IServer)m_server).GetSourceServers() : throw new NotConnectedException();
            m_sourceServers.Initialize((ICollection)sourceServers);
            return sourceServers;
        }

        public GeneralResponse AddSourceServers(SourceServer[] servers)
        {
            GeneralResponse generalResponse = m_server != null ? ((IServer)m_server).AddSourceServers(servers) : throw new NotConnectedException();
            if (generalResponse != null)
            {
                GetSourceServers();
                m_version = generalResponse.Version;
            }
            return generalResponse;
        }

        public GeneralResponse ModifySourceServers(SourceServer[] servers)
        {
            GeneralResponse generalResponse = m_server != null ? ((IServer)m_server).ModifySourceServers(servers) : throw new NotConnectedException();
            if (generalResponse != null)
            {
                GetSourceServers();
                m_version = generalResponse.Version;
            }
            return generalResponse;
        }

        public GeneralResponse DeleteSourceServers(ItemIdentifier[] servers)
        {
            GeneralResponse generalResponse = m_server != null ? ((IServer)m_server).DeleteSourceServers(servers) : throw new NotConnectedException();
            if (generalResponse != null)
            {
                GetSourceServers();
                m_version = generalResponse.Version;
            }
            return generalResponse;
        }

        public GeneralResponse CopyDefaultSourceServerAttributes(
          bool configToStatus,
          ItemIdentifier[] servers)
        {
            if (m_server == null)
                throw new NotConnectedException();
            GeneralResponse generalResponse = ((IServer)m_server).CopyDefaultSourceServerAttributes(configToStatus, servers);
            if (generalResponse != null)
            {
                if (!configToStatus)
                    GetSourceServers();
                m_version = generalResponse.Version;
            }
            return generalResponse;
        }

        public DXConnection[] QueryDXConnections(
          string browsePath,
          DXConnection[] connectionMasks,
          bool recursive,
          out ResultID[] errors)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).QueryDXConnections(browsePath, connectionMasks, recursive, out errors);
        }

        public GeneralResponse AddDXConnections(DXConnection[] connections)
        {
            GeneralResponse generalResponse = m_server != null ? ((IServer)m_server).AddDXConnections(connections) : throw new NotConnectedException();
            if (generalResponse != null)
                m_version = generalResponse.Version;
            return generalResponse;
        }

        public GeneralResponse ModifyDXConnections(DXConnection[] connections)
        {
            GeneralResponse generalResponse = m_server != null ? ((IServer)m_server).ModifyDXConnections(connections) : throw new NotConnectedException();
            if (generalResponse != null)
                m_version = generalResponse.Version;
            return generalResponse;
        }

        public GeneralResponse UpdateDXConnections(
          string browsePath,
          DXConnection[] connectionMasks,
          bool recursive,
          DXConnection connectionDefinition,
          out ResultID[] errors)
        {
            if (m_server == null)
                throw new NotConnectedException();
            GeneralResponse generalResponse = ((IServer)m_server).UpdateDXConnections(browsePath, connectionMasks, recursive, connectionDefinition, out errors);
            if (generalResponse != null)
                m_version = generalResponse.Version;
            return generalResponse;
        }

        public GeneralResponse DeleteDXConnections(
          string browsePath,
          DXConnection[] connectionMasks,
          bool recursive,
          out ResultID[] errors)
        {
            if (m_server == null)
                throw new NotConnectedException();
            GeneralResponse generalResponse = ((IServer)m_server).DeleteDXConnections(browsePath, connectionMasks, recursive, out errors);
            if (generalResponse != null)
                m_version = generalResponse.Version;
            return generalResponse;
        }

        public GeneralResponse CopyDXConnectionDefaultAttributes(
          bool configToStatus,
          string browsePath,
          DXConnection[] connectionMasks,
          bool recursive,
          out ResultID[] errors)
        {
            if (m_server == null)
                throw new NotConnectedException();
            GeneralResponse generalResponse = ((IServer)m_server).CopyDXConnectionDefaultAttributes(configToStatus, browsePath, connectionMasks, recursive, out errors);
            if (generalResponse != null)
                m_version = generalResponse.Version;
            return generalResponse;
        }

        public string ResetConfiguration(string configurationVersion)
        {
            if (m_server == null)
                throw new NotConnectedException();
            m_version = ((IServer)m_server).ResetConfiguration(configurationVersion == null ? m_version : configurationVersion);
            return m_version;
        }

        private class Names
        {
            internal const string QUERIES = "Queries";
        }
    }
}
