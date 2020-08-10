using System;
using System.Text.RegularExpressions;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Utilities.Prometheus;
using Prometheus;

namespace OhmGraphite
{
    public class PrometheusCollection
    {
        private static readonly Regex Rx = new Regex("[^a-zA-Z0-9_:]", RegexOptions.Compiled);
        private readonly MetricFactory _metrics;


        public void UpdateMetrics()
        {
            //Logger.LogAction("prometheus update metrics", PollSensors);
        }

       

    }
}
