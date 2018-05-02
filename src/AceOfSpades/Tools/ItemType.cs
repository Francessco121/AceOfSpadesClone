using System;

/* ItemType.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    [Flags]
    public enum ItemType
    {
        Weapon = 1,
        Gun = 2,
        BlockItem = 4,
        Spade = 8,
        Grenade = 16
    }
}
