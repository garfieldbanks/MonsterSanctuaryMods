using System;

namespace garfieldbanks.MonsterSanctuary.ModsMenu.Extensions
{
    public static class NumberExtensions
    {
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
            {
                return min;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (value.CompareTo(max) > 0)
            {
                return max;
            }

            return value;
        }
    }
}
