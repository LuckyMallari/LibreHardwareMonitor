﻿using System.Net.Sockets;

namespace LibreHardwareMonitor.Utilities
{
    static class SocketExtension
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
}
