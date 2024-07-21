using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace Tweaks.Patches;

[HarmonyPatch]
internal static class Testing
{
	[HarmonyPatch(typeof(HarvestMinigameView), nameof(HarvestMinigameView.Show))]
	internal static class HarvestMinigameView_Show_Patch
    {
        public static void Prefix(HarvestMinigameView __instance, HarvestPOI harvestPOI)
        {
            string text = harvestPOI.HarvestPOIData.MainItemsHaveAberrations() ? " aber " : " ";
            string text2 = (harvestPOI.HarvestPOIData.usesTimeSpecificStock && harvestPOI.HarvestPOIData.NighttimeItemsHaveAberrations()) ? "night aber " : "";
            string text3 = "Stock " + (int)harvestPOI.Stock + "/" + harvestPOI.harvestable.GetMaxStock();
            float depthRaw = GameManager.Instance.WaveController.SampleWaterDepthAtPosition(GameManager.Instance.WaveController.GetSamplePositionByWorldPosition(((Component)(object)harvestPOI).transform.position));
            string formattedDepthString = GameManager.Instance.ItemManager.GetFormattedDepthString(depthRaw);
            Util.Message("Stock " + text3 + " Restocks " + harvestPOI.harvestable.GetDoesRestock());
        }
    }

	[HarmonyPatch(typeof(DeployableItemData), nameof(DeployableItemData.CatchRate), MethodType.Getter)]
	internal static class DeployableItemData_CatchRate_Patch
    {
        public static void Postfix(DeployableItemData __instance, ref float __result)
        {
            Util.Log("DeployableItemData CatchRate " + __result);
        }
    }

	[HarmonyPatch(typeof(GridManager), nameof(GridManager.AddActiveGrid))]
	internal static class GridManager_AddActiveGrid_PrefixPatch
    {
        public static void Prefix(GridManager __instance, GridUI gridUI)
        {
            Util.Log("AddActiveGrid " + gridUI.name);
        }
    }

	[HarmonyPatch(typeof(GridManager), nameof(GridManager.ObjectPickedUp))]
	internal static class GridManager_ObjectPickedUp_PrefixPatch
    {
        public static void Prefix(GridManager __instance, GridObject o)
        {
            Util.Message("ObjectPickedUp " + o.name + " id: " + o.ItemData.id);
            Util.Log("ObjectPickedUp " + o.name + " id: " + o.ItemData.id);
        }
    }

	[HarmonyPatch(typeof(BanishAbility), nameof(BanishAbility.Awake))]
	internal static class BanishAbility_Awake_Patch
	{
        public static void Prefix(BanishAbility __instance)
        {
            Util.Log(" BanishAbility cooldown " + __instance.abilityData.cooldown);
        }
    }

	[HarmonyPatch(typeof(CinemachineFreeLookInputProvider), nameof(CinemachineFreeLookInputProvider.GetAxisCustom))]
	internal static class CinemachineFreeLookInputProvider_GetAxisCustom_Patch
    {
        private static float mouseX;

