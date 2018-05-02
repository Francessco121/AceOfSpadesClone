using Dash.Engine.Diagnostics;
using Dash.Net;
using System;

/* GlobalNetwork.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Contains global network data needed by the shared library.
    /// </summary>
    public static class GlobalNetwork
    {
        public static bool IsServer { get; set; }
        public static bool IsClient { get; set; }
        public static bool IsConnected { get; set; }

        static bool loggingSetup;

        public static void SetupLogging()
        {
            if (loggingSetup)
                throw new Exception("NetLogging is already setup!");

            loggingSetup = true;

            if (NetLogger.LogMethod != NetLoggerMethod.Event)
            {
                NetLogger.LogMethod = NetLoggerMethod.Event;
                NetLogger.MessageLogged += NetLogger_MessageLogged;
            }
        }

        public static void EnableFullLogging()
        {
            NetLogger.LogAcks = true;
            NetLogger.LogDebugs = true;
            NetLogger.LogPacketSends = true;
            NetLogger.LogPacketReceives = true;
            NetLogger.LogReliableResends = true;
            NetLogger.LogVerboses = true;
            NetLogger.LogAlreadyHandledAcks = true;
            NetLogger.LogAlreadyHandledPackets = true;
            NetLogger.LogErrors = true;
            NetLogger.LogWarnings = true;
            NetLogger.LogFlowControl = true;
            NetLogger.LogImportants = true;
            NetLogger.LogObjectStateChanges = true;
            NetLogger.LogPartials = true;
        }

        static void NetLogger_MessageLogged(NetLog log)
        {
            switch (log.Type)
            {
                case NetLogType.Error:
                    DashCMD.WriteError(log.Message); break;
                case NetLogType.Warning:
                    DashCMD.WriteWarning(log.Message); break;
                case NetLogType.Important:
                    DashCMD.WriteLine(log.Message, ConsoleColor.Green); break;
                case NetLogType.Verbose:
                    DashCMD.WriteLine(log.Message, ConsoleColor.DarkGray); break;
                default:
                    DashCMD.WriteStandard(log.Message); break;
            }
        }
    }
}
