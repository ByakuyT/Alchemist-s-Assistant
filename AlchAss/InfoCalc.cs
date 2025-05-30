using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.Path;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using System.Linq;
using UnityEngine;

namespace AlchAss
{
    public static class InfoCalc
    {
        public static string FormatSaltRotation(float rotation)
        {
            return rotation > 0f
                ? $"<sprite=\"IconsAtlas\" name=\"SunSalt\"> {rotation}"
                : $"<sprite=\"IconsAtlas\" name=\"MoonSalt\"> {-rotation}";
        }
        public static string FormatPosition(Vector2 position)
        {
            return Vars.xOy
                ? $"x: {position.x}\ny: {position.y}"
                : $"r: {position.magnitude}\nθ: {Vector2.SignedAngle(Vector2.right, position)}";
        }
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
            var positionText = FormatPosition(indicatorPosition);
            var rotationText = FormatSaltRotation(rotationSalt);
            return $@"{positionText}
{LocalizationManager.GetText("rotation_salt")}{rotationText}";
        }
        public static string VortexCalc()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem == null)
                return "";
            var vortexCenter = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var vortexRadius = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius;
            var distance = (vortexCenter - indicatorPosition).magnitude;
            var maxDistance = vortexRadius + Vars.PotionBottleRadius;
            var currentAngle = Vector2.SignedAngle(indicatorPosition, indicatorPosition - vortexCenter);
            var rotationLimit = Mathf.Acos(vortexRadius / vortexCenter.magnitude) * Mathf.Rad2Deg;
            var tangentPoint = Quaternion.Euler(0, 0, -rotationLimit) * (-vortexCenter) / vortexCenter.magnitude * maxDistance;
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
{LocalizationManager.GetText("stir_direction")}{Vars.lineDirection[0]}'
{LocalizationManager.GetText("ladle_direction")}{Vars.lineDirection[1]}'";
        }
        public static string ZoneCalc()
        {
            bool[] zoneActive = { ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart)) > 0, ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) > 0, ZonePart.GetZonesActivePartsCount(typeof(WeakDangerZonePart)) > 0, ZonePart.GetZonesActivePartsCount(typeof(HealZonePart)) > 0 };
            var currentPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var currentStage = Managers.RecipeMap.path.deletedGraphicsSegments;
            var currentProgress = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            var savedPosition = Vars.zonePoints[Vars.zoneMode, 0] == null ? Vector3.zero : (Vector3)Vars.zonePoints[Vars.zoneMode, 0];
            var savedStage = Vars.zonePoints[Vars.zoneMode, 1] == null ? 0f : (float)Vars.zonePoints[Vars.zoneMode, 1];
            var savedProgress = Vars.zonePoints[Vars.zoneMode, 2] == null ? 0f : (float)Vars.zonePoints[Vars.zoneMode, 2];
            var positionText = FormatPosition(savedPosition);
            return $@"{LocalizationManager.GetText("zone")}{LocalizationManager.GetText(Vars.zoneModeName[Vars.zoneMode])}
{positionText}
{LocalizationManager.GetText("stir_stage")}{savedStage}
{LocalizationManager.GetText("stir_progress")}{savedProgress}";
        }
        public static string TargetCalc()
        {
            Vars.SharedCache.UpdateCache();
            if (!Vars.SharedCache.isValid)
                return "";
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var targetPositionText = FormatPosition(Vars.SharedCache.targetPosition);
            var targetRotationText = FormatSaltRotation(Vars.SharedCache.targetRotation);
            return $@"{LocalizationManager.GetText("target_id")}{Vars.SharedCache.targetEffect.Effect.GetLocalizedTitle()}
{targetPositionText}
{LocalizationManager.GetText("rotation_salt")}{targetRotationText}
{LocalizationManager.GetText("direction")}{Vars.lineDirection[2]}'";
        }
        public static string DeviationCalc()
        {
            Vars.SharedCache.UpdateCache();
            if (!Vars.SharedCache.isValid)
                return "";
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var positionDeviation = (Vars.SharedCache.targetPosition - indicatorPosition).magnitude * 1800f;
            var rotationDeviation = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Vars.SharedCache.targetEffect.transform.localEulerAngles.z)) / 3f * 25f;
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
            Vars.SharedCache.UpdateCache();
            if (!Vars.SharedCache.isValid)
            {
                Vars.closestPoints[0] = null;
                Vars.closestPointDis[0] = float.MaxValue;
                return "";
            }
            var pathHints = Managers.RecipeMap.path.fixedPathHints;
            var pathPoints = pathHints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).Skip(1);
            if (!pathPoints.Any())
            {
                Vars.closestPoints[0] = null;
                Vars.closestPointDis[0] = float.MaxValue;
                return "";
            }
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var minDistance = float.MaxValue;
            var closestPoint = Vector2.zero;
            if (!Vars.endMode)
            {
                var previousPosition = indicatorPosition;
                foreach (var point in pathPoints)
                {
                    Vector2 currentPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(point));
                    var segmentVector = currentPosition - previousPosition;
                    var segmentLengthSquared = segmentVector.sqrMagnitude;
                    var currentClosest = segmentLengthSquared == 0 ? previousPosition : previousPosition + Mathf.Clamp01(Vector2.Dot(Vars.SharedCache.targetPosition - previousPosition, segmentVector) / segmentLengthSquared) * segmentVector;
                    var currentDistance = Vector2.Distance(Vars.SharedCache.targetPosition, currentClosest);
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
                minDistance = Vector2.Distance(closestPoint, Vars.SharedCache.targetPosition);
            }
            Vars.closestPoints[0] = closestPoint;
            Vars.closestPointDis[0] = minDistance;
            var positionDeviation = minDistance * 1800f;
            var rotationDeviation = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Vars.SharedCache.targetEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var totalDeviation = positionDeviation + rotationDeviation;
            var positionLevel = positionDeviation <= 100f ? 3 : positionDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var totalLevel = totalDeviation <= 100f ? 3 : totalDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var pathAngle = Mathf.DeltaAngle(Vars.lineDirection[0], Vars.lineDirection[2]);
            var directionToClosest = Vector2.SignedAngle(Vector2.right, closestPoint - Vars.SharedCache.targetPosition);
            return $@"{LocalizationManager.GetText("deviation_general")}<color=red>L{totalLevel}</color> {totalDeviation}%
{LocalizationManager.GetText("deviation_position")}<color=red>L{positionLevel}</color> {positionDeviation}%
{LocalizationManager.GetText("angle_path")}{pathAngle}'
{LocalizationManager.GetText("direction_close")}{directionToClosest}'";
        }
        public static string LadleCalc()
        {
            Vars.SharedCache.UpdateCache();
            if (!Vars.SharedCache.isValid)
            {
                Vars.closestPoints[1] = null;
                Vars.closestPointDis[1] = float.MaxValue;
                return "";
            }
            Vector2 indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var indicatorDistanceSquared = indicatorPosition.sqrMagnitude;
            var closestPoint = indicatorDistanceSquared == 0 ? Vector2.zero : Mathf.Clamp01(Vector2.Dot(Vars.SharedCache.targetPosition, indicatorPosition) / indicatorDistanceSquared) * indicatorPosition;
            Vars.closestPoints[1] = closestPoint;
            var minDistance = Vector2.Distance(Vars.SharedCache.targetPosition, closestPoint);
            Vars.closestPointDis[1] = minDistance;
            var positionDeviation = minDistance * 1800f;
            var rotationDeviation = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, Vars.SharedCache.targetEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var totalDeviation = positionDeviation + rotationDeviation;
            var positionLevel = positionDeviation <= 100f ? 3 : positionDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var totalLevel = totalDeviation <= 100f ? 3 : totalDeviation <= 600f ? 2 : positionDeviation <= 2754f ? 1 : 0;
            var ladleAngle = Mathf.DeltaAngle(Vars.lineDirection[1], Vars.lineDirection[2]);
            var directionToClosest = Vector2.SignedAngle(Vector2.right, closestPoint - Vars.SharedCache.targetPosition);
            return $@"{LocalizationManager.GetText("deviation_general")}<color=red>L{totalLevel}</color> {totalDeviation}%
{LocalizationManager.GetText("deviation_position")}<color=red>L{positionLevel}</color> {positionDeviation}%
{LocalizationManager.GetText("angle_ladle")}{ladleAngle}'
{LocalizationManager.GetText("direction_close")}{directionToClosest}'";
        }
    }
}
