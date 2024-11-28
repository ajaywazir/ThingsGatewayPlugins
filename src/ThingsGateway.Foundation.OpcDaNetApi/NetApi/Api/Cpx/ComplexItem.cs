

using Opc.Da;
using System;
using System.Collections;
using System.IO;
using System.Xml;


namespace Opc.Cpx
{
  public class ComplexItem : ItemIdentifier
  {
    public const string CPX_BRANCH = "CPX";
    public const string CPX_DATA_FILTERS = "DataFilters";
    public static readonly PropertyID[] CPX_PROPERTIES = new PropertyID[6]
    {
      Property.TYPE_SYSTEM_ID,
      Property.DICTIONARY_ID,
      Property.TYPE_ID,
      Property.UNCONVERTED_ITEM_ID,
      Property.UNFILTERED_ITEM_ID,
      Property.DATA_FILTER_VALUE
    };
    private string m_name;
    private string m_typeSystemID;
    private string m_dictionaryID;
    private string m_typeID;
    private ItemIdentifier m_dictionaryItemID;
    private ItemIdentifier m_typeItemID;
    private ItemIdentifier m_unconvertedItemID;
    private ItemIdentifier m_unfilteredItemID;
    private ItemIdentifier m_filterItem;
    private string m_filterValue;

    public string Name => this.m_name;

    public string TypeSystemID => this.m_typeSystemID;

    public string DictionaryID => this.m_dictionaryID;

    public string TypeID => this.m_typeID;

    public ItemIdentifier DictionaryItemID => this.m_dictionaryItemID;

    public ItemIdentifier TypeItemID => this.m_typeItemID;

    public ItemIdentifier UnconvertedItemID => this.m_unconvertedItemID;

    public ItemIdentifier UnfilteredItemID => this.m_unfilteredItemID;

    public ItemIdentifier DataFilterItem => this.m_filterItem;

    public string DataFilterValue
    {
      get => this.m_filterValue;
      set => this.m_filterValue = value;
    }

    public ComplexItem()
    {
    }

    public ComplexItem(ItemIdentifier itemID)
    {
      this.ItemPath = itemID.ItemPath;
      this.ItemName = itemID.ItemName;
    }

    public override string ToString()
    {
      return this.m_name != null || this.m_name.Length != 0 ? this.m_name : this.ItemName;
    }

    public ComplexItem GetRootItem()
    {
      if (this.m_unconvertedItemID != null)
        return ComplexTypeCache.GetComplexItem(this.m_unconvertedItemID);
      return this.m_unfilteredItemID != null ? ComplexTypeCache.GetComplexItem(this.m_unfilteredItemID) : this;
    }

    public void Update(Opc.Da.Server server)
    {
      this.Clear();
      ItemPropertyCollection[] properties = server.GetProperties(new ItemIdentifier[1]
      {
        (ItemIdentifier) this
      }, ComplexItem.CPX_PROPERTIES, true);
      if (properties == null || properties.Length != 1)
        throw new ApplicationException("Unexpected results returned from server.");
      if (!this.Init((ItemProperty[]) properties[0].ToArray(typeof (ItemProperty))))
        throw new ApplicationException("Not a valid complex item.");
      this.GetDataFilterItem(server);
    }

    public ComplexItem[] GetTypeConversions(Opc.Da.Server server)
    {
      if (this.m_unconvertedItemID != null || this.m_unfilteredItemID != null)
        return (ComplexItem[]) null;
      BrowsePosition position = (BrowsePosition) null;
      try
      {
        BrowseFilters filters = new BrowseFilters();
        filters.ElementNameFilter = "CPX";
        filters.BrowseFilter = browseFilter.branch;
        filters.ReturnAllProperties = false;
        filters.PropertyIDs = (PropertyID[]) null;
        filters.ReturnPropertyValues = false;
        BrowseElement[] browseElementArray1 = server.Browse((ItemIdentifier) this, filters, out position);
        if (browseElementArray1 == null || browseElementArray1.Length == 0)
          return (ComplexItem[]) null;
        if (position != null)
        {
          position.Dispose();
          position = (BrowsePosition) null;
        }
        ItemIdentifier itemID = new ItemIdentifier(browseElementArray1[0].ItemPath, browseElementArray1[0].ItemName);
        filters.ElementNameFilter = (string) null;
        filters.BrowseFilter = browseFilter.item;
        filters.ReturnAllProperties = false;
        filters.PropertyIDs = ComplexItem.CPX_PROPERTIES;
        filters.ReturnPropertyValues = true;
        BrowseElement[] browseElementArray2 = server.Browse(itemID, filters, out position);
        if (browseElementArray2 == null || browseElementArray2.Length == 0)
          return new ComplexItem[0];
        ArrayList arrayList = new ArrayList(browseElementArray2.Length);
        foreach (BrowseElement element in browseElementArray2)
        {
          if (element.Name != "DataFilters")
          {
            ComplexItem complexItem = new ComplexItem();
            if (complexItem.Init(element))
            {
              complexItem.GetDataFilterItem(server);
              arrayList.Add((object) complexItem);
            }
          }
        }
        return (ComplexItem[]) arrayList.ToArray(typeof (ComplexItem));
      }
      finally
      {
        position?.Dispose();
      }
    }

