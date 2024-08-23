using System;
using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using Winch.Core;

namespace Tweaks.Patches;

internal static class UI_patch
{
    [HarmonyPatch(typeof(MapWindow), "RefreshPlayerMarkerPosition")]
	public static class MapWindow_RefreshPlayerMarkerPosition_PostfixPatch
    {
        public static void Prefix(MapWindow __instance)
        {
            bool value = Main.Config.showPlayerMarkerOnMap;
            __instance.youAreHereMarkerTransform.gameObject.SetActive(value);
        }
    }

    [HarmonyPatch(typeof(SpyglassUI), "Update")]
	public static class SpyglassUI_Update_Patch
    {
        public static bool Prefix(SpyglassUI __instance)
        {
            return Main.Config.spyGlassShowsFishingSpots;
        }
    }

    [HarmonyPatch(typeof(InteractPointUI), "Show")]
	public static class UIController_ShowDestination_Patch
    {
        public static void Prefix(InteractPointUI __instance)
        {
            InteractPointUI.showInteractIcon = Main.Config.showPOIicon;
        }
    }

	public static class PlayerFundsUI_OnPlayerFundsChanged_Patch
    {
        public static bool Prefix(PlayerFundsUI __instance, decimal newTotal, decimal changeAmount)
        {
            __instance._textField.text = "$" + newTotal.ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
            return false;
        }
    }

