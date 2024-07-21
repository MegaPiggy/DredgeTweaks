using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using Winch.Core;

namespace Tweaks;

public static class Util
{
	internal static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
	{
		NullValueHandling = NullValueHandling.Ignore,
		DefaultValueHandling = DefaultValueHandling.Include,
		Formatting = Formatting.Indented,
	};

	public static T GetJSON<T>(string filename)
	{
		var folderpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		string filepath = Path.Combine(folderpath, filename + ".json");
		if (File.Exists(filepath))
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(filepath));
			}
			catch (Exception ex)
			{
				WinchCore.Log.Error(string.Format("Error on 'GetJSON' call. Config at '{0}' cannot be read from: {1}", filepath, ex));
			}
		}
		else if (!File.Exists(filepath)) // Generate file
		{
			try
			{
				var obj = default(T);
				File.WriteAllText(filepath, JsonConvert.SerializeObject(obj, typeof(T), jsonSettings));
				return obj;
			}
			catch (Exception ex)
			{
				WinchCore.Log.Error(string.Format("Error on 'GetJSON' call. Config at '{0}' cannot be written to: {1}", filepath, ex));
			}
		}
		return default(T);
	}

	public static Dictionary<HarvestableType, float> typeAvDepth = new Dictionary<HarvestableType, float>();

	public static float MapTo01range(int value, int min, int max)
	{
		int num = max - min;
		if (num == 0)
		{
			return 0f;
		}
		return ((float)value - (float)min) / (float)num;
	}

	public static float MapTo01range(float value, float min, float max)
	{
		float num = max - min;
		if (num == 0f)
		{
			return 0f;
		}
		return (value - min) / num;
	}

	public static int MapToRange(int value, int oldMin, int oldMax, int newMin, int newMax)
	{
		int num = oldMax - oldMin;
		if (num == 0)
		{
			return newMin;
		}
		int num2 = newMax - newMin;
		return (value - oldMin) * num2 / num + newMin;
	}

	public static float MapToRange(float value, float oldMin, float oldMax, float newMin, float newMax)
	{
		float num = oldMax - oldMin;
		if (num == 0f)
		{
			return newMin;
		}
		float num2 = newMax - newMin;
		return (value - oldMin) * num2 / num + newMin;
	}

	public static void Message(string s)
	{
		if (!(GameEvents.Instance == null) && s != null && s.Length != 0)
		{
			GameEvents.Instance.TriggerNotification(NotificationType.ERROR, s);
		}
	}

	public static void Log(string s) => WinchCore.Log.Info(s);

	public static void LogError(string s) => WinchCore.Log.Error(s);

	public static void PrintItems()
	{
		foreach (ItemData allItem in GameManager.Instance.ItemManager.allItems)
		{
			WinchCore.Log.Info(allItem.id + ", " + allItem.itemNameKey.GetLocalizedString() + ", type " + allItem.itemType.ToString() + ", subtype " + allItem.itemSubtype);
		}
	}

	public static void ShowGameTime()
	{
		Message(GameManager.Instance.Time.GetTimeFormatted(GameManager.Instance.Time.Time));
	}

	public static string GetGameTime()
	{
		var num = GameManager.Instance.Time.Time * 24;
		var num2 = (float)Mathf.FloorToInt(num);
		var num3 = Mathf.FloorToInt((num - num2) * 60f);
		string text = "";
		string text2 = "";
		string text3 = num3.ToString("00.##");
		if (GameManager.Instance.SettingsSaveData.clockStyle == 0)
		{
			if (num2 < 12)
			{
				text = " AM";
			}
			if (num2 >= 12)
			{
				num2 %= 12;
				text = " PM";
			}
			if (num2 == 0)
			{
				num2 = 12;
			}
			text2 = num2.ToString();
		}
		else if (GameManager.Instance.SettingsSaveData.clockStyle == 1)
		{
			text2 = num2.ToString("00.##");
		}
		return text2 + " " + text3 + " " + text;
	}

	public static HarvestQueryEnum IsHarvestable(HarvestPOIDataModel harvestPOIDataModel)
	{
		bool flag = (GameManager.Instance.Time.IsDaytime && harvestPOIDataModel.items.Count == 0) || (!GameManager.Instance.Time.IsDaytime && harvestPOIDataModel.nightItems.Count == 0);
		if (harvestPOIDataModel.usesTimeSpecificStock && flag)
		{
			return HarvestQueryEnum.INVALID_INCORRECT_TIME;
		}
		return (harvestPOIDataModel.GetStockCount(ignoreTimeOfDay: false) >= 1) ? HarvestQueryEnum.VALID : HarvestQueryEnum.INVALID_NO_STOCK;
	}

	public static void PrintHarvestPOIData(HarvestPOI __instance)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (__instance.harvestable.GetUsesTimeSpecificStock())
		{
			List<ItemData> items = __instance.harvestable.GetItems();
			if (items.Count > 0)
			{
				stringBuilder.Append(" day ");
				stringBuilder.Append(items[0].id);
			}
			List<ItemData> nightItems = __instance.harvestable.GetNightItems();
			if (nightItems.Count > 0)
			{
				stringBuilder.Append(" night ");
				stringBuilder.Append(nightItems[0].id);
			}
		}
		else
		{
			List<ItemData> items2 = __instance.harvestable.GetItems();
			if (items2.Count > 0)
			{
				stringBuilder.Append(" 24H ");
				stringBuilder.Append(items2[0].id);
			}
		}
		Log(__instance.name + " " + GetGameTime() + stringBuilder.ToString());
	}

	public static bool IsfishingSpot(HarvestPOI harvestPOI)
	{
		return harvestPOI.harvestable.GetHarvestableItemSubType() == ItemSubtype.FISH && !harvestPOI.IsBaitPOI && !harvestPOI.IsCrabPotPOI;
	}

	public static bool IsdredgingSpot(HarvestPOI harvestPOI)
	{
		return harvestPOI.harvestable.GetHarvestableItemSubType() != ItemSubtype.FISH;
	}

	public static bool IsQuestItem(ItemData itemData)
	{
		return itemData.id.StartsWith("quest");
	}

	public static T EnsureComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static Vector3 getPos(Transform transform, float dist)
	{
		Vector3 position = transform.position;
		float num = transform.eulerAngles.z * -90f / 180f;
		Quaternion quaternion = Quaternion.AngleAxis(0f - num, Vector3.forward);
		Vector3 vector = (position + Vector3.up).normalized * dist;
		Vector3 position2 = transform.position;
		return position2 + quaternion * vector;
	}

	public static void PrintPlayerPos()
	{
		int num = (int)GameManager.Instance.Player.transform.position.x;
		int num2 = (int)GameManager.Instance.Player.transform.position.y;
		int num3 = (int)GameManager.Instance.Player.transform.position.z;
		float depthRaw = GameManager.Instance.WaveController.SampleWaterDepthAtPlayerPosition();
		string formattedDepthString = GameManager.Instance.ItemManager.GetFormattedDepthString(depthRaw);
		Message(num + " " + num2 + " " + num3 + " depth " + formattedDepthString);
	}

	public static void GetHarvestPOIdata(HarvestPOI __instance)
	{
		if (IsfishingSpot(__instance))
		{
			HarvestableType harvestType = __instance.harvestable.GetHarvestType();
			Vector2 samplePositionByWorldPosition = GameManager.Instance.WaveController.GetSamplePositionByWorldPosition(__instance.transform.position);
			float num = GameManager.Instance.WaveController.SampleWaterDepthAtPosition(samplePositionByWorldPosition);
			if (typeAvDepth.ContainsKey(harvestType))
			{
				float value = (typeAvDepth[harvestType] + num) * 0.5f;
				typeAvDepth[harvestType] = value;
			}
			else
			{
				typeAvDepth.Add(harvestType, num);
			}
		}
	}

	public static void PrintPlayerTarget()
	{
		Vector3 position = GameManager.Instance.Player.ColliderCenter.position;
		Vector3 forward = GameManager.Instance.Player.transform.forward;
		int num = 64;
		LayerMask layerMask = num;
		Message("PrintPlayerTarget ");
		if (!Physics.Raycast(position, forward, out var hitInfo, 111f, ~(int)layerMask, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		Message("player target " + hitInfo.collider.gameObject.name);
		if (hitInfo.collider.transform.parent != null)
		{
			Message("player target parent " + hitInfo.collider.gameObject.transform.parent.name);
			if (hitInfo.collider.transform.parent.parent != null)
			{
				Message("player target parent parent " + hitInfo.collider.gameObject.transform.parent.parent.name);
			}
		}
	}

	public static void SetBoatWeight()
	{
		int cargoNonHiddenCells = GameManager.Instance.SaveData.Inventory.GetCountNonHiddenCells();
		if (Main.Config.cargoBoatMassMult > 0f)
		{
			int cargoFilledCells = GameManager.Instance.SaveData.Inventory.GetFilledCells(ItemType.ALL);
			GameManager.Instance.Player.Controller.rb.mass = 1f + (cargoFilledCells / (float)cargoNonHiddenCells);
		}
		if (Main.Config.netBoatMassMult > 0f)
		{
			int netFilledCells = GameManager.Instance.SaveData.TrawlNet.GetFilledCells(ItemType.ALL);
			int netNonHiddenCells = GameManager.Instance.SaveData.TrawlNet.GetCountNonHiddenCells();
			GameManager.Instance.Player.Controller.rb.mass += (netFilledCells / (float)netNonHiddenCells) * (netNonHiddenCells / (float)cargoNonHiddenCells);
		}
	}

	public static void SetBoatWeight(SerializableGrid grid)
	{
		if ((grid == GameManager.Instance.SaveData.Inventory || grid == GameManager.Instance.SaveData.TrawlNet) && (Main.Config.cargoBoatMassMult > 0f || Main.Config.netBoatMassMult > 0f))
		{
			int cargoFilledCells = GameManager.Instance.SaveData.Inventory.GetFilledCells(ItemType.ALL);
			int cargoNonHiddenCells = GameManager.Instance.SaveData.Inventory.GetCountNonHiddenCells();
			GameManager.Instance.Player.Controller.rb.mass = 1f + ((cargoFilledCells / (float)cargoNonHiddenCells) * Main.Config.cargoBoatMassMult);
			int netFilledCells = GameManager.Instance.SaveData.TrawlNet.GetFilledCells(ItemType.ALL);
			int netNonHiddenCells = GameManager.Instance.SaveData.TrawlNet.GetCountNonHiddenCells();
			GameManager.Instance.Player.Controller.rb.mass += (netFilledCells / (float)netNonHiddenCells) * Main.Config.netBoatMassMult * (netNonHiddenCells / (float)cargoNonHiddenCells);
		}
	}

	public static bool IsPlayerMoving()
	{
		return GameManager.Instance.Player.Controller.rb.velocity.magnitude > 0.1f;
	}

	public static void PrintFishData()
	{
		List<SpatialItemData> spatialItemDataBySubtype = GameManager.Instance.ItemManager.GetSpatialItemDataBySubtype(ItemSubtype.FISH);
		spatialItemDataBySubtype.Sort((SpatialItemData x, SpatialItemData y) => x.id.CompareTo(y.id));
		foreach (SpatialItemData item in spatialItemDataBySubtype)
		{
			if (item is FishItemData fishItemData && !fishItemData.IsAberration)
			{
				StringBuilder stringBuilder = new StringBuilder(",");
				if (fishItemData.canBeCaughtByRod)
				{
					stringBuilder.Append(" canBeCaughtByRod,");
				}
				if (fishItemData.canBeCaughtByNet)
				{
					stringBuilder.Append(" canBeCaughtByNet,");
				}
				if (fishItemData.canBeCaughtByPot)
				{
					stringBuilder.Append(" canBeCaughtByPot,");
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				if (fishItemData.hasMinDepth)
				{
					stringBuilder2.Append(" minDepth " + fishItemData.minDepth.ToString() + ",");
				}
				if (fishItemData.hasMaxDepth)
				{
					stringBuilder2.Append(" maxDepth " + fishItemData.maxDepth.ToString() + ",");
				}
				StringBuilder stringBuilder3 = new StringBuilder();
				if (fishItemData.minWorldPhaseRequired > 0)
				{
					stringBuilder3.Append(" minWorldPhaseRequired " + fishItemData.minWorldPhaseRequired + ",");
				}
				StringBuilder stringBuilder4 = new StringBuilder();
				if (fishItemData.aberrations.Count == 1)
				{
					stringBuilder4.Append(" has aberration,");
				}
				else if (fishItemData.aberrations.Count > 1)
				{
					stringBuilder4.Append(" has " + fishItemData.aberrations.Count + " aberrations,");
				}
				StringBuilder stringBuilder5 = new StringBuilder();
				if (fishItemData.locationHiddenUntilCaught)
				{
					stringBuilder5.Append(" locationHiddenUntilCaught,");
				}
				StringBuilder stringBuilder6 = new StringBuilder();
				if (fishItemData.canAppearInBaitBalls)
				{
					stringBuilder6.Append(" canAppearInBaitBalls,");
				}
				StringBuilder stringBuilder7 = new StringBuilder();
				if (fishItemData.harvestableType != 0)
				{
					stringBuilder7.Append(" harvestableType " + fishItemData.harvestableType.ToString() + ",");
				}
				StringBuilder stringBuilder8 = new StringBuilder();
				if (fishItemData.harvestPOICategory != 0)
				{
					stringBuilder8.Append(" harvestPOICategory " + fishItemData.harvestPOICategory.ToString() + ",");
				}
			}
		}
	}
}
