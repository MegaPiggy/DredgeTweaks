using Newtonsoft.Json;
using Sirenix.Serialization;
using Steamworks;
using System;
using System.IO;
using System.Reflection;
using Winch.Config;
using Winch.Core;

namespace Tweaks;

public static class Util
{
	public static void PrepareConfig()
	{
		var folderpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		string filepath = Path.Combine(folderpath, Winch.Constants.ModConfigFileName);
		if (!File.Exists(filepath)) // Generate file
		{
			try
			{
				JSONConfig.WriteConfig(filepath, new Config());
			}
			catch (Exception ex)
			{
				WinchCore.Log.Error(string.Format("Error on 'PrepareConfig' call. Config at '{0}' cannot be written to: {1}", filepath, ex));
			}
		}
	}

	public static T GetJSON<T>(string filename)
	{
		var folderpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		string filepath = Path.Combine(folderpath, filename + ".json");
		filepath = File.Exists(filepath) ? filepath : Path.Combine(folderpath, filename + ".jsonc");
		if (File.Exists(filepath))
		{
			try
			{
				return JSONConfig.ReadConfig<T>(filepath);
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
				T instance = (T)Activator.CreateInstance(typeof(T));
				JSONConfig.WriteConfig(filepath, instance, typeof(T));
				return instance;
			}
			catch (Exception ex)
			{
				WinchCore.Log.Error(string.Format("Error on 'GetJSON' call. Config at '{0}' cannot be written to: {1}", filepath, ex));
			}
		}
		return default(T);
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
}
