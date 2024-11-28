

using System;
using System.Text;


namespace Opc
{
  [Serializable]
  public class ItemIdentifier : ICloneable
  {
    private string m_itemName;
    private string m_itemPath;
    private object m_clientHandle;
    private object m_serverHandle;

    public string ItemName
    {
      get => this.m_itemName;
      set => this.m_itemName = value;
    }

    public string ItemPath
    {
      get => this.m_itemPath;
      set => this.m_itemPath = value;
    }

    public object ClientHandle
    {
      get => this.m_clientHandle;
      set => this.m_clientHandle = value;
    }

    public object ServerHandle
    {
      get => this.m_serverHandle;
      set => this.m_serverHandle = value;
    }

    public string Key
    {
      get
      {
        return new StringBuilder(64).Append(this.ItemName == null ? "null" : this.ItemName).Append("\r\n").Append(this.ItemPath == null ? "null" : this.ItemPath).ToString();
      }
    }

    public ItemIdentifier()
    {
    }

    public ItemIdentifier(string itemName)
    {
      this.ItemPath = (string) null;
      this.ItemName = itemName;
    }

    public ItemIdentifier(string itemPath, string itemName)
    {
      this.ItemPath = itemPath;
      this.ItemName = itemName;
    }

    public ItemIdentifier(ItemIdentifier itemID)
    {
      if (itemID == null)
        return;
      this.ItemPath = itemID.ItemPath;
      this.ItemName = itemID.ItemName;
      this.ClientHandle = itemID.ClientHandle;
      this.ServerHandle = itemID.ServerHandle;
    }

    public virtual object Clone() => this.MemberwiseClone();
  }
}
