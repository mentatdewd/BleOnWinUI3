using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace WinUI3Test.Bluetooth
{
    public delegate void RssiUpdateHandler(object sender, RssiInfo rssiInfo);
    public delegate void DeviceDiscovered(object sender, Guid deviceGuid);

    public class BleAdvertisements
    {
        private int _advertisementCount;
        private DateTime _start;
        private readonly BluetoothLEAdvertisementPublisher _bluetoothLEAdvertisementPublisher;
        private readonly BluetoothLEAdvertisementWatcher _bluetoothLEAdvertisementWatcher;
        private readonly int _powerLevelAt1Meter = -69;
        private readonly Window _mainWindow;

        public event RssiUpdateHandler RssiUpdate;

        public BleAdvertisements(Window mainWindow)
        {
            _mainWindow = mainWindow;

            // Publisher
            _bluetoothLEAdvertisementPublisher = new();

            _bluetoothLEAdvertisementWatcher = new()
            {
                AllowExtendedAdvertisements = true,
                ScanningMode = BluetoothLEScanningMode.Active
            };
        }

        public void Initialize(BluetoothAdapter adapter)
        {
            BluetoothLEManufacturerData bluetoothLEManufacturerData = new()
            {
                CompanyId = 0xfffe
            };
            DataWriter dataWriter = new();

            dataWriter.WriteString("testdev");
            bluetoothLEManufacturerData.Data = dataWriter.DetachBuffer();

            _bluetoothLEAdvertisementPublisher.Advertisement.ManufacturerData.Add(bluetoothLEManufacturerData);
            
            // Publisher
            _bluetoothLEAdvertisementPublisher.StatusChanged += HandlePublisherStatusChanged;

            // Watcher
            _bluetoothLEAdvertisementWatcher.Received += HandleAdvertisementReceived;
            _bluetoothLEAdvertisementWatcher.Stopped += HandleAdvertismentWatcherStopped;
        }

        private void HandlePublisherStatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs status)
        {
            if (status.Error != BluetoothError.Success)
            {
                switch (status.Error)
                {
                    case BluetoothError.ConsentRequired:
                        Console.WriteLine("Error: ConsentRequired");
                        break;
                    case BluetoothError.DeviceNotConnected:
                        Console.WriteLine("Error: DeviceNotConnected");
                        break;
                    case BluetoothError.DisabledByPolicy:
                        Console.WriteLine("Error: DisabledByPolicy");
                        break;
                    case BluetoothError.DisabledByUser:
                        Console.WriteLine("Error: DisabledByUser");
                        break;
                    case BluetoothError.NotSupported:
                        Console.WriteLine("Error: NotSupported");
                        break;
                    case BluetoothError.OtherError:
                        Console.WriteLine("Error: OtherError");
                        break;
                    case BluetoothError.RadioNotAvailable:
                        Console.WriteLine("Error: RadioNotAvailable");
                        break;
                    case BluetoothError.ResourceInUse:
                        Console.WriteLine("Error: ResourceInUse");
                        break;
                    case BluetoothError.Success:
                        break;
                    case BluetoothError.TransportNotSupported:
                        break;
                    default:
                        Console.WriteLine("Error: Unknown");
                        break;
                }
                return;
            }

        }
        private void HandleAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs e)
        {
            foreach (BluetoothLEManufacturerData bluetoothLEManufacturerDataEntry in e.Advertisement.ManufacturerData)
            {
                if (bluetoothLEManufacturerDataEntry.CompanyId == 0xfffe)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan elapsed_seconds = now - _start;

                    double delta = _advertisementCount / elapsed_seconds.TotalSeconds;

                    DataReader dr = DataReader.FromBuffer(bluetoothLEManufacturerDataEntry.Data);

                    uint btAddress = dr.ReadUInt32();

                    short rssi = e.RawSignalStrengthInDBm;

                    RssiInfo rssiInfo = new()
                    {
                        RssiSignalStrength = rssi,
                        AdvertisementCount = ++_advertisementCount,
                        AdvertisementFrequency = delta,
                        Distance = GetDistance(rssi),
                        ElapsedTime = elapsed_seconds.TotalSeconds
                    };
                    short? pl = e.TransmitPowerLevelInDBm;
                    if (pl != null)
                    {
                        rssiInfo.AdvertisementTxStrength = pl.Value;
                    }
                    rssiInfo.BluetoothAddress = btAddress;

                    rssiInfo.AdvertisementTxStrength = e.TransmitPowerLevelInDBm;
                    RssiUpdate.Invoke(this, rssiInfo); // TODO: Add event

                }
            }
        }
        private void HandleAdvertismentWatcherStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs e)
        {
            if (e.Error != BluetoothError.Success)
            {
                switch (e.Error)
                {
                    case BluetoothError.OtherError:
                        Console.WriteLine("Other Error");
                        break;
                    case BluetoothError.ConsentRequired:
                        Console.WriteLine("Consent Required");
                        break;
                    case BluetoothError.NotSupported:
                        Console.WriteLine("Not Supported");
                        break;
                    case BluetoothError.DeviceNotConnected:
                        Console.WriteLine("Device not connected");
                        break;
                    case BluetoothError.ResourceInUse:
                        Console.WriteLine("Resource in use");
                        break;
                    case BluetoothError.TransportNotSupported:
                        Console.WriteLine("Transport not supported");
                        break;
                    case BluetoothError.DisabledByPolicy:
                        Console.WriteLine("Disabled by policy");
                        break;
                    case BluetoothError.DisabledByUser:
                        Console.WriteLine("Disabled by user");
                        break;
                    case BluetoothError.RadioNotAvailable:
                        Console.WriteLine("Radio not available");
                        break;
                    case BluetoothError.Success:
                        break;
                    default:
                        break;
                }
            }
        }

        public void StartAdvertising(bool useExtendedAdvertising,
        bool sendTxPowerLevel,
        short? preferredTxPowerLevel)
        {
            _bluetoothLEAdvertisementPublisher.UseExtendedAdvertisement = useExtendedAdvertising;
            _bluetoothLEAdvertisementPublisher.IncludeTransmitPowerLevel = sendTxPowerLevel;
            _bluetoothLEAdvertisementPublisher.PreferredTransmitPowerLevelInDBm = preferredTxPowerLevel;

            try
            {
                _bluetoothLEAdvertisementPublisher.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void StopAdvertising()
        {
            _bluetoothLEAdvertisementPublisher.Stop();
        }

        public void StartListening()
        {
            _start = DateTime.Now;

            _bluetoothLEAdvertisementWatcher.Start();
        }

        public void StopListening()
        {
            _bluetoothLEAdvertisementWatcher.Stop();
        }

        private double GetDistance(double rssi)
        {
            double distance = Math.Pow(10, _powerLevelAt1Meter - (rssi / 110));

            return distance;
        }
    }
}
