//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using Opc.Da;

using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.OpcDaNetApi;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.OpcDaNetApi;

/// <summary>
/// <inheritdoc/>
/// </summary>
[OnlyWindowsSupport]
public class OpcDaNetApiMaster : CollectBase
{
    private readonly OpcDaNetApiMasterProperty _driverProperties = new();

    private ThingsGateway.Foundation.OpcDaNetApi.OpcDaNetApiMaster _plc;

    private CancellationToken _token;

    private volatile bool success = true;

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverProperties;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.OpcDaNetApiMaster);

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override IProtocol Protocol => null;

    /// <inheritdoc/>
    protected override void Init(IChannel? channel = null)
    {
        //载入配置
        Foundation.OpcDaNetApi.OpcDaNetApiMasterProperty opcNode = new()
        {
            OpcIP = _driverProperties.OpcIP,
            OpcName = _driverProperties.OpcName,
            UpdateRate = _driverProperties.UpdateRate,
            DeadBand = _driverProperties.DeadBand,
            ActiveSubscribe = _driverProperties.ActiveSubscribe,
            CheckRate = _driverProperties.CheckRate
        };
        if (_plc == null)
        {
            _plc = new();
            _plc.DataChangedHandler += DataChangedHandler;
            _plc.LogEvent = (a, b, c, d) => LogMessage.Log((LogLevel)a, b, c, d);
        }
        _plc.Init(opcNode);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => _plc?.IsConnected == true;

    public override string ToString()
    {
        return $"{_driverProperties.OpcIP}-{_driverProperties.OpcName}";
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
            _plc.DataChangedHandler -= DataChangedHandler;
        _plc?.SafeDispose();
        base.Dispose(disposing);
    }



    protected override string GetAddressDescription()
    {
        return _plc?.GetAddressDescription();
    }

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        _plc.Connect();
        return base.ProtectedBeforStartAsync(cancellationToken);
    }
    protected override async ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverProperties.ActiveSubscribe)
        {
            //获取设备连接状态
            if (IsConnected())
            {
                //更新设备活动时间
                CurrentDevice.SetDeviceStatus(TimerX.Now, 0);
            }
            else
            {
                CurrentDevice.SetDeviceStatus(TimerX.Now, 999);
            }

            ScriptVariableRun(cancellationToken);

        }
        else
        {
            await base.ProtectedExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
    }
    private Dictionary<string, List<Opc.Da.Item>>? Items;
    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {

        var datas = deviceVariables.GroupBy(a => a.IntervalTime).ChunkBetter(_driverProperties.GroupSize);
        List<VariableSourceRead> reads = new();
        foreach (var datags in datas)
        {
            foreach (var datag in datags)
            {
                var read = new VariableSourceRead()
                {
                    TimeTick = new(datag.Key ?? CurrentDevice.IntervalTime),
                    RegisterAddress = new Guid().ToString(),
                };
                read.AddVariableRange(datag.Select(a => a));
                reads.Add(read);
            }
        }


        Items = reads.ToDictionary(a => a.RegisterAddress, a => a.VariableRunTimes.Where(a => !a.RegisterAddress.IsNullOrEmpty()).Select(a => new Opc.Da.Item() { ItemName = a.RegisterAddress }).ToList());

        _plc.Save(Items);

        return reads;
    }

    /// <inheritdoc/>
    protected override async ValueTask<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        try
        {
            await WriteLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            var result = _plc.ReadItems(deviceVariableSourceRead.VariableRunTimes.Select(a => a.RegisterAddress));
            DateTime time = DateTime.Now;
            foreach (var data in result)
            {
                if (!CurrentDevice.KeepRun)
                    return new();
                if (_token.IsCancellationRequested)
                    return new(new OperationCanceledException());
                var type = data.Value.GetType();
                if (data.Value is Array)
                {
                    type = type.GetElementType();
                }
                var itemReads = CurrentDevice.VariableRunTimes.Select(a => a.Value).Where(it => it.RegisterAddress == data.ItemName);
                foreach (var item in itemReads)
                {
                    if (!CurrentDevice.KeepRun)
                        return new();
                    if (_token.IsCancellationRequested)
                        return new(new OperationCanceledException());
                    var value = data.Value;
                    var quality = data.Quality;
                    if (_driverProperties.SourceTimestampEnable)
                    {
                        time = data.Timestamp.ToLocalTime();
                    }
                    if (quality.GetCode() == 192)
                    {
                        if (item.DataType == DataTypeEnum.Object)
                            if (type.Namespace.StartsWith("System"))
                            {
                                var enumValues = Enum.GetValues<DataTypeEnum>();
                                var stringList = enumValues.Select(e => e.ToString());
                                if (stringList.Contains(type.Name))
                                    try { item.DataType = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                            }

                        var jToken = JToken.FromObject(value);
                        object newValue;
                        if (jToken is JValue jValue)
                        {
                            newValue = jValue.Value;
                        }
                        else
                        {
                            newValue = jToken;
                        }
                        item.SetValue(newValue, time);
                    }
                    else
                    {
                        item.SetValue(null, time, false);
                        item.VariableSource.LastErrorMessage = $"Bad quality：{quality}";
                    }
                }
            }

            return OperResult.CreateSuccessResult(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>($"ReadSourceAsync Error：{Environment.NewLine}{ex}");
        }
        finally
        {
            WriteLock.Release();
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            await WriteLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            var result = _plc.WriteItem(writeInfoLists.ToDictionary(a => a.Key.RegisterAddress!, a => a.Value.GetObjectFromJToken()!));
            return result.ToDictionary(a =>
            {
                return writeInfoLists.Keys.FirstOrDefault(b => b.RegisterAddress == a.Key).Name;
            }, a =>
            {
                if (!a.Value.ResultID.Succeeded())
                    return new OperResult(a.Value.ResultID.ToString());
                else
                    return OperResult.Success;
            }
                 );
        }
        finally
        {
            WriteLock.Release();
        }
    }

    private void DataChangedHandler(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
    {
        DateTime time = DateTime.Now;
        try
        {
            if (!CurrentDevice.KeepRun)
                return;
            if (_token.IsCancellationRequested)
                return;
            LogMessage.Trace($"{ToString()} Change:{Environment.NewLine} {values?.ToJsonNetString()}");

            foreach (var data in values)
            {
                if (!CurrentDevice.KeepRun)
                    return;
                if (_token.IsCancellationRequested)
                    return;
                var type = data.Value.GetType();
                if (data.Value is Array)
                {
                    type = type.GetElementType();
                }
                var itemReads = CurrentDevice.VariableRunTimes.Select(a => a.Value).Where(it => it.RegisterAddress == data.ItemName);
                foreach (var item in itemReads)
                {
                    if (!CurrentDevice.KeepRun)
                        return;
                    if (_token.IsCancellationRequested)
                        return;
                    var value = data.Value;
                    var quality = data.Quality;
                    if (_driverProperties.SourceTimestampEnable)
                    {
                        time = data.Timestamp.ToLocalTime();
                    }
                    if (quality.GetCode() == 192)
                    {
                        if (item.DataType == DataTypeEnum.Object)
                            if (type.Namespace.StartsWith("System"))
                            {
                                var enumValues = Enum.GetValues<DataTypeEnum>();
                                var stringList = enumValues.Select(e => e.ToString());
                                if (stringList.Contains(type.Name))
                                    try { item.DataType = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                            }

                        var jToken = JToken.FromObject(value);
                        object newValue;
                        if (jToken is JValue jValue)
                        {
                            newValue = jValue.Value;
                        }
                        else
                        {
                            newValue = jToken;
                        }
                        item.SetValue(newValue, time);
                    }
                    else
                    {
                        item.SetValue(null, time, false);
                        item.VariableSource.LastErrorMessage = $"Bad quality：{quality}";
                    }
                }
            }

            success = true;
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }
    }
}
