//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Opc;
using Opc.Da;

using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

using Timer = System.Timers.Timer;

namespace ThingsGateway.Foundation.OpcDaNetApi;
public class OpcDaTagModel
{
    public List<OpcDaTagModel> Children { get; set; }
    public string NodeId => (Tag?.ItemName)?.ToString();
    public BrowseElement Tag { get; set; }
    public string Name { get; set; }

    public List<OpcDaTagModel> GetAllTags()
    {
        List<OpcDaTagModel> allTags = new();
        GetAllTagsRecursive(this, allTags);
        return allTags;
    }

    private void GetAllTagsRecursive(OpcDaTagModel parentTag, List<OpcDaTagModel> allTags)
    {
        allTags.Add(parentTag);
        if (parentTag.Children != null)
            foreach (OpcDaTagModel childTag in parentTag.Children)
            {
                GetAllTagsRecursive(childTag, allTags);
            }
    }
}

/// <summary>
/// 日志输出
/// </summary>
public delegate void LogEventHandler(byte level, object sender, string message, Exception ex);

/// <summary>
/// OPCDAClient
/// </summary>
public class OpcDaNetApiMaster : IDisposable
{
    /// <summary>
    /// OPCDAClient
    /// </summary>
    ~OpcDaNetApiMaster()
    {
        Dispose();
    }

    #region 配置项

    /// <summary>
    /// 当前配置
    /// </summary>
    public OpcDaNetApiMasterProperty OpcDaNetApiMasterProperty { get; private set; }

    /// <summary>
    /// 数据变化事件
    /// </summary>
    public event DataChangedEventHandler DataChangedHandler;

    /// <summary>
    /// 日志输出
    /// </summary>
    public LogEventHandler LogEvent;

    #endregion 配置项

    private readonly object checkLock = new();

    private Timer checkTimer;

    private int IsExit = 1;

