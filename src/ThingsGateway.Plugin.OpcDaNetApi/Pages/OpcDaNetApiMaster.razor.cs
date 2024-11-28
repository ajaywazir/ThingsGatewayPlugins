//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.OpcDaNetApi;
using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class OpcDaNetApiMaster : IDisposable
{
    public LoggerGroup? LogMessage;
    private readonly OpcDaNetApiMasterProperty OpcDaNetApiMasterProperty = new();
    private ThingsGateway.Foundation.OpcDaNetApi.OpcDaNetApiMaster _plc;
    private string LogPath;
    private string RegisterAddress;
    private string WriteValue;

    /// <inheritdoc/>
    ~OpcDaNetApiMaster()
    {
        this.SafeDispose();
    }

    private AdapterDebugComponent AdapterDebugComponent { get; set; }

    [Inject]
    private DialogService DialogService { get; set; }

    [Inject]
    private IStringLocalizer<OpcDaNetApiMasterProperty> OpcDaNetApiMasterPropertyLocalizer { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _plc?.SafeDispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _plc = new ThingsGateway.Foundation.OpcDaNetApi.OpcDaNetApiMaster();

        _plc.Init(OpcDaNetApiMasterProperty);
        LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.CreateTextLogger(_plc.GetHashCode().ToLong().GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);

        _plc.LogEvent = (a, b, c, d) => LogMessage.Log((LogLevel)a, b, c, d);
        _plc.DataChangedHandler += (a, b, c) => LogMessage.Trace(c.ToJsonString());
        base.OnInitialized();
    }

    private void Add()
    {
        var tags = new Dictionary<string, List<Opc.Da.Item>>();
        var tag = new Opc.Da.Item() { ItemName = RegisterAddress };
        tags.Add(Guid.NewGuid().ToString(), new List<Opc.Da.Item>() { tag });
        try
        {
            _plc. Save(tags);
            _plc.AddItems(tags);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    private void Connect()
    {
        try
        {
            _plc.Disconnect();
            LogPath = _plc?.GetHashCode().ToLong().GetDebugLogPath();
            GetOpc().Connect();
        }
        catch (Exception ex)
        {
            LogMessage?.Log(LogLevel.Error, null, ex.Message, ex);
        }
    }

    private void Disconnect()
    {
        try
        {
            _plc.Disconnect();
        }
        catch (Exception ex)
        {
            LogMessage?.Log(LogLevel.Error, null, ex.Message, ex);
        }
    }

    private ThingsGateway.Foundation.OpcDaNetApi.OpcDaNetApiMaster GetOpc()
    {
        //载入配置
        _plc.Init(OpcDaNetApiMasterProperty);
        return _plc;
    }

    private async Task ReadAsync()
    {
        try
        {
            var data = _plc.ReadItems(new List<string>() { RegisterAddress });
            if (data.Length > 0)
            {
                foreach (var item in data)
                {
                    if (item.ResultID.Succeeded())
                        LogMessage?.LogInformation(item.Value.ToJsonString());
                    else
                        LogMessage?.LogWarning(item.ToJsonString());
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        await Task.CompletedTask;
    }

    private void Remove()
    {
        _plc.ClearSubscriptions();
    }

    private async Task ShowImport()
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = OpcDaNetApiMasterPropertyLocalizer["ShowImport"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<OpcDaNetApiImportVariable>(new Dictionary<string, object?>
        {
            [nameof(OpcDaNetApiImportVariable.Plc)] = _plc,
        });
        await DialogService.Show(op);
    }

    private async Task WriteAsync()
    {
        try
        {
            JToken tagValue = WriteValue.GetJTokenFromString();
            var obj = tagValue.GetObjectFromJToken();

            var data = _plc.WriteItem(
                new()
                {
                {RegisterAddress,  obj}
                }
                );
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    if (item.Value.ResultID.Succeeded())
                        LogMessage?.LogInformation(item.ToJsonString());
                    else
                        LogMessage?.LogWarning(item.ToJsonString());
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        await Task.CompletedTask;
    }
}
