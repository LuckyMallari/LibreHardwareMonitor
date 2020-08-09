using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using LibreHardwareMonitor.Utilities;
using InTheHand.Net;

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

        private void getInfo()
        {
            BeginInvoke((Action)(() =>
            {
                valueDeviceAddress.Text = selectBluetoothDeviceDialog.SelectedDevice?.DeviceAddress.ToString();
                valueDeviceName.Text = selectBluetoothDeviceDialog.SelectedDevice?.DeviceName;
                valueRssi.Text = selectBluetoothDeviceDialog.SelectedDevice?.Rssi.ToString();
                checkBoxAuthenticated.Checked = selectBluetoothDeviceDialog.SelectedDevice?.Authenticated == true;
                checkBoxConnected.Checked = client?.Connected == true;
                textBoxInfo.Text = JsonConvert.SerializeObject(selectBluetoothDeviceDialog.SelectedDevice, Formatting.Indented);
            }));
        }
    }
}
