using System.Reflection;
using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ManagersSystem.TMP;
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

            RegisterLoc("grind_progress", "Progress: ", "进度: ");

            RegisterLoc("stir_stage", "Stage: ", "阶段: ");
            RegisterLoc("stir_progress", "Progress: ", "进度: ");
            RegisterLoc("stir_direction", "Direction: ", "方向: ");
            RegisterLoc("stir_zone", "Zone: ", "区域: ");

            RegisterLoc("closest_potion_path", "Potion:", "目标: ");
            RegisterLoc("closest_general_path", "General:", "总体: ");
            RegisterLoc("closest_distance_path", "Distance:", "距离: ");
            RegisterLoc("closest_rotation_path", "Rotation:", "旋转: ");

            RegisterLoc("closest_potion_ladle", "Potion:", "目标: ");
            RegisterLoc("closest_general_ladle", "General:", "总体: ");
            RegisterLoc("closest_distance_ladle", "Distance:", "距离: ");
            RegisterLoc("closest_rotation_ladle", "Rotation:", "旋转: ");

            RegisterLoc("position_distance", "Distance: ", "距离: ");
            RegisterLoc("position_position", "Position: ", "坐标: ");
            RegisterLoc("position_rotation", "Rotation: ", "旋转: ");
            RegisterLoc("position_salt", "Salt: ", "盐量: ");

            RegisterLoc("deviation_general", "General: ", "总体: ");
            RegisterLoc("deviation_distance", "Distance: ", "距离: ");
            RegisterLoc("deviation_rotation", "Rotation: ", "旋转: ");

            RegisterLoc("vortex_direction", "Direction: ", "方向: ");
            RegisterLoc("vortex_angle", "Angle: ", "夹角: ");
            RegisterLoc("vortex_distance", "Distance: ", "距离: ");
            RegisterLoc("vortex_edge", "Edge: ", "边缘: ");

            RegisterLoc("health_status", "Health: ", "血量: ");

            RegisterLoc("aopen", "is opened", "已开启");
            RegisterLoc("aclose", "is closed", "已关闭");

            RegisterLoc("aline", "Indicator line", "指示线");
            RegisterLoc("avortex", "Vortex approaching", "漩涡贴边");
            RegisterLoc("aend", "End distance mode", "终点距离模式");
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
        public static Vector2 StringToVector2(string input)
        {
            var values = input.Trim('(', ')', ' ').Split(',');
            if (values.Length < 2)
                return Vector2.zero;
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
