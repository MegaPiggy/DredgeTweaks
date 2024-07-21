using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Tweaks.Patches;

internal class TrawlNet_Patch
{
    [HarmonyPatch(typeof(TrawlNetAbility))]
    public class TrawlNetAbility_OnEnable_Patch
    {
        private static readonly string[] materials = new string[4] { "scrap", "metal", "cloth", "lumber" };

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(TrawlNetAbility __instance)
        {
            if (Main.Config.netCatchChance == 1f && Main.Config.netBreaks)
            {
                return true;
            }
            if (!__instance.isActive || __instance.trawlNetItemInstance == null || __instance.trawlNetItemInstance.durability <= 0 || !GameManager.Instance.Player.Controller.IsMoving)
            {
                return false;
            }
            __instance.change = GameManager.Instance.Time.GetTimeChangeThisFrame();
            if (Main.Config.netBreaks)
            {
                __instance.modifiedChange = __instance.change * (decimal)(1 - GameManager.Instance.PlayerStats.ResearchedEquipmentMaintenanceModifier);
                __instance.trawlNetItemInstance.ChangeDurability(0 - (float)__instance.modifiedChange);
            }
            if (__instance.trawlNetItemInstance.durability > 0)
            {
                __instance.timeUntilNextCatchRoll -= __instance.change;
                if (__instance.timeUntilNextCatchRoll > 0)
                {
                    return false;
                }
                if (Main.Config.netCatchChance >= UnityEngine.Random.value)
                {
                    __instance.AddTrawlItem();
                }
                __instance.RefreshTimeUntilNextCatchRoll();
            }
            else
            {
                __instance.Deactivate();
                GameEvents.Instance.TriggerItemInventoryChanged(__instance.trawlNetItemInstance.GetItemData<DeployableItemData>());
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RefreshTimeUntilNextCatchRoll")]
        public static void RefreshTimeUntilNextCatchRollPostfix(TrawlNetAbility __instance)
        {
            __instance.timeUntilNextCatchRoll /= (decimal)Main.Config.netCatchRateMult;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddTrawlItem", new Type[] { })]
        public static bool AddTrawlItemPrefix(TrawlNetAbility __instance)
        {
            if (Main.Config.netCatchSound && Main.Config.netCatchMaterialChance == 0)
            {
                return true;
            }
            float currentDepth = GameManager.Instance.WaveController.SampleWaterDepthAtPlayerPosition();
            List<string> harvestableItemIds = GameManager.Instance.Player.HarvestZoneDetector.GetHarvestableItemIds(__instance.CheckCanBeCaughtByThisNet, currentDepth, GameManager.Instance.Time.IsDaytime);
            bool flag = Main.Config.netCatchMaterialChance >= UnityEngine.Random.value;
            if (harvestableItemIds.Count == 0 && !flag)
            {
                return false;
            }
            string text = "";
            if (flag)
            {
                int num = UnityEngine.Random.Range(0, materials.Length);
                text = materials[num];
                var grid = GameManager.Instance.SaveData.TrawlNet.grid;
                foreach (GridCellData gridCellData in grid)
                {
                    gridCellData.acceptedItemSubtype |= ItemSubtype.MATERIAL;
                }
            }
            else
            {
                float[] array = new float[harvestableItemIds.Count];
                for (int k = 0; k < harvestableItemIds.Count; k++)
                {
                    array[k] = GameManager.Instance.ItemManager.GetItemDataById<HarvestableItemData>(harvestableItemIds[k]).harvestItemWeight;
                }
                int randomWeightedIndex = MathUtil.GetRandomWeightedIndex(array);
                text = harvestableItemIds[randomWeightedIndex];
            }
            HarvestableItemData itemDataById = GameManager.Instance.ItemManager.GetItemDataById<HarvestableItemData>(text);
            if (itemDataById == null)
            {
                return false;
            }
            Vector3Int foundPosition = default;
            if (!GameManager.Instance.SaveData.TrawlNet.FindPositionForObject(itemDataById, out foundPosition))
            {
                return false;
            }
            if (flag)
            {
                SpatialItemInstance spatialItemInstance = GameManager.Instance.ItemManager.CreateItem<SpatialItemInstance>(text);
                GameManager.Instance.SaveData.TrawlNet.AddObjectToGridData(spatialItemInstance, foundPosition, dispatchEvent: true);
                GameManager.Instance.ItemManager.SetItemSeen(spatialItemInstance);
            }
            else
            {
                FishSizeGenerationMode sizeGenerationMode = FishSizeGenerationMode.NO_BIG_TROPHY;
                float aberrationBonusMultiplier = 1;
                if (!itemDataById.canBeCaughtByPot && !itemDataById.canBeCaughtByRod)
                {
                    sizeGenerationMode = FishSizeGenerationMode.ANY;
                    aberrationBonusMultiplier = 2;
                }
                FishItemInstance fishItemInstance = GameManager.Instance.ItemManager.CreateFishItem(text, FishAberrationGenerationMode.RANDOM_CHANCE, isSpecialSpot: false, sizeGenerationMode, aberrationBonusMultiplier);
                GameManager.Instance.SaveData.TrawlNet.AddObjectToGridData(fishItemInstance, foundPosition, dispatchEvent: true);
                GameManager.Instance.ItemManager.SetItemSeen(fishItemInstance);
            }
            GameManager.Instance.SaveData.NetFishCaught++;
            GameEvents.Instance.TriggerFishCaught();
            if (Main.Config.netCatchSound)
            {
                GameManager.Instance.AudioPlayer.PlaySFX(__instance.catchSFX, AudioLayer.SFX_PLAYER, __instance.catchSFXVolume);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(NetTabUI), "OnEnable")]
    public class NetTabUI_OnEnable_Patch
    {
        public static void Postfix(NetTabUI __instance)
        {
            if (!Main.Config.showNetCatchCount)
            {
                __instance.itemCounterUI.gameObject.SetActive(value: false);
            }
        }
    }

    [HarmonyPatch(typeof(TrawlActiveTab))]
    private class TrawlActiveTab_xPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Toggle")]
        public static bool TogglePrefix(TrawlActiveTab __instance)
        {
            return Main.Config.showNetCatchCount;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        public static void OnEnablePrefix(TrawlActiveTab __instance)
        {
            if (!Main.Config.showNetCatchCount)
            {
                __instance.container.gameObject.SetActive(value: false);
            }
        }
    }
}
