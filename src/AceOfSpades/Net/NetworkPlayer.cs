/* NetworkPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Represents game information about a NetConnection.
    /// </summary>
    public class NetworkPlayer
    {
        public ushort Id { get; }
        public string Name { get; set; }
        public Team Team { get; set; }
        public ushort? CharacterId { get; set; }
        public int Score { get; set; }
        public int Ping { get; set; }

        public NetworkPlayer(ushort id)
        {
            Id = id;
            Team = Team.None;
        }

        public NetworkPlayer(string playerName, ushort id)
        {
            Id = id;
            Team = Team.None;
            Name = playerName;
        }
    }
}