        public static bool Prefix(CinemachineFreeLookInputProvider __instance, string axisName, ref float __result)
        {
            if (!GameManager.Instance.IsPaused && __instance.canMoveCamera && !__instance.playerCamera.IsRecentering && !GameManager.Instance.UI.IsShowingRadialMenu && (GameManager.Instance.Input.IsUsingController || __instance.freelook || __instance.spyglassEnabled || GameManager.Instance.Input.Controls.CameraMoveButton.IsPressed))
            {
                if (axisName.Equals("Mouse X"))
                {
                    mouseX = Input.GetAxisRaw("Mouse X");
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        Util.Message("GetAxisRaw " + Input.GetAxisRaw("Mouse X") + " GetAxis " + Input.GetAxis("Mouse X"));
                    }
                    __result = mouseX;
                    return false;
                }
                if (axisName.Equals("Mouse Y"))
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        Util.Message(" Mouse Y ");
                    }
                    __result = Input.GetAxisRaw("Mouse Y");
                    return false;
                }
            }
            __result = 0f;
            return false;
        }
    }

	[HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.CalculateRodStats))]
	internal static class PlayerStats_CalculateRodStats_Patch
    {
        public static bool Prefix(PlayerStats __instance)
        {
            Util.Log("CalculateRodStats  ");
            return false;
        }
    }

	[HarmonyPatch(typeof(TooltipUI), nameof(TooltipUI.ConstructSpatialItemTooltip))]
	internal static class TooltipUI_ConstructSpatialItemTooltip_Patch
	{
        public static bool Prefix(TooltipUI __instance, SpatialItemInstance itemInstance, ItemData itemData, TooltipUI.TooltipMode tooltipMode)
        {
            __instance.PrepareForTooltipShow();
            __instance.activeTooltipSections.Add(__instance.itemHeaderWithIcon);
            __instance.itemHeaderWithIcon.Init(itemData, tooltipMode);
            __instance.itemHeaderWithIcon.SetObscured(tooltipMode == TooltipUI.TooltipMode.MYSTERY);
            if (tooltipMode == TooltipUI.TooltipMode.HINT && itemData.itemSubtype == ItemSubtype.FISH)
            {
                __instance.activeTooltipSections.Add(__instance.fishHarvestDetails);
                __instance.fishHarvestDetails.Init(itemData as FishItemData, tooltipMode);
            }
            if (tooltipMode == TooltipUI.TooltipMode.HOVER)
            {
                if (itemData.itemSubtype == ItemSubtype.FISH)
                {
                    __instance.activeTooltipSections.Add(__instance.fishDetails);
                    __instance.fishDetails.Init(itemInstance as FishItemInstance, tooltipMode);
                }
                if (itemData.itemType == ItemType.EQUIPMENT && itemData.itemSubtype != ItemSubtype.POT)
                {
                    Util.Message("EQUIPMENT tooltip " + itemInstance.id);
                    __instance.activeTooltipSections.Add(__instance.equipmentDetails);
                    __instance.equipmentDetails.Init(itemInstance, tooltipMode);
                }
            }
            if (tooltipMode == TooltipUI.TooltipMode.HOVER || tooltipMode == TooltipUI.TooltipMode.RESEARCH_PREVIEW || tooltipMode == TooltipUI.TooltipMode.MYSTERY)
            {
                if (itemData.itemSubtype == ItemSubtype.ROD)
                {
                    __instance.activeTooltipSections.Add(__instance.rodDetails);
                    __instance.rodDetails.Init(itemData as RodItemData, itemInstance, tooltipMode);
                }
                if (itemData.itemSubtype == ItemSubtype.DREDGE)
                {
                    __instance.activeTooltipSections.Add(__instance.dredgeDetails);
                    __instance.dredgeDetails.Init(itemData as DredgeItemData, tooltipMode);
                }
                if (itemData.itemSubtype == ItemSubtype.ENGINE && tooltipMode != TooltipUI.TooltipMode.MYSTERY)
                {
                    __instance.activeTooltipSections.Add(__instance.engineDetails);
                    __instance.engineDetails.Init(itemData as EngineItemData, itemInstance, tooltipMode);
                }
                if (itemData.itemSubtype == ItemSubtype.LIGHT)
                {
                    __instance.activeTooltipSections.Add(__instance.lightDetails);
                    __instance.lightDetails.Init(itemData as LightItemData, itemInstance, tooltipMode);
                }
                if (itemData.itemSubtype == ItemSubtype.POT && tooltipMode != TooltipUI.TooltipMode.MYSTERY)
                {
                    __instance.activeTooltipSections.Add(__instance.deployableDetails);
                    __instance.deployableDetails.Init(itemData as DeployableItemData, itemInstance, tooltipMode);
                }
                if (itemData.itemSubtype == ItemSubtype.NET)
                {
                    __instance.activeTooltipSections.Add(__instance.deployableDetails);
                    __instance.deployableDetails.Init(itemData as DeployableItemData, itemInstance, tooltipMode);
                }
                if (tooltipMode != TooltipUI.TooltipMode.MYSTERY)
                {
                    __instance.activeTooltipSections.Add(__instance.description);
                    __instance.description.Init(itemData, tooltipMode);
                }
            }
            if (tooltipMode != TooltipUI.TooltipMode.HINT)
            {
                __instance.activeTooltipSections.Add(__instance.controlPrompts);
                __instance.controlPrompts.Init();
            }
            if (__instance.layoutCoroutine != null)
            {
                ((MonoBehaviour)(object)__instance).StopCoroutine(__instance.layoutCoroutine);
            }
            __instance.layoutCoroutine = ((MonoBehaviour)(object)__instance).StartCoroutine(__instance.DoUpdateLayoutGroups());
            return false;
        }
    }

	[HarmonyPatch(typeof(TooltipSectionEquipmentDetails), nameof(TooltipSectionEquipmentDetails.OnEnable))]
	internal static class TooltipSectionEquipmentDetails_Awake_Patch
	{
        public static bool Prefix(TooltipSectionEquipmentDetails __instance)
        {
            __instance.isLayedOut = false;
            __instance.isLayedOut = true;
            return false;
        }
    }

	[HarmonyPatch(typeof(TooltipUI), nameof(TooltipUI.DoUpdateLayoutGroups))]
	internal static class TooltipUI_DoUpdateLayoutGroups_Patch
	{
        public static bool Prefix(TooltipUI __instance, ref IEnumerator __result)
        {
            __result = DoUpdateLayoutGroups(__instance);
            return false;
        }
    }

	[HarmonyPatch(typeof(GameEvents), nameof(GameEvents.TriggerWorldPhaseChanged))]
	internal static class GameEvents_TriggerWorldPhaseChanged_Patch
	{
        public static void Postfix(GameEvents __instance, int worldPhase)
        {
            Util.Message("TriggerWorldPhaseChanged " + worldPhase);
            Util.Log("TriggerWorldPhaseChanged " + worldPhase);
        }
    }

	[HarmonyPatch(typeof(HarvestPOI), nameof(HarvestPOI.Awake))]
	internal static class HarvestPOI_Awake_Patch
{
        public static void Postfix(HarvestPOI __instance)
        {
            if (!Util.IsfishingSpot(__instance))
            {
                return;
            }
            if (__instance.harvestPOIData == null)
            {
                Util.Log("HarvestPOI harvestPOIData == null");
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            List<HarvestableItemData> items = __instance.harvestPOIData.items;
            List<HarvestableItemData> nightItems = __instance.harvestPOIData.nightItems;
            if (items != null && items.Count > 0)
            {
                string id = items[0].id;
                if (fishCount.ContainsKey(id))
                {
                    fishCount[id]++;
                }
                else
                {
                    fishCount[id] = 1;
                }
            }
            else if (nightItems != null && nightItems.Count > 0)
            {
                string id2 = nightItems[0].id;
                if (fishCount.ContainsKey(id2))
                {
                    fishCount[id2]++;
                }
                else
                {
                    fishCount[id2] = 1;
                }
            }
        }
    }

