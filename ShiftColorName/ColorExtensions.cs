using UnityEngine;

namespace eradev.monstersanctuary.ShiftColorName
{
    public static class ColorExtensions
    {
        public static string ToHtmlRGBA(this Color input)
        {
            return ColorUtility.ToHtmlStringRGBA(input);
        }
    }
}
