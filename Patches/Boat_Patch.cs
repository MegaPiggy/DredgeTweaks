using System;
using HarmonyLib;
using UnityEngine;
using Winch.Core;

namespace Tweaks.Patches;

[HarmonyPatch]
internal static class Boat_Patch
{
    [HarmonyPatch(typeof(PlayerController))]
    public static class PlayerController_Patch
    {
        private static class Rigidbody_AddTorque_Patch
        {
            public static bool Prefix(Rigidbody __instance, Vector3 torque)
            {
                WinchCore.Log.Info("Rigidbody AddTorque " + __instance.name);
                return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(PlayerController __instance)
        {
            baseMovementModifier = __instance._baseMovementModifier;
            baseTurnSpeed = __instance._baseTurnSpeed;
            __instance._baseMovementModifier = baseMovementModifier * Main.Config.boatSpeedMult;
            __instance._baseTurnSpeed = baseTurnSpeed * Main.Config.boatTurnMult;
        }

        [HarmonyPrefix]
        [HarmonyPatch("FixedUpdate")]
        public static void FixedUpdatePrefix(PlayerController __instance)
        {
            if (Main.Config.boatTurnsOnlyWhenMoving)
            {
                Vector2 value = GameManager.Instance.Input.GetValue(__instance.moveAction);
                __instance._baseTurnSpeed = baseTurnSpeed * Mathf.Abs(value.y);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerCollider), "OnCollisionEnter")]
	public static class PlayerCollider_OnCollisionEnter_Patch
    {
        public static bool Prefix(PlayerCollider __instance, Collision other)
        {
            if (Main.Config.safeCollisionMagnitudeThreshold == 0f)
            {
                return true;
            }
            bool isSafeCollider = other.gameObject.CompareTag(__instance.safeColliderTag);
            if ((__instance.iceLayer.value & 1 << other.gameObject.layer) > 0 && GameManager.Instance.SaveData.GetIsIcebreakerEquipped())
            {
                isSafeCollider = true;
            }
            if (other.relativeVelocity.magnitude < Main.Config.safeCollisionMagnitudeThreshold)
            {
                isSafeCollider = true;
            }
            bool isMonster = other.gameObject.CompareTag(__instance.monsterTag);
            bool hasUniqueVibration = other.gameObject.CompareTag(__instance.uniqueVibrationTag);
            __instance.ProcessHit(isSafeCollider, isMonster, hasUniqueVibration);
            return false;
        }
    }

    [HarmonyPatch(typeof(SerializableGrid))]
	public static class SerializableGrid_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AddObjectToGridData")]
        public static void AddObjectToGridDataPatch(SerializableGrid __instance, SpatialItemInstance spatialItemInstance, Vector3Int pos, bool dispatchEvent)
        {
            Util.SetBoatWeight(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("RemoveObjectFromGridData")]
        public static void RemoveObjectFromGridDataPostfix(SerializableGrid __instance, SpatialItemInstance spatialItemInstanceToRemove, bool notify)
        {
            Util.SetBoatWeight(__instance);
        }
    }

    public static float baseMovementModifier;

    public static float baseTurnSpeed;

    public static void BoatMoveSpeed_SettingChanged()
    {
        if (baseMovementModifier > 0f && GameManager.Instance != null && GameManager.Instance.Player != null && GameManager.Instance.Player._controller != null)
        {
            GameManager.Instance.Player._controller._baseMovementModifier = baseMovementModifier * Main.Config.boatSpeedMult;
        }
    }

    public static void BoatTurnSpeed_SettingChanged()
    {
        if (baseTurnSpeed > 0f && GameManager.Instance != null && GameManager.Instance.Player != null && GameManager.Instance.Player._controller != null)
        {
            GameManager.Instance.Player._controller._baseTurnSpeed = baseTurnSpeed * Main.Config.boatTurnMult;
        }
    }
}