    public static Dictionary<string, int> fishCount = new Dictionary<string, int>();

    public static Dictionary<string, string> fishF = new Dictionary<string, string>();

    public static HashSet<string> dayFishSpots = new HashSet<string>();

    public static HashSet<string> nightFishSpots = new HashSet<string>();

    private static IEnumerator DoUpdateLayoutGroups(TooltipUI tooltipUi)
    {
        do
        {
            yield return new WaitForEndOfFrame();
        }
        while (!tooltipUi.activeTooltipSections.TrueForAll((x) => x.IsLayedOut));
        foreach (ILayoutable layoutable in tooltipUi.activeTooltipSections)
        {
            if (!(layoutable.GameObject.name == "TooltipSectionControlPrompts"))
            {
                Util.Log("layoutable type " + layoutable.GetType()?.ToString() + " GameObject " + layoutable.GameObject.name);
                layoutable.GameObject.SetActive(value: true);
            }
        }
        tooltipUi.verticalLayoutGroup.enabled = false;
        yield return new WaitForEndOfFrame();
        tooltipUi.verticalLayoutGroup.enabled = true;
        yield return new WaitForEndOfFrame();
        tooltipUi.canvasGroupFadeTween = tooltipUi.canvasGroup.DOFade(1f, 0.35f);
        tooltipUi.canvasGroupFadeTween.SetUpdate(isIndependentUpdate: true);
        tooltipUi.canvasGroupFadeTween.OnComplete(delegate
        {
            tooltipUi.canvasGroupFadeTween = null;
        });
        tooltipUi.layoutCoroutine = null;
    }

    public static bool ListQuests(QuestManager questManager)
    {
        StringBuilder stringBuilder = new StringBuilder("___ Quests begin ___");
        foreach (QuestData item in questManager.allQuests.Values.ToList())
        {
            stringBuilder.Append(item.name);
            stringBuilder.Append(", ");
        }
        stringBuilder.Append("___ Quests end ___");
        Util.Log(stringBuilder.ToString());
        return false;
    }

    public static bool ListHoodedQuests(QuestManager questManager)
    {
        StringBuilder stringBuilder = new StringBuilder("___ Quests begin ___");
        stringBuilder.Append(Environment.NewLine);
        foreach (QuestData item in questManager.allQuests.Values.ToList())
        {
            if (!item.name.StartsWith("Quest_HoodedFigure"))
            {
                continue;
            }
            stringBuilder.Append(item.name);
            stringBuilder.Append(Environment.NewLine);
            foreach (QuestData subquest in item.subquests)
            {
                stringBuilder.Append(item.name + " subquest " + subquest.name);
                stringBuilder.Append(Environment.NewLine);
            }
            if (item.steps != null && item.steps.Count > 0)
            {
                stringBuilder.Append(item.name + " steps");
                stringBuilder.Append(Environment.NewLine);
            }
            foreach (QuestStepData step in item.steps)
            {
                if (step.failureEvents == null || step.failureEvents.Count > 0)
                {
                }
                stringBuilder.Append(Environment.NewLine);
            }
        }
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("___ Quests end ___");
        Util.Log(stringBuilder.ToString());
        return false;
    }

    public static bool ListFailableQuestSteps(QuestManager questManager)
    {
        StringBuilder stringBuilder = new StringBuilder("Failable quests");
        foreach (QuestStepData item in questManager.allQuestSteps.Values.ToList())
        {
            if (item.canBeFailed)
            {
                stringBuilder.Append(item.name);
                stringBuilder.Append(", ");
            }
        }
        Util.Log(stringBuilder.ToString());
        return false;
    }
}
