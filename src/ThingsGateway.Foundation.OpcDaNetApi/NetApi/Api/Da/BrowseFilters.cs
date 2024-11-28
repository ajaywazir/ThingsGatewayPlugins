

using System;


namespace Opc.Da
{
  [Serializable]
  public class BrowseFilters : ICloneable
  {
    private int m_maxElementsReturned;
    private browseFilter m_browseFilter;
    private string m_elementNameFilter;
    private string m_vendorFilter;
    private bool m_returnAllProperties;
    private PropertyID[] m_propertyIDs;
    private bool m_returnPropertyValues;

    public int MaxElementsReturned
    {
      get => this.m_maxElementsReturned;
      set => this.m_maxElementsReturned = value;
    }

    public browseFilter BrowseFilter
    {
      get => this.m_browseFilter;
      set => this.m_browseFilter = value;
    }

    public string ElementNameFilter
    {
      get => this.m_elementNameFilter;
      set => this.m_elementNameFilter = value;
    }

    public string VendorFilter
    {
      get => this.m_vendorFilter;
      set => this.m_vendorFilter = value;
    }

    public bool ReturnAllProperties
    {
      get => this.m_returnAllProperties;
      set => this.m_returnAllProperties = value;
    }

    public PropertyID[] PropertyIDs
    {
      get => this.m_propertyIDs;
      set => this.m_propertyIDs = value;
    }

    public bool ReturnPropertyValues
    {
      get => this.m_returnPropertyValues;
      set => this.m_returnPropertyValues = value;
    }

    public virtual object Clone()
    {
      BrowseFilters browseFilters = (BrowseFilters) this.MemberwiseClone();
      browseFilters.PropertyIDs = this.PropertyIDs != null ? (PropertyID[]) this.PropertyIDs.Clone() : (PropertyID[]) (object) null;
      return (object) browseFilters;
    }
  }
}
