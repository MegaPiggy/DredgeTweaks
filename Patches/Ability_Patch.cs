using System;
using HarmonyLib;

namespace Tweaks.Patches;

internal static class Ability_Patch
{
    [HarmonyPatch(typeof(HasteAbilityInfoPanel), "Show")]
	public static class HasteAbilityInfoPanel_Show_PostfixPatch
    {
        public static bool Prefix(HasteAbilityInfoPanel __instance)
        {
            return Main.Config.showBoostGauge;
        }
    }

    [HarmonyPatch(typeof(BoostAbility))]
	public static class BoostAbility_Patch
    {
        private static float hasteHeatLossOriginal;

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(BoostAbility __instance)
        {
            boostAbility = __instance;
            hasteHeatLossOriginal = __instance.hasteHeatLoss;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Activate")]
        public static bool ActivatePrefix(BoostAbility __instance)
        {
            __instance.hasteHeatLoss = hasteHeatLossOriginal * Main.Config.boostCooldownMult;
            return Main.Config.onlyActivateBoostWhenMoving && GameManager.Instance.Player.Controller.IsMoving;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(BoostAbility __instance)
        {
            if (Main.Config.boostSpeedMult > 1f && !__instance.isOnCooldown && __instance.isActive)
            {
                __instance.playerControllerRef.AbilitySpeedModifier = Main.Config.boostSpeedMult;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerAbilityManager), "GetTimeSinceLastCast")]
	public static class PlayerAbilityManager_Patch
    {
        public static void Postfix(PlayerAbilityManager __instance, ref float __result)
        {
            if (Main.Config.noAbilityCooldown)
            {
                __result = float.MaxValue;
            }
        }
    }

    private static BoostAbility boostAbility;

    public static void BoostCooldownMult_SettingChanged()
    {
        if (boostAbility != null)
		{
            boostAbility.hasteHeatLoss = GameManager.Instance.GameConfigData.HasteHeatCooldown * Main.Config.boostCooldownMult;
        }
    }
}