    public ComplexItem[] GetDataFilters(Opc.Da.Server server)
    {
      if (this.m_unfilteredItemID != null)
        return (ComplexItem[]) null;
      if (this.m_filterItem == null)
        return (ComplexItem[]) null;
      BrowsePosition position = (BrowsePosition) null;
      try
      {
        BrowseElement[] browseElementArray = server.Browse(this.m_filterItem, new BrowseFilters()
        {
          ElementNameFilter = (string) null,
          BrowseFilter = browseFilter.item,
          ReturnAllProperties = false,
          PropertyIDs = ComplexItem.CPX_PROPERTIES,
          ReturnPropertyValues = true
        }, out position);
        if (browseElementArray == null || browseElementArray.Length == 0)
          return new ComplexItem[0];
        ArrayList arrayList = new ArrayList(browseElementArray.Length);
        foreach (BrowseElement element in browseElementArray)
        {
          ComplexItem complexItem = new ComplexItem();
          if (complexItem.Init(element))
            arrayList.Add((object) complexItem);
        }
        return (ComplexItem[]) arrayList.ToArray(typeof (ComplexItem));
      }
      finally
      {
        position?.Dispose();
      }
    }

    public ComplexItem CreateDataFilter(Opc.Da.Server server, string filterName, string filterValue)
    {
      if (this.m_unfilteredItemID != null)
        return (ComplexItem) null;
      if (this.m_filterItem == null)
        return (ComplexItem) null;
      BrowsePosition position = (BrowsePosition) null;
      try
      {
        ItemValue itemValue = new ItemValue(this.m_filterItem);
        StringWriter w = new StringWriter();
        XmlTextWriter xmlTextWriter = new XmlTextWriter((TextWriter) w);
        xmlTextWriter.WriteStartElement("DataFilters");
        xmlTextWriter.WriteAttributeString("Name", filterName);
        xmlTextWriter.WriteString(filterValue);
        xmlTextWriter.WriteEndElement();
        xmlTextWriter.Close();
        itemValue.Value = (object) w.ToString();
        itemValue.Quality = Quality.Bad;
        itemValue.QualitySpecified = false;
        itemValue.Timestamp = DateTime.MinValue;
        itemValue.TimestampSpecified = false;
        IdentifiedResult[] identifiedResultArray = server.Write(new ItemValue[1]
        {
          itemValue
        });
        if (identifiedResultArray == null || identifiedResultArray.Length == 0)
          throw new ApplicationException("Unexpected result from server.");
        if (identifiedResultArray[0].ResultID.Failed())
          throw new ApplicationException("Could not create new data filter.");
        BrowseElement[] browseElementArray = server.Browse(this.m_filterItem, new BrowseFilters()
        {
          ElementNameFilter = filterName,
          BrowseFilter = browseFilter.item,
          ReturnAllProperties = false,
          PropertyIDs = ComplexItem.CPX_PROPERTIES,
          ReturnPropertyValues = true
        }, out position);
        if (browseElementArray == null || browseElementArray.Length == 0)
          throw new ApplicationException("Could not browse to new data filter.");
        ComplexItem complexItem = new ComplexItem();
        return complexItem.Init(browseElementArray[0]) ? complexItem : throw new ApplicationException("Could not initialize to new data filter.");
      }
      finally
      {
        position?.Dispose();
      }
    }

    public void UpdateDataFilter(Opc.Da.Server server, string filterValue)
    {
      if (this.m_unfilteredItemID == null)
        throw new ApplicationException("Cannot update the data filter for this item.");
      IdentifiedResult[] identifiedResultArray = server.Write(new ItemValue[1]
      {
        new ItemValue((ItemIdentifier) this)
        {
          Value = (object) filterValue,
          Quality = Quality.Bad,
          QualitySpecified = false,
          Timestamp = DateTime.MinValue,
          TimestampSpecified = false
        }
      });
      if (identifiedResultArray == null || identifiedResultArray.Length == 0)
        throw new ApplicationException("Unexpected result from server.");
      if (identifiedResultArray[0].ResultID.Failed())
        throw new ApplicationException("Could not update data filter.");
      this.m_filterValue = filterValue;
    }

