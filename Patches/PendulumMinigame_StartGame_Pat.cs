using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(PendulumMinigame), "StartGame")]
internal static class PendulumMinigame_StartGame_Patch
{
    public static void Postfix(PendulumMinigame __instance)
    {
        if (!Main.Config.showMinigameAnimationFeedback)
        {
            __instance.feedbackAnimationController = null;
        }
    }
}
