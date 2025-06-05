using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.Settings;
using System;
using System.Collections.Generic;
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
            Variables.selectedVortexIndex = 0;
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

        #region 辅助渲染
        public static void InitializeLineRenderers(SolventDirectionHint instance, SpriteRenderer spriteRenderer)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Variables.lineRenderer[i] == null)
                {
                    var newLine = new GameObject("Line") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newLine.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.lineColor[i], new Vector2(200f, Variables.lineWidth.Value));
                }
            }
            for (int i = 4; i < 10; i++)
            {
                if (Variables.lineRenderer[i] == null)
                {
                    var newNode = new GameObject("Node") { layer = instance.gameObject.layer };
                    Variables.lineRenderer[i] = newNode.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(Variables.lineRenderer[i], spriteRenderer, Variables.lineColor[i], new Vector2(Variables.pointSize.Value, Variables.pointSize.Value));
                    Variables.lineRenderer[i].sortingOrder += 3;
                }
            }
        }
        public static void HandleDirectionLineDisplay(SolventDirectionHint instance, SpriteRenderer spriteRenderer)
        {
            if (Variables.currentMapName != null && Variables.currentMapName != Managers.RecipeMap.currentMap.potionBase.name)
            {
                Variables.allVortexData.Clear();
                Variables.currentMapName = null;
                Variables.hoveredItemName = null;
                if (Variables.directionLine)
                    LoadOrScanVortexData();
            }
            if (Variables.directionLine)
            {
                Functions.UpdateLineDirections();
                if (Keyboard.current.slashKey.wasPressedThisFrame)
                {
                    spriteRenderer.enabled = false;
                    for (int i = 0; i < 10; i++)
                        Variables.lineRenderer[i].enabled = true;
                    Variables.lastPathHintCount = Managers.RecipeMap.path.fixedPathHints.Count;
                    Variables.targetCircleNeedsUpdate = true;
                    if (Variables.enablePathRendering)
                        HideOriginalPaths();
                    SetPotionTranslucent(true);
                }
                var instancePosition = instance.transform.position;
                var indicatorLocalPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                for (int i = 0; i < 4; i++)
                {
                    Variables.lineRenderer[i].transform.position = instancePosition;
                    Variables.lineRenderer[i].transform.localEulerAngles = new Vector3(0f, 0f, Variables.lineDirection[i]);
                }
                for (int i = 4; i < 10; i++)
                {
                    Vector3? pointPosition = null;
                    if (i < 8)
                    {
                        var zoneIndex = i - 4;
                        if (Variables.zonePoints[zoneIndex, 0] != null)
                            pointPosition = (Vector3)Variables.zonePoints[zoneIndex, 0];
                    }
                    else
                    {
                        var closestIndex = i - 8;
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
                HandleVortexCircleDisplay(instancePosition, indicatorLocalPosition);
                if (Variables.enablePathRendering)
                    HandleIngredientPathDisplay(instancePosition, indicatorLocalPosition);
                HandleTargetEffectCircleDisplay(instancePosition, indicatorLocalPosition);
            }
            else if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                spriteRenderer.enabled = true;
                for (int i = 0; i < 10; i++)
                    Variables.lineRenderer[i].enabled = false;
                for (int i = 0; i < Variables.vortexCircleRenderer.Length; i++)
                    Variables.vortexCircleRenderer[i].enabled = false;
                for (int i = 0; i < Variables.vortexIntersectionRenderer.Length; i++)
                    Variables.vortexIntersectionRenderer[i].enabled = false;
                for (int i = 0; i < Variables.pathLineRenderer.Length; i++)
                    Variables.pathLineRenderer[i].enabled = false;
                for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
                    if (Variables.targetEffectCircleRenderer[i] != null)
                        Variables.targetEffectCircleRenderer[i].enabled = false;
                Variables.lastPathHintCount = 0;
                ShowOriginalPaths();
                SetPotionTranslucent(false);
            }
        }
        public static void HandleVortexCircleDisplay(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            int requiredCount = Variables.allVortexData.Count;
            if (Variables.vortexCircleRenderer.Length < requiredCount)
            {
                var oldRenderers = Variables.vortexCircleRenderer;
                Variables.vortexCircleRenderer = new SpriteRenderer[requiredCount];
                for (int i = 0; i < oldRenderers.Length; i++)
                    Variables.vortexCircleRenderer[i] = oldRenderers[i];
                for (int i = oldRenderers.Length; i < requiredCount; i++)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var vortexCircle = new GameObject("VortexCircle") { layer = layer };
                    var renderer = vortexCircle.AddComponent<SpriteRenderer>();
                    renderer.sortingLayerName = Variables.lineRenderer[0].sortingLayerName;
                    renderer.sortingOrder = Variables.lineRenderer[0].sortingOrder + 1;
                    renderer.color = Variables.lineColor[11];
                    Variables.vortexCircleRenderer[i] = renderer;
                    renderer.enabled = false;
                }
            }
            for (int i = 0; i < Variables.vortexCircleRenderer.Length; i++)
            {
                var renderer = Variables.vortexCircleRenderer[i];
                if (i < Variables.allVortexData.Count)
                {
                    if (i == Variables.selectedVortexIndex)
                    {
                        var vortexData = Variables.allVortexData[i];
                        renderer.enabled = true;
                        renderer.transform.position = new Vector3(
                            vortexData.center.x,
                            vortexData.center.y + instancePosition.y - indicatorLocalPosition.y,
                            instancePosition.z);
                        renderer.sprite = GetVortexSpriteByRadius(vortexData.radius);
                        renderer.transform.localScale = Vector3.one;
                        renderer.color = Variables.lineColor[11];
                    }
                    else
                        renderer.enabled = false;
                }
                else
                    renderer.enabled = false;
            }
            DisplayVortexIntersectionPoints(instancePosition, indicatorLocalPosition);
        }
        public static void HandleIngredientPathDisplay(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            var currentPathHintCount = Managers.RecipeMap.path.fixedPathHints.Count;
            if (currentPathHintCount != Variables.lastPathHintCount)
            {
                if (Variables.enablePathRendering)
                    HideOriginalPaths();
                Variables.lastPathHintCount = currentPathHintCount;
            }
            if (!Variables.enablePathRendering)
            {
                for (int i = 0; i < Variables.pathLineRenderer.Length; i++)
                    Variables.pathLineRenderer[i].enabled = false;
                return;
            }
            if (Variables.ingredientPathGroups.Count == 0)
            {
                for (int i = 0; i < Variables.pathLineRenderer.Length; i++)
                    Variables.pathLineRenderer[i].enabled = false;
                return;
            }
            int totalLineSegments = 0;
            foreach (var group in Variables.ingredientPathGroups)
            {
                totalLineSegments += group.normalSegments.Sum(segment => segment.Length - 1);
                totalLineSegments += group.teleportationSegments.Count;
            }
            if (Variables.pathLineRenderer.Length < totalLineSegments)
            {
                var oldRenderers = Variables.pathLineRenderer;
                Variables.pathLineRenderer = new SpriteRenderer[totalLineSegments];
                Array.Copy(oldRenderers, Variables.pathLineRenderer, oldRenderers.Length);
                for (int i = oldRenderers.Length; i < totalLineSegments; i++)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var pathLine = new GameObject("IngredientPathLine") { layer = layer };
                    var renderer = pathLine.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(renderer, Variables.lineRenderer[0], Color.white, new Vector2(1f, Variables.lineWidth.Value));
                    renderer.sortingOrder = Variables.lineRenderer[0].sortingOrder + 1;
                    Variables.pathLineRenderer[i] = renderer;
                    renderer.enabled = false;
                }
            }
            int rendererIndex = 0;
            var mapTransform = Managers.RecipeMap.currentMap.referencesContainer.transform;
            var pathTransform = Managers.RecipeMap.path.thisTransform;
            foreach (var group in Variables.ingredientPathGroups)
            {
                foreach (var segment in group.normalSegments)
                    for (int pointIdx = 0; pointIdx < segment.Length - 1 && rendererIndex < Variables.pathLineRenderer.Length; pointIdx++)
                    {
                        var renderer = Variables.pathLineRenderer[rendererIndex];
                        var startPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[pointIdx]));
                        var endPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[pointIdx + 1]));
                        renderer.enabled = true;
                        renderer.color = group.pathColor;
                        var midPoint = (startPoint + endPoint) * 0.5f;
                        var direction = endPoint - startPoint;
                        renderer.transform.position = new Vector3(
                            midPoint.x,
                            midPoint.y + instancePosition.y - indicatorLocalPosition.y,
                            instancePosition.z);
                        renderer.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
                        renderer.size = new Vector2(direction.magnitude, Variables.lineWidth.Value);
                        SetupSolidLine(renderer);
                        rendererIndex++;
                    }
                foreach (var segment in group.teleportationSegments)
                    if (segment.Length >= 2 && rendererIndex < Variables.pathLineRenderer.Length)
                    {
                        var renderer = Variables.pathLineRenderer[rendererIndex];
                        var startPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[0]));
                        var endPoint = mapTransform.InverseTransformPoint(pathTransform.TransformPoint(segment[1]));
                        renderer.enabled = true;
                        renderer.color = group.pathColor;
                        var midPoint = (startPoint + endPoint) * 0.5f;
                        var direction = endPoint - startPoint;
                        renderer.transform.position = new Vector3(
                            midPoint.x,
                            midPoint.y + instancePosition.y - indicatorLocalPosition.y,
                            instancePosition.z);
                        renderer.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
                        renderer.size = new Vector2(direction.magnitude, Variables.lineWidth.Value);
                        SetupDashedLine(renderer, direction.magnitude);
                        rendererIndex++;
                    }
            }
            for (int i = rendererIndex; i < Variables.pathLineRenderer.Length; i++)
                Variables.pathLineRenderer[i].enabled = false;
        }
        public static void HandleTargetEffectCircleDisplay(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            Variables.SharedCache.UpdateCache();
            if (!Variables.SharedCache.isValid)
            {
                for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
                    if (Variables.targetEffectCircleRenderer[i] != null)
                        Variables.targetEffectCircleRenderer[i].enabled = false;
                return;
            }
            if (!Variables.targetCircleNeedsUpdate)
                return;
            float[] adjustedRadiusValues = new float[3];
            if (Variables.useAngleAdjustedRadius)
            {
                var angleDelta = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Variables.SharedCache.targetEffect.transform.localEulerAngles.z));
                adjustedRadiusValues[0] = 1.53f;
                adjustedRadiusValues[1] = 1f / 3f - angleDelta / 216f;
                adjustedRadiusValues[2] = 1f / 18f - angleDelta / 216f;
            }
            else
            {
                adjustedRadiusValues[0] = 1.53f;
                adjustedRadiusValues[1] = 1f / 3f;
                adjustedRadiusValues[2] = 1f / 18f;
            }
            Variables.targetCircleNeedsUpdate = false;
            for (int i = 0; i < Variables.targetEffectCircleRenderer.Length; i++)
            {
                if (Variables.targetEffectCircleRenderer[i] == null)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var targetCircle = new GameObject($"TargetEffectCircle_L{3 - i}") { layer = layer };
                    var renderer = targetCircle.AddComponent<SpriteRenderer>();
                    renderer.sortingLayerName = Variables.lineRenderer[0].sortingLayerName;
                    renderer.sortingOrder = Variables.lineRenderer[0].sortingOrder + 1;
                    renderer.color = Variables.lineColor[11];
                    Variables.targetEffectCircleRenderer[i] = renderer;
                    renderer.enabled = false;
                }
                var targetRenderer = Variables.targetEffectCircleRenderer[i];
                if (adjustedRadiusValues[i] < 0f)
                {
                    targetRenderer.enabled = false;
                    continue;
                }
                targetRenderer.enabled = true;
                targetRenderer.transform.position = new Vector3(
                    Variables.SharedCache.targetPosition.x,
                    Variables.SharedCache.targetPosition.y + instancePosition.y - indicatorLocalPosition.y,
                    instancePosition.z);
                Variables.targetEffectCircleSprite[i] = CreateCircleSpriteForRadius(adjustedRadiusValues[i]);
                targetRenderer.sprite = Variables.targetEffectCircleSprite[i];
                targetRenderer.transform.localScale = Vector3.one;
                targetRenderer.color = Variables.lineColor[11];
            }
        }
        public static void DisplayVortexIntersectionPoints(Vector3 instancePosition, Vector2 indicatorLocalPosition)
        {
            int requiredCount = Variables.vortexIntersectionPoints.Count;
            if (Variables.vortexIntersectionRenderer.Length < requiredCount)
            {
                var oldRenderers = Variables.vortexIntersectionRenderer;
                Variables.vortexIntersectionRenderer = new SpriteRenderer[requiredCount];
                for (int i = 0; i < oldRenderers.Length; i++)
                    Variables.vortexIntersectionRenderer[i] = oldRenderers[i];
                for (int i = oldRenderers.Length; i < requiredCount; i++)
                {
                    var layer = Variables.solventDirectionHint?.gameObject.layer ?? 0;
                    var intersectionNode = new GameObject("VortexIntersection") { layer = layer };
                    var renderer = intersectionNode.AddComponent<SpriteRenderer>();
                    SetupSpriteRenderer(renderer, Variables.lineRenderer[3], Variables.lineColor[10], new Vector2(Variables.pointSize.Value, Variables.pointSize.Value));
                    renderer.sortingOrder = Variables.lineRenderer[3].sortingOrder + 2;
                    renderer.transform.localEulerAngles = Vector3.zero;
                    Variables.vortexIntersectionRenderer[i] = renderer;
                    renderer.enabled = false;
                }
            }
            for (int i = 0; i < Variables.vortexIntersectionRenderer.Length; i++)
            {
                var renderer = Variables.vortexIntersectionRenderer[i];
                if (i < Variables.vortexIntersectionPoints.Count)
                {
                    var intersectionPoint = Variables.vortexIntersectionPoints[i];
                    renderer.enabled = true;
                    renderer.transform.position = new Vector3(
                        intersectionPoint.x,
                        intersectionPoint.y + instancePosition.y - indicatorLocalPosition.y,
                        instancePosition.z);
                }
                else
                    renderer.enabled = false;
            }
        }
        public static Sprite GetVortexSpriteByRadius(float radius)
        {
            if (Mathf.Abs(radius - 1.74f) < 0.1f)
            {
                if (Variables.circleSprite[0] == null)
                    Variables.circleSprite[0] = CreateCircleSpriteForRadius(1.74f);
                return Variables.circleSprite[0];
            }
            else if (Mathf.Abs(radius - 1.99f) < 0.1f)
            {
                if (Variables.circleSprite[1] == null)
                    Variables.circleSprite[1] = CreateCircleSpriteForRadius(1.99f);
                return Variables.circleSprite[1];
            }
            else
            {
                if (Variables.circleSprite[2] == null)
                    Variables.circleSprite[2] = CreateCircleSpriteForRadius(2.39f);
                return Variables.circleSprite[2];
            }
        }
        public static Sprite CreateCircleSpriteForRadius(float radius)
        {
            float diameter = radius * 2f;
            int textureSize = 500;
            float lineThickness = Variables.lineWidth.Value;
            Texture2D circleTexture = new(textureSize, textureSize);
            Color[] pixels = new Color[textureSize * textureSize];
            Vector2 center = new(textureSize * 0.5f, textureSize * 0.5f);
            float outerRadius = textureSize * 0.5f;
            float lineThicknessInPixels = (lineThickness / diameter) * textureSize;
            float innerRadius = outerRadius - lineThicknessInPixels;
            for (int y = 0; y < textureSize; y++)
                for (int x = 0; x < textureSize; x++)
                {
                    Vector2 pos = new(x, y);
                    float distance = Vector2.Distance(pos, center);
                    if (distance < outerRadius && distance >= innerRadius)
                    {
                        float alpha = 1.0f;
                        if (distance > outerRadius - 1f)
                            alpha = outerRadius - distance;
                        else if (distance < innerRadius + 1f)
                            alpha = distance - innerRadius;
                        alpha = Mathf.Clamp01(alpha);
                        pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                        pixels[y * textureSize + x] = Color.clear;
                }
            circleTexture.SetPixels(pixels);
            circleTexture.Apply();
            circleTexture.filterMode = FilterMode.Bilinear;
            float pixelsPerUnit = textureSize / diameter;
            return Sprite.Create(circleTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
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
        public static void SetupSpriteRenderer(SpriteRenderer renderer, SpriteRenderer referenceRenderer, Color color, Vector2 size)
        {
            renderer.sortingLayerName = referenceRenderer.sortingLayerName;
            renderer.sortingOrder = referenceRenderer.sortingOrder;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.sprite = Sprite.Create(Variables.texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
            renderer.color = color;
            renderer.size = size;
        }
        public static void SetupSolidLine(SpriteRenderer renderer)
        {
            if (renderer.sprite == null || renderer.sprite.texture != Variables.texture)
                renderer.sprite = Sprite.Create(Variables.texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f, 0, SpriteMeshType.FullRect);
            renderer.drawMode = SpriteDrawMode.Tiled;
        }
        public static void SetupDashedLine(SpriteRenderer renderer, float lineLength)
        {
            if (Variables.dashedTexture == null)
                CreateDashedTexture();
            renderer.sprite = Variables.dashedSprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            var tilingX = lineLength / 0.3f;
            renderer.size = new Vector2(lineLength, Variables.lineWidth.Value);
        }
        public static void CreateDashedTexture()
        {
            int textureWidth = 12;
            int textureHeight = 1;
            Variables.dashedTexture = new Texture2D(textureWidth, textureHeight);
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int x = 0; x < textureWidth; x++)
                pixels[x] = x < 6 ? Color.white : Color.clear;
            Variables.dashedTexture.SetPixels(pixels);
            Variables.dashedTexture.Apply();
            Variables.dashedTexture.filterMode = FilterMode.Point;
            Variables.dashedTexture.wrapMode = TextureWrapMode.Repeat;
            Variables.dashedSprite = Sprite.Create(Variables.dashedTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), textureWidth / 0.3f, 0, SpriteMeshType.FullRect);
        }
        #endregion
    }
}
