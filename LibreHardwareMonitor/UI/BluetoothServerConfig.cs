using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using LibreHardwareMonitor.Utilities;
using InTheHand.Net;
using System.Linq;


namespace LibreHardwareMonitor.UI
{
    public partial class BluetoothServerConfig : Form
    {
        public BluetoothAddress BluetoothAddress { get; private set; }
        private BluetoothSender bluetoothSender;
        private BluetoothClient client;

        public BluetoothServerConfig(BluetoothSender bluetoothSender)
        {
            InitializeComponent();
            this.bluetoothSender = bluetoothSender;
            client = bluetoothSender.Client;

            BluetoothAddress = bluetoothSender.cachedBtAddress;
            this.Load += BluetoothServerConfig_Load;
        }

        

        private void BluetoothServerConfig_Load(object sender, EventArgs e)
        {
            if (BluetoothRadio.PrimaryRadio == null)
            {
                MessageBox.Show("There are no bluetooth radios found on this device.");
                Close();
                return;
            }

            if (BluetoothRadio.PrimaryRadio.Mode == RadioMode.PowerOff)
            {
                MessageBox.Show("Bluetooth radio is powered off");
                Close();
                return;
            }

            if (BluetoothAddress != null)
            {
                // BT Exists. Load it
                var deviceInfo = client.DiscoverDevices(10, true, true, false)
                    .FirstOrDefault(d => d.DeviceAddress == BluetoothAddress);
                getInfo(deviceInfo);
            }
        }

        private void buttonSelectBtDevice_Click(object sender, EventArgs e)
        {
            selectBluetoothDeviceDialog.ShowDialog();
            if (selectBluetoothDeviceDialog.SelectedDevice == null)
                return;

            if (client == null || !client.Connected)
            {
                BluetoothAddress = selectBluetoothDeviceDialog.SelectedDevice.DeviceAddress;
                bluetoothSender.Connect(BluetoothAddress);
                getInfo();
            }
        }

        private void getInfo(BluetoothDeviceInfo bluetoothDeviceInfo = null)
        {
            BluetoothDeviceInfo deviceInfo = bluetoothDeviceInfo ?? selectBluetoothDeviceDialog.SelectedDevice;
            BeginInvoke((Action)(() =>
            {
                valueDeviceAddress.Text = deviceInfo?.DeviceAddress.ToString();
                valueDeviceName.Text = deviceInfo?.DeviceName;
                valueRssi.Text = deviceInfo?.Rssi.ToString();
                checkBoxAuthenticated.Checked = deviceInfo?.Authenticated == true;
                checkBoxConnected.Checked = client?.IsConnected() == true;
                textBoxInfo.Text = JsonConvert.SerializeObject(deviceInfo, Formatting.Indented);
            }));
        }
    }
}
