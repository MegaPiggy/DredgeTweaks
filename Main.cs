using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Winch.Core;

namespace Tweaks
{
	public static class Main
	{
		public static Config Config;

		public static void Preload()
		{
			var folderpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string filepath = Path.Combine(folderpath, "default_config.jsonc");
			string newfilepath = Path.Combine(folderpath, "config.jsonc");
			if (File.Exists(filepath) && !File.Exists(newfilepath))
			{
				File.Move(filepath, newfilepath);
			}
		}

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
