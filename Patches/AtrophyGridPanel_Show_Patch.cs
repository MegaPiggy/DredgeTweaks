using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(QuickHarvestGridPanel), "Show")]
internal static class QuickHarvestGridPanel_Show_Patch
{
    public static void Prefix(QuickHarvestGridPanel __instance)
    {
        GameManager.Instance.GameConfigData.atrophyGuaranteedAberrationCount = Main.Config.minAtrophyAberrations;
    }
}
