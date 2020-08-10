using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Utilities.Prometheus;
using Microsoft.Win32;
using Prometheus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LibreHardwareMonitor.Utilities
{
    public class BluetoothSender
    {
        private readonly IComputer _computer;
        private string[] _identifiers;
        private ISensor[] _sensors;
        private DateTime _lastLoggedTime = DateTime.MinValue;
        private static readonly Regex Rx = new Regex("[^a-zA-Z0-9_:]", RegexOptions.Compiled);
        private bool isConnecting;
        private DateTime lastConnect = DateTime.Now;
        public BluetoothAddress cachedBtAddress { get; private set; }
        public bool IsProcessShutdown { get; set; }

        static CollectorRegistry registry = Metrics.DefaultRegistry;
        MetricFactory metricFactory = Metrics.WithCustomRegistry(registry);

        public BluetoothClient Client { get; private set; }

        public BluetoothSender(IComputer computer)
        {
            _computer = computer;
            _computer.HardwareAdded += HardwareAdded;
            _computer.HardwareRemoved += HardwareRemoved;

            if (BluetoothRadio.PrimaryRadio != null && BluetoothRadio.PrimaryRadio.Mode != RadioMode.PowerOff)
            {
                Client = new BluetoothClient();
            }

            RefreshSensors();
            var registry = Metrics.DefaultRegistry;
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
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (_sensors == null)
                        return;

                    var sensor = _sensors[i];
                    var hwInstance = _sensors[i].Hardware.Identifier.ToString();
                    var ind = hwInstance.LastIndexOf('/');
                    hwInstance = hwInstance.Substring(ind + 1);
                    var reportedValue = new PrometheusValue(sensor.Identifier.ToString(),
                                        sensor.Name,
                                        sensor.Value.Value,
                                        sensor.SensorType,
                                        sensor.Hardware.Name,
                                        sensor.Hardware.HardwareType,
                                        hwInstance,
                                        sensor.Index);

                    var (unit, val) = PrometheusValue.Convert(reportedValue);
                    var hw = Enum.GetName(typeof(HardwareType), sensor.Hardware.HardwareType)?.ToLowerInvariant();
                    var name = Rx.Replace($"ohm_{hw}_{unit}", "_");
                    metricFactory
                        .CreateGauge(name, "Metric reported by open hardware sensor", "hardware", "sensor", "hw_instance")
                        .WithLabels(reportedValue.Hardware, reportedValue.Sensor, reportedValue.HardwareInstance)
                        .Set(val);
                }

                if (Client?.Connected == true)
                {
                    using (var stream = new MemoryStream())
                    {
                        registry.CollectAndExportAsTextAsync(stream);
                        string text = Encoding.UTF8.GetString(stream.ToArray());
                        sendText($"###<BEGIN>\r\n{text}###<END>");
                    }
                }
            }
            catch (IOException) { }

            _lastLoggedTime = now;
        }


        public void sendText(string s)
        {
            if (Client?.IsConnected() != true)
            {
                Connect(cachedBtAddress);
                return;
            }

            WriteToStream(Client?.GetStream(), s);
        }

        private void WriteToStream(Stream stream, string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            stream?.Write(bytes, 0, bytes.Length);
        }

        public void Connect(string btAddressString)
        {
            BluetoothAddress bluetoothAddress;
            BluetoothAddress.TryParse(btAddressString, out bluetoothAddress);
            Connect(bluetoothAddress);
        }

        public void Connect(BluetoothAddress btAddress)
        {
            cachedBtAddress = btAddress;

            var lastConnectDuration = DateTime.Now - lastConnect;
            
            if (cachedBtAddress == null || lastConnectDuration.TotalSeconds < 5 ||  isConnecting)
                return;

            BluetoothSecurity.PairRequest(cachedBtAddress, "123456");

            try
            {
                Client.BeginConnect(cachedBtAddress, BluetoothService.SerialPort, new AsyncCallback(OnConnectCallback), null);
            }
            catch {
                CleanupBt();
            }

            lastConnect = DateTime.Now;

        }

        private void OnConnectCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                if (Client?.Connected == true)
                {
                    Client.EndConnect(ar);
                }
                else
                {
                    CleanupBt();
                }
            }
        }

        private void CleanupBt()
        {
            Client?.Close();
            Client?.Dispose();
            Client = null;
            Client = new BluetoothClient();
        }

        public void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (!IsProcessShutdown)
                return;

            sendShutdown();
        }

        public void sendShutdown()
        {
            sendText("###<SHUTDOWN>");
            CleanupBt();
        }
    }
}
