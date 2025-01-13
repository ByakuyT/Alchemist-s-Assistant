using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.ScriptableObjects;
using PotionCraft.Settings;
using UnityEngine;

namespace AlchAss
{
    public static class Helper
    {
        public static void SetModLocalization()
        {
            RegisterLoc("dialog_grind_status", "Grind Status", "研磨信息");
            RegisterLoc("dialog_stir_status", "Stir Status", "搅拌信息");
            RegisterLoc("dialog_path_status", "Path Status", "路径信息");
            RegisterLoc("dialog_ladle_status", "Ladle Status", "加水信息");
            RegisterLoc("dialog_position_status", "Position Status", "位置信息");
            RegisterLoc("dialog_deviation_status", "Deviation Status", "偏离信息");
            RegisterLoc("dialog_vortex_status", "Vortex Status", "漩涡信息");
            RegisterLoc("dialog_health_status", "Health Status", "血量信息");
            RegisterLoc("grind_progress", "PR: ", "进度: ");
            RegisterLoc("stir_stage", "ST: ", "阶段: ");
            RegisterLoc("stir_progress", "PR: ", "进度: ");
            RegisterLoc("stir_direction", "DI: ", "方向: ");
            RegisterLoc("stir_swamp", "ZO: ", "区域: ");
            RegisterLoc("closest_potion_path", "PO:", "目标: ");
            RegisterLoc("closest_general_path", "GE:", "总体: ");
            RegisterLoc("closest_distance_path", "DI:", "距离: ");
            RegisterLoc("closest_angle_path", "AN:", "旋转: ");
            RegisterLoc("closest_potion_ladle", "PO:", "目标: ");
            RegisterLoc("closest_general_ladle", "GE:", "总体: ");
            RegisterLoc("closest_distance_ladle", "DI:", "距离: ");
            RegisterLoc("closest_angle_ladle", "AN:", "旋转: ");
            RegisterLoc("position_distance", "DI: ", "距离: ");
            RegisterLoc("position_rotation", "RO: ", "旋转: ");
            RegisterLoc("position_angle", "AN: ", "方向: ");
            RegisterLoc("position_salt", "SA: ", "盐量: ");
            RegisterLoc("deviation_general", "GE: ", "总体: ");
            RegisterLoc("deviation_distance", "DI: ", "距离: ");
            RegisterLoc("deviation_rotation", "RO: ", "旋转: ");
            RegisterLoc("vortex_direction", "DI: ", "方向: ");
            RegisterLoc("vortex_angle", "AN: ", "夹角: ");
            RegisterLoc("vortex_dist", "DI: ", "距离: ");
            RegisterLoc("vortex_edge", "ED: ", "边缘: ");
            RegisterLoc("health_status", "HP: ", "血量: ");
        }
        public static void SpawnMessageText(string msg)
        {
            var cursorPosition = Managers.Cursor.cursor.transform.position;
            var commonAtlasName = Settings<TMPManagerSettings>.Asset.CommonAtlasName;
            var formattedText = string.Format("<voffset=0.085em><size=81%><sprite=\"{1}\" name=\"SpeechBubble ExclamationMark Icon\"></size>\u202f{0}", msg, commonAtlasName);
            var textContent = new CollectedFloatingText.FloatingTextContent(formattedText, CollectedFloatingText.FloatingTextContent.Type.Text, 0f);
            var assetValue = typeof(Settings<IngredientManagerSettings>).GetProperty("Asset", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            var collectedFloatingTextField = assetValue.GetType().GetProperty("CollectedFloatingText", BindingFlags.NonPublic | BindingFlags.Instance);
            var collectedFloatingText = collectedFloatingTextField.GetValue(assetValue) as CollectedFloatingText;
            CollectedFloatingText.SpawnNewText(collectedFloatingText.gameObject, cursorPosition, new[] { textContent }, Managers.Game.Cam.transform, false, false);
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
        public static Vector2 StringToVector2(string input)
        {
            var values = input.Trim('(', ')', ' ').Split(',');
            return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
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
