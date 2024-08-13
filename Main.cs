using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using Tweaks.Patches;
using UnityEngine;
using Winch.Config;
using Winch.Core;

namespace Tweaks
{
	public static class Main
	{
		public static ModConfig ModConfig => ModConfig.GetConfig();
		public static Config Config;

		public static void Preload()
		{
			var folderpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string filepath = Path.Combine(folderpath, "config.jsonc");
			string newfilepath = Path.Combine(folderpath, "config.json");
			if (File.Exists(filepath) && !File.Exists(newfilepath))
			{
				File.Move(filepath, newfilepath);
			}
			Util.PrepareConfig();
		}

		public static void Initialize()
		{
			new Harmony("megapiggy.tweaks").PatchAll(Assembly.GetExecutingAssembly());
			RefreshConfig();
			ModConfig.OnConfigChanged += ModConfig_OnConfigChanged;
			ModConfig.OnConfigValueChanged += ModConfig_OnConfigValueChanged;
			ApplicationEvents.Instance.OnGameStartable += OnGameStartable;
			ApplicationEvents.Instance.OnGameUnloaded += OnGameUnloaded;
			GameManager.Instance.OnGameStarted += OnGameStarted;
			GameManager.Instance.OnGameEnded += OnGameEnded;
		}

		public static void RefreshConfig()
		{
			Config = ModConfig.ToObject<Config>();
		}

		private static void ModConfig_OnConfigChanged()
		{
			RefreshConfig();
		}

		private static void ModConfig_OnConfigValueChanged(string key)
		{
			try
			{
				switch (key)
				{
					case nameof(Tweaks.Config.boatSpeedMult):
						Boat_Patch.BoatMoveSpeed_SettingChanged();
						break;
					case nameof(Tweaks.Config.boatTurnMult):
						Boat_Patch.BoatTurnSpeed_SettingChanged();
						break;
					case nameof(Tweaks.Config.boatTurnsOnlyWhenMoving):
						Boat_Patch.BoatTurnSpeed_SettingChanged();
						break;
					case nameof(Tweaks.Config.boostCooldownMult):
						Ability_Patch.BoostCooldownMult_SettingChanged();
						break;
					case nameof(Tweaks.Config.cameraFOV):
						MiscPatch.CameraFOV_SettingChanged();
						break;
					case nameof(Tweaks.Config.sanityMultiplier):
						MiscPatch.SanityMult_SettingChanged();
						break;
				}
			}
			catch (System.Exception ex)
			{
				WinchCore.Log.Error(ex.ToString());
			}
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
			if (Config.buyLockedLights)
			{
				foreach (var lightShopData in Resources.FindObjectsOfTypeAll<ShopData>().Where(shopData => shopData.name.EndsWith("_Lights")))
				{
					foreach (var shopItem in lightShopData.phaseLinkedShopData.SelectMany(phaseLinked => phaseLinked.itemData))
					{
						lightShopData.alwaysInStock.Add(shopItem);
					}
					lightShopData.phaseLinkedShopData.Clear();
				}
			}
		}

		private static void OnGameEnded()
		{
			WinchCore.Log.Info("OnGameEnded");
		}
	}
}
