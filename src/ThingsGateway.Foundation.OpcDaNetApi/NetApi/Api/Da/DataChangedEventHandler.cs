


namespace Opc.Da
{
    public delegate void DataChangedEventHandler(
      object subscriptionHandle,
      object requestHandle,
      ItemValueResult[] values);
}
