using HarmonyLib;
using UnityEngine;

namespace Tweaks.Patches;

[HarmonyPatch(typeof(HarvestPOIDataModel))]
internal static class HarvestPOIDataModel_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch("GetDoesRestock")]
    public static void GetDoesRestockPostfix(HarvestPOIDataModel __instance, ref bool __result)
    {
        HarvestableType harvestType = __instance.GetHarvestType();
        if (Main.Config.dredgeSpots == Spots.NeverRestock && harvestType == HarvestableType.DREDGE)
        {
            __result = false;
        }
        else if (Main.Config.fishingSpots == Spots.NeverRestock && (harvestType == HarvestableType.ABYSSAL || harvestType == HarvestableType.ABYSSAL || harvestType == HarvestableType.COASTAL || harvestType == HarvestableType.HADAL || harvestType == HarvestableType.ICE || harvestType == HarvestableType.MANGROVE || harvestType == HarvestableType.OCEANIC || harvestType == HarvestableType.SHALLOW || harvestType == HarvestableType.VOLCANIC))
        {
            __result = false;
        }
    }

    public static void GetNighttimeSpecialChancePostfix(HarvestPOIDataModel __instance, ref float __result)
    {
        if (Mathf.Approximately(__result, 0.1f))
        {
            __result = Main.Config.nighttimeAberrationChance;
        }
    }

    public static void GetDaytimeSpecialChancePostfix(HarvestPOIDataModel __instance, ref float __result)
    {
        if (Mathf.Approximately(__result, 0.025f))
        {
            __result = Main.Config.daytimeAberrationChance;
        }
    }
}
