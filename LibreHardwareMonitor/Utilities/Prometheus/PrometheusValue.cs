using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitor.Utilities.Prometheus
{
    public class PrometheusValue
    {
        public PrometheusValue(string identifier,
            string sensor,
            float value,
            SensorType sensorType,
            string hardware,
            HardwareType hardwareType,
            string hwInstance,
            int sensorIndex)
        {
            Identifier = identifier;
            Sensor = sensor;
            Value = value;
            SensorType = sensorType;
            Hardware = hardware;
            HardwareType = hardwareType;
            SensorIndex = sensorIndex;
            HardwareInstance = hwInstance;
        }

        /// <summary>
        /// A globally unique identifier for each metric. Eg: /amdcpu/0/power/5. This
        /// identifier can be read as "The 6th power sensor on the 1st amd cpu"
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Descriptive name for sensor. Eg: CPU Core #10
        /// </summary>
        public string Sensor { get; }

        /// <summary>
        /// The reported sensor reading
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// The type of sensor
        /// </summary>
        public SensorType SensorType { get; }

        /// <summary>
        /// Descriptive name for the hardware. Eg: AMD Ryzen 7 2700. Note
        /// that this name does not need to be unique among hardware types such
        /// as in multi-gpu or multi-hdd setups
        /// </summary>
        public string Hardware { get; }

        /// <summary>
        /// The type of hardware the sensor is monitoring
        /// </summary>
        public HardwareType HardwareType { get; }

        /// <summary>
        /// The index. The "5" in /amdcpu/0/power/5. There typically isn't
        /// ambiguity for sensors as they have differing names
        /// (else wouldn't they be measuring the same thing?)
        /// </summary>
        public int SensorIndex { get; }

        /// <summary>
        /// The disambiguation factor for same hardware (multi-gpu and multi-hdd).
        /// This is typically the index of the hardware found in the identifier
        /// (eg: the "0" in /amdcpu/0/power/5). It's not always the index, for
        /// NIC sensors, the NIC's GUID is used.
        /// </summary>
        public string HardwareInstance { get; }

        public static (string, double) Convert(PrometheusValue report)
        {
            // Convert reported value into a base value by converting MB and GB into bytes, etc.
            // Flow rate is still liters per hour, even though liters per second may seem more
            // "base-unity", as grafana contained the former but not the latter. Fan speed remains
            // revolutions per minute, as I'm unaware of any manufacturer reporting fan speed as
            // revolutions per second.
            double BaseValue()
            {
                double value = report.Value;
                switch (report.SensorType)
                {
                    case SensorType.Data: // GB = 2^30 Bytes
                        return value * (1L << 30);
                    case SensorType.SmallData: // MB = 2^20 Bytes
                        return value * (1L << 20);
                    case SensorType.Clock: // MHz
                        return value * 1000000;
                    default:
                        return value;
                }
            }

            string BaseUnit()
            {
                switch (report.SensorType)
                {
                    case SensorType.Voltage: // V
                        return "volts";
                    case SensorType.Frequency: // Hz
                    case SensorType.Clock: // MHz
                        return "hertz";
                    case SensorType.Temperature: // °C
                        return "celsius";
                    case SensorType.Power: // W
                        return "watts";
                    case SensorType.Data: // GB = 2^30 Bytes
                    case SensorType.SmallData: // MB = 2^20 Bytes
                        return "bytes";
                    case SensorType.Throughput: // B/s
                        return "bytes_per_second";
                    case SensorType.Load: // %
                        return "load_percent";
                    case SensorType.Control: // %
                        return "control_percent";
                    case SensorType.Level: // %
                        return "level_percent";
                    case SensorType.Fan: // RPM
                        return "revolutions_per_minute";
                    case SensorType.Flow: // L/h
                        return "liters_per_hour";
                    case SensorType.Factor: // 1
                    default:
                        return report.SensorType.ToString().ToLowerInvariant();
                }
            }

            return (BaseUnit(), BaseValue());
        }
    }
}
