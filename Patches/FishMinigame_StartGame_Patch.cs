using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(FishMinigame), "StartGame")]
internal static class FishMinigame_StartGame_Patch
{
    public static void Postfix(FishMinigame __instance)
    {
        if (!Main.Config.showMinigameAnimationFeedback)
        {
            __instance.feedbackAnimationController = null;
        }
    }
}
