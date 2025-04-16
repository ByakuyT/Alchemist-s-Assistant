using System;
using System.Linq;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.Path;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using UnityEngine;

namespace AlchAss
{
    public static class InfoCalc
    {
        public static string GrindCalc(Mortar mortar)
        {
            if (mortar.ContainedStack == null)
                return "";
            return LocalizationManager.GetText("grind_progress") + (mortar.ContainedStack.overallGrindStatus * 100f).ToString() + "%";
        }
        public static string HealthCalc(float health)
        {
            return LocalizationManager.GetText("health_status") + (health * 100f).ToString() + "%";
        }
        public static string PositionCalc()
        {
            Vector2 indpos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var rots = Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, 0f) / 9f * 25f;
            var pots = AlchAss.xOy ? "x: " + indpos.x.ToString() + "\ny: " + indpos.y.ToString() : "r: " + indpos.magnitude.ToString() + "\nθ: " + Vector2.SignedAngle(Vector2.right, indpos).ToString();
            var salt = rots > 0f ? "<sprite=\"IconsAtlas\" name=\"SunSalt\"> " + rots.ToString() : "<sprite=\"IconsAtlas\" name=\"MoonSalt\"> " + (-rots).ToString();
            return pots + "\n" + LocalizationManager.GetText("rotation_salt") + salt;
        }
        public static string VortexCalc()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem == null)
                return "";
            var v1 = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
            var v2 = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var radV = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius;
            var dist = (v1 - v2).magnitude;
            var maxd = radV + 0.74f;
            if (AlchAss.vortexEdgeControl && AlchAss.vortexEdgeOn >= 0)
            {
                if (dist < maxd - 0.135f)
                    AlchAss.vortexEdgeOn = float.MaxValue;
                else
                    AlchAss.vortexEdgeOn = Mathf.Pow((maxd - dist) / 1.74f, 1.35f);
            }
            var ang2 = Vector2.SignedAngle(v2, v2 - v1);
            var rot = Mathf.Acos(radV / v1.magnitude) * Mathf.Rad2Deg;
            var ct = Quaternion.Euler(0, 0, -rot) * (-v1) / v1.magnitude * maxd;
            var ang1 = Vector2.SignedAngle(v1 + ct, ct);
            return LocalizationManager.GetText("vortex_angle") + ang2.ToString() + "'\n" + LocalizationManager.GetText("vortex_anglmax") + ang1.ToString() + "'\n" + LocalizationManager.GetText("vortex_distance") + dist.ToString() + "\n" + LocalizationManager.GetText("vortex_distmax") + maxd.ToString();
        }
        public static string MoveCalc()
        {
            var Hints = Managers.RecipeMap.path.fixedPathHints;
            var list = Hints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).ToList<Vector3>();
            if (list.Count<Vector3>() > 1)
            {
                var srtPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(list[0]));
                var endPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(list[1]));
                AlchAss.lineDirection[0] = Vector2.SignedAngle(Vector2.right, endPosition - srtPosition);
            }
            AlchAss.lineDirection[1] = Vector2.SignedAngle(Vector2.right, -Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition);
            var stage = Managers.RecipeMap.path.deletedGraphicsSegments;
            var progr = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            return LocalizationManager.GetText("stir_stage") + stage.ToString() + "\n" + LocalizationManager.GetText("stir_progress") + progr.ToString() + "\n" + LocalizationManager.GetText("stir_direction") + AlchAss.lineDirection[0].ToString() + "'\n" + LocalizationManager.GetText("ladle_direction") + AlchAss.lineDirection[1].ToString() + "'";
        }
        public static string ZoneCalc()
        {
            bool[] zone = { ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart)) > 0, ZonePart.GetZonesActivePartsCount(typeof(HealZonePart)) > 0, ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) > 0, ZonePart.GetZonesActivePartsCount(typeof(WeakDangerZonePart)) > 0 };
            var post = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            if (AlchAss.resetZone)
            {
                Array.Clear(AlchAss.zoneLen, 0, AlchAss.zoneLen.Length);
                AlchAss.resetZone = false;
            }
            else
                for (int i = 0; i < 4; i++)
                    if (zone[i])
                        AlchAss.zoneLen[i] += (post - AlchAss.prePost).magnitude;
            AlchAss.prePost = post;
            return LocalizationManager.GetText("zone_swamp") + AlchAss.zoneLen[0] + "\n" + LocalizationManager.GetText("zone_heal") + AlchAss.zoneLen[1] + "\n" + LocalizationManager.GetText("zone_strong") + AlchAss.zoneLen[2] + "\n" + LocalizationManager.GetText("zone_weak") + AlchAss.zoneLen[3];
        }
        public static string TargetCalc()
        {
            var clickedEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == AlchAss.hoveredItemName);
            if (clickedEffect == null)
                return "";
            var clickedPos = clickedEffect.transform.localPosition;
            var clickedRot = clickedEffect.transform.localEulerAngles.z / 9f * 25f;
            AlchAss.lineDirection[2] = Vector2.SignedAngle(Vector2.right, clickedPos - Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition);
            var pots = AlchAss.xOy ? "x: " + clickedPos.x.ToString() + "\ny: " + clickedPos.y.ToString() : "r: " + clickedPos.magnitude.ToString() + "\nθ: " + Vector2.SignedAngle(Vector2.right, clickedPos).ToString();
            var rots = clickedRot > 0f ? "<sprite=\"IconsAtlas\" name=\"SunSalt\"> " + clickedRot.ToString() : "<sprite=\"IconsAtlas\" name=\"MoonSalt\"> " + (-clickedRot).ToString();
            return LocalizationManager.GetText("target_id") + clickedEffect.Effect.GetLocalizedTitle() + "\n" + pots + "\n" + LocalizationManager.GetText("rotation_salt") + rots + "\n" + LocalizationManager.GetText("direction") + AlchAss.lineDirection[2].ToString() + "'";
        }
        public static string DeviationCalc()
        {
            var clickedEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == AlchAss.hoveredItemName);
            if (clickedEffect == null)
                return "";
            var effpos = clickedEffect.transform.localPosition;
            var indpos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var devpos = (effpos - indpos).magnitude * 1800f;
            var devrot = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, clickedEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var devtot = devpos + devrot;
            var lvlpos = devpos <= 100f ? 3 : devpos <= 600f ? 2 : devpos <= 2754f ? 1 : 0;
            var lvlrot = devrot <= 100f ? 3 : devrot <= 600f ? 2 : 1;
            var lvltot = devtot <= 100f ? 3 : devtot <= 600f ? 2 : devpos <= 2754f ? 1 : 0;
            return LocalizationManager.GetText("deviation_general") + "<color=red>L" + lvltot.ToString() + "</color> " + devtot.ToString() + "%\n" + LocalizationManager.GetText("deviation_position") + "<color=red>L" + lvlpos.ToString() + "</color> " + devpos.ToString() + "%\n" + LocalizationManager.GetText("deviation_rotation") + "<color=red>L" + lvlrot.ToString() + "</color> " + devrot.ToString() + "%";
        }
        public static string PathCalc()
        {
            var clickedEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == AlchAss.hoveredItemName);
            if (clickedEffect == null)
                return "";
            var hints = Managers.RecipeMap.path.fixedPathHints;
            var points = hints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).Skip(1);
            if (!points.Any())
                return "";
            var clickedPos = clickedEffect.transform.localPosition;
            var localPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var mindis = float.MaxValue;
            if (!AlchAss.endMode)
            {
                var prepos = localPosition;
                foreach (var point in points)
                {
                    var tmppos = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(point));
                    var tmpdis = float.MaxValue;
                    var vab = tmppos - prepos;
                    var len = vab.sqrMagnitude;
                    if (len == 0)
                        tmpdis = Vector2.Distance(clickedPos, prepos);
                    else
                    {
                        var vap = clickedPos - prepos;
                        var ti = Mathf.Clamp01(Vector2.Dot(vap, vab) / len);
                        tmpdis = Vector2.Distance(clickedPos, prepos + ti * vab);
                    }
                    if (tmpdis < mindis)
                        mindis = tmpdis;
                    prepos = tmppos;
                }
            }
            else
                mindis = Vector2.Distance(Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(points.Last())), clickedEffect.transform.localPosition);
            var devpos = mindis * 1800f;
            var devrot = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, clickedEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var devtot = devpos + devrot;
            var lvlpos = devpos <= 100f ? 3 : devpos <= 600f ? 2 : devpos <= 2754f ? 1 : 0;
            var lvlrot = devrot <= 100f ? 3 : devrot <= 600f ? 2 : 1;
            var lvltot = devtot <= 100f ? 3 : devtot <= 600f ? 2 : devpos <= 2754f ? 1 : 0;
            var angpth = Mathf.DeltaAngle(AlchAss.lineDirection[0], AlchAss.lineDirection[2]);
            return LocalizationManager.GetText("deviation_general") + "<color=red>L" + lvltot.ToString() + "</color> " + devtot.ToString() + "%\n" + LocalizationManager.GetText("deviation_position") + "<color=red>L" + lvlpos.ToString() + "</color> " + devpos.ToString() + "%\n" + LocalizationManager.GetText("deviation_rotation") + "<color=red>L" + lvlrot.ToString() + "</color> " + devrot.ToString() + "%\n" + LocalizationManager.GetText("angle_path") + angpth.ToString() + "'";
        }
        public static string LadleCalc()
        {
            var clickedEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == AlchAss.hoveredItemName);
            if (clickedEffect == null)
                return "";
            var clickedPos = clickedEffect.transform.localPosition;
            var localPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var mindis = Vector3.Cross(localPosition, clickedPos).magnitude / localPosition.magnitude;
            var devpos = localPosition.sqrMagnitude >= clickedPos.sqrMagnitude ? mindis * 1800f : float.NaN;
            var devrot = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, clickedEffect.transform.localEulerAngles.z)) / 3f * 25f;
            var devtot = devpos + devrot;
            var lvlpos = devpos <= 100f ? 3 : devpos <= 600f ? 2 : devpos <= 2754f ? 1 : 0;
            var lvlrot = devrot <= 100f ? 3 : devrot <= 600f ? 2 : 1;
            var lvltot = devtot <= 100f ? 3 : devtot <= 600f ? 2 : devpos <= 2754f ? 1 : 0;
            var anglad = Mathf.DeltaAngle(AlchAss.lineDirection[1], AlchAss.lineDirection[2]);
            return LocalizationManager.GetText("deviation_general") + "<color=red>L" + lvltot.ToString() + "</color> " + devtot.ToString() + "%\n" + LocalizationManager.GetText("deviation_position") + "<color=red>L" + lvlpos.ToString() + "</color> " + devpos.ToString() + "%\n" + LocalizationManager.GetText("deviation_rotation") + "<color=red>L" + lvlrot.ToString() + "</color> " + devrot.ToString() + "%\n" + LocalizationManager.GetText("angle_ladle") + anglad.ToString() + "'";
        }
    }
}
