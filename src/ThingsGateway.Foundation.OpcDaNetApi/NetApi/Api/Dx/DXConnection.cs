

using System;
using System.Collections;


namespace Opc.Dx
{
  [Serializable]
  public class DXConnection : ItemIdentifier
  {
    private string m_name;
    private BrowsePathCollection m_browsePaths = new BrowsePathCollection();
    private string m_description;
    private string m_keyword;
    private bool m_defaultSourceItemConnected;
    private bool m_defaultSourceItemConnectedSpecified;
    private bool m_defaultTargetItemConnected;
    private bool m_defaultTargetItemConnectedSpecified;
    private bool m_defaultOverridden;
    private bool m_defaultOverriddenSpecified;
    private object m_defaultOverrideValue;
    private bool m_enableSubstituteValue;
    private bool m_enableSubstituteValueSpecified;
    private object m_substituteValue;
    private string m_targetItemName;
    private string m_targetItemPath;
    private string m_sourceServerName;
    private string m_sourceItemName;
    private string m_sourceItemPath;
    private int m_sourceItemQueueSize = 1;
    private bool m_sourceItemQueueSizeSpecified;
    private int m_updateRate;
    private bool m_updateRateSpecified;
    private float m_deadband;
    private bool m_deadbandSpecified;
    private string m_vendorData;

    public string Name
    {
      get => this.m_name;
      set => this.m_name = value;
    }

    public BrowsePathCollection BrowsePaths => this.m_browsePaths;

    public string Description
    {
      get => this.m_description;
      set => this.m_description = value;
    }

    public string Keyword
    {
      get => this.m_keyword;
      set => this.m_keyword = value;
    }

    public bool DefaultSourceItemConnected
    {
      get => this.m_defaultSourceItemConnected;
      set => this.m_defaultSourceItemConnected = value;
    }

    public bool DefaultSourceItemConnectedSpecified
    {
      get => this.m_defaultSourceItemConnectedSpecified;
      set => this.m_defaultSourceItemConnectedSpecified = value;
    }

    public bool DefaultTargetItemConnected
    {
      get => this.m_defaultTargetItemConnected;
      set => this.m_defaultTargetItemConnected = value;
    }

    public bool DefaultTargetItemConnectedSpecified
    {
      get => this.m_defaultTargetItemConnectedSpecified;
      set => this.m_defaultTargetItemConnectedSpecified = value;
    }

    public bool DefaultOverridden
    {
      get => this.m_defaultOverridden;
      set => this.m_defaultOverridden = value;
    }

    public bool DefaultOverriddenSpecified
    {
      get => this.m_defaultOverriddenSpecified;
      set => this.m_defaultOverriddenSpecified = value;
    }

    public object DefaultOverrideValue
    {
      get => this.m_defaultOverrideValue;
      set => this.m_defaultOverrideValue = value;
    }

    public bool EnableSubstituteValue
    {
      get => this.m_enableSubstituteValue;
      set => this.m_enableSubstituteValue = value;
    }

    public bool EnableSubstituteValueSpecified
    {
      get => this.m_enableSubstituteValueSpecified;
      set => this.m_enableSubstituteValueSpecified = value;
    }

    public object SubstituteValue
    {
      get => this.m_substituteValue;
      set => this.m_substituteValue = value;
    }

    public string TargetItemName
    {
      get => this.m_targetItemName;
      set => this.m_targetItemName = value;
    }

    public string TargetItemPath
    {
      get => this.m_targetItemPath;
      set => this.m_targetItemPath = value;
    }

    public string SourceServerName
    {
      get => this.m_sourceServerName;
      set => this.m_sourceServerName = value;
    }

    public string SourceItemName
    {
      get => this.m_sourceItemName;
      set => this.m_sourceItemName = value;
    }

    public string SourceItemPath
    {
      get => this.m_sourceItemPath;
      set => this.m_sourceItemPath = value;
    }

    public int SourceItemQueueSize
    {
      get => this.m_sourceItemQueueSize;
      set => this.m_sourceItemQueueSize = value;
    }

    public bool SourceItemQueueSizeSpecified
    {
      get => this.m_sourceItemQueueSizeSpecified;
      set => this.m_sourceItemQueueSizeSpecified = value;
    }

    public int UpdateRate
    {
      get => this.m_updateRate;
      set => this.m_updateRate = value;
    }

    public bool UpdateRateSpecified
    {
      get => this.m_updateRateSpecified;
      set => this.m_updateRateSpecified = value;
    }

    public float Deadband
    {
      get => this.m_deadband;
      set => this.m_deadband = value;
    }

    public bool DeadbandSpecified
    {
      get => this.m_deadbandSpecified;
      set => this.m_deadbandSpecified = value;
    }

    public string VendorData
    {
      get => this.m_vendorData;
      set => this.m_vendorData = value;
    }

    public DXConnection()
    {
    }

    public DXConnection(ItemIdentifier item)
      : base(item)
    {
    }

    public DXConnection(DXConnection connection)
      : base((ItemIdentifier) connection)
    {
      if (connection == null)
        return;
      this.BrowsePaths.AddRange((ICollection) connection.BrowsePaths);
      this.Name = connection.Name;
      this.Description = connection.Description;
      this.Keyword = connection.Keyword;
      this.DefaultSourceItemConnected = connection.DefaultSourceItemConnected;
      this.DefaultSourceItemConnectedSpecified = connection.DefaultSourceItemConnectedSpecified;
      this.DefaultTargetItemConnected = connection.DefaultTargetItemConnected;
      this.DefaultTargetItemConnectedSpecified = connection.DefaultTargetItemConnectedSpecified;
      this.DefaultOverridden = connection.DefaultOverridden;
      this.DefaultOverriddenSpecified = connection.DefaultOverriddenSpecified;
      this.DefaultOverrideValue = connection.DefaultOverrideValue;
      this.EnableSubstituteValue = connection.EnableSubstituteValue;
      this.EnableSubstituteValueSpecified = connection.EnableSubstituteValueSpecified;
      this.SubstituteValue = connection.SubstituteValue;
      this.TargetItemName = connection.TargetItemName;
      this.TargetItemPath = connection.TargetItemPath;
      this.SourceServerName = connection.SourceServerName;
      this.SourceItemName = connection.SourceItemName;
      this.SourceItemPath = connection.SourceItemPath;
      this.SourceItemQueueSize = connection.SourceItemQueueSize;
      this.SourceItemQueueSizeSpecified = connection.SourceItemQueueSizeSpecified;
      this.UpdateRate = connection.UpdateRate;
      this.UpdateRateSpecified = connection.UpdateRateSpecified;
      this.Deadband = connection.Deadband;
      this.DeadbandSpecified = connection.DeadbandSpecified;
      this.VendorData = connection.VendorData;
    }

    public override object Clone() => (object) new DXConnection(this);
  }
}
