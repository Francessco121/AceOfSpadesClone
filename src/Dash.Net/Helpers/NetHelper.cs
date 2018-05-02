﻿using System;
using System.Net;
using System.Net.Sockets;

namespace Dash.Net
{
    /// <summary>
    /// Helper methods for networking things.
    /// </summary>
    public static class NetHelper
    {
        /// <summary>
        /// Returns the internal address of the computer this application is on.
        /// </summary>
        /// <returns>This computers internal address.</returns>
        public static IPAddress GetInternalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress address in host.AddressList)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address;

            return null;
        }

        /// <summary>
        /// Checks if the two endpoints are equal in value.
        /// </summary>
        /// <param name="a">First endpoint.</param>
        /// <param name="b">Second endpoint.</param>
        /// <returns>Are these two equal in value or not.</returns>
        public static bool IPEquals(IPEndPoint a, IPEndPoint b)
        {
            return (a.Port == b.Port && a.Address.Equals(b.Address));
        }

        /// <summary>
        /// Takes into account 'localhost' and 'auto' for ip names.
        /// <para>localhost = 127.0.0.1</para>
        /// <para>auto = internal ip of current machine</para>
        /// </summary>
        public static bool TryParseIP(string ip, out IPAddress address)
        {
            if (ip.ToLower() == "auto")
            {
                address = GetInternalIP();
                return true;
            }
            else
            {
                if (ip.ToLower() == "localhost")
                    ip = "127.0.0.1";

                return IPAddress.TryParse(ip, out address);
            }
        }
    }
}
