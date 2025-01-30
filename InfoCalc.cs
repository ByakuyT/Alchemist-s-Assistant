using System;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ObjectBased.Bellows;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.Pestle;
using PotionCraft.ObjectBased.RecipeMap.Path;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.Zones;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAss
{
    public static class InfoCalc
    {
        public static void EndMode()
        {
            if (Keyboard.current.backslashKey.wasPressedThisFrame)
            {
                AlchAss.endMode = !AlchAss.endMode;
                Helper.SpawnMessageText(LocalizationManager.GetText("aend") + LocalizationManager.GetText(AlchAss.endMode ? "aopen" : "aclose"));
            }
        }
        public static void DirectionLine()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                AlchAss.directionLine = !AlchAss.directionLine;
                Helper.SpawnMessageText(LocalizationManager.GetText("aline") + LocalizationManager.GetText(AlchAss.directionLine ? "aopen" : "aclose"));
                if (AlchAss.solventDirectionHint != null)
                    Traverse.Create(AlchAss.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
            }
        }
        public static void VortexEdge()
        {
            if (Keyboard.current.quoteKey.wasPressedThisFrame)
            {
                if (AlchAss.vortexEdgeControl)
                {
                    AlchAss.vortexEdgeControl = false;
                    AlchAss.vortexEdgeOn = 1;
                }
                else
                    AlchAss.vortexEdgeControl = true;
                Helper.SpawnMessageText(LocalizationManager.GetText("avortex") + LocalizationManager.GetText(AlchAss.vortexEdgeControl ? "aopen" : "aclose"));
            }
        }
        public static void CoolDown()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                if (Managers.Cursor.hoveredInteractiveItem != null)
                    if (Managers.Cursor.hoveredInteractiveItem.GetType() == typeof(Bellows))
                    {
                        var coals = Managers.Ingredient.coals;
                        var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.grindHeatPath));
                        if (lines.Length > 0)
                            Traverse.Create(coals).Field("_heat").SetValue(float.Parse(lines[0]) / 100f);
                        Traverse.Create(coals).Method("Update", Array.Empty<object>()).GetValue();
                    }
        }
        public static void GrindAll()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                if (Managers.Cursor.hoveredInteractiveItem != null)
                    if (Managers.Cursor.hoveredInteractiveItem.GetType() == typeof(Pestle))
                    {
                        var mortar = Managers.Ingredient.mortar;
                        if (mortar.ContainedStack != null)
                        {
                            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.grindHeatPath));
                            if (lines.Length > 0)
                                mortar.ContainedStack.overallGrindStatus = float.Parse(lines[0]) / 100f;
                        }
                    }
        }
        public static void GrindSlowDown(ref float pestleLinearSpeed, ref float pestleAngularSpeed, int opt)
        {
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.speedBrewPath));
            if (lines.Length > opt)
            {
                pestleLinearSpeed /= float.Parse(lines[opt]);
                pestleAngularSpeed /= float.Parse(lines[opt]);
            }
        }
        public static void StirSlowDown(ref float ___StirringValue, int opt)
        {
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.speedBrewPath));
            if (lines.Length > opt)
                ___StirringValue /= float.Parse(lines[opt]);
        }
        public static void LadleSlowDown(ref float __result, int opt)
        {
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.speedBrewPath));
            if (lines.Length > opt)
                __result /= float.Parse(lines[opt]);
        }
        public static void HeatSlowDown(ref float __state, int opt)
        {
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.speedBrewPath));
            if (lines.Length > opt)
            {
                var asset = Settings<RecipeMapManagerVortexSettings>.Asset;
                __state = asset.vortexMovementSpeed;
                asset.vortexMovementSpeed /= float.Parse(lines[opt]);
            }
        }
        public static string GrindCalc(Mortar mortar)
        {
            if (mortar.ContainedStack != null)
                return LocalizationManager.GetText("grind_progress") + (mortar.ContainedStack.overallGrindStatus * 100f).ToString() + "%";
            else
                return LocalizationManager.GetText("grind_progress");
        }
        public static string HealthCalc(float health)
        {
            return LocalizationManager.GetText("health_status") + (health * 100f).ToString() + "%";
        }
        public static string VortexCalc()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem != null)
            {
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
                return LocalizationManager.GetText("vortex_angle") + ang2.ToString() + "\n" + LocalizationManager.GetText("vortex_direction") + ang1.ToString() + "\n" + LocalizationManager.GetText("vortex_distance") + dist.ToString() + "\n" + LocalizationManager.GetText("vortex_edge") + maxd.ToString();
            }
            return LocalizationManager.GetText("vortex_direction") + "\n" + LocalizationManager.GetText("vortex_angle") + "\n" + LocalizationManager.GetText("vortex_distance") + "\n" + LocalizationManager.GetText("vortex_edge");
        }
        public static string StirCalc()
        {
            var Hints = Managers.RecipeMap.path.fixedPathHints;
            var direction = float.MaxValue;
            var localPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var list = Hints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).ToList<Vector3>();
            if (list.Count<Vector3>() > 1)
            {
                var endPosition = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(list[1]));
                direction = Vector2.SignedAngle(endPosition - localPosition, Vector2.up);
            }
            float deletedGraphicsSegments = Managers.RecipeMap.path.deletedGraphicsSegments;
            float segmentLengthToDeletePhysics = Managers.RecipeMap.path.segmentLengthToDeletePhysics;
            direction = direction < float.MaxValue ? direction : 0f;
            var zone = 0f;
            AlchAss.endDirection = -direction;
            var zonePart = ZonePart.GetZonesActivePartsCount(typeof(StrongDangerZonePart)) + ZonePart.GetZonesActivePartsCount(typeof(SwampZonePart));
            if (zonePart == 0)
                AlchAss.zoneStir = deletedGraphicsSegments;
            zone = deletedGraphicsSegments - AlchAss.zoneStir;
            return LocalizationManager.GetText("stir_stage") + deletedGraphicsSegments.ToString() + "\n" + LocalizationManager.GetText("stir_progress") + segmentLengthToDeletePhysics.ToString() + "\n" + LocalizationManager.GetText("stir_direction") + direction.ToString() + "\n" + LocalizationManager.GetText("stir_zone") + zone.ToString();
        }
        public static Tuple<string, string> PositionDeviationCalc()
        {
            string pos, dev;
            var indicatorContainer = Managers.RecipeMap.recipeMapObject.indicatorContainer;
            Vector2 indpos = indicatorContainer.localPosition;
            if (Managers.RecipeMap.currentPotionEffectMapItem == null)
            {
                var magnitude = indpos.magnitude;
                var num = Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, 0f);
                var num3 = num / 9f * 25f;
                pos = LocalizationManager.GetText("position_distance") + magnitude.ToString() + "\n" + LocalizationManager.GetText("position_position") + indpos.ToString() + "\n" + LocalizationManager.GetText("position_rotation") + num.ToString() + "'\n" + LocalizationManager.GetText("position_salt") + ((num3 > 0f) ? ("<sprite=\"IconsAtlas\" name=\"SunSalt\"> " + num3.ToString()) : ("<sprite=\"IconsAtlas\" name=\"MoonSalt\"> " + (-num3).ToString()));
                dev = LocalizationManager.GetText("deviation_general") + "\n" + LocalizationManager.GetText("deviation_distance") + "\n" + LocalizationManager.GetText("deviation_rotation");
                return new Tuple<string, string>(pos, dev);
            }
            var transform = Managers.RecipeMap.currentPotionEffectMapItem.transform;
            Vector2 effpos = transform.localPosition;
            var num4 = Vector2.Distance(indpos, effpos);
            var deltapos = indpos - effpos;
            var num5 = Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, transform.eulerAngles.z);
            var num7 = num5 / 9f * 25f;
            pos = LocalizationManager.GetText("position_distance") + num4.ToString() + "\n" + LocalizationManager.GetText("position_position") + deltapos.ToString() + "\n" + LocalizationManager.GetText("position_rotation") + num5.ToString() + "'\n" + LocalizationManager.GetText("position_salt") + ((num7 > 0f) ? ("<sprite=\"IconsAtlas\" name=\"MoonSalt\"> " + num7.ToString()) : ("<sprite=\"IconsAtlas\" name=\"SunSalt\"> " + (-num7).ToString()));
            var num8 = num4 * 1800f;
            var num9 = Mathf.Abs(num5) / 3f * 25f;
            var num10 = ((num8 <= 100f) ? 3 : ((num8 <= 600f) ? 2 : 1));
            var num11 = ((num9 <= 100f) ? 3 : ((num9 <= 600f) ? 2 : 1));
            dev = LocalizationManager.GetText("deviation_general") + (num8 + num9).ToString() + "%\n" + LocalizationManager.GetText("deviation_distance") + "<color=red>L" + num10.ToString() + "</color> " + num8.ToString() + "%\n" + LocalizationManager.GetText("deviation_rotation") + "<color=red>L" + num11.ToString() + "</color> " + num9.ToString() + "%";
            return new Tuple<string, string>(pos, dev);
        }
        public static string PathCalc()
        {
            var clickedEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == AlchAss.hoveredItemName);
            var fixedPathHints = Managers.RecipeMap.path.fixedPathHints;
            var num = float.MaxValue;
            var points = fixedPathHints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points).Skip(1);
            if (AlchAss.endMode)
            {
                if (points.Any())
                    if (clickedEffect != null)
                        num = Vector2.Distance(Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(points.Last())), clickedEffect.transform.localPosition);
            }
            else
            {
                var prepos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                foreach (var vector in points)
                {
                    if (clickedEffect == null)
                        break;
                    var tmppos = Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(vector));
                    var num4 = float.MaxValue;
                    var vab = tmppos - prepos;
                    var len = vab.sqrMagnitude;
                    if (len == 0)
                        num4 = Vector2.Distance(clickedEffect.transform.localPosition, prepos);
                    else
                    {
                        var vap = clickedEffect.transform.localPosition - prepos;
                        var ti = Mathf.Clamp01(Vector2.Dot(vap, vab) / len);
                        num4 = Vector2.Distance(clickedEffect.transform.localPosition, prepos + ti * vab);
                    }
                    if (num4 < num)
                        num = num4;
                    prepos = tmppos;
                }
            }
            if (num < float.MaxValue)
            {
                var num6 = num * 1800f;
                var num7 = (num6 <= 100f) ? 3 : ((num6 <= 600f) ? 2 : ((num6 <= 2754f) ? 1 : 0));
                var num8 = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, clickedEffect.transform.localEulerAngles.z)) / 3f * 25f;
                var num9 = (num8 <= 100f) ? 3 : ((num8 <= 600f) ? 2 : 1);
                var num10 = num6 + num8;
                var num11 = (num10 <= 100f) ? 3 : ((num10 <= 600f) ? 2 : ((num6 <= 2754f) ? 1 : 0));
                return LocalizationManager.GetText("closest_potion_path") + clickedEffect.Effect.GetLocalizedTitle() + "\n" + LocalizationManager.GetText("closest_general_path") + "<color=red>L" + num11.ToString() + "</color> " + num10.ToString() + "%\n" + LocalizationManager.GetText("closest_distance_path") + "<color=red>L" + num7.ToString() + "</color> " + num6.ToString() + "%\n" + LocalizationManager.GetText("closest_rotation_path") + "<color=red>L" + num9.ToString() + "</color> " + num8.ToString() + "%";
            }
            return LocalizationManager.GetText("closest_potion_path") + "\n" + LocalizationManager.GetText("closest_general_path") + "\n" + LocalizationManager.GetText("closest_distance_path") + "\n" + LocalizationManager.GetText("closest_rotation_path");
        }
        public static string LadleCalc()
        {
            var clickedEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == AlchAss.hoveredItemName);
            var num2 = float.MaxValue;
            var localPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            if (clickedEffect != null)
            {
                var vector2 = clickedEffect.transform.localPosition;
                if (Vector2.Angle(localPosition, vector2) <= 90f)
                    if (Vector2.Distance(vector2, Vector2.zero) <= Vector2.Distance(localPosition, Vector2.zero))
                        num2 = Vector3.Cross(localPosition, vector2).magnitude / localPosition.magnitude;
            }
            if (num2 < float.MaxValue)
            {
                var num12 = num2 * 1800f;
                var num13 = (num12 <= 100f) ? 3 : ((num12 <= 600f) ? 2 : ((num12 <= 2754f) ? 1 : 0));
                var num14 = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, clickedEffect.transform.localEulerAngles.z)) / 3f * 25f;
                var num15 = (num14 <= 100f) ? 3 : ((num14 <= 600f) ? 2 : 1);
                var num16 = num12 + num14;
                var num17 = (num16 <= 100f) ? 3 : ((num16 <= 600f) ? 2 : ((num12 <= 2754f) ? 1 : 0));
                return LocalizationManager.GetText("closest_potion_ladle") + clickedEffect.Effect.GetLocalizedTitle() + "\n" + LocalizationManager.GetText("closest_general_ladle") + "<color=red>L" + num17.ToString() + "</color> " + num16.ToString() + "%\n" + LocalizationManager.GetText("closest_distance_ladle") + "<color=red>L" + num13.ToString() + "</color> " + num12.ToString() + "%\n" + LocalizationManager.GetText("closest_rotation_ladle") + "<color=red>L" + num15.ToString() + "</color> " + num14.ToString() + "%";
            }
            return LocalizationManager.GetText("closest_potion_ladle") + "\n" + LocalizationManager.GetText("closest_general_ladle") + "\n" + LocalizationManager.GetText("closest_distance_ladle") + "\n" + LocalizationManager.GetText("closest_rotation_ladle");
        }
        public static void BrewRecipe(ref int count, IRecipeBookPageContent recipePageContent, int opt)
        {
            var lines = File.ReadAllLines(Path.Combine(Paths.PluginPath, AlchAss.speedBrewPath));
            if (lines.Length > opt)
            {
                var times = (int)float.Parse(lines[opt]);
                if (Helper.CanBrewTimes(recipePageContent, count, times))
                    count *= times;
            }
        }
    }
}
