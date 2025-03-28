using System;
using BepInEx;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Bellows;
using PotionCraft.ObjectBased.Pestle;
using PotionCraft.Settings;
using UnityEngine.InputSystem;
using System.IO;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;

namespace AlchAss
{
    public static class Controler
    {
        public static void EndMode()
        {
            if (Keyboard.current.backslashKey.wasPressedThisFrame)
            {
                AlchAss.endMode = !AlchAss.endMode;
                Helper.SpawnMessageText(LocalizationManager.GetText("aend") + LocalizationManager.GetText(AlchAss.endMode ? "aopen" : "aclose"));
            }
        }
        public static void PositionMode()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                AlchAss.xOy = !AlchAss.xOy;
                Helper.SpawnMessageText(LocalizationManager.GetText("axoy") + LocalizationManager.GetText(AlchAss.xOy ? "aopen" : "aclose"));
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
