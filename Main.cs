using HarmonyLib;
using System.Reflection;
using Winch.Core;

namespace Tweaks
{
	public static class Main
	{
		public static Config Config;

		public static void Initialize()
		{
			new Harmony("megapiggy.tweaks").PatchAll(Assembly.GetExecutingAssembly());
			Config = Util.GetJSON<Config>("config");
			//Config.boatSpeedMult.SettingChanged += Boat_Patch.BoatMoveSpeed_SettingChanged;
			//Config.boatTurnMult.SettingChanged += Boat_Patch.BoatTurnSpeed_SettingChanged;
			//Config.boatTurnsOnlyWhenMoving.SettingChanged += Boat_Patch.BoatTurnSpeed_SettingChanged;
			//Config.boostCooldownMult.SettingChanged += Ability_Patch.BoostCooldownMult_SettingChanged;
			//Config.cameraFOV.SettingChanged += MiscPatch.CameraFOV_SettingChanged;
			//Config.sanityMultiplier.SettingChanged += MiscPatch.SanityMult_SettingChanged;
			ApplicationEvents.Instance.OnGameStartable += OnGameStartable;
			ApplicationEvents.Instance.OnGameUnloaded += OnGameUnloaded;
			GameManager.Instance.OnGameStarted += OnGameStarted;
			GameManager.Instance.OnGameEnded += OnGameEnded;
		}

		private static void OnGameStartable()
		{
			WinchCore.Log.Info("OnGameStartable");
			Util.SetBoatWeight();
		}

		private static void OnGameUnloaded()
		{
			WinchCore.Log.Info("OnGameUnloaded");
		}

		private static void OnGameStarted()
		{
			WinchCore.Log.Info("OnGameStarted");
		}

		private static void OnGameEnded()
		{
			WinchCore.Log.Info("OnGameEnded");
		}
	}
}
