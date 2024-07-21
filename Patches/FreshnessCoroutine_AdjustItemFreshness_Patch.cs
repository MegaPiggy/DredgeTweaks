using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(FreshnessCoroutine), "AdjustItemFreshness")]
internal static class FreshnessCoroutine_AdjustItemFreshness_Patch
{
    public static bool Prefix(FreshnessCoroutine __instance)
    {
        return Main.Config.fishDecays;
    }
}
