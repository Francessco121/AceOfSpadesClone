using Dash.Engine;
using Dash.Engine.Graphics;
using System;
using System.Runtime.InteropServices;

/* BlockType.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Block
    {
        public const float CUBE_SIZE = 6f;
        public const float HALF_CUBE_SIZE = CUBE_SIZE / 2f;
        public static readonly Vector3 CUBE_3D_SIZE = new Vector3(CUBE_SIZE);
        public static readonly Vector3 HALF_CUBE_3D_SIZE = new Vector3(HALF_CUBE_SIZE);

        public static readonly Block AIR = new Block(0);
        public static readonly Block STONE = new Block(1);
        public static readonly Block EDITOR_TRANSPARENT = new Block(2);
        public static readonly Block DIRT = new Block(3);
        public static readonly Block GRASS = new Block(4);
        public static readonly Block WATER = new Block(5);
        public static readonly Block CUSTOM = new Block(6);

        [FieldOffset(0)]
        public byte R;
        [FieldOffset(1)]
        public byte G;
        [FieldOffset(2)]
        public byte B;
        [FieldOffset(3)]
        public Nybble2 Data;

        public byte Material
        {
            get { return Data.GetLower(); }
        }

        public byte Health
        {
            get { return Data.GetUpper(); }
        }

        public Block(byte material)
        {
            Data = new Nybble2(material, 5);
            R = G = B = 0;
        }

        public Block(byte material, byte r, byte g, byte b)
        {
            Data = new Nybble2(material, 5);
            R = r;
            G = g;
            B = b;
        }

        public Block(Nybble2 data, byte r, byte g, byte b)
        {
            Data = data;
            R = r;
            G = g;
            B = b;
        }

        public Block(byte material, byte health, byte r, byte g, byte b)
        {
            Data = new Nybble2(material, health);
            R = r;
            G = g;
            B = b;
        }

        public bool HasCollision()
        {
            return Material != AIR.Material
                && Material != WATER.Material;
        }

        public bool IsOpaque()
        {
            return Material != AIR.Material
                && !IsTranslucent();
        }

        public bool IsTranslucent()
        {
            return Material == WATER.Material;
        }

        public bool IsOpaqueTo(Block other)
        {
            return IsOpaque() || (IsTranslucent() && other.IsTranslucent());
        }

        public Color GetColor()
        {
            return new Color(R, G, B);
        }

        public Color4 GetColor4()
        {
            float health = Math.Max(Health / 5f, 0.2f);
            return new Color4(R / 255f * health, G / 255f * health, B / 255f * health, Material == WATER.Material ? 0.5f : 1);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Block))
                return ((Block)obj).Material == Material;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Material * R - G + B;
        }

        public static bool operator ==(Block a, Block b)
        {
            return a.Material == b.Material;
        }

        public static bool operator !=(Block a, Block b)
        {
            return !(a == b);
        }
    }
}
