using HarmonyLib;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(DredgeMinigame), "StartGame")]
internal static class DredgeMinigame_StartGame_Patch
{
    public static void Postfix(DredgeMinigame __instance)
    {
        if (!Main.Config.showMinigameAnimationFeedback)
        {
            __instance.feedbackAnimationController = null;
        }
    }
}
