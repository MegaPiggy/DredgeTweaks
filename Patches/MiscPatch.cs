using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Yarn;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Tweaks.Patches;

[HarmonyPatch]
internal static class MiscPatch
{
	public static class PlayerSanityUI_Start_PrefixPatch
    {
        public static void Postfix(PlayerSanityUI __instance)
        {
        }
    }

    [HarmonyPatch(typeof(VibrationManager), "Vibrate")]
	public static class VibrationManager_Vibrate_Patch
    {
        public static bool Prefix(VibrationManager __instance)
        {
            return Main.Config.controllerRumble;
        }
    }

    [HarmonyPatch(typeof(PlayerCamera), "Start")]
	public static class PlayerCamera_Start_Patch
    {
        public static void Postfix(PlayerCamera __instance)
        {
            __instance.defaultFOV = Main.Config.cameraFOV;
            __instance.cinemachineCamera.m_Lens.FieldOfView = __instance.defaultFOV;
        }
    }

	public static class InspectPOI_OnEnable_Patch
    {
        public static void Postfix(InspectPOI __instance)
        {
            DisableGlint(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(ConversationPOI), "Start")]
	public static class ConversationPOI_Start_PostfixPatch
    {
        public static void Postfix(ConversationPOI __instance)
        {
            DisableGlint(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(WreckMonster), "OnEnable")]
	public static class WreckMonster_OnEnable_Patch
    {
        public static void Postfix(WreckMonster __instance)
        {
            DisableGlint(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(TimeController))]
	public static class TimeController_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(TimeController __instance)
        {
            if (Main.Config.dayLengthMult == 1f)
            {
                return true;
            }
            if (Application.isPlaying)
            {
                __instance.wasDaytimeHelperVar = __instance.IsDaytime;
                decimal num = __instance.GetTimeChangeThisFrame();
                if (__instance.IsTimePassingForcefully())
                {
                    __instance.timeRemainingToForcefullyPass -= num;
                    if (__instance.timeRemainingToForcefullyPass <= 0m)
                    {
                        __instance.currentTimePassageMode = TimePassageMode.NONE;
                        GameEvents.Instance.TriggerTimeForcefullyPassingChanged(isPassing: false, "", __instance.currentTimePassageMode);
                    }
                }
                if (__instance._freezeTime)
                {
                    num = default;
                }
                if (num > 0)
                {
                    num *= (decimal)Main.Config.dayLengthMult;
                    __instance.timeProxy.SetTimeAndDay(__instance._timeAndDay + num);
                }
            }
            __instance._timeAndDay = __instance.timeProxy.GetTimeAndDay();
            __instance._time = __instance._timeAndDay % 1;
            __instance._day = (int)Math.Floor(__instance._timeAndDay);
            __instance._isDaytime = __instance.Time > __instance.dawnTime && __instance.Time < __instance.duskTime;
            if (__instance.wasDaytimeHelperVar != __instance.IsDaytime && GameEvents.Instance != null)
            {
                GameEvents.Instance.TriggerDayNightChanged();
            }
            if (__instance._lastDay < __instance.Day)
            {
                GameEvents.Instance.TriggerDayChanged(__instance.Day);
                __instance._lastDay = __instance.Day;
            }
            __instance.SceneLightness = __instance.RecalculateSceneLightness();
            Shader.SetGlobalFloat("_SceneLightness", __instance.SceneLightness);
            Shader.SetGlobalFloat("_TimeOfDay", __instance.Time);
            __instance.directionalLight.transform.eulerAngles = new Vector3(__instance.lightAngleMin + 360f * __instance.Time, -90f, 0);
            __instance.directionalLight.color = __instance.sunColour.Evaluate(__instance.Time);
            RenderSettings.ambientLight = __instance.ambientLightColor.Evaluate(__instance.Time);
            if (__instance.playerProxy != null)
            {
                Vector3 playerPosition = __instance.playerProxy.GetPlayerPosition();
                Shader.SetGlobalVector("_FogCenter", new Vector4(playerPosition.x, playerPosition.y, playerPosition.z, 0));
            }
            else
            {
                Camera current = Camera.current;
                if (current != null)
                {
                    Shader.SetGlobalVector("_FogCenter", new Vector4(current.transform.position.x, current.transform.position.y, current.transform.position.z, 0f));
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerSanity))]
	public static class PlayerSanity_PrefixPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(PlayerSanity __instance)
        {
            globalSanityModifier = GameManager.Instance.GameConfigData.globalSanityModifier;
            GameManager.Instance.GameConfigData.globalSanityModifier = globalSanityModifier * Main.Config.sanityMultiplier;
        }
    }

