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
            RegisterLoc("dialog_target_status", "Target Status", "目标信息");
            RegisterLoc("dialog_zone_status", "Zone Status", "区域信息");
            RegisterLoc("dialog_tooltip", "Tooltips", "键位表");
            RegisterLoc("grind_progress", "Progress: ", "研磨进度: ");
            RegisterLoc("health_status", "Health: ", "当前血量: ");
            RegisterLoc("rotation_salt", "Rotation: ", "旋转盐量: ");
            RegisterLoc("vortex_angle", "Angle: ", "当前夹角: ");
            RegisterLoc("vortex_anglmax", "Max Angle: ", "最大夹角: ");
            RegisterLoc("vortex_distance", "Distance: ", "当前距离: ");
            RegisterLoc("vortex_distmax", "Max Distance: ", "最大距离: ");
            RegisterLoc("stir_stage", "Stir Stage: ", "搅拌阶段: ");
            RegisterLoc("stir_progress", "Stir Progress: ", "阶段进度: ");
            RegisterLoc("stir_direction", "Stir Direction: ", "搅拌方向: ");
            RegisterLoc("ladle_direction", "Ladle Direction: ", "加水方向: ");
            RegisterLoc("zone_swamp", "Swamp: ", "沼泽总长: ");
            RegisterLoc("zone_strong", "Strong Danger: ", "骷髅总长: ");
            RegisterLoc("zone_weak", "Weak Danger: ", "碎骨总长: ");
            RegisterLoc("zone_heal", "Heal: ", "回复总长: ");
            RegisterLoc("target_id", "Target: ", "目标效果: ");
            RegisterLoc("deviation_general", "General: ", "总体偏离: ");
            RegisterLoc("deviation_position", "Position: ", "位置偏离: ");
            RegisterLoc("deviation_rotation", "Rotation: ", "旋转偏离: ");
            RegisterLoc("direction", "Direction: ", "目标方向: ");
            RegisterLoc("angle_path", "Path Angle: ", "路径夹角: ");
            RegisterLoc("angle_ladle", "Ladle Angle: ", "加水夹角: ");
            RegisterLoc("aline", "Stir indicator mode ", "搅拌示线模式");
            RegisterLoc("avortex", "Vortex auto-stop ", "漩涡制动");
            RegisterLoc("aend", "Boundary mode ", "末端距离模式");
            RegisterLoc("axoy", "Cartesian mode ", "直角坐标模式");
            RegisterLoc("aopen", "is opened", "已开启");
            RegisterLoc("aclose", "is closed", "已关闭");
            RegisterLoc("tooltip", "Deceleration & Batch Brewing: Z / X\nSet Value & Target Selection: Right Click\nStir / Ladle Indicator: /\nProximity / Stir Boundary: \\\nCartesian / Polar Mode: Spacebar\nVortex Auto-stop: '", "减速操作 & 批量制作: Z / X\n定量操作 & 目标选择: 右键\n搅拌 / 加水方向示线: /\n最近 / 搅拌末端距离: \\\n直角 / 极坐标: 空格\n漩涡制动: '");
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
            window.transform.localScale *= AlchAss.windowScale.Value;
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
