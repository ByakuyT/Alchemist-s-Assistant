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
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAss
{
    public static class InfoCalc
    {
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
        public static string GrindCalc(Mortar __instance)
        {
            if (__instance.ContainedStack != null)
            {
                var grind = __instance.ContainedStack.overallGrindStatus;
                return LocalizationManager.GetText("#mod_grind_progress") + (grind * 100f).ToString() + "%";
            }
            else
                return LocalizationManager.GetText("#mod_grind_progress");
        }
        public static string HealthCalc(IndicatorMapItem __instance)
        {
            var health = (float)AccessTools.Field(typeof(IndicatorMapItem), "health").GetValue(__instance);
            return LocalizationManager.GetText("#mod_health_status") + (health * 100f).ToString() + "%";
        }
        public static string VortexCalc()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem != null)
            {
                var v1 = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
                var v2 = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                float ang1 = Vector2.SignedAngle(v1 - v2, Vector2.up);
                float ang2 = Mathf.DeltaAngle(Vector2.SignedAngle(v2, Vector2.up), ang1);
                return LocalizationManager.GetText("#mod_vortex_direction") + ang1.ToString() + "\n" + LocalizationManager.GetText("#mod_vortex_angle") + ang2.ToString();
            }
            else
                return LocalizationManager.GetText("#mod_vortex_direction") + "\n" + LocalizationManager.GetText("#mod_vortex_angle");
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
            AlchAss.endDirection = -direction;
            return LocalizationManager.GetText("#mod_stir_stage") + deletedGraphicsSegments.ToString() + "\n" + LocalizationManager.GetText("#mod_stir_progress") + segmentLengthToDeletePhysics.ToString() + "\n" + LocalizationManager.GetText("#mod_stir_direction") + direction.ToString();
        }
        public static Tuple<string, string> PositionDeviationCalc()
        {
            string pos, dev;
            var indicatorContainer = Managers.RecipeMap.recipeMapObject.indicatorContainer;
            if (Managers.RecipeMap.currentPotionEffectMapItem == null)
            {
                var vector = indicatorContainer.localPosition;
                var magnitude = vector.magnitude;
                var num = Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, 0f);
                var num2 = Vector2.SignedAngle(vector, Vector2.up);
                var num3 = num / 9f * 25f;
                pos = LocalizationManager.GetText("#mod_position_distance") + magnitude.ToString() + "\n" + LocalizationManager.GetText("#mod_position_angle") + num2.ToString() + "'\n" + LocalizationManager.GetText("#mod_position_rotation") + num.ToString() + "'\n" + LocalizationManager.GetText("#mod_position_salt") + ((num3 > 0f) ? ("<sprite=\"IconsAtlas\" name=\"SunSalt\"> " + num3.ToString()) : ("<sprite=\"IconsAtlas\" name=\"MoonSalt\"> " + (-num3).ToString()));
                dev = LocalizationManager.GetText("#mod_deviation_general") + "\n" + LocalizationManager.GetText("#mod_deviation_distance") + "\n" + LocalizationManager.GetText("#mod_deviation_rotation");
                return new Tuple<string, string>(pos, dev);
            }
            var transform = Managers.RecipeMap.currentPotionEffectMapItem.transform;
            var vector2 = indicatorContainer.localPosition;
            var vector3 = transform.localPosition;
            var num4 = Vector2.Distance(vector2, vector3);
            var num5 = Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, transform.eulerAngles.z);
            var num6 = Vector2.SignedAngle(vector2, vector3);
            var num7 = num5 / 9f * 25f;
            pos = LocalizationManager.GetText("#mod_position_distance") + num4.ToString() + "\n" + LocalizationManager.GetText("#mod_position_angle") + num6.ToString() + "'\n" + LocalizationManager.GetText("#mod_position_rotation") + num5.ToString() + "'\n" + LocalizationManager.GetText("#mod_position_salt") + ((num7 > 0f) ? ("<sprite=\"IconsAtlas\" name=\"MoonSalt\"> " + num7.ToString()) : ("<sprite=\"IconsAtlas\" name=\"SunSalt\"> " + (-num7).ToString()));
            var num8 = num4 * 1800f;
            var num9 = Mathf.Abs(num5) / 3f * 25f;
            var num10 = ((num8 <= 100f) ? 3 : ((num8 <= 600f) ? 2 : 1));
            var num11 = ((num9 <= 100f) ? 3 : ((num9 <= 600f) ? 2 : 1));
            dev = LocalizationManager.GetText("#mod_deviation_general") + (num8 + num9).ToString() + "%\n" + LocalizationManager.GetText("#mod_deviation_distance") + "<color=red>L" + num10.ToString() + "</color> " + num8.ToString() + "%\n" + LocalizationManager.GetText("#mod_deviation_rotation") + "<color=red>L" + num11.ToString() + "</color> " + num9.ToString() + "%";
            return new Tuple<string, string>(pos, dev);
        }
        public static string PathCalc()
        {
            var nearestEffects = Helper.GetNearestEffects();
            var fixedPathHints = Managers.RecipeMap.path.fixedPathHints;
            var num = float.MaxValue;
            PotionEffectMapItem potionEffectMapItem = null;
            foreach (var vector in fixedPathHints.Select((FixedHint fixedHint) => fixedHint.evenlySpacedPointsFixedPhysics.points).SelectMany((Vector3[] points) => points))
            {
                foreach (var MapItem in nearestEffects)
                {
                    if (Managers.RecipeMap.currentMap.referencesContainer.transform == null)
                        break;
                    var num4 = Vector2.Distance(Managers.RecipeMap.currentMap.referencesContainer.transform.InverseTransformPoint(Managers.RecipeMap.path.thisTransform.TransformPoint(vector)), MapItem.transform.localPosition);
                    if (num4 < num)
                    {
                        num = num4;
                        potionEffectMapItem = MapItem;
                    }
                }
            }
            if (num < float.MaxValue)
            {
                var num6 = num * 1800f;
                var num7 = ((num6 <= 100f) ? 3 : ((num6 <= 600f) ? 2 : ((num6 <= 2755f) ? 1 : 0)));
                var num8 = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, potionEffectMapItem.transform.localEulerAngles.z)) / 3f * 25f;
                var num9 = ((num8 <= 100f) ? 3 : ((num8 <= 600f) ? 2 : 1));
                var num10 = num6 + num8;
                var num11 = ((num10 <= 100f) ? 3 : ((num10 <= 600f) ? 2 : ((num6 <= 2755f) ? 1 : 0)));
                return LocalizationManager.GetText("#mod_closest_potion_path") + potionEffectMapItem.Effect.GetLocalizedTitle() + "\n" + LocalizationManager.GetText("#mod_closest_general_path") + "<color=red>L" + num11.ToString() + "</color> " + num10.ToString() + "%\n" + LocalizationManager.GetText("#mod_closest_distance_path") + "<color=red>L" + num7.ToString() + "</color> " + num6.ToString() + "%\n" + LocalizationManager.GetText("#mod_closest_angle_path") + "<color=red>L" + num9.ToString() + "</color> " + num8.ToString() + "%";
            }
            return LocalizationManager.GetText("#mod_closest_potion_path") + "\n" + LocalizationManager.GetText("#mod_closest_general_path") + "\n" + LocalizationManager.GetText("#mod_closest_distance_path") + "\n" + LocalizationManager.GetText("#mod_closest_angle_path");
        }
        public static string LadleCalc()
        {
            var nearestEffects = Helper.GetNearestEffects();
            var num2 = float.MaxValue;
            var localPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            PotionEffectMapItem potionEffectMapItem = null;
            var num3 = Vector2.Distance(localPosition, Vector2.zero);
            foreach (var MapItem in nearestEffects)
            {
                if (Managers.RecipeMap.currentMap.referencesContainer.transform == null)
                    break;
                var vector2 = MapItem.transform.localPosition;
                if (Vector2.Angle(localPosition, vector2) <= 90f)
                    if (Vector2.Distance(vector2, Vector2.zero) <= num3)
                    {
                        var num5 = Vector3.Cross(localPosition, vector2).magnitude / localPosition.magnitude;
                        if (num5 < num2)
                        {
                            num2 = num5;
                            potionEffectMapItem = MapItem;
                        }
                    }
            }
            if (num2 < float.MaxValue)
            {
                var num12 = num2 * 1800f;
                var num13 = ((num12 <= 100f) ? 3 : ((num12 <= 600f) ? 2 : ((num12 <= 2755f) ? 1 : 0)));
                var num14 = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, potionEffectMapItem.transform.localEulerAngles.z)) / 3f * 25f;
                var num15 = ((num14 <= 100f) ? 3 : ((num14 <= 600f) ? 2 : 1));
                var num16 = num12 + num14;
                var num17 = ((num16 <= 100f) ? 3 : ((num16 <= 600f) ? 2 : ((num12 <= 2755f) ? 1 : 0)));
                return LocalizationManager.GetText("#mod_closest_potion_ladle") + potionEffectMapItem.Effect.GetLocalizedTitle() + "\n" + LocalizationManager.GetText("#mod_closest_general_ladle") + "<color=red>L" + num17.ToString() + "</color> " + num16.ToString() + "%\n" + LocalizationManager.GetText("#mod_closest_distance_ladle") + "<color=red>L" + num13.ToString() + "</color> " + num12.ToString() + "%\n" + LocalizationManager.GetText("#mod_closest_angle_ladle") + "<color=red>L" + num15.ToString() + "</color> " + num14.ToString() + "%";
            }
            return LocalizationManager.GetText("#mod_closest_potion_ladle") + "\n" + LocalizationManager.GetText("#mod_closest_general_ladle") + "\n" + LocalizationManager.GetText("#mod_closest_distance_ladle") + "\n" + LocalizationManager.GetText("#mod_closest_angle_ladle");
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