    public string GetTypeDictionary(Opc.Da.Server server)
    {
      ItemPropertyCollection[] properties = server.GetProperties(new ItemIdentifier[1]
      {
        this.m_dictionaryItemID
      }, new PropertyID[1]{ Property.DICTIONARY }, true);
      if (properties == null || properties.Length == 0 || properties[0].Count == 0)
        return (string) null;
      ItemProperty itemProperty = properties[0][0];
      return !itemProperty.ResultID.Succeeded() ? (string) null : (string) itemProperty.Value;
    }

    public string GetTypeDescription(Opc.Da.Server server)
    {
      ItemPropertyCollection[] properties = server.GetProperties(new ItemIdentifier[1]
      {
        this.m_typeItemID
      }, new PropertyID[1]{ Property.TYPE_DESCRIPTION }, true);
      if (properties == null || properties.Length == 0 || properties[0].Count == 0)
        return (string) null;
      ItemProperty itemProperty = properties[0][0];
      return !itemProperty.ResultID.Succeeded() ? (string) null : (string) itemProperty.Value;
    }

    public void GetDataFilterItem(Opc.Da.Server server)
    {
      this.m_filterItem = (ItemIdentifier) null;
      if (this.m_unfilteredItemID != null)
        return;
      BrowsePosition position = (BrowsePosition) null;
      try
      {
        ItemIdentifier itemID = new ItemIdentifier((ItemIdentifier) this);
        BrowseFilters filters = new BrowseFilters();
        filters.ElementNameFilter = "DataFilters";
        filters.BrowseFilter = browseFilter.all;
        filters.ReturnAllProperties = false;
        filters.PropertyIDs = (PropertyID[]) null;
        filters.ReturnPropertyValues = false;
        if (this.m_unconvertedItemID == null)
        {
          filters.ElementNameFilter = "CPX";
          BrowseElement[] browseElementArray = server.Browse(itemID, filters, out position);
          if (browseElementArray == null || browseElementArray.Length == 0)
            return;
          if (position != null)
          {
            position.Dispose();
            position = (BrowsePosition) null;
          }
          itemID = new ItemIdentifier(browseElementArray[0].ItemPath, browseElementArray[0].ItemName);
          filters.ElementNameFilter = "DataFilters";
        }
        BrowseElement[] browseElementArray1 = server.Browse(itemID, filters, out position);
        if (browseElementArray1 == null || browseElementArray1.Length == 0)
          return;
        this.m_filterItem = new ItemIdentifier(browseElementArray1[0].ItemPath, browseElementArray1[0].ItemName);
      }
      finally
      {
        position?.Dispose();
      }
    }

    private void Clear()
    {
      this.m_typeSystemID = (string) null;
      this.m_dictionaryID = (string) null;
      this.m_typeID = (string) null;
      this.m_dictionaryItemID = (ItemIdentifier) null;
      this.m_typeItemID = (ItemIdentifier) null;
      this.m_unconvertedItemID = (ItemIdentifier) null;
      this.m_unfilteredItemID = (ItemIdentifier) null;
      this.m_filterItem = (ItemIdentifier) null;
      this.m_filterValue = (string) null;
    }

    private bool Init(BrowseElement element)
    {
      this.ItemPath = element.ItemPath;
      this.ItemName = element.ItemName;
      this.m_name = element.Name;
      return this.Init(element.Properties);
    }

    private bool Init(ItemProperty[] properties)
    {
      this.Clear();
      if (properties == null || properties.Length < 3)
        return false;
      foreach (ItemProperty property in properties)
      {
        if (property.ResultID.Succeeded())
        {
          if (property.ID == Property.TYPE_SYSTEM_ID)
            this.m_typeSystemID = (string) property.Value;
          else if (property.ID == Property.DICTIONARY_ID)
          {
            this.m_dictionaryID = (string) property.Value;
            this.m_dictionaryItemID = new ItemIdentifier(property.ItemPath, property.ItemName);
          }
          else if (property.ID == Property.TYPE_ID)
          {
            this.m_typeID = (string) property.Value;
            this.m_typeItemID = new ItemIdentifier(property.ItemPath, property.ItemName);
          }
          else if (property.ID == Property.UNCONVERTED_ITEM_ID)
            this.m_unconvertedItemID = new ItemIdentifier(this.ItemPath, (string) property.Value);
          else if (property.ID == Property.UNFILTERED_ITEM_ID)
            this.m_unfilteredItemID = new ItemIdentifier(this.ItemPath, (string) property.Value);
          else if (property.ID == Property.DATA_FILTER_VALUE)
            this.m_filterValue = (string) property.Value;
        }
      }
      return this.m_typeSystemID != null && this.m_dictionaryID != null && this.m_typeID != null;
    }
  }
}
