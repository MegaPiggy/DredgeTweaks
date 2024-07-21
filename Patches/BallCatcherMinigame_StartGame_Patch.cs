using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(BallCatcherMinigame), "StartGame")]
internal static class BallCatcherMinigame_StartGame_Patch
{
    public static void Postfix(BallCatcherMinigame __instance)
    {
        if (!Main.Config.showMinigameAnimationFeedback)
        {
            __instance.feedbackAnimationController = null;
        }
    }
}