    /*
	public static class VirtualMachine_Patch
    {
        public static bool Prefix(Yarn.VirtualMachine __instance)
        {
            if (!(__instance.state.currentNodeName == "HoodedFigure1_Root") && !(__instance.state.currentNodeName == "HoodedFigure2_Root") && !(__instance.state.currentNodeName == "HoodedFigure3_Root") && !(__instance.state.currentNodeName == "HoodedFigure4_Root"))
            {
                return true;
            }
            __instance.CheckCanContinue();
            if (__instance.CurrentExecutionState == Yarn.VirtualMachine.ExecutionState.DeliveringContent)
            {
                __instance.CurrentExecutionState = Yarn.VirtualMachine.ExecutionState.Running;
                return false;
            }
            __instance.CurrentExecutionState = Yarn.VirtualMachine.ExecutionState.Running;
            while (__instance.CurrentExecutionState == Yarn.VirtualMachine.ExecutionState.Running)
            {
                Instruction instruction = __instance.currentNode.Instructions[__instance.state.programCounter];
                bool flag = false;
                if (instruction.opcode_ == Instruction.Types.OpCode.CallFunc)
                {
                    foreach (Operand item in instruction.operands_)
                    {
                        if (item.valueCase_ == Operand.ValueOneofCase.StringValue && item.value_.ToString() == "GetDaysSinceTemporalMarker")
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    __instance.RunInstruction(instruction);
                }
                __instance.state.programCounter++;
                if (__instance.state.programCounter >= __instance.currentNode.Instructions.Count())
                {
                    __instance.NodeCompleteHandler(__instance.currentNode.Name);
                    __instance.CurrentExecutionState = Yarn.VirtualMachine.ExecutionState.Stopped;
                    __instance.DialogueCompleteHandler();
                    __instance.dialogue.LogDebugMessage("Run complete.");
                }
            }
            return false;
        }
    }
    */

    [HarmonyPatch(typeof(GridUI))]
	public static class GridUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreateObject")]
        public static void CreateObjectPrefix(GridUI __instance, SpatialItemInstance entry)
        {
            if (Main.Config.sellExtraItems)
            {
                if (entry.id == "engine1")
                {
                    SpatialItemData engine = entry.GetItemData<SpatialItemData>();
                    if (engine != null)
                    {
                        engine.canBeSoldByPlayer = true;
                        engine.value = 180m;
                    }
                }
                else if (entry.id == "rod5")
                {
                    SpatialItemData rod = entry.GetItemData<SpatialItemData>();
                    if (rod != null)
                    {
                        rod.canBeSoldByPlayer = true;
                        rod.value = 300m;
                    }
                }
                else if (entry.id == "quest-map-1" || entry.id == "quest-map-2" || entry.id == "quest-map-3")
                {
                    SpatialItemData questMap = entry.GetItemData<SpatialItemData>();
                    if (questMap != null)
                    {
                        questMap.canBeSoldByPlayer = true;
                        questMap.value = 30m;
                        questMap.itemSubtype = ItemSubtype.TRINKET;
                    }
                }
            }
        }
    }

    private static float globalSanityModifier;

    public static void CameraFOV_SettingChanged()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerCamera != null)
        {
            GameManager.Instance.PlayerCamera.defaultFOV = Main.Config.cameraFOV;
            GameManager.Instance.PlayerCamera.cinemachineCamera.m_Lens.FieldOfView = Main.Config.cameraFOV;
        }
    }

    public static void SanityMult_SettingChanged()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameConfigData != null)
        {
            GameManager.Instance.GameConfigData.globalSanityModifier = globalSanityModifier * Main.Config.sanityMultiplier;
        }
    }

    private static void DisableGlint(GameObject __instance)
    {
        if (!Main.Config.showPOIglint)
        {
            Transform transform = __instance.transform.Find("InspectionGlint");
            if (transform != null)
            {
                transform.gameObject.SetActive(value: false);
            }
        }
    }
}
