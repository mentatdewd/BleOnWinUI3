using Microsoft.UI.Xaml;
using Windows.Devices.Bluetooth;

namespace WinUI3Test.Bluetooth
{
    public class BluetoothManager
    {
        public BluetoothManager(Window mainWindow)
        {
            _bleAdvertisements = new(mainWindow);
        }

        public void Initialize()
        {
            BluetoothAdapter adapter = BluetoothAdapter.GetDefaultAsync().GetResults();

            _bleAdvertisements.Initialize(adapter);

            _bleAdvertisements.StartAdvertising(false, false, -10);
        }

        private readonly BleAdvertisements _bleAdvertisements;
    }
}
