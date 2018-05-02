using AceOfSpades.Characters;
using Dash.Engine;

namespace AceOfSpades
{
    /// <summary>
    /// Represents a kaboom!
    /// </summary>
    public struct Explosion
    {
        public Vector3 Origin;
        public float BlockRadius;
        public float PlayerRadius;

        public float Damage;
        public float DamageFalloff;

        public Player Owner;

        public Explosion(Player owner, Vector3 origin, float blockRadius, float playerRadius)
        {
            Owner = owner;
            Origin = origin;
            BlockRadius = blockRadius;
            PlayerRadius = playerRadius;
            Damage = 0;
            DamageFalloff = 1f;
        }

        public Explosion(Player owner, Vector3 origin, float blockRadius, float playerRadius, 
            float damage, float damageFalloff)
        {
            Owner = owner;
            Origin = origin;
            BlockRadius = blockRadius;
            PlayerRadius = playerRadius;
            Damage = damage;
            DamageFalloff = damageFalloff;
        }
    }
}
