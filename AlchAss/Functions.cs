using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.Path;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace AlchAss
{
    public static class Functions
    {
        #region 模式切换
        public static void EndMode()
        {
            if (Keyboard.current.backslashKey.wasPressedThisFrame)
            {
                Variables.endMode = !Variables.endMode;
                Depends.SpawnMessageText(LocalizationManager.GetText("aend") + LocalizationManager.GetText(Variables.endMode ? "aopen" : "aclose"));
            }
        }
        public static void PositionMode()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Variables.xOy = !Variables.xOy;
                Depends.SpawnMessageText(LocalizationManager.GetText("axoy") + LocalizationManager.GetText(Variables.xOy ? "aopen" : "aclose"));
            }
        }
        public static void ZoneMode()
        {
            if (Keyboard.current.semicolonKey.wasPressedThisFrame)
            {
                Variables.zoneMode = (Variables.zoneMode + 1) % 4;
                Depends.SpawnMessageText(LocalizationManager.GetText("azone") + LocalizationManager.GetText(Variables.zoneModeName[Variables.zoneMode]));
            }
        }
        public static void VortexSelection()
        {
            var nextPressed = Keyboard.current.periodKey.wasPressedThisFrame;
            var prevPressed = Keyboard.current.commaKey.wasPressedThisFrame;
            if (!nextPressed && !prevPressed)
                return;
            if (Variables.allVortexData.Count == 0)
                return;
            Variables.VortexData currentSelectedVortex = null;
            if (Variables.selectedVortexIndex >= 0 && Variables.selectedVortexIndex < Variables.allVortexData.Count)
                currentSelectedVortex = Variables.allVortexData[Variables.selectedVortexIndex];
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            Variables.allVortexData = [.. Variables.allVortexData.OrderBy(v => Vector2.Distance(v.center, indicatorPosition))];
            if (currentSelectedVortex != null)
                for (var i = 0; i < Variables.allVortexData.Count; i++)
                    if (Vector2.Distance(Variables.allVortexData[i].center, currentSelectedVortex.center) < 0.01f)
                    {
                        Variables.selectedVortexIndex = i;
                        break;
                    }
            if (nextPressed)
                Variables.selectedVortexIndex = Math.Min(Variables.selectedVortexIndex + 1, Variables.allVortexData.Count - 1);
            else
                Variables.selectedVortexIndex = Math.Max(Variables.selectedVortexIndex - 1, 0);
        }
        public static void DirectionLine()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                Variables.directionLine = !Variables.directionLine;
                if (Variables.directionLine)
                {
                    Depends.LoadOrScanVortexData();
                    if (Variables.allVortexData.Count > 0)
                    {
                        Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                        Variables.allVortexData = [.. Variables.allVortexData.OrderBy(v => Vector2.Distance(v.center, indicatorPosition))];
                        Variables.selectedVortexIndex = 0;
                    }
                }
                Depends.SpawnMessageText(LocalizationManager.GetText("aline") + LocalizationManager.GetText(Variables.directionLine ? "aopen" : "aclose"));
                if (Variables.solventDirectionHint != null)
                    Traverse.Create(Variables.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
            }
        }
        #endregion

        #region 信息计算
        public static string GrindCalc(Mortar mortar)
        {
            if (mortar.ContainedStack == null)
                return "";
            return $"{LocalizationManager.GetText("grind_progress")}{mortar.ContainedStack.overallGrindStatus * 100f}%";
        }
        public static string HealthCalc(float health)
        {
            return $"{LocalizationManager.GetText("health_status")}{health * 100f}%";
        }
        public static string PositionCalc()
        {
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var rotationSalt = Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, 0f) / 9f * 25f;
            var positionText = Depends.FormatPosition(indicatorPosition);
            var rotationText = Depends.FormatRotation(rotationSalt);
            return $@"{positionText}
{LocalizationManager.GetText("rotation_salt")}{rotationText}";
        }
        public static string VortexCalc()
        {
            Vector2 vortexCenter;
            float vortexRadius;
            float maxDistance;
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            if (Managers.RecipeMap.CurrentVortexMapItem != null)
            {
                vortexCenter = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
                vortexRadius = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius;
                maxDistance = vortexRadius + Variables.PotionBottleRadius;
            }
            else if (Variables.allVortexData.Count > 0 && Variables.selectedVortexIndex >= 0 && Variables.selectedVortexIndex < Variables.allVortexData.Count)
            {
                var selectedVortex = Variables.allVortexData[Variables.selectedVortexIndex];
                vortexCenter = selectedVortex.center;
                vortexRadius = selectedVortex.radius - Variables.PotionBottleRadius;
                maxDistance = selectedVortex.radius;
            }
            else
                return "";
            var distance = (vortexCenter - indicatorPosition).magnitude;
            var currentAngle = Vector2.SignedAngle(indicatorPosition, indicatorPosition - vortexCenter);
            var rotationLimit = Mathf.Acos(vortexRadius / vortexCenter.magnitude) * Mathf.Rad2Deg;
            Vector2 tangentPoint = Quaternion.Euler(0, 0, -rotationLimit) * (-vortexCenter) / vortexCenter.magnitude * maxDistance;
            var maxAngle = Vector2.SignedAngle(vortexCenter + tangentPoint, tangentPoint);
            return $@"{LocalizationManager.GetText("vortex_angle")}{currentAngle}'
{LocalizationManager.GetText("vortex_anglmax")}{maxAngle}'
{LocalizationManager.GetText("vortex_distance")}{distance}
{LocalizationManager.GetText("vortex_distmax")}{maxDistance}";
        }
        public static string MoveCalc()
        {
            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            var pathPoints = pathHints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).ToList<Vector3>();
            var currentStage = Managers.RecipeMap.path.deletedGraphicsSegments;
            var currentProgress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            return $@"{LocalizationManager.GetText("stir_stage")}{currentStage}
{LocalizationManager.GetText("stir_progress")}{currentProgress}
{LocalizationManager.GetText("stir_direction")}{Variables.lineDirection[0]}'
{LocalizationManager.GetText("ladle_direction")}{Variables.lineDirection[1]}'";
        }
        public static string ZoneCalc()
        {
            bool[] zoneActive = [
                ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(WeakDangerZonePart)) > 0,
                ZonePart.GetZonesActivePartsCount(typeof(HealZonePart)) > 0
            ];
            var currentPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var currentStage = Managers.RecipeMap.path.deletedGraphicsSegments;
            var currentProgress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            if (Variables.resetWhenLoading)
            {
                Array.Clear(Variables.zonePoints, 0, Variables.zonePoints.Length);
                Variables.resetWhenLoading = false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (zoneActive[i])
                        if (Variables.zonePoints[i, 3] == null || !(bool)Variables.zonePoints[i, 3])
                        {
                            Variables.zonePoints[i, 0] = currentPosition;
                            Variables.zonePoints[i, 1] = currentStage;
                            Variables.zonePoints[i, 2] = currentProgress;
                        }
                    Variables.zonePoints[i, 3] = zoneActive[i];
                }
            }
            var savedPosition = Variables.zonePoints[Variables.zoneMode, 0] == null ? Vector3.zero : (Vector3)Variables.zonePoints[Variables.zoneMode, 0];
            var savedStage = Variables.zonePoints[Variables.zoneMode, 1] == null ? 0f : (float)Variables.zonePoints[Variables.zoneMode, 1];
            var savedProgress = Variables.zonePoints[Variables.zoneMode, 2] == null ? 0f : (float)Variables.zonePoints[Variables.zoneMode, 2];
            var positionText = Depends.FormatPosition(savedPosition);
            return $@"{LocalizationManager.GetText("zone")}{LocalizationManager.GetText(Variables.zoneModeName[Variables.zoneMode])}
{positionText}
{LocalizationManager.GetText("stir_stage")}{savedStage}
{LocalizationManager.GetText("stir_progress")}{savedProgress}";
        }
        public static string TargetCalc()
        {
            Variables.SharedCache.UpdateCache();
            if (!Variables.SharedCache.isValid)
                return "";
            var targetPositionText = Depends.FormatPosition(Variables.SharedCache.targetPosition);
            var targetRotationText = Depends.FormatRotation(Variables.SharedCache.targetRotation);
            return $@"{LocalizationManager.GetText("target_id")}{Variables.SharedCache.targetEffect.Effect.GetLocalizedTitle()}
{targetPositionText}
{LocalizationManager.GetText("rotation_salt")}{targetRotationText}
{LocalizationManager.GetText("direction")}{Variables.lineDirection[2]}'";
        }
        public static string DeviationCalc()
        {
            Variables.SharedCache.UpdateCache();
            if (!Variables.SharedCache.isValid)
                return "";
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var positionDeviation = (Variables.SharedCache.targetPosition - indicatorPosition).magnitude * 1800f;
            var rotationDeviation = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Variables.SharedCache.targetEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var totalDeviation = positionDeviation + rotationDeviation;
            var positionLevel = positionDeviation <= 100f ? 3 : positionDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var rotationLevel = rotationDeviation <= 100f ? 3 : rotationDeviation <= 600f ? 2 : 1;
            var totalLevel = totalDeviation <= 100f ? 3 : totalDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            return $@"{LocalizationManager.GetText("deviation_general")}<color=red>L{totalLevel}</color> {totalDeviation}%
{LocalizationManager.GetText("deviation_position")}<color=red>L{positionLevel}</color> {positionDeviation}%
{LocalizationManager.GetText("deviation_rotation")}<color=red>L{rotationLevel}</color> {rotationDeviation}%";
        }
        public static string PathCalc()
        {
            Variables.SharedCache.UpdateCache();
            if (!Variables.SharedCache.isValid)
            {
                Variables.closestPoints[0] = null;
                Variables.closestPointDis[0] = float.MaxValue;
                return "";
            }
            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            var pathPoints = pathHints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).Skip(1);
            if (!pathPoints.Any())
            {
                Variables.closestPoints[0] = null;
                Variables.closestPointDis[0] = float.MaxValue;
                return "";
            }
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var minDistance = float.MaxValue;
            var closestPoint = Vector2.zero;
            if (!Variables.endMode)
            {
                var previousPosition = indicatorPosition;
                foreach (var point in pathPoints)
                {
                    Vector2 currentPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(point));
                    var segmentVector = currentPosition - previousPosition;
                    var segmentLengthSquared = segmentVector.sqrMagnitude;
                    var currentClosest = segmentLengthSquared == 0 ? previousPosition : previousPosition + Mathf.Clamp01(Vector2.Dot(Variables.SharedCache.targetPosition - previousPosition, segmentVector) / segmentLengthSquared) * segmentVector;
                    var currentDistance = Vector2.Distance(Variables.SharedCache.targetPosition, currentClosest);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        closestPoint = currentClosest;
                    }
                    previousPosition = currentPosition;
                }
            }
            else
            {
                closestPoint = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(pathPoints.Last()));
                minDistance = Vector2.Distance(closestPoint, Variables.SharedCache.targetPosition);
            }
            Variables.closestPoints[0] = closestPoint;
            Variables.closestPointDis[0] = minDistance;
            var positionDeviation = minDistance * 1800f;
            var rotationDeviation = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Variables.SharedCache.targetEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var totalDeviation = positionDeviation + rotationDeviation;
            var positionLevel = positionDeviation <= 100f ? 3 : positionDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var totalLevel = totalDeviation <= 100f ? 3 : totalDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var pathAngle = Mathf.DeltaAngle(Variables.lineDirection[0], Variables.lineDirection[2]);
            var directionToClosest = Vector2.SignedAngle(Vector2.right, closestPoint - Variables.SharedCache.targetPosition);
            return $@"{LocalizationManager.GetText("deviation_general")}<color=red>L{totalLevel}</color> {totalDeviation}%
{LocalizationManager.GetText("deviation_position")}<color=red>L{positionLevel}</color> {positionDeviation}%
{LocalizationManager.GetText("angle_path")}{pathAngle}'
{LocalizationManager.GetText("direction_close")}{directionToClosest}'";
        }
        public static string LadleCalc()
        {
            Variables.SharedCache.UpdateCache();
            if (!Variables.SharedCache.isValid)
            {
                Variables.closestPoints[1] = null;
                Variables.closestPointDis[1] = float.MaxValue;
                return "";
            }
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var indicatorDistanceSquared = indicatorPosition.sqrMagnitude;
            var closestPoint = indicatorDistanceSquared == 0 ? Vector2.zero : Mathf.Clamp01(Vector2.Dot(Variables.SharedCache.targetPosition, indicatorPosition) / indicatorDistanceSquared) * indicatorPosition;
            Variables.closestPoints[1] = closestPoint;
            var minDistance = Vector2.Distance(Variables.SharedCache.targetPosition, closestPoint);
            Variables.closestPointDis[1] = minDistance;
            var positionDeviation = minDistance * 1800f;
            var rotationDeviation = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Variables.SharedCache.targetEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var totalDeviation = positionDeviation + rotationDeviation;
            var positionLevel = positionDeviation <= 100f ? 3 : positionDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var totalLevel = totalDeviation <= 100f ? 3 : totalDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var ladleAngle = Mathf.DeltaAngle(Variables.lineDirection[1], Variables.lineDirection[2]);
            var directionToClosest = Vector2.SignedAngle(Vector2.right, closestPoint - Variables.SharedCache.targetPosition);
            return $@"{LocalizationManager.GetText("deviation_general")}<color=red>L{totalLevel}</color> {totalDeviation}%
{LocalizationManager.GetText("deviation_position")}<color=red>L{positionLevel}</color> {positionDeviation}%
{LocalizationManager.GetText("angle_ladle")}{ladleAngle}'
{LocalizationManager.GetText("direction_close")}{directionToClosest}'";
        }
        #endregion

        #region 渲染计算
        public static void UpdateLineDirections()
        {
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            var pathPoints = pathHints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).ToList<Vector3>();
            Variables.vortexIntersectionPoints.Clear();
            var targetVortexes = new List<Variables.VortexData>();
            if (Variables.selectedVortexIndex >= 0 && Variables.selectedVortexIndex < Variables.allVortexData.Count)
                targetVortexes.Add(Variables.allVortexData[Variables.selectedVortexIndex]);
            if (pathPoints.Count > 1)
            {
                var startPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(pathPoints[0]));
                var endPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(pathPoints[1]));
                Variables.lineDirection[0] = Vector2.SignedAngle(Vector2.right, endPosition - startPosition);
                var previousPosition = indicatorPosition;
                foreach (var point in pathPoints.Skip(1))
                {
                    Vector2 currentPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(point));
                    foreach (var vortex in targetVortexes)
                    {
                        var intersections = Depends.GetLineCircleIntersections(previousPosition, currentPosition, vortex.center, vortex.radius);
                        Variables.vortexIntersectionPoints.AddRange(intersections);
                    }
                    previousPosition = currentPosition;
                }
            }
            Variables.lineDirection[1] = Vector2.SignedAngle(Vector2.right, -indicatorPosition);
            foreach (var vortex in targetVortexes)
            {
                var intersections = Depends.GetLineCircleIntersections(indicatorPosition, Vector2.zero, vortex.center, vortex.radius);
                Variables.vortexIntersectionPoints.AddRange(intersections);
            }
            Variables.SharedCache.UpdateCache();
            if (Variables.SharedCache.isValid)
                Variables.lineDirection[2] = Vector2.SignedAngle(Vector2.right, Variables.SharedCache.targetPosition - indicatorPosition);
            if (Variables.selectedVortexIndex >= 0 && Variables.selectedVortexIndex < Variables.allVortexData.Count)
            {
                var selectedVortex = Variables.allVortexData[Variables.selectedVortexIndex];
                Vector2 direction = (selectedVortex.center - indicatorPosition).normalized;
                Variables.lineDirection[3] = Vector2.SignedAngle(Vector2.right, direction);
            }
        }
        #endregion
    }
}
