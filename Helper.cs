using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAss
{
    public static class Helper
    {
        public static void SetModLocalization()
        {
            RegisterLoc("#mod_dialog_grind_status", "Grind Status", "研磨信息");
            RegisterLoc("#mod_dialog_stir_status", "Stir Status", "搅拌信息");
            RegisterLoc("#mod_dialog_path_status", "Path Status", "路径信息");
            RegisterLoc("#mod_dialog_ladle_status", "Ladle Status", "加水信息");
            RegisterLoc("#mod_dialog_position_status", "Position Status", "位置信息");
            RegisterLoc("#mod_dialog_deviation_status", "Deviation Status", "偏离信息");
            RegisterLoc("#mod_dialog_vortex_status", "Vortex Status", "漩涡信息");
            RegisterLoc("#mod_dialog_health_status", "Health Status", "血量信息");
            RegisterLoc("#mod_grind_progress", "PR: ", "进度: ");
            RegisterLoc("#mod_stir_stage", "ST: ", "阶段: ");
            RegisterLoc("#mod_stir_progress", "PR: ", "进度: ");
            RegisterLoc("#mod_stir_direction", "DI: ", "方向: ");
            RegisterLoc("#mod_closest_potion_path", "PO:", "目标: ");
            RegisterLoc("#mod_closest_general_path", "GE:", "总体: ");
            RegisterLoc("#mod_closest_distance_path", "DI:", "距离: ");
            RegisterLoc("#mod_closest_angle_path", "AN:", "旋转: ");
            RegisterLoc("#mod_closest_potion_ladle", "PO:", "目标: ");
            RegisterLoc("#mod_closest_general_ladle", "GE:", "总体: ");
            RegisterLoc("#mod_closest_distance_ladle", "DI:", "距离: ");
            RegisterLoc("#mod_closest_angle_ladle", "AN:", "旋转: ");
            RegisterLoc("#mod_position_distance", "DI: ", "距离: ");
            RegisterLoc("#mod_position_rotation", "RO: ", "旋转: ");
            RegisterLoc("#mod_position_angle", "AN: ", "方向: ");
            RegisterLoc("#mod_position_salt", "SA: ", "盐量: ");
            RegisterLoc("#mod_deviation_general", "GE: ", "总体: ");
            RegisterLoc("#mod_deviation_distance", "DI: ", "距离: ");
            RegisterLoc("#mod_deviation_rotation", "RO: ", "旋转: ");
            RegisterLoc("#mod_vortex_direction", "DI: ", "方向: ");
            RegisterLoc("#mod_vortex_angle", "AN: ", "夹角: ");
            RegisterLoc("#mod_health_status", "HP: ", "血量: ");
        }
        public static void RegisterLoc(string key, string en, string zh)
        {
            for (var localeIndex = 0; localeIndex <= 13; ++localeIndex)
                AccessTools.StaticFieldRefAccess<LocalizationData>(typeof(LocalizationManager), "localizationData").Add(localeIndex, key, localeIndex == 9 ? zh : en);
        }
        public static DebugWindow CreateDebugWindow(string name, Vector2 position)
        {
            if (AlchAss.lab == null)
                return null;
            var window = DebugWindow.Init(LocalizationManager.GetText(name), true);
            AlchAss.foreground_queue.Add(window);
            window.ToForeground();
            window.transform.SetParent(AlchAss.lab.transform, false);
            window.transform.localPosition = position;
            return window;
        }
        public static List<PotionEffectMapItem> GetNearestEffects()
        {
            Vector2 indicatorLocalPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var availableEffects = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.Where(effect => effect.Status != PotionEffectStatus.Collected);
            return availableEffects.OrderBy(effect => ((Vector2)effect.thisTransform.localPosition - indicatorLocalPosition).sqrMagnitude).ToList();
        }
        public static void WindowsPosition()
        {
            if (Keyboard.current.f11Key.wasPressedThisFrame)
                AlchAss.windowsPosition = !AlchAss.windowsPosition;
        }
        public static void DirectionLine()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                AlchAss.directionLine = !AlchAss.directionLine;
                if (AlchAss.solventDirectionHint != null)
                    Traverse.Create(AlchAss.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
            }
        }
        public static bool CanBrewTimes(IRecipeBookPageContent recipePageContent, int count, int times)
        {
            var requiredComponents = RecipeBookRecipeBrewController.GetUsedDuringBrewingIngredientsAmount(recipePageContent.GetComponentsToUseInBrewWithPreparedIngredients(), recipePageContent.GetComponentsToUseInBrewWithoutPreparedIngredients(), count * times, true);
            foreach (var component in requiredComponents)
            {
                if (component.Type == AlchemySubstanceComponentType.InventoryItem)
                {
                    var inventoryItem = component.Component as InventoryItem;
                    if (inventoryItem == null)
                        continue;
                    int availableAmount = Managers.Player.Inventory.GetItemCount(inventoryItem);
                    if (availableAmount < component.Amount)
                        return false;
                }
            }
            return true;
        }
    }
}