    public Opc.Da.Server Server { get; private set; }
    private bool publicConnect;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public OpcDaNetApiMaster()
    {
#if (NETSTANDARD2_0_OR_GREATER)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotSupportedException("Non Windows systems are not supported");
        }
#endif
    }

    /// <summary>
    /// 获取变量说明
    /// </summary>
    /// <returns></returns>
    public string GetAddressDescription()
    {
        return "ItemName";
    }

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public bool IsConnected => Server?.IsConnected == true;


    public void ClearSubscriptions()
    {
        if (this.Server != null)
        {
            foreach (Subscription subscription in this.Server.Subscriptions)
            {
                subscription.Dispose();
                try
                {
                    ((OpcRcw.Da.IOPCServer)this.Server).RemoveGroup((int)subscription.GetState().ServerHandle, 0);
                }
                catch
                {
                }
            }
            this.Server.Subscriptions.Clear();
        }
    }


    /// <summary>
    /// 添加节点，需要在连接成功后执行
    /// </summary>
    /// <param name="items">组名称/变量节点，注意每次添加的组名称不能相同</param>
    public void AddItems(Dictionary<string, List<Opc.Da.Item>> items)
    {
        if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaNetApiMaster));
        foreach (var item in items)
        {
            if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaNetApiMaster));
            try
            {
                var groupSet = new Opc.Da.SubscriptionState()
                {
                    Active = OpcDaNetApiMasterProperty.ActiveSubscribe,
                    Deadband = OpcDaNetApiMasterProperty.DeadBand,
                    KeepAlive = 3000,
                    UpdateRate = OpcDaNetApiMasterProperty.UpdateRate,
                    Locale = null,
                    Name = item.Key,
                    ClientHandle = 1,
                    ServerHandle = null
                };
                var group = Server.CreateSubscription(groupSet);
                group.DataChanged += Group_DataChanged; ;
                var result = group.AddItems(item.Value.ToArray());

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item1 in result)
                {
                    if (!item1.ResultID.Succeeded())
                        stringBuilder.AppendLine($"{item1.ResultID}");
                }
                if (stringBuilder.Length > 0)
                    LogEvent?.Invoke(3, this, $"Failed to add variable：{stringBuilder}", null);

            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(3, this, $"Failed to add group：{ex.Message}", ex);
            }
        }
    }

    private void Group_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
    {
        DataChangedHandler?.Invoke(subscriptionHandle, requestHandle, values);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connect()
    {
        publicConnect = true;
        Interlocked.CompareExchange(ref IsExit, 0, 1);
        PrivateConnect();
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        Interlocked.CompareExchange(ref IsExit, 1, 0);
        PrivateDisconnect();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            PrivateDisconnect();
        }
        catch (Exception ex)
        {
            LogEvent?.Invoke(3, this, $"Disconnect warn：{ex.Message}", ex);
        }
        checkTimer?.Dispose();
        Interlocked.CompareExchange(ref IsExit, 1, 0);
    }

    /// <summary>
    /// 浏览节点
    /// </summary>
    public List<OpcDaTagModel> GetTag(bool all, string itemPath = null, string itemName = null)
    {
        List<OpcDaTagModel> opcDaTagModels = new();
        BrowseFilters m_filters = new BrowseFilters()
        {
            PropertyIDs = new PropertyID[] { new(1), new(3), new(4), new(5), new(6), new(101) },//浏览的属性ID

            ReturnAllProperties = false,//获取数据项的属性

            ReturnPropertyValues = true,//要求返回属性的值
        };
        ItemIdentifier itemID = null;
        itemID = new ItemIdentifier(itemPath, itemName);
        BrowseElement[] elements = Server.Browse(itemID, m_filters, out BrowsePosition position);
        if (elements != null)
        {
            foreach (BrowseElement element in elements)
            {
                OpcDaTagModel opcDaTagModel = new();
                opcDaTagModel.Tag = element;
                opcDaTagModel.Name = element.Name;

                if (element.HasChildren)
                {
                    if (all)
                        opcDaTagModel.Children = GetTag(all, element.ItemPath, element.ItemName);
                    else
                        opcDaTagModel.Children = new();
                }
                opcDaTagModels.Add(opcDaTagModel);
            }

        }
        return opcDaTagModels;
    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <returns></returns>
    public ServerStatus GetServerStatus()
    {
        return Server?.GetStatus();
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="config"></param>
    public void Init(OpcDaNetApiMasterProperty config)
    {
        if (config != null)
            OpcDaNetApiMasterProperty = config;
        checkTimer?.Stop();
        checkTimer?.Dispose();
        checkTimer = new Timer(Math.Max(OpcDaNetApiMasterProperty.CheckRate, 1) * 60 * 1000);
        checkTimer.Elapsed += CheckTimer_Elapsed;
        checkTimer.Start();
        try
        {
            Server?.Dispose();
        }
        catch (Exception ex)
        {
            LogEvent?.Invoke(3, this, $"Disconnect warn：{ex.Message}", ex);
        }
        Server = findServer();
        if (Server == null)
        {
            LogEvent?.Invoke(3, this, $"Server not found", null);
        }
    }
    private Opc.Da.Server findServer()
    {
        Opc.IDiscovery m_discovery = new OpcCom.ServerEnumerator1();
        Opc.Server[] servers = m_discovery.GetAvailableServers(Opc.Specification.COM_DA_20, OpcDaNetApiMasterProperty.OpcIP, null);

        Opc.Da.Server server = (Opc.Da.Server)servers.FirstOrDefault(a => string.Equals(a.Name, OpcDaNetApiMasterProperty.OpcName, StringComparison.OrdinalIgnoreCase));

        if (server == null)
        {
            m_discovery = new OpcCom.ServerEnumerator2();
            servers = m_discovery.GetAvailableServers(Opc.Specification.COM_DA_20, OpcDaNetApiMasterProperty.OpcIP, null);

            server = (Opc.Da.Server)servers.FirstOrDefault(a => string.Equals(a.Name, OpcDaNetApiMasterProperty.OpcName, StringComparison.OrdinalIgnoreCase));

        }
        return server;
    }
    /// <summary>
    /// 按OPC组读取组内变量，结果会在订阅事件中返回
    /// </summary>
    /// <returns></returns>
    public ItemValueResult[] ReadItems(IEnumerable<string> items)
    {
        if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaNetApiMaster));
        return Server?.Read(items.Select(a => new Opc.Da.Item() { ItemName = a }).ToArray());
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return OpcDaNetApiMasterProperty?.ToString();
    }

    /// <summary>
    /// 批量写入值，返回（名称，是否成功，错误描述）
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, IdentifiedResult> WriteItem(Dictionary<string, object> writeInfos)
    {
        if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaNetApiMaster));
        Dictionary<string, Tuple<bool, string>> results = new();


        var writeResult = Server?.Write(
            writeInfos.Select(a => new ItemValue(a.Key) { Value = a.Value }).ToArray());

        return writeResult.ToDictionary(a => a.ItemName);
    }

    private void CheckTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        lock (checkLock)
        {
            if (IsExit == 0)
            {
                try
                {
                    var status = GetServerStatus();
                    if ((status.ServerState != serverState.running))
                    {
                        throw new();
                    }
                }
                catch
                {
                    if (IsExit == 0 && publicConnect)
                    {
                        try
                        {
                            PrivateConnect();
                            LogEvent?.Invoke(1, this, $"Successfully reconnected", null);
                        }
                        catch (Exception ex)
                        {
                            LogEvent?.Invoke(3, this, $"Reconnect failed：{ex.Message}", ex);
                        }
                    }
                }
            }
            else
            {
                var timeer = sender as Timer;
                timeer.Enabled = false;
                timeer.Stop();
            }
        }
    }

    private void PrivateConnect()
    {
        lock (this)
        {
            if (Server?.IsConnected == true)
            {
                try
                {
                    var status = GetServerStatus();
                    if ((status.ServerState != serverState.running))
                    {
                        throw new();
                    }

                }
                catch
                {
                    try
                    {
                        var status1 = GetServerStatus();
                        if ((status1.ServerState != serverState.running))
                        {
                            throw new();
                        }
                    }
                    catch
                    {
                        PrivateDisconnect();
                        Init(OpcDaNetApiMasterProperty);
                        Server?.Connect();
                        LogEvent?.Invoke(1, this, $"{Server.Url.HostName} - {Server.Name} - Connection successful", null);
                        PrivateAddItems();
                    }
                }
            }
            else
            {
                PrivateDisconnect();
                Init(OpcDaNetApiMasterProperty);
                Server?.Connect();
                LogEvent?.Invoke(1, this, $"{Server.Url.HostName} - {Server.Name} - Connection successful", null);
                PrivateAddItems();
            }
        }
    }

    private Dictionary<string, List<Opc.Da.Item>>? SaveItems;
    public void Save(Dictionary<string, List<Opc.Da.Item>>? saveItems)
    {
        SaveItems = saveItems;
    }
    private void PrivateAddItems()
    {
        try
        {
            if (SaveItems?.Count > 0)
            {
                ClearSubscriptions();
                AddItems(SaveItems);
            }
        }
        catch (Exception ex)
        {
            LogEvent?.Invoke(3, this, $"Add variable failed：{ex.Message}", ex);
        }

    }
    private void PrivateDisconnect()
    {
        lock (this)
        {
            if (IsConnected)
                LogEvent?.Invoke(1, this, $"{Server.Url.HostName} - {Server.Name} - Disconnect", null);
            if (checkTimer != null)
            {
                checkTimer.Enabled = false;
                checkTimer.Stop();
            }

            try
            {
                Server?.Dispose();
                Server = null;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(3, this, $"Connection dispose failed：{ex.Message}", ex);
            }
        }
    }


}
