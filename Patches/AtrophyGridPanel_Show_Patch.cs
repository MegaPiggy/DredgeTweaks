using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(AtrophyGridPanel), "Show")]
internal static class AtrophyGridPanel_Show_Patch
{
    public static void Prefix(AtrophyGridPanel __instance)
    {
        GameManager.Instance.GameConfigData.atrophyGuaranteedAberrationCount = Main.Config.minAtrophyAberrations;
    }
}