	public static class EncyclopediaPage_Patch
    {
        public static bool Prefix(EncyclopediaPage __instance, FishItemData itemData, int index)
        {
            if (itemData == null)
            {
                __instance.pageContainer.SetActive(value: false);
                return false;
            }
            __instance.pageContainer.SetActive(value: true);
            Color color = GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEUTRAL);
            Color color2 = __instance.harvestTypeTagConfig.textColorLookup[itemData.harvestableType];
            int caughtCountById = GameManager.Instance.SaveData.GetCaughtCountById(itemData.id);
            bool flag = caughtCountById > 0;
            string arg = flag ? itemData.itemNameKey.GetLocalizedString() : "???";
            __instance.itemNameText.text = $"#{index + 1} {arg}";
            __instance.itemImage.sprite = itemData.sprite;
            __instance.itemImage.color = flag ? __instance.itemImageColorIdentified : __instance.itemImageColorUnidentified;
            __instance.itemImageGrid.sizeDelta = new Vector2(itemData.GetWidth() * 128f, itemData.GetHeight() * 128f);
            __instance.undiscoveredItemImage.sprite = itemData.IsAberration ? __instance.undiscoveredAberrationSprite : __instance.undiscoveredRegularSprite;
            __instance.undiscoveredItemImage.color = itemData.IsAberration ? GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEUTRAL) : GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEGATIVE);
            __instance.undiscoveredItemImage.gameObject.SetActive(!flag);
            if (__instance.harvestTypeTagConfig.colorLookup.ContainsKey(itemData.harvestableType))
            {
                color = __instance.harvestTypeTagConfig.colorLookup[itemData.harvestableType];
            }
            else
            {
                Debug.LogError($"[EncyclopediaPage] RefreshUI({itemData.harvestableType}) couldn't find color in Config.");
            }
            __instance.harvestTypeBackplate.color = color;
            if (itemData.harvestableType == HarvestableType.NONE || itemData.harvestableType == HarvestableType.CRAB)
            {
                string depthString = itemData.GetDepthString();
                __instance.harvestTypeLocalizedText.enabled = false;
                __instance.harvestTypeLocalizedText.StringReference.SetReference(LanguageManager.STRING_TABLE, "encyclopedia.depth");
                __instance.harvestTypeLocalizedText.StringReference.Arguments = new object[1] { depthString };
                __instance.harvestTypeLocalizedText.enabled = true;
            }
            else if (__instance.harvestTypeTagConfig.stringLookup.ContainsKey(itemData.harvestableType))
            {
                __instance.harvestTypeLocalizedText.StringReference.SetReference(LanguageManager.STRING_TABLE, __instance.harvestTypeTagConfig.stringLookup[itemData.harvestableType]);
            }
            else
            {
                Debug.LogError($"[EncyclopediaPage] RefreshUI({itemData.harvestableType}) couldn't find string in Config.");
            }
            __instance.harvestTypeText.color = color2;
            if (caughtCountById > 0)
            {
                __instance.caughtCounterLocalizedText.enabled = false;
                __instance.caughtCounterLocalizedText.StringReference.SetReference(LanguageManager.STRING_TABLE, "encyclopedia.caught-some");
                __instance.caughtCounterLocalizedText.StringReference.Arguments = new object[1] { caughtCountById };
                __instance.caughtCounterLocalizedText.enabled = true;
                __instance.caughtCounterText.color = Color.black;
                __instance.caughtCounterDiamond.color = Color.black;
            }
            else
            {
                __instance.caughtCounterLocalizedText.enabled = false;
                __instance.caughtCounterLocalizedText.StringReference.SetReference(LanguageManager.STRING_TABLE, "encyclopedia.caught-none");
                __instance.caughtCounterLocalizedText.enabled = true;
                __instance.caughtCounterText.color = GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEGATIVE);
                __instance.caughtCounterDiamond.color = GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEGATIVE);
            }
            __instance.descriptionLocalizedText.text = flag ? itemData.itemDescriptionKey.GetLocalizedString() : "???";
            __instance.descriptionLocalizedText.color = color2;
            __instance.descriptionBackplate.color = color;
            LocalizedString stringReference = __instance.encyclopediaConfig.zoneStrings[itemData.zonesFoundIn];
            Sprite sprite = __instance.encyclopediaConfig.zoneIconSprites[itemData.zonesFoundIn];
            if (itemData.LocationHiddenUntilCaught && caughtCountById == 0)
            {
                stringReference = __instance.encyclopediaConfig.zoneStrings[ZoneEnum.NONE];
                sprite = __instance.encyclopediaConfig.zoneIconSprites[ZoneEnum.NONE];
            }
            __instance.zoneImage.sprite = sprite;
            __instance.localizedZoneText.StringReference = stringReference;
            __instance.zoneText.HighlightedBackplateColor = color;
            __instance.zoneText.HighlightedTextColor = color2;
            __instance.zoneText.SetHighlighted(highlighted: true);
            foreach (KeyValuePair<ItemSubtype, ToggleableTextWithBackplate> catchTypeText in __instance.catchTypeTexts)
            {
                catchTypeText.Value.HighlightedTextColor = color2;
                catchTypeText.Value.HighlightedBackplateColor = color;
                if (catchTypeText.Key == ItemSubtype.ROD)
                {
                    catchTypeText.Value.SetHighlighted(itemData.canBeCaughtByRod);
                }
                if (catchTypeText.Key == ItemSubtype.POT)
                {
                    catchTypeText.Value.SetHighlighted(itemData.canBeCaughtByPot);
                }
                if (catchTypeText.Key == ItemSubtype.NET)
                {
                    catchTypeText.Value.SetHighlighted(itemData.canBeCaughtByNet);
                }
            }
            if (flag)
            {
                float largestFishRecordById = GameManager.Instance.SaveData.GetLargestFishRecordById(itemData.id);
                __instance.largestText.text = GameManager.Instance.ItemManager.GetFormattedFishSizeString(largestFishRecordById, itemData);
                __instance.trophyIcon.color = largestFishRecordById > GameManager.Instance.GameConfigData.TrophyMaxSize ? GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.VALUABLE) * __instance.trophyIconColorMultiplier : Color.black;
                __instance.trophyIcon.color = new Color(__instance.trophyIcon.color.r, __instance.trophyIcon.color.g, __instance.trophyIcon.color.b, 1);
            }
            else
            {
                __instance.largestText.text = "-";
                __instance.trophyIcon.color = Color.black;
            }
            __instance.valueText.text = GameManager.Instance.DialogueRunner.GetNumItemsSoldById(itemData.id) > 0 ? itemData.value.ToString("n0", LocalizationSettings.SelectedLocale.Formatter) : "???";
            __instance.dayToggler.HighlightedBackplateColor = color;
            __instance.dayToggler.HighlightedImageColor = color2;
            __instance.nightToggler.HighlightedBackplateColor = color;
            __instance.nightToggler.HighlightedImageColor = color2;
            __instance.dayToggler.SetHighlighted(itemData.Day);
            __instance.nightToggler.SetHighlighted(itemData.Night);
            if (itemData.IsAberration)
            {
                FishItemData nonAberrationParent = itemData.NonAberrationParent;
                __instance.aberrationInfos[0].BasicButtonWrapper.OnSubmitAction = delegate
                {
                    __instance.PageLinkRequest?.Invoke(itemData.NonAberrationParent);
                };
                __instance.aberrationInfos[0].SetData(itemData.NonAberrationParent);
                __instance.aberrationInfos[0].gameObject.SetActive(value: true);
                __instance.aberrationInfos[1].gameObject.SetActive(value: false);
                __instance.aberrationInfos[2].gameObject.SetActive(value: false);
                __instance.aberrationsContainer.SetActive(value: true);
                __instance.aberrationsHeaderText.StringReference = __instance.aberrationOfLocalizedString;
            }
            else if (itemData.Aberrations.Count > 0)
            {
                for (int i = 0; i < __instance.aberrationInfos.Count; i++)
                {
                    if (itemData.Aberrations.Count > i)
                    {
                        FishItemData aberrationData = itemData.Aberrations[i];
                        __instance.aberrationInfos[i].BasicButtonWrapper.OnSubmitAction = delegate
                        {
                            __instance.PageLinkRequest?.Invoke(aberrationData);
                        };
                        __instance.aberrationInfos[i].SetData(aberrationData);
                        __instance.aberrationInfos[i].gameObject.SetActive(value: true);
                    }
                    else
                    {
                        __instance.aberrationInfos[i].gameObject.SetActive(value: false);
                    }
                }
                __instance.aberrationsHeaderText.StringReference = __instance.aberrationsLocalizedString;
                __instance.aberrationsContainer.SetActive(value: true);
            }
            else
            {
                __instance.aberrationsContainer.SetActive(value: false);
            }
            __instance.fadeTween = __instance.canvasGroup.DOFade(1f, __instance.fadeDurationSec);
            __instance.fadeTween.SetUpdate(isIndependentUpdate: true);
            __instance.fadeTween.OnComplete(delegate
            {
                __instance.fadeTween = null;
            });
            return false;
        }
    }

	public static class UpgradeGridPanel_Patch
    {
        public static bool Prefix(UpgradeGridPanel __instance, decimal total, decimal change)
        {
            string text = "___!!!___" + __instance.upgradeData.MonetaryCost.ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
            string text2 = !(total >= __instance.upgradeData.MonetaryCost) ? "<color=#" + GameManager.Instance.LanguageManager.GetColorCode(DredgeColorTypeEnum.NEGATIVE) + ">" + text + "</color>" : text;
            __instance.bottomButton.LocalizedString.StringReference.Arguments = new string[1] { text2 };
            __instance.bottomButton.LocalizedString.StringReference.RefreshString();
            return false;
        }
    }

	public static class UIController_Patch
    {
        public static bool Prefix(UIController __instance, NotificationType notificationType, LocalizedString itemNameKey, decimal incomeAmount, decimal debtRepaymentAmount, bool isRefund)
        {
            AsyncOperationHandle<string> localizedStringAsync = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(itemNameKey.TableReference, itemNameKey.TableEntryReference, null, FallbackBehavior.UseProjectSettings);
            localizedStringAsync.Completed += delegate (AsyncOperationHandle<string> op)
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    if (debtRepaymentAmount > 0m)
                    {
                        GameManager.Instance.UI.ShowNotification(notificationType, "notification.sell-fish-debt", new object[3]
                        {
                            op.Result,
                            "<color=#" + GameManager.Instance.LanguageManager.GetColorCode(DredgeColorTypeEnum.POSITIVE) + ">+$" + incomeAmount.ToString("n0", LocalizationSettings.SelectedLocale.Formatter) + "</color>",
                            "<color=#" + GameManager.Instance.LanguageManager.GetColorCode(DredgeColorTypeEnum.NEGATIVE) + ">-$" + debtRepaymentAmount.ToString("n0", LocalizationSettings.SelectedLocale.Formatter) + "</color>"
                        });
                    }
                    else if (isRefund)
                    {
                        GameManager.Instance.UI.ShowNotification(notificationType, "notification.refund-item", new object[2]
                        {
                            op.Result,
                            "<color=#" + GameManager.Instance.LanguageManager.GetColorCode(DredgeColorTypeEnum.POSITIVE) + ">+$" + incomeAmount.ToString("n0", LocalizationSettings.SelectedLocale.Formatter) + "</color>"
                        });
                    }
                    else
                    {
                        GameManager.Instance.UI.ShowNotification(notificationType, "notification.sell-item", new object[2]
                        {
                            op.Result,
                            "<color=#" + GameManager.Instance.LanguageManager.GetColorCode(DredgeColorTypeEnum.POSITIVE) + ">+$" + incomeAmount.ToString("n0", LocalizationSettings.SelectedLocale.Formatter) + "</color>"
                        });
                    }
                }
            };
            return false;
        }
    }

	public static class TooltipSectionUpgradeCost_Patch
    {
        public static bool Prefix(TooltipSectionUpgradeCost __instance, IUpgradeCost upgradeCost)
        {
            SerializableGrid grid = GameManager.Instance.SaveData.GetGridByKey(upgradeCost.GetGridKey());
            __instance.isLayedOut = false;
            __instance.upgradeCostIcons.ForEach(delegate (TooltipUpgradeCostIcon icon)
            {
                icon.gameObject.SetActive(value: false);
            });
            upgradeCost.GetItemCost().ForEach(delegate (ItemCountCondition uc)
            {
                UnityEngine.Object.Instantiate<GameObject>(__instance.upgradeCostIconPrefab, __instance.upgradeCostContainer).GetComponent<TooltipUpgradeCostIcon>().Init(uc.item, uc.CountItems(grid), uc.count);
            });
            __instance.monetaryCostText.text = "$" + upgradeCost.GetMonetaryCost().ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
            __instance.monetaryCostText.color = GameManager.Instance.SaveData.Funds >= upgradeCost.GetMonetaryCost() ? GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEUTRAL) : GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEGATIVE);
            __instance.isLayedOut = true;
            return false;
        }
    }

	public static class SellModeActionHandler_Patch
    {
        public static bool OnFocusedItemChangedPrefix(SellModeActionHandler __instance, GridObject gridObject)
        {
            GameManager.Instance.Input.RemoveActionListener(new DredgePlayerActionBase[2] { __instance.sellAction, __instance.sellHoldAction }, ActionLayer.UI_WINDOW);
            if (!gridObject)
            {
                gridObject = GameManager.Instance.GridManager.GetCurrentlyFocusedObject();
            }
            if (GameManager.Instance.GridManager.CurrentlyHeldObject != null && gridObject != GameManager.Instance.GridManager.CurrentlyHeldObject || !gridObject || !__instance.DoesStoreAcceptThisItem(gridObject.ItemData, bulkMode: false))
            {
                return false;
            }
            decimal num = default;
            if (gridObject.state == GridObjectState.IN_INVENTORY || gridObject.state == GridObjectState.IN_STORAGE || gridObject.state == GridObjectState.BEING_HARVESTED)
            {
                num = GameManager.Instance.ItemManager.GetItemValue(gridObject.SpatialItemInstance, ItemManager.BuySellMode.SELL, __instance.sellValueModifier);
                __instance.sellAction.promptString = "prompt.sell";
                __instance.sellHoldAction.promptString = "prompt.sell";
            }
            else
            {
                if (gridObject.state != GridObjectState.JUST_PURCHASED)
                {
                    if (gridObject.state != GridObjectState.IN_SHOP)
                    {
                        Debug.LogWarning($"[SellModeActionHandler] OnFocusedItemChanged({gridObject}) has a weird state: {gridObject.state}");
                    }
                    return false;
                }
                num = GameManager.Instance.ItemManager.GetItemValue(gridObject.SpatialItemInstance, ItemManager.BuySellMode.BUY);
                __instance.sellAction.promptString = "prompt.refund";
                __instance.sellHoldAction.promptString = "prompt.refund";
            }
            string text = num.ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
            string text2 = ColorUtility.ToHtmlStringRGB(GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.POSITIVE));
            __instance._SellPrice = "<color=#" + text2 + ">$" + text + "</color>";
            object[] localizationArguments = new object[1] { __instance._SellPrice };
            __instance.sellAction.LocalizationArguments = localizationArguments;
            __instance.sellHoldAction.LocalizationArguments = localizationArguments;
            __instance.sellAction.TriggerPromptArgumentsChanged();
            __instance.sellHoldAction.TriggerPromptArgumentsChanged();
            GameManager.Instance.Input.AddActionListener(new DredgePlayerActionBase[1] { __instance.GetSellAction(gridObject) }, ActionLayer.UI_WINDOW);
            return false;
        }

        public static bool CheckSellAllValidityPrefix(SellModeActionHandler __instance)
        {
            bool flag = true;
            if (GameManager.Instance.GridManager.CurrentlyHeldObject != null && GameManager.Instance.GridManager.CurrentlyHeldObject.state == GridObjectState.JUST_PURCHASED)
            {
                flag = false;
            }
            if (flag)
            {
                List<SpatialItemInstance> bulkSellableItemInstances = __instance.GetBulkSellableItemInstances();
                decimal sellAllValue = default;
                bulkSellableItemInstances.ForEach(delegate (SpatialItemInstance itemInstance)
                {
                    sellAllValue += GameManager.Instance.ItemManager.GetItemValue(itemInstance, ItemManager.BuySellMode.SELL, __instance.sellValueModifier);
                });
                __instance.sellAllValueString = "$" + sellAllValue.ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
                object[] array = __instance.sellAllAction.LocalizationArguments = new string[1] { __instance.sellAllValueString };
                __instance.sellAllAction.TriggerPromptArgumentsChanged();
                flag = flag && bulkSellableItemInstances.Count > 0;
                if (bulkSellableItemInstances.Count > 0)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                __instance.sellAllAction.Enable();
            }
            else
            {
                __instance.sellAllAction.Disable(dispatchPressEnd: true);
            }
            return false;
        }

        public static bool SellModeActionHandlerPrefix(SellModeActionHandler __instance)
        {
            WinchCore.Log.Info("SellModeActionHandler constructor " + __instance.sellAllValueString);
            __instance.sellAllValueString = "$0";
            __instance.sellAction = new DredgePlayerActionPress("prompt.sell", GameManager.Instance.Input.Controls.SellItem);
            __instance.sellAction.showInTooltip = true;
            DredgePlayerActionPress sellAction = __instance.sellAction;
            sellAction.OnPressEnd = (Action)Delegate.Combine(sellAction.OnPressEnd, new Action(__instance.SellFocusedItem));
            __instance.sellAction.priority = 4;
            __instance.sellHoldAction = new DredgePlayerActionHold("prompt.sell", GameManager.Instance.Input.Controls.SellItem, 0.75f);
            __instance.sellHoldAction.showInTooltip = true;
            DredgePlayerActionHold sellHoldAction = __instance.sellHoldAction;
            sellHoldAction.OnPressComplete = (Action)Delegate.Combine(sellHoldAction.OnPressComplete, new Action(__instance.SellFocusedItem));
            __instance.sellHoldAction.priority = 4;
            __instance.sellAllAction = new DredgePlayerActionHold("prompt.sell-all", GameManager.Instance.Input.Controls.SellItem, 0.5f);
            object[] array = __instance.sellAllAction.LocalizationArguments = new string[1] { __instance.sellAllValueString };
            __instance.sellAllAction.showInControlArea = true;
            DredgePlayerActionHold sellAllAction = __instance.sellAllAction;
            sellAllAction.OnPressComplete = (Action)Delegate.Combine(sellAllAction.OnPressComplete, new Action(__instance.OnSellAllPressed));
            __instance.sellAllAction.priority = 5;
            GameEvents.Instance.OnItemRemovedFromCursor += __instance.OnItemRemovedFromCursor;
            WinchCore.Log.Info("SellModeActionHandler constructor 1 " + __instance.sellAllValueString);
            return false;
        }
    }

	public static class RepairActionHandler_Patch
    {
        public static bool Prefix(RepairActionHandler __instance, decimal cost, ref string __result)
        {
            string text = cost.ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
            __result = "<color=#" + (!(GameManager.Instance.SaveData.Funds >= cost) ? ColorUtility.ToHtmlStringRGB(GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEGATIVE)) : ColorUtility.ToHtmlStringRGB(Color.white)) + ">$" + text + "</color>";
            return false;
        }
    }

	public static class BuyModeActionHandler_Patch
    {
        public static bool Prefix(BuyModeActionHandler __instance, GridObject gridObject)
        {
            bool flag = false;
            if (!GameManager.Instance.GridManager.CurrentlyHeldObject && gridObject != null && gridObject.state == GridObjectState.IN_SHOP)
            {
                flag = true;
            }
            if (flag)
            {
                GameManager.Instance.Input.AddActionListener(new DredgePlayerActionBase[1] { __instance.buyAction }, ActionLayer.UI_WINDOW);
                decimal itemValue = GameManager.Instance.ItemManager.GetItemValue(gridObject.SpatialItemInstance, ItemManager.BuySellMode.BUY);
                bool flag2 = GameManager.Instance.SaveData.Funds >= itemValue;
                string text = itemValue.ToString("n0", LocalizationSettings.SelectedLocale.Formatter);
                __instance._BuyPrice = "<color=#" + ColorUtility.ToHtmlStringRGB(flag2 ? GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEUTRAL) : GameManager.Instance.LanguageManager.GetColor(DredgeColorTypeEnum.NEGATIVE)) + ">$" + text + "</color>";
                __instance.buyAction.LocalizationArguments = new object[1] { __instance._BuyPrice };
                __instance.buyAction.TriggerPromptArgumentsChanged();
                if (flag2)
                {
                    __instance.buyAction.Enable();
                }
                else
                {
                    __instance.buyAction.Disable(dispatchPressEnd: false);
                }
            }
            else
            {
                GameManager.Instance.Input.RemoveActionListener(new DredgePlayerActionBase[1] { __instance.buyAction }, ActionLayer.UI_WINDOW);
            }
            return false;
        }
    }
}
