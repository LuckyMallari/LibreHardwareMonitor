﻿/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.Heatmaster {
  internal class Heatmaster : Hardware, IDisposable {

    private string portName;
    private SerialPort serialPort;

    private int hardwareRevision;
    private int firmwareRevision;
    private int firmwareCRC;

    private Sensor[] fans;
    private Sensor[] controls;
    private Sensor[] temperatures;
    private Sensor[] flows;
    private Sensor[] relays;
    
    private bool available = false;

    private StringBuilder buffer = new StringBuilder();

    private string ReadLine(int timeout) {
      int i = 0;
      StringBuilder builder = new StringBuilder();
      while (i <= timeout) {
        while (serialPort.BytesToRead > 0) {
          byte b = (byte)serialPort.ReadByte();
          switch (b) {
            case 0xAA: return ((char)b).ToString();
            case 0x0D: return builder.ToString();
            default: builder.Append((char)b); break;
          }
        }
        i++;
        Thread.Sleep(1);
      }
      throw new TimeoutException();
    }

    private string ReadField(int device, char field) {
      serialPort.WriteLine("[0:" + device + "]R" + field);
      for (int i = 0; i < 5; i++) {
        string s = ReadLine(200);
        Match match = Regex.Match(s, @"-\[0:" + 
          device.ToString(CultureInfo.InvariantCulture) + @"\]R" +
          Regex.Escape(field.ToString(CultureInfo.InvariantCulture)) + ":(.*)");
        if (match.Success) 
          return match.Groups[1].Value;
      }
      return null;
    }

    private string ReadString(int device, char field) {
      string s = ReadField(device, field);
      if (s != null && s[0] == '"' && s[s.Length - 1] == '"')
        return s.Substring(1, s.Length - 2);
      else
        return null;
    }

    private int ReadInteger(int device, char field) {
      string s = ReadField(device, field);      
      int i;
      if (int.TryParse(s, out i))
        return i;
      else
        return 0;
    }

    private bool WriteField(int device, char field, string value) {
      serialPort.WriteLine("[0:" + device + "]W" + field + ":" + value);
      for (int i = 0; i < 5; i++) {
        string s = ReadLine(200);
        Match match = Regex.Match(s, @"-\[0:" + 
          device.ToString(CultureInfo.InvariantCulture) + @"\]W" + 
          Regex.Escape(field.ToString(CultureInfo.InvariantCulture)) +
          ":" + value);
        if (match.Success)
          return true;
      }
      return false;
    }

    private bool WriteInteger(int device, char field, int value) {
      return WriteField(device, field, 
        value.ToString(CultureInfo.InvariantCulture));
    }

    private bool WriteString(int device, char field, string value) {
      return WriteField(device, field, '"' + value + '"');
    }

    public Heatmaster(string portName, ISettings settings) {

      this.portName = portName;
      try {
        serialPort = new SerialPort(portName, 38400, Parity.None, 8,
          StopBits.One);
        serialPort.Open();
        serialPort.NewLine = ((char)0x0D).ToString();
        
        hardwareRevision = ReadInteger(0, 'H');
        firmwareRevision = ReadInteger(0, 'V');
        firmwareCRC = ReadInteger(0, 'C');

        int fanCount = Math.Min(ReadInteger(32, '?'), 4);
        int temperatureCount = Math.Min(ReadInteger(48, '?'), 6);
        int flowCount = Math.Min(ReadInteger(64, '?'), 1);
        int relayCount =  Math.Min(ReadInteger(80, '?'), 1);

        fans = new Sensor[fanCount];
        controls = new Sensor[fanCount];
        for (int i = 0; i < fanCount; i++) {
          int device = 33 + i;
          string name = ReadString(device, 'C');
          fans[i] = new Sensor(name, device, SensorType.Fan, this, settings);          
          fans[i].Value = ReadInteger(device, 'R');
          ActivateSensor(fans[i]);
          controls[i] =
            new Sensor(name, device, SensorType.Control, this, settings);
          controls[i].Value = (100 / 255.0f) * ReadInteger(device, 'P');
          ActivateSensor(controls[i]);
        }
        
        for (int i = 0; i < fanCount; i++) {
          int device = 33 + i;
          string name = ReadString(device, 'C');
          
          fans[i].Value = ReadInteger(device, 'R');
          ActivateSensor(fans[i]);
        }

        temperatures = new Sensor[temperatureCount];
        for (int i = 0; i < temperatureCount; i++) {
          int device = 49 + i;
          string name = ReadString(device, 'C');
          temperatures[i] =
            new Sensor(name, device, SensorType.Temperature, this, settings);
          int value = ReadInteger(device, 'T');
          temperatures[i].Value = 0.1f * value;
          if (value != -32768)
            ActivateSensor(temperatures[i]);
        }

        flows = new Sensor[flowCount];
        for (int i = 0; i < flowCount; i++) {
          int device = 65 + i;
          string name = ReadString(device, 'C');
          flows[i] = new Sensor(name, device, SensorType.Flow, this, settings);
          flows[i].Value = 0.1f * ReadInteger(device, 'L');
          ActivateSensor(flows[i]);
        }

        relays = new Sensor[relayCount];
        for (int i = 0; i < relayCount; i++) {
          int device = 81 + i;
          string name = ReadString(device, 'C');
          relays[i] = 
            new Sensor(name, device, SensorType.Control, this, settings);
          relays[i].Value = 100 * ReadInteger(device, 'S');
          ActivateSensor(relays[i]);
        }

        // set the update rate to 2 Hz
        WriteInteger(0, 'L', 2);
        
        available = true;

      } catch (IOException) { } catch (TimeoutException) { }      
    }

    public override HardwareType HardwareType {
      get { return HardwareType.Heatmaster; }
    }

    public override Identifier Identifier {
      get {
        return new Identifier("heatmaster",
          serialPort.PortName.TrimStart(new char[]{'/'}).ToLowerInvariant());
      }
    }

    public override string Name {
      get { return "Heatmaster"; }
    }

    private void ProcessUpdateLine(string line) {
      Match match = Regex.Match(line, @">\[0:(\d+)\]([0-9:\|-]+)");
      if (match.Success) {
        int device;
        if (int.TryParse(match.Groups[1].Value, out device)) {
          foreach (string s in match.Groups[2].Value.Split('|')) {
            string[] strings = s.Split(':');
            int[] ints = new int[strings.Length];
            for (int i = 0; i < ints.Length; i++)
              ints[i] = int.Parse(strings[i], CultureInfo.InvariantCulture);
            switch (device) {
              case 32:
                if (ints.Length == 3 && ints[0] <= fans.Length) {
                  fans[ints[0] - 1].Value = ints[1];
                  controls[ints[0] - 1].Value = (100 / 255.0f) * ints[2];
                }
                break;
              case 48:
                if (ints.Length == 2 && ints[0] <= temperatures.Length)
                  temperatures[ints[0] - 1].Value = 0.1f * ints[1];
                break;
              case 64:
                if (ints.Length == 3 && ints[0] <= flows.Length)
                  flows[ints[0] - 1].Value = 0.1f * ints[1];
                break;
              case 80:
                if (ints.Length == 2 && ints[0] <= relays.Length)
                  relays[ints[0] - 1].Value = 100 * ints[1];
                break;
            }
          }
        }
      }
    }

    public override void Update() {
      if (!available)
        return;

      while (serialPort.BytesToRead > 0) {
        byte b = (byte)serialPort.ReadByte();
        if (b == 0x0D) {
          ProcessUpdateLine(buffer.ToString());
          buffer.Length = 0;
        } else {
          buffer.Append((char)b);
        }
      }
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("Heatmaster");
      r.AppendLine();
      r.Append("Port: ");
      r.AppendLine(portName);
      r.Append("Hardware Revision: ");
      r.AppendLine(hardwareRevision.ToString(CultureInfo.InvariantCulture));
      r.Append("Firmware Revision: ");
      r.AppendLine(firmwareRevision.ToString(CultureInfo.InvariantCulture));
      r.Append("Firmware CRC: ");
      r.AppendLine(firmwareCRC.ToString(CultureInfo.InvariantCulture));
      r.AppendLine();

      return r.ToString();
    }

    public void Close() {
      serialPort.Close();
      serialPort.Dispose();
      serialPort = null;
    }

    public void Dispose() {
      if (serialPort != null) {
        serialPort.Dispose();
        serialPort = null;
      }
    }
  }
}