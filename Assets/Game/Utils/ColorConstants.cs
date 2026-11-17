// This file is auto-generated via 'Tools > Stat Colors Editor'

using UnityEngine;
using ZLinq;

namespace Game.Utils
{
    public static class ColorConstants
    {
        public static readonly Color Damage             = new Color(0.91f, 0.39f, 0.13f, 1.00f); // #E86321FF
        public static readonly Color FinalDamagePct     = new Color(1.00f, 1.00f, 1.00f, 1.00f); // #FFFFFFFF
        public static readonly Color AbilityPower       = new Color(0.00f, 0.90f, 1.00f, 1.00f); // #00E6FFFF
        public static readonly Color CritChance         = new Color(1.00f, 0.20f, 0.20f, 1.00f); // #FF3333FF
        public static readonly Color CritDamage         = new Color(0.74f, 0.12f, 0.24f, 1.00f); // #BD1F3DFF
        public static readonly Color AttackSpeed        = new Color(1.00f, 0.82f, 0.25f, 1.00f); // #FFD140FF
        public static readonly Color ProjectileSpeed    = new Color(0.35f, 0.94f, 0.71f, 1.00f); // #59F0B5FF
        public static readonly Color Prismatic          = new Color(0.69f, 0.26f, 1.00f, 1.00f); // #B042FFFF

        public static Color[] GetAll()
        {
            return typeof(ColorConstants).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .AsValueEnumerable()
                .Where(f => f.FieldType == typeof(Color))
                .Select(f => (Color)f.GetValue(null))
                .ToArray();
        }
    }
}
