using UnityEngine;

namespace garfieldbanks.MonsterSanctuary.ShiftColorName
{
    public static class ColorExtensions
    {
        public static string ToHtmlRGBA(this Color input)
        {
            return ColorUtility.ToHtmlStringRGBA(input);
        }
    }
}
