


namespace Opc.Da
{
    public enum qualityBits
    {
        bad = 0,
        badConfigurationError = 4,
        badNotConnected = 8,
        badDeviceFailure = 12, // 0x0000000C
        badSensorFailure = 16, // 0x00000010
        badLastKnownValue = 20, // 0x00000014
        badCommFailure = 24, // 0x00000018
        badOutOfService = 28, // 0x0000001C
        badWaitingForInitialData = 32, // 0x00000020
        uncertain = 64, // 0x00000040
        uncertainLastUsableValue = 68, // 0x00000044
        uncertainSensorNotAccurate = 80, // 0x00000050
        uncertainEUExceeded = 84, // 0x00000054
        uncertainSubNormal = 88, // 0x00000058
        good = 192, // 0x000000C0
        goodLocalOverride = 216, // 0x000000D8
    }
}
