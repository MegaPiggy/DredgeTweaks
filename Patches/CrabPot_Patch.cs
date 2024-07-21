using HarmonyLib;
using UnityEngine;

namespace Tweaks.Patches;

[HarmonyPatch]
internal static class CrabPot_Patch
{
    [HarmonyPatch(typeof(SerializedCrabPotPOIData))]
    public static class SerializedCrabPotPOIData_CalculateCatchRoll_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void InitPostfix(SerializedCrabPotPOIData __instance)
        {
            __instance.timeUntilNextCatchRoll *= Main.Config.crabPotCatchRateMult;
        }

        [HarmonyPrefix]
        [HarmonyPatch("CalculateCatchRoll")]
        public static bool CalculateCatchRollPrefix(SerializedCrabPotPOIData __instance, ref float gameTimeElapsed, ref bool __result)
        {
            if (Main.Config.crabPotCatchChance == 1 && Main.Config.crabPotCatchRateMult == 1)
            {
                return true;
            }
            bool flag = false;
            float num = Mathf.Min(gameTimeElapsed, __instance.deployableItemData.TimeBetweenCatchRolls * Main.Config.crabPotCatchRateMult);
            gameTimeElapsed -= num;
            __instance.timeUntilNextCatchRoll -= num;
            if (__instance.timeUntilNextCatchRoll <= 0 && __instance.durability > 0)
            {
                if (Random.value < Main.Config.crabPotCatchChance)
                {
                    MathUtil.GetRandomWeightedIndex(__instance.GetItemWeights());
                    HarvestableItemData harvestableItemData = __instance.GetRandomHarvestableItem();
                    if ((Object)(object)harvestableItemData == null)
                    {
                        return false;
                    }
                    if (harvestableItemData.canBeReplacedWithResearchItem && Random.value < GameManager.Instance.GameConfigData.ResearchItemCrabPotSpawnChance)
                    {
                        harvestableItemData = GameManager.Instance.ResearchHelper.ResearchItemData;
                    }
                    if (__instance.grid.FindPositionForObject(harvestableItemData, out var foundPosition))
                    {
                        SpatialItemInstance spatialItemInstance;
                        if (harvestableItemData.itemSubtype == ItemSubtype.FISH)
                        {
                            spatialItemInstance = GameManager.Instance.ItemManager.CreateFishItem(harvestableItemData.id, FishAberrationGenerationMode.RANDOM_CHANCE, isSpecialSpot: false, FishSizeGenerationMode.ANY, 1f + __instance.deployableItemData.aberrationBonus);
                        }
                        else
                        {
                            SpatialItemInstance spatialItemInstance2 = new SpatialItemInstance();
                            spatialItemInstance2.id = harvestableItemData.id;
                            spatialItemInstance = spatialItemInstance2;
                        }
                        __instance.grid.AddObjectToGridData(spatialItemInstance, foundPosition, dispatchEvent: false);
                        flag = true;
                    }
                }
                __instance.timeUntilNextCatchRoll = __instance.deployableItemData.TimeBetweenCatchRolls * Main.Config.crabPotCatchRateMult;
            }
            if (gameTimeElapsed > 0)
            {
                flag = __instance.CalculateCatchRoll(gameTimeElapsed) || flag;
            }
            __result = flag;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AdjustDurability")]
        public static bool AdjustDurabilityPrefix(SerializedCrabPotPOIData __instance, ref bool __result, float newGameTime)
        {
            if (Main.Config.crabPotDurabilityMultiplier == 1)
            {
                return true;
            }
            __instance.hadDurabilityRemaining = __instance.durability > 0;
            float num = newGameTime - __instance.lastUpdate;
            __instance.lastUpdate = newGameTime;
            __instance.durability -= num * (1 - GameManager.Instance.PlayerStats.ResearchedEquipmentMaintenanceModifier) / Main.Config.crabPotDurabilityMultiplier;
            __instance.durability = Mathf.Clamp(__instance.durability, 0, __instance.deployableItemData.MaxDurabilityDays);
            __result = __instance.durability <= 0 && __instance.hadDurabilityRemaining;
            return false;
        }
    }
}
