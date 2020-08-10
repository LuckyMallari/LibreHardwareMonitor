using InTheHand.Net.Sockets;

namespace LibreHardwareMonitor.Utilities
{
    static class BluetoothClientExtension
    {
        public static bool IsConnected(this BluetoothClient bluetoothClient)
        {
            return bluetoothClient.Connected && bluetoothClient.Client?.IsConnected() == true;
        }
    }
}
