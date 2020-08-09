using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreHardwareMonitor.Utilities
{
    public class BluetoothSender
    {
        private readonly IComputer _computer;
        private string[] _identifiers;
        private ISensor[] _sensors;
        private DateTime _lastLoggedTime = DateTime.MinValue;

        public BluetoothClient Client { get; private set; }

        public BluetoothSender(IComputer computer)
        {
            _computer = computer;
            _computer.HardwareAdded += HardwareAdded;
            _computer.HardwareRemoved += HardwareRemoved;
            Client = new BluetoothClient();
            RefreshSensors();
        }

        private void HardwareRemoved(IHardware hardware)
        {
            hardware.SensorAdded -= SensorAdded;
            hardware.SensorRemoved -= SensorRemoved;

            foreach (ISensor sensor in hardware.Sensors)
                SensorRemoved(sensor);

            foreach (IHardware subHardware in hardware.SubHardware)
                HardwareRemoved(subHardware);
        }

        private void HardwareAdded(IHardware hardware)
        {
            foreach (ISensor sensor in hardware.Sensors)
                SensorAdded(sensor);

            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;

            foreach (IHardware subHardware in hardware.SubHardware)
                HardwareAdded(subHardware);
        }

        private void SensorAdded(ISensor sensor)
        {
            if (_sensors == null)
                return;

            for (int i = 0; i < _sensors.Length; i++)
            {
                if (sensor.Identifier.ToString() == _identifiers[i])
                    _sensors[i] = sensor;
            }
        }

        private void SensorRemoved(ISensor sensor)
        {
            if (_sensors == null)
                return;

            for (int i = 0; i < _sensors.Length; i++)
            {
                if (sensor == _sensors[i])
                    _sensors[i] = null;
            }
        }

        public TimeSpan LoggingInterval { get; set; }

        private void RefreshSensors()
        {
            IList<ISensor> list = new List<ISensor>();
            SensorVisitor visitor = new SensorVisitor(sensor =>
            {
                list.Add(sensor);
            });
            visitor.VisitComputer(_computer);
            _sensors = list.ToArray();
            _identifiers = _sensors.Select(s => s.Identifier.ToString()).ToArray();
        }

        public void Send()
        {
            if (_sensors == null || _sensors.Length == 0)
                RefreshSensors();


            DateTime now = DateTime.Now;

            if (_lastLoggedTime + LoggingInterval - new TimeSpan(5000000) > now)
                return;
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(now.ToString("G"));
                sb.Append(",");
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (_sensors[i] != null)
                    {
                        float? value = _sensors[i].Value;
                        if (value.HasValue)
                            sb.Append(value.Value.ToString("R"));
                    }
                    if (i < _sensors.Length - 1)
                        sb.Append(",");
                    else
                        sb.AppendLine();
                }
                sendText(sb.ToString());
            }
            catch (IOException) { }

            _lastLoggedTime = now;
        }

        public void sendText(string s)
        {
            if (Client?.Connected != true)
            {
                return;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(s);
            var bluetoothStream = Client?.GetStream();
            if (bluetoothStream != null)
            {
                bluetoothStream.Write(bytes, 0, bytes.Length);
            }
        }

        public void Connect(string btAddressString)
        {
            BluetoothAddress bluetoothAddress;
            BluetoothAddress.TryParse(btAddressString, out bluetoothAddress);
            Connect(bluetoothAddress);
        }

        public void Connect(BluetoothAddress btAddress)
        {
            if (btAddress == null)
                return;

            BluetoothSecurity.PairRequest(btAddress, "123456");
            Client.BeginConnect(btAddress, BluetoothService.SerialPort, new AsyncCallback(OnConnectCallback), Client);
        }

        private void OnConnectCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                Client.EndConnect(ar);
            }
        }
    }
}
