namespace WinUI3Test.Bluetooth
{
    public class RssiInfo
    {
        public double RssiSignalStrength { get; set; }
        public double AdvertisementFrequency { get; set; }
        public double Distance { get; set; }
        public double ElapsedTime { get; set; }
        public int AdvertisementCount { get; set; }
        public short? AdvertisementTxStrength { get; set; }
        public ulong BluetoothAddress { get; set; }
    }
}
