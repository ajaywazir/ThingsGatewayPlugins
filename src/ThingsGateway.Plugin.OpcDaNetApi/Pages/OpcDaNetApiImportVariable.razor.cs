//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

using Opc.Da;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Extension;
using ThingsGateway.Foundation.OpcDaNetApi;

#if Plugin

using ThingsGateway.Gateway.Application;

#endif

using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

/// <summary>
/// 导入变量
/// </summary>
public partial class OpcDaNetApiImportVariable
{
    private List<TreeViewItem<OpcDaTagModel>> Items = new();
    private IEnumerable<OpcDaTagModel> Nodes;
    private bool ShowSkeleton = true;

    /// <summary>
    /// Opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.OpcDaNetApi.OpcDaNetApiMaster Plc { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<Foundation.OpcDaNetApi.OpcDaNetApiMasterProperty>? OpcDaNetApiMasterPropertyLocalizer { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Run(async () =>
            {
                Items = BuildTreeItemList(PopulateBranch(), RenderTreeItem).ToList();
                ShowSkeleton = false;
                await InvokeAsync(StateHasChanged);
            });
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// 构建树节点
    /// </summary>
    private static IEnumerable<TreeViewItem<OpcDaTagModel>> BuildTreeItemList(IEnumerable<OpcDaTagModel> opcDaTagModels, Microsoft.AspNetCore.Components.RenderFragment<OpcDaTagModel> render = null, TreeViewItem<OpcDaTagModel>? parent = null)
    {
        if (opcDaTagModels == null) return Enumerable.Empty<TreeViewItem<OpcDaTagModel>>();
        var trees = new List<TreeViewItem<OpcDaTagModel>>();
        foreach (var node in opcDaTagModels)
        {
            if (node == null) continue;
            var item = new TreeViewItem<OpcDaTagModel>(node)
            {
                Text = node.Name,
                Parent = parent,
                IsExpand = false,
                Template = render,
                HasChildren = node.Children != null,
            };
            item.Items = BuildTreeItemList(node.Children, render, item).ToList();
            trees.Add(item);
        }
        return trees;
    }

    private static bool ModelEqualityComparer(OpcDaTagModel x, OpcDaTagModel y) => x.NodeId == y.NodeId;

    private Task<IEnumerable<TreeViewItem<OpcDaTagModel>>> OnExpandNodeAsync(TreeViewItem<OpcDaTagModel> treeViewItem)
    {
        var data = BuildTreeItemList(PopulateBranch(treeViewItem.Value.NodeId), RenderTreeItem);
        return Task.FromResult(data);
    }

    private Task OnTreeItemChecked(List<TreeViewItem<OpcDaTagModel>> items)
    {
        Nodes = items.Select(a => a.Value);
        return Task.CompletedTask;
    }

    private List<OpcDaTagModel> PopulateBranch(string sourceId = null, bool isAll = false)
    {
        try
        {
            return Plc.GetTag(isAll, null, sourceId);
        }
        catch (Exception ex)
        {
            return new()
            {
                new()
                {
                    Name = ex.Message,
                    Tag = new(),
                    Children = null
                }
            };
        }
    }

