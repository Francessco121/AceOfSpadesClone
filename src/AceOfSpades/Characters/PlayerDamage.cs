using System;

namespace AceOfSpades.Characters
{
    public class PlayerDamage
    {
        public Player Attacker { get; }
        public Player AttackerAssistant { get; }
        public int? AttackerAssistedAt { get; }
        public Player Attacked { get; }
        public float Damage { get; }
        public string Cause { get; }
        public int DamagedAt { get; }

        public PlayerDamage(Player attacker, Player attacked, float damage, string cause)
        {
            Attacker = attacker;
            Attacked = attacked;
            Damage = damage;
            Cause = cause;

            DamagedAt = Environment.TickCount;
        }

        public PlayerDamage(Player attacked, float damage, string cause)
        {
            Attacked = attacked;
            Damage = damage;
            Cause = cause;

            DamagedAt = Environment.TickCount;
        }

        /// <summary>
        /// Constructs a player damage object based on an existing one,
        /// this is used for when a player causes the indirect damage of
        /// a player. (ex. player gets shot, then backs off of cliff 
        /// while retreating).
        /// </summary>
        public PlayerDamage(PlayerDamage original, float damage, string cause)
        {
            Attacker = original.Attacker;
            Attacked = original.Attacked;
            Damage = damage;
            Cause = cause;

            DamagedAt = Environment.TickCount;
        }

        /// <summary>
        /// Constructs a player damage object where the previous
        /// player damage can receive assistant credit.
        /// </summary>
        public PlayerDamage(PlayerDamage previous, Player attacker, Player attacked, 
            float damage, string cause)
        {
            Attacker = attacker;
            if (attacker != previous.Attacker)
            {
                AttackerAssistant = previous.Attacker;
                AttackerAssistedAt = previous.DamagedAt;
            }
            else if (previous.AttackerAssistant != null)
            {
                AttackerAssistant = previous.AttackerAssistant;
                AttackerAssistedAt = previous.AttackerAssistedAt;
            }
            Attacked = attacked;
            Damage = damage;
            Cause = cause;

            DamagedAt = Environment.TickCount;
        }
    }
}
