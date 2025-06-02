using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased.RecipeMap.Path;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.Settings;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

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

            RegisterLoc("aline", "Stir indicator ", "搅拌示线");
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
            CollectedFloatingText.SpawnNewText(collectedFloatingText.gameObject, cursorPosition, new[] { textContent }, Managers.Game.Cam.transform, false, false);
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
        #endregion

        #region 示线渲染
        public static void InitializeLineRenderers(SolventDirectionHint instance, SpriteRenderer spriteRenderer)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Variables.lineRenderer[i] == null)
                {
                    var newLine = new GameObject("SecondLine") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newLine.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.lineColor[i], new Vector2(200f, 0.075f));
                }
            }
            for (int i = 3; i < 9; i++)
            {
                if (Variables.lineRenderer[i] == null)
                {
                    var newNode = new GameObject("Node") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newNode.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.lineColor[i], new Vector2(0.2f, 0.2f));
                    Variables.lineRenderer[i].transform.localEulerAngles = Vector3.zero;
                }
            }
        }
        public static void SetupSpriteRenderer(SpriteRenderer renderer, SpriteRenderer referenceRenderer, Color color, Vector2 size)
        {
            renderer.sortingLayerName = referenceRenderer.sortingLayerName;
            renderer.sortingOrder = referenceRenderer.sortingOrder;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.sprite = Sprite.Create(Variables.texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
            renderer.color = color;
            renderer.size = size;
        }
        public static void HandleDirectionLineDisplay(SolventDirectionHint instance, SpriteRenderer spriteRenderer)
        {
            if (Variables.directionLine)
            {
                Functions.UpdateLineDirections();
                if (Keyboard.current.slashKey.wasPressedThisFrame)
                {
                    spriteRenderer.enabled = false;
                    for (int i = 0; i < 3; i++)
                        Variables.lineRenderer[i].enabled = true;
                }
                var instancePosition = instance.transform.position;
                var indicatorLocalPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                for (int i = 0; i < 3; i++)
                {
                    Variables.lineRenderer[i].transform.position = instancePosition;
                    Variables.lineRenderer[i].transform.localEulerAngles = new Vector3(0f, 0f, Variables.lineDirection[i]);
                }
                for (int i = 3; i < 9; i++)
                {
                    Vector3? pointPosition = null;
                    if (i < 7)
                    {
                        var zoneIndex = i - 3;
                        if (Variables.zonePoints[zoneIndex, 0] != null)
                            pointPosition = (Vector3)Variables.zonePoints[zoneIndex, 0];
                    }
                    else
                    {
                        var closestIndex = i - 7;
                        if (Variables.closestPoints[closestIndex].HasValue)
                            pointPosition = (Vector3)Variables.closestPoints[closestIndex].Value;
                    }
                    Variables.lineRenderer[i].enabled = pointPosition.HasValue;
                    if (pointPosition.HasValue)
                    {
                        var adjustedPosition = pointPosition.Value;
                        adjustedPosition.y = adjustedPosition.y + instancePosition.y - indicatorLocalPosition.y;
                        Variables.lineRenderer[i].transform.position = adjustedPosition;
                    }
                }
            }
            else if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                spriteRenderer.enabled = true;
                for (int i = 0; i < 9; i++)
                    Variables.lineRenderer[i].enabled = false;
            }
        }
        #endregion
    }
}
