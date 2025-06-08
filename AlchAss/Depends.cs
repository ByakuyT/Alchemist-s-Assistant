using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.Settings;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AlchAss
{
    public static class Depends
    {
        #region 本地适配
        public static void SetModLocalization()
        {
            RegisterLoc("dialog_grind_status", "Grinding", "研磨信息");
            RegisterLoc("dialog_stir_status", "Stirring", "搅拌信息");
            RegisterLoc("dialog_path_status", "Path", "路径信息");
            RegisterLoc("dialog_ladle_status", "Ladling", "加水信息");
            RegisterLoc("dialog_position_status", "Position", "位置信息");
            RegisterLoc("dialog_deviation_status", "Deviation", "偏离信息");
            RegisterLoc("dialog_vortex_status", "Vortex", "漩涡信息");
            RegisterLoc("dialog_health_status", "Health", "血量信息");
            RegisterLoc("dialog_target_status", "Target", "目标信息");
            RegisterLoc("dialog_zone_status", "Zones", "区域信息");
            RegisterLoc("dialog_tooltip", "Controls", "键位表");

            RegisterLoc("grind_progress", "Progress: ", "研磨进度: ");
            RegisterLoc("health_status", "Health: ", "当前血量: ");
            RegisterLoc("rotation_salt", "Rotation: ", "旋转盐量: ");
            RegisterLoc("vortex_angle", "Angle: ", "当前夹角: ");
            RegisterLoc("vortex_anglmax", "Max Angle: ", "最大夹角: ");
            RegisterLoc("vortex_distance", "Distance: ", "当前距离: ");
            RegisterLoc("vortex_distmax", "Max Distance: ", "最大距离: ");
            RegisterLoc("stir_stage", "Stage: ", "搅拌阶段: ");
            RegisterLoc("stir_progress", "Progress: ", "阶段进度: ");
            RegisterLoc("stir_direction", "Stir Dir: ", "搅拌方向: ");
            RegisterLoc("ladle_direction", "Ladle Dir: ", "加水方向: ");
            RegisterLoc("target_id", "Target: ", "目标效果: ");
            RegisterLoc("deviation_general", "Overall: ", "总体偏离: ");
            RegisterLoc("deviation_position", "Position: ", "位置偏离: ");
            RegisterLoc("deviation_rotation", "Rotation: ", "旋转偏离: ");
            RegisterLoc("direction", "Direction: ", "目标方向: ");
            RegisterLoc("direction_close", "Closest Dir: ", "近点方向: ");
            RegisterLoc("angle_path", "Path Angle: ", "路径夹角: ");
            RegisterLoc("angle_ladle", "Ladle Angle: ", "加水夹角: ");
            RegisterLoc("zone", "Zone: ", "显示区域: ");

            RegisterLoc("aline", "Auxiliary Renderer ", "辅助渲染器");
            RegisterLoc("apath", "Path Rendering ", "路径渲染");
            RegisterLoc("acircle", "Rotation Effect ", "旋转影响等级范围");
            RegisterLoc("aend", "Boundary mode ", "末端距离模式");
            RegisterLoc("axoy", "Cartesian mode ", "直角坐标模式");
            RegisterLoc("aopen", "enabled", "已开启");
            RegisterLoc("aclose", "disabled", "已关闭");
            RegisterLoc("azone", "Zone switched to: ", "显示区域已切换至: ");
            RegisterLoc("swamp", "Swamp", "沼泽");
            RegisterLoc("sdanger", "Skull", "骷髅");
            RegisterLoc("wdanger", "Bone", "碎骨");
            RegisterLoc("heal", "Healing", "回复");

            RegisterLoc("tooltip", Variables.englishTooltip, Variables.chineseTooltip);
        }
        public static void RegisterLoc(string localizationKey, string englishText, string chineseText)
        {
            var localizationData = AccessTools.StaticFieldRefAccess<LocalizationData>(typeof(LocalizationManager), "localizationData");
            for (var localeIndex = 0; localeIndex <= 13; ++localeIndex)
            {
                var localizedText = localeIndex == 9 ? chineseText : englishText;
                localizationData.Add(localeIndex, localizationKey, localizedText);
            }
        }
        #endregion

        #region 加载配置
        public static void LoadWindowPositions()
        {
            var windowConfigPath = Path.Combine(Variables.ConfigDirectory, Variables.windowPath);
            if (!File.Exists(windowConfigPath))
            {
                File.WriteAllText(windowConfigPath, string.Empty);
                return;
            }
            var lines = File.ReadAllLines(windowConfigPath);
            if (lines.Length <= 10)
                return;
            Variables.positionGrindDebugWindow = StringToVector2(lines[0]);
            Variables.positionHealthDebugWindow = StringToVector2(lines[1]);
            Variables.positionVortexDebugWindow = StringToVector2(lines[2]);
            Variables.positionStirDebugWindow = StringToVector2(lines[3]);
            Variables.positionPositionDebugWindow = StringToVector2(lines[4]);
            Variables.positionDeviationDebugWindow = StringToVector2(lines[5]);
            Variables.positionPathDebugWindow = StringToVector2(lines[6]);
            Variables.positionLadleDebugWindow = StringToVector2(lines[7]);
            Variables.positionTargetDebugWindow = StringToVector2(lines[8]);
            Variables.positionTooltipDebugWindow = StringToVector2(lines[9]);
            Variables.positionZoneDebugWindow = StringToVector2(lines[10]);
        }
        #endregion

        #region 工具方法
        public static void SpawnMessageText(string message)
        {
            var cursorPosition = Managers.Cursor.cursor.transform.position;
            var commonAtlasName = Settings<TMPManagerSettings>.Asset.CommonAtlasName;
            var formattedText = string.Format("<voffset=0.085em><size=81%><sprite=\"{1}\" name=\"SpeechBubble ExclamationMark Icon\"></size>\u202f{0}", message, commonAtlasName);
            var textContent = new CollectedFloatingText.FloatingTextContent(formattedText, CollectedFloatingText.FloatingTextContent.Type.Text, 0f);
            var ingredientManagerAsset = typeof(Settings<IngredientManagerSettings>).GetProperty("Asset", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            var collectedFloatingTextField = ingredientManagerAsset.GetType().GetProperty("CollectedFloatingText", BindingFlags.NonPublic | BindingFlags.Instance);
            var collectedFloatingText = collectedFloatingTextField.GetValue(ingredientManagerAsset) as CollectedFloatingText;
            CollectedFloatingText.SpawnNewText(collectedFloatingText.gameObject, cursorPosition, [textContent], Managers.Game.Cam.transform, false, false);
        }
        public static DebugWindow CreateDebugWindow(string windowName, Vector2 windowPosition)
        {
            if (Variables.lab == null)
                return null;
            var debugWindow = DebugWindow.Init(LocalizationManager.GetText(windowName), true);
            Variables.foreground_queue.Add(debugWindow);
            debugWindow.ToForeground();
            debugWindow.transform.SetParent(Variables.lab.transform, false);
            debugWindow.transform.localPosition = windowPosition;
            debugWindow.transform.localScale *= Variables.windowScale.Value;
            return debugWindow;
        }
        public static Vector2 StringToVector2(string vectorString)
        {
            var coordinateValues = vectorString.Trim('(', ')', ' ').Split(',');
            if (coordinateValues.Length < 2)
                return Vector2.zero;
            return new Vector2(float.Parse(coordinateValues[0]), float.Parse(coordinateValues[1]));
        }
        public static Color ParseColor(string colorString)
        {
            string[] parts = colorString.Split(',');
            return new Color(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()), float.Parse(parts[3].Trim()));
        }
        public static string FormatRotation(float rotation)
        {
            return rotation > 0f
                ? $"<sprite=\"IconsAtlas\" name=\"SunSalt\"> {rotation}"
                : $"<sprite=\"IconsAtlas\" name=\"MoonSalt\"> {-rotation}";
        }
        public static string FormatPosition(Vector2 position)
        {
            return Variables.xOy
                ? $"x: {position.x}\ny: {position.y}"
                : $"r: {position.magnitude}\nθ: {Vector2.SignedAngle(Vector2.right, position)}";
        }
        public static List<Vector2> GetLineCircleIntersections(Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float radius)
        {
            var intersections = new List<Vector2>();
            Vector2 d = lineEnd - lineStart;
            Vector2 f = lineStart - circleCenter;
            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - radius * radius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return intersections;
            float sqrt_discriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b - sqrt_discriminant) / (2 * a);
            float t2 = (-b + sqrt_discriminant) / (2 * a);
            if (t1 >= 0 && t1 <= 1)
                intersections.Add(lineStart + t1 * d);
            if (t2 >= 0 && t2 <= 1)
                intersections.Add(lineStart + t2 * d);
            return intersections;
        }
        public static void LoadOrScanVortexData()
        {
            var currentMap = Managers.RecipeMap.currentMap;
            if (currentMap?.referencesContainer == null)
                return;
            var mapId = currentMap.potionBase.name;
            var vortexDataPath = Path.Combine(Variables.ConfigDirectory, $"AlchAssVortexData_{mapId}.txt");
            if (Variables.allVortexData.Count > 0 && Variables.currentMapName == mapId)
                return;
            Variables.allVortexData.Clear();
            Variables.currentMapName = mapId;
            if (File.Exists(vortexDataPath))
            {
                var lines = File.ReadAllLines(vortexDataPath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var centerX = float.Parse(parts[0]);
                        var centerY = float.Parse(parts[1]);
                        var radius = float.Parse(parts[2]);
                        Variables.allVortexData.Add(new Variables.VortexData(new Vector2(centerX, centerY), radius));
                    }
                }
                if (Variables.allVortexData.Count > 0)
                {
                    SortVortexData();
                    return;
                }
            }
            ScanAllVortexData();
            if (Variables.allVortexData.Count > 0)
            {
                SortVortexData();
                SaveVortexData(mapId);
            }
        }
        public static void SortVortexData()
        {
            Variables.allVortexData.Sort((a, b) =>
            {
                float distanceA = a.center.magnitude;
                float distanceB = b.center.magnitude;
                return distanceA.CompareTo(distanceB);
            });
            Variables.selectedVortexIndex = -1;
        }
        public static void ScanAllVortexData()
        {
            var currentMap = Managers.RecipeMap.currentMap;
            if (currentMap?.referencesContainer == null)
                return;
            var allVortexItems = UnityEngine.Object.FindObjectsByType<VortexMapItem>(FindObjectsSortMode.None);
            var currentMapTransform = currentMap.referencesContainer.transform;
            foreach (var vortex in allVortexItems)
                if (vortex.transform.IsChildOf(currentMapTransform))
                {
                    var center = vortex.thisTransform.localPosition;
                    var vortexCollider = Traverse.Create(vortex).Field("vortexCollider").GetValue() as CircleCollider2D;
                    var radius = vortexCollider.radius + Variables.PotionBottleRadius;
                    Variables.allVortexData.Add(new Variables.VortexData(center, radius));
                }
        }
        public static void SaveVortexData(string mapId)
        {
            var vortexDataPath = Path.Combine(Variables.ConfigDirectory, $"AlchAssVortexData_{mapId}.txt");
            Directory.CreateDirectory(Variables.ConfigDirectory);
            var lines = new List<string>();
            foreach (var vortex in Variables.allVortexData)
                lines.Add($"{vortex.center.x},{vortex.center.y},{vortex.radius}");
            File.WriteAllLines(vortexDataPath, lines);
        }
        #endregion

        #region 渲染辅助
        public static void HideOriginalPaths()
        {
            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            foreach (var hint in pathHints)
                if (hint != null)
                {
                    bool isTeleportation = hint.GetType().Name == "TeleportationFixedHint";
                    if (!isTeleportation)
                    {
                        var allRenderers = hint.GetComponentsInChildren<Renderer>(true);
                        foreach (var renderer in allRenderers)
                            renderer.enabled = false;
                        hint.gameObject.SetActive(false);
                    }
                    else
                    {
                        var allRenderers = hint.GetComponentsInChildren<Renderer>(true);
                        foreach (var renderer in allRenderers)
                            renderer.enabled = false;
                    }
                }
            if (Managers.RecipeMap.path.currentPathHint != null)
            {
                bool isCurrentTeleportation = Managers.RecipeMap.path.currentPathHint.GetType().Name == "TeleportationFixedHint";
                if (!isCurrentTeleportation)
                {
                    var allCurrentRenderers = Managers.RecipeMap.path.currentPathHint.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in allCurrentRenderers)
                        renderer.enabled = false;
                    Managers.RecipeMap.path.currentPathHint.gameObject.SetActive(false);
                }
                else
                {
                    var allCurrentRenderers = Managers.RecipeMap.path.currentPathHint.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in allCurrentRenderers)
                        renderer.enabled = false;
                }
            }
        }
        public static void ShowOriginalPaths()
        {
            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            foreach (var hint in pathHints)
                if (hint != null)
                {
                    hint.gameObject.SetActive(true);
                    var allRenderers = hint.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in allRenderers)
                        renderer.enabled = true;
                }
            if (Managers.RecipeMap.path.currentPathHint != null)
            {
                Managers.RecipeMap.path.currentPathHint.gameObject.SetActive(true);
                var allCurrentRenderers = Managers.RecipeMap.path.currentPathHint.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in allCurrentRenderers)
                    renderer.enabled = true;
            }
            if (Managers.RecipeMap.path.fixedPathHintsContainer != null)
            {
                var containerRenderers = Managers.RecipeMap.path.fixedPathHintsContainer.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in containerRenderers)
                    renderer.enabled = true;
            }
            if (Managers.RecipeMap.path.pathHintsContainer != null)
            {
                var pathContainerRenderers = Managers.RecipeMap.path.pathHintsContainer.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in pathContainerRenderers)
                    renderer.enabled = true;
            }
        }
        public static void SetPotionTranslucent(bool translucent)
        {
            var indicatorMapItems = UnityEngine.Object.FindObjectsByType<IndicatorMapItem>(FindObjectsSortMode.None);
            foreach (var indicatorMapItem in indicatorMapItems)
            {
                if (translucent)
                {
                    indicatorMapItem.backgroundSpriteRenderer.enabled = false;
                    var liquidAnimator = Traverse.Create(indicatorMapItem).Field("liquidColorChangeAnimator").GetValue();
                    if (liquidAnimator != null)
                    {
                        var upperContainer = Traverse.Create(liquidAnimator).Field("upperContainer").GetValue();
                        var lowerContainer = Traverse.Create(liquidAnimator).Field("lowerContainer").GetValue();
                        if (upperContainer != null)
                            Traverse.Create(upperContainer).Method("SetAlpha", 0f).GetValue();
                        if (lowerContainer != null)
                            Traverse.Create(lowerContainer).Method("SetAlpha", 0f).GetValue();
                    }
                }
                else
                {
                    indicatorMapItem.backgroundSpriteRenderer.enabled = true;
                    var liquidAnimator = Traverse.Create(indicatorMapItem).Field("liquidColorChangeAnimator").GetValue();
                    if (liquidAnimator != null)
                    {
                        var upperContainer = Traverse.Create(liquidAnimator).Field("upperContainer").GetValue();
                        var lowerContainer = Traverse.Create(liquidAnimator).Field("lowerContainer").GetValue();
                        if (upperContainer != null)
                            Traverse.Create(upperContainer).Method("SetAlpha", 1f).GetValue();
                        if (lowerContainer != null)
                            Traverse.Create(lowerContainer).Method("SetAlpha", 1f).GetValue();
                    }
                }
            }
        }
        #endregion
    }
}
