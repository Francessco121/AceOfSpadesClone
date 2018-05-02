/* NetPacketType.cs
 * Author: Ethan Lafrenais
 * Last Update: 11/23/15
*/

namespace Dash.Net
{
    enum NetPacketType : byte
    {
        Custom = 0,

        PingRequest = 1,
        PingResponse = 2,

        /// <summary>
        /// Holds a reliable message id, says that it was received 
        /// </summary>
        AckResponse = 3,

        /// <summary>
        /// Sent by the client, indicates a connection request that must be either approved or denied.
        /// </summary>
        ConnectionRequest = 4,
        /// <summary>
        /// Sent by the server, indicates that the connection request was approved.
        /// </summary>
        ConnectionApproved = 5,
        /// <summary>
        /// Sent by the server, indicates that the connection request was denied.
        /// </summary>
        ConnectionDenied = 6,
        /// <summary>
        /// Indicates that the connection is ready to receive messages.
        /// </summary>
        ConnectionReady = 7,
        /// <summary>
        /// When a NetMessenger's connection was dropped.
        /// </summary>
        Disconnected = 8,

        /// <summary>
        /// Sent by a MatchmakingerServer, indicates this NetServer lost connection.
        /// </summary>
        MatchmakingServerTimedOut = 10,
        /// <summary>
        /// Sent by a MatchmakingerServer, indicates that the registration was approved.
        /// </summary>
        MatchmakingServerRegisterApproved = 11,
        /// <summary>
        /// Sent by a MatchmakingerServer, indicates that the registration was denied.
        /// </summary>
        MatchmakingServerRegisterDenied = 12,

        RemoteFunction = 20,
        RemoteEvent = 21,
        RemoteFunctionResponse = 22,

        MTUTest = 23,
    }
}
