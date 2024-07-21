using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Tweaks.Patches;

[HarmonyPatch]
internal static class HarvestPatch
{
    [HarmonyPatch(typeof(HarvestMinigameView))]
	public static class HarvestMinigameView_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RefreshHarvestTarget")]
        public static void RefreshHarvestTargetPostfix(HarvestMinigameView __instance)
        {
            if (!Main.Config.showFishSpotInfo)
            {
                __instance.hintImage.color = Color.black;
                __instance.stockText.text = "";
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("SpawnItem")]
        public static bool SpawnItemPrefix(HarvestMinigameView __instance, bool isTrophy)
        {
            bool deductFromStock = true;
            SpatialItemInstance spatialItemInstance = null;
            if ((Object)(object)__instance.itemDataToHarvest != null)
            {
                if (__instance.itemDataToHarvest.itemSubtype == ItemSubtype.FISH)
                {
                    FishAberrationGenerationMode aberrationGenerationMode = FishAberrationGenerationMode.RANDOM_CHANCE;
                    if (__instance.currentPOI.IsCurrentlySpecial && __instance.currentPOI.Stock < 2f)
                    {
                        aberrationGenerationMode = FishAberrationGenerationMode.FORCE;
                    }
                    spatialItemInstance = GameManager.Instance.ItemManager.CreateFishItem(__instance.itemDataToHarvest.id, aberrationGenerationMode, __instance.currentPOI.IsCurrentlySpecial, !isTrophy ? FishSizeGenerationMode.NO_BIG_TROPHY : FishSizeGenerationMode.FORCE_BIG_TROPHY);
                    deductFromStock = !__instance.itemDataToHarvest.affectedByFishingSustain || Random.value > GameManager.Instance.PlayerStats.ResearchedFishingSustainModifier;
                    if (spatialItemInstance.GetItemData<FishItemData>().IsAberration)
                    {
                        __instance.currentPOI.SetIsCurrentlySpecial(val: false);
                    }
                    GameManager.Instance.SaveData.FishUntilNextTrophyNotch--;
                    GameManager.Instance.SaveData.RodFishCaught++;
                }
                else if (__instance.itemDataToHarvest.canBeReplacedWithResearchItem && Main.Config.chanceToCatchResearchPart >= Random.value)
                {
                    SpatialItemInstance spatialItemInstance2 = new SpatialItemInstance();
                    spatialItemInstance2.id = GameManager.Instance.ResearchHelper.ResearchItemData.id;
                    spatialItemInstance = spatialItemInstance2;
                    deductFromStock = false;
                }
                else
                {
                    SpatialItemInstance spatialItemInstance3 = new SpatialItemInstance();
                    spatialItemInstance3.id = __instance.itemDataToHarvest.id;
                    spatialItemInstance = spatialItemInstance3;
                }
            }
            if (__instance.itemDataToHarvest.itemSubtype == ItemSubtype.FISH && Main.Config.fishingSpots == Spots.NeverDeplete)
            {
                deductFromStock = false;
            }
            if (__instance.currentPOI.IsDredgePOI && Main.Config.dredgeSpots == Spots.NeverDeplete)
            {
                deductFromStock = false;
            }
            __instance.currentPOI.OnHarvested(deductFromStock);
            GameManager.Instance.GridManager.AddItemOfTypeToCursor(spatialItemInstance, GridObjectState.BEING_HARVESTED);
            GameManager.Instance.ItemManager.SetItemSeen(spatialItemInstance);
            GameEvents.Instance.TriggerFishCaught();
            __instance.itemDataToHarvest = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(HarvestableParticles))]
	public static class HarvestableParticles_OnEnable_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        public static void OnEnablePrefix(HarvestableParticles __instance)
        {
            if (!Main.Config.waterRipplesOnFishingSpot)
            {
                Transform transform = __instance.transform.Find("DisturbedWaterParticles");
                if (transform != null)
                {
                    transform.gameObject.SetActive(value: false);
                }
                PlacedHarvestPOI componentInParent = __instance.GetComponentInParent<PlacedHarvestPOI>();
                if ((Object)(object)componentInParent != null && componentInParent.IsCrabPotPOI)
                {
                    __instance.gameObject.SetActive(value: false);
                }
            }
            if (!Main.Config.showOrangeParticlesOnPOI)
            {
                Transform transform2 = __instance.transform.Find("Embers");
                if (transform2 != null)
                {
                    transform2.gameObject.SetActive(value: false);
                }
            }
            if (!Main.Config.showRelicParticles && __instance.name == "RelicParticles(Clone)")
            {
                Transform transform3 = __instance.transform.Find("Beam");
                if (transform3 != null)
                {
                    transform3.gameObject.SetActive(value: false);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetSpecialStatus")]
        public static bool SetSpecialStatusPrefix(HarvestableParticles __instance)
        {
            return Main.Config.specialFishingSpots || Main.Config.aberrationParticleFXonFishingSpot;
        }
    }

    [HarmonyPatch(typeof(HarvestPOI))]
	public static class HarvestPOI_Awake_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnDayNightChanged")]
        public static void OnDayNightChangedPostfix(HarvestPOI __instance)
        {
            if (__instance.harvestable != null)
            {
                if (Main.Config.randomizeFishStock && Util.IsfishingSpot(__instance))
                {
                    RandomizeStock(__instance);
                }
                else if (Main.Config.randomizeDredgeStock && Util.IsdredgingSpot(__instance))
                {
                    RandomizeStock(__instance);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        public static void OnEnablePostfix(HarvestPOI __instance)
        {
            if (!GameManager.Instance.IsPlaying || !Util.IsfishingSpot(__instance))
            {
                return;
            }
            if (Main.Config.fishingSpotDisableChance > 0f)
            {
                __instance.gameObject.EnsureComponent<HarvestPOIdisabler>();
                return;
            }
            HarvestPOIdisabler component = __instance.GetComponent<HarvestPOIdisabler>();
            if (component != null)
            {
                Object.Destroy(component);
            }
        }
    }

    [HarmonyPatch(typeof(ItemManager), "CreateFishItem")]
	public static class ItemManager_CreateFishItem_Patch
    {
        public static bool Prefix(ItemManager __instance, ref string itemId, FishAberrationGenerationMode aberrationGenerationMode, bool isSpecialSpot, FishSizeGenerationMode sizeGenerationMode, ref FishItemInstance __result, float aberrationBonusMultiplier = 1f)
        {
            if (Main.Config.aberrationCatchBonusCap == 0.35f && Main.Config.daytimeAberrationChance == 0.01f && Main.Config.nighttimeAberrationChance == 0.03f && Main.Config.sanityAberrationCatchBonus == 0f)
            {
                return true;
            }
            if (aberrationGenerationMode == FishAberrationGenerationMode.RANDOM_CHANCE || aberrationGenerationMode == FishAberrationGenerationMode.FORCE)
            {
                bool flag = false;
                FishItemData itemDataById = __instance.GetItemDataById<FishItemData>(itemId);
                if (itemDataById != null && GameManager.Instance.SaveData.CanCatchAberrations && itemDataById.Aberrations.Count > 0 && GameManager.Instance.SaveData.GetCaughtCountById(itemDataById.id) > 0)
                {
                    float num = 0f;
                    if (aberrationGenerationMode == FishAberrationGenerationMode.RANDOM_CHANCE)
                    {
                        float num2 = GameManager.Instance.Time.IsDaytime ? Main.Config.daytimeAberrationChance : Main.Config.nighttimeAberrationChance;
                        float totalAberrationCatchModifier = GameManager.Instance.PlayerStats.TotalAberrationCatchModifier;
                        float num3 = Mathf.Min(Main.Config.aberrationCatchBonusCap, num2 + (float)GameManager.Instance.SaveData.AberrationSpawnModifier + totalAberrationCatchModifier);
                        float num4 = 0f;
                        if (isSpecialSpot)
                        {
                            num4 = GameManager.Instance.SaveData.HasCaughtAberrationAtSpecialSpot ? GameManager.Instance.GameConfigData.SpecialSpotAberrationSpawnBonus : 1f;
                        }
                        float num5 = 1f - GameManager.Instance.Player.Sanity.CurrentSanity;
                        num5 *= Main.Config.sanityAberrationCatchBonus;
                        num = num3 * aberrationBonusMultiplier + num4;
                        num += num * num5;
                        Debug.Log($"[ItemManager] aberration spawn chance is {num} (time of day chance: {num2} + player chance: {GameManager.Instance.SaveData.AberrationSpawnModifier} + bonus chance {num4} + gear chance {totalAberrationCatchModifier}. Modifier was {aberrationBonusMultiplier}x)");
                    }
                    if (aberrationGenerationMode == FishAberrationGenerationMode.FORCE || Random.value < num)
                    {
                        int worldPhase = GameManager.Instance.SaveData.WorldPhase;
                        List<FishItemData> candidates = new List<FishItemData>();
                        itemDataById.Aberrations.ForEach((aberrationItemData) =>
                        {
                            if (worldPhase >= aberrationItemData.MinWorldPhaseRequired && GameManager.Instance.SaveData.GetCaughtCountById(aberrationItemData.id) == 0)
                            {
                                candidates.Add(aberrationItemData);
                            }
                        });
                        if (candidates.Count == 0)
                        {
                            itemDataById.Aberrations.ForEach((aberrationItemData) =>
                            {
                                if (worldPhase >= aberrationItemData.MinWorldPhaseRequired)
                                {
                                    candidates.Add(aberrationItemData);
                                }
                            });
                        }
                        if (candidates.Count > 0)
                        {
                            FishItemData fishItemData = candidates[Random.Range(0, candidates.Count)];
                            if (fishItemData != null)
                            {
                                itemId = fishItemData.id;
                                flag = true;
                            }
                        }
                    }
                }
                if (flag)
                {
                    GameManager.Instance.SaveData.AberrationSpawnModifier = 0m;
                    if (isSpecialSpot)
                    {
                        GameManager.Instance.SaveData.HasCaughtAberrationAtSpecialSpot = true;
                    }
                    GameManager.Instance.SaveData.NumAberrationsCaught++;
                }
                else
                {
                    GameManager.Instance.SaveData.AberrationSpawnModifier += GameManager.Instance.GameConfigData.SpawnChanceIncreasePerNonAberrationCaught;
                }
            }
            float size = 0.5f;
            if (__instance.DebugNextFishSize == -1)
            {
                switch (sizeGenerationMode)
                {
                    case FishSizeGenerationMode.ANY:
                        size = MathUtil.GetRandomGaussian();
                        break;
                    case FishSizeGenerationMode.NO_BIG_TROPHY:
                        size = MathUtil.GetRandomGaussian(0f, GameManager.Instance.GameConfigData.TrophyMaxSize - 0.01f);
                        break;
                    case FishSizeGenerationMode.FORCE_BIG_TROPHY:
                        size = Random.Range(GameManager.Instance.GameConfigData.TrophyMaxSize, 1);
                        break;
                }
            }
            else
            {
                size = __instance.DebugNextFishSize;
            }
            FishItemInstance fishItemInstance = new FishItemInstance();
            fishItemInstance.id = itemId;
            fishItemInstance.size = size;
            fishItemInstance.freshness = GameManager.Instance.GameConfigData.MaxFreshness;
            __result = fishItemInstance;
            return false;
        }
    }

    private static void RandomizeStock(HarvestPOI harvestPOI)
    {
        var maxStock = harvestPOI.harvestable.GetMaxStock();
        if (maxStock != 1)
        {
            string id = harvestPOI.harvestPOIData.id;
            var newStock = Random.Range(1, maxStock + 1);
            GameManager.Instance.SaveData.harvestSpotStocks[id] = newStock;
        }
    }
}