#if Plugin

    private void PopulateBranch(OpcDaTagModel model)
    {
        if (model.Children != null)
        {
            if (model.Children.Count == 0)
            {
                var sourceId = model.Tag.ItemName;
                model.Children = PopulateBranch(sourceId);
            }
            foreach (var item in model.Children)
            {
                PopulateBranch(item);
            }
        }
    }

    private List<OpcDaTagModel> GetAllTag(IEnumerable<OpcDaTagModel> opcDaTagModels)
    {
        List<OpcDaTagModel> result = new();
        foreach (var item in opcDaTagModels)
        {
            PopulateBranch(item);

            result.AddRange(item.GetAllTags().Where(a => a.Children == null).ToList());
        }

        return result;
    }

    private async Task OnClickClose()
    {
        if (OnCloseAsync != null)
            await OnCloseAsync();
    }

    private async Task OnClickExport()
    {
        try
        {
            if (Nodes == null) return;
            var data = GetImportVariableList(GetAllTag(Nodes).DistinctBy(a => a.Name));
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await ToastService.Warning(OpcDaNetApiMasterPropertyLocalizer["NoVariablesAvailable"], OpcDaNetApiMasterPropertyLocalizer["NoVariablesAvailable"]);
                return;
            }

            await DownChannelExportAsync(data.Item1);
            await DownDeviceExportAsync(data.Item2, data.Item1.Name);
            await DownDeviceVariableExportAsync(data.Item3, data.Item2.Name);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    private async Task OnClickSave()
    {
        try
        {
            if (Nodes == null) return;
            var data = GetImportVariableList(GetAllTag(Nodes).DistinctBy(a => a.Name));
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await ToastService.Warning(OpcDaNetApiMasterPropertyLocalizer["NoVariablesAvailable"], OpcDaNetApiMasterPropertyLocalizer["NoVariablesAvailable"]);
                return;
            }
            await App.RootServices.GetRequiredService<IChannelService>().SaveChannelAsync(data.Item1, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IDeviceService>().SaveDeviceAsync(data.Item2, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IVariableService>().AddBatchAsync(data.Item3);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    private (Channel, Device, List<Variable>) GetImportVariableList(IEnumerable<OpcDaTagModel> opcDaTagModels)
    {
        var channel = GetImportChannel();
        var device = GetImportDevice(channel.Id);

        var data = opcDaTagModels.Select(b =>
        {
            var a = b.Tag;
            if (!a.IsItem || string.IsNullOrEmpty(a.ItemName))
            {
                return null!;
            }

            ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
            try
            {
                var userAccessLevel = (accessRights)(a.Properties.FirstOrDefault(b => b.ID.Code == 5).Value);
                level = userAccessLevel == accessRights.readable ? userAccessLevel == accessRights.writable ? ProtectTypeEnum.WriteOnly : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.ReadWrite;
            }
            catch
            {
            }

            var id = CommonUtils.GetSingleId();
            return new Variable()
            {
                Name = a.Name + "-" + id,
                RegisterAddress = a.ItemName,
                DeviceId = device.Id,
                Enable = true,
                Id = id,
                ProtectType = level,
                IntervalTime = "1000",
                RpcWriteEnable = true,
            };
        }).Where(a => a != null).ToList();
        return (channel, device, data);
    }

    private Device GetImportDevice(long channelId)
    {
        var id = CommonUtils.GetSingleId();
        var data = new Device()
        {
            Name = Plc.OpcDaNetApiMasterProperty.OpcName + "-" + id,
            Id = id,
            ChannelId = channelId,
            Enable = true,
            DevicePropertys = new(),
            PluginName = "ThingsGateway.Plugin.OpcDaNetApi.OpcDaNetApiMaster",
        };
        data.DevicePropertys.Add(nameof(OpcDaNetApiMasterProperty.OpcName), Plc.OpcDaNetApiMasterProperty.OpcName);
        data.DevicePropertys.Add(nameof(OpcDaNetApiMasterProperty.OpcIP), Plc.OpcDaNetApiMasterProperty.OpcIP);
        data.DevicePropertys.Add(nameof(OpcDaNetApiMasterProperty.ActiveSubscribe), Plc.OpcDaNetApiMasterProperty.ActiveSubscribe.ToString());
        data.DevicePropertys.Add(nameof(OpcDaNetApiMasterProperty.CheckRate), Plc.OpcDaNetApiMasterProperty.CheckRate.ToString());
        data.DevicePropertys.Add(nameof(OpcDaNetApiMasterProperty.DeadBand), Plc.OpcDaNetApiMasterProperty.DeadBand.ToString());
        data.DevicePropertys.Add(nameof(OpcDaNetApiMasterProperty.UpdateRate), Plc.OpcDaNetApiMasterProperty.UpdateRate.ToString());
        return data;
    }

    private Channel GetImportChannel()
    {
        var id = CommonUtils.GetSingleId();
        var data = new Channel()
        {
            Name = Plc.OpcDaNetApiMasterProperty.OpcName + "-" + id,
            Id = id,
            Enable = true,
            ChannelType = ChannelTypeEnum.Other,
        };
        return data;
    }

    [Inject]
    private DownloadService DownloadService { get; set; }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownChannelExportAsync(Channel data)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IChannelService>().ExportMemoryStream(new List<Channel>() { data });
        await DownloadService.DownloadFromStreamAsync($"channel{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(Device data, string channelName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IDeviceService>().ExportMemoryStream(new List<Device>() { data }, PluginTypeEnum.Collect, channelName);
        await DownloadService.DownloadFromStreamAsync($"device{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceVariableExportAsync(List<Variable> data, string devName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IVariableService>().ExportMemoryStream(data, devName);
        await DownloadService.DownloadFromStreamAsync($"variable{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

#endif

}
