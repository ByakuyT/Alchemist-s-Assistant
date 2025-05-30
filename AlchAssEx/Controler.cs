using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Bellows;
using PotionCraft.ObjectBased.Pestle;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssEx
{
    public static class Controler
    {
        #region 自动制动
        public static void VortexEdgeControl()
        {
            if (Keyboard.current.quoteKey.wasPressedThisFrame)
            {
                if (Vars.vortexEdgeControl)
                {
                    Vars.vortexEdgeControl = false;
                    Vars.vortexEdgeSpeed = float.MaxValue;
                }
                else
                {
                    Helper.UpdateFunctionsConfigCache();
                    Vars.vortexEdgeControl = true;
                }
                AlchAss.Helper.SpawnMessageText(LocalizationManager.GetText("avortex") + LocalizationManager.GetText(Vars.vortexEdgeControl ? "aopen" : "aclose"));
            }
        }
        public static void ClosestPointControl()
        {
            if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
            {
                if (Vars.closestPointControl)
                {
                    Vars.closestPointControl = false;
                    Vars.closestPointspeed[0] = float.MaxValue;
                    Vars.closestPointspeed[1] = float.MaxValue;
                }
                else
                {
                    Helper.UpdateFunctionsConfigCache();
                    Vars.closestPointControl = true;
                }
                AlchAss.Helper.SpawnMessageText(LocalizationManager.GetText("aclosest") + LocalizationManager.GetText(Vars.closestPointControl ? "aopen" : "aclose"));
            }
        }
        public static void TargetProximityControl()
        {
            if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
            {
                if (Vars.targetProximityControl)
                {
                    Vars.targetProximityControl = false;
                    Vars.targetProximitySpeed[0] = float.MaxValue;
                    Vars.targetProximitySpeed[1] = float.MaxValue;
                    Vars.targetProximitySpeed[2] = float.MaxValue;
                }
                else
                {
                    Helper.UpdateFunctionsConfigCache();
                    Vars.targetProximityControl = true;
                }
                AlchAss.Helper.SpawnMessageText(LocalizationManager.GetText("aproximity") + LocalizationManager.GetText(Vars.targetProximityControl ? "aopen" : "aclose"));
            }
        }
        public static void UpdateVortexEdgeControl()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem == null || !Vars.vortexEdgeControl)
                return;
            if (Vars.vortexEdgeSpeed == -1f)
                return;
            var vortexCenter = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var vortexRadius = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius;
            var distance = (vortexCenter - indicatorPosition).magnitude;
            var maxDistance = vortexRadius + AlchAss.Vars.PotionBottleRadius;
            var distanceToEdge = maxDistance - distance;
            Vars.vortexEdgeSpeed = CalculateControlSpeedFactor(distanceToEdge);
        }
        public static void UpdateClosestPointControl()
        {
            if (!Vars.closestPointControl)
                return;
            if (!AlchAss.Vars.SharedCache.isValid)
            {
                Vars.closestPointspeed[0] = float.MaxValue;
                Vars.closestPointspeed[1] = float.MaxValue;
                return;
            }
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            for (int i = 0; i < 2; i++)
            {
                if (AlchAss.Vars.closestPoints[i].HasValue)
                {
                    var distance = Vector2.Distance(indicatorPosition, AlchAss.Vars.closestPoints[i].Value);
                    Vars.closestPointspeed[i] = CalculateControlSpeedFactor(distance);
                }
                else
                    Vars.closestPointspeed[i] = float.MaxValue;
            }
        }
        public static void UpdateTargetProximityControl()
        {
            if (!Vars.targetProximityControl)
            {
                Vars.targetProximitySpeed[0] = float.MaxValue;
                Vars.targetProximitySpeed[1] = float.MaxValue;
                Vars.targetProximitySpeed[2] = float.MaxValue;
                return;
            }
            if (!AlchAss.Vars.SharedCache.isValid)
            {
                Vars.targetProximitySpeed[0] = float.MaxValue;
                Vars.targetProximitySpeed[1] = float.MaxValue;
                Vars.targetProximitySpeed[2] = float.MaxValue;
                return;
            }
            float[] distances = {
                AlchAss.Vars.closestPointDis[0],
                AlchAss.Vars.closestPointDis[1],
                Mathf.Min(AlchAss.Vars.closestPointDis[0], AlchAss.Vars.closestPointDis[1])
            };
            for (int i = 0; i < 3; i++)
                Vars.targetProximitySpeed[i] = CalculateControlSpeedFactor(distances[i]);
        }
        private static float CalculateControlSpeedFactor(float distance)
        {
            var normalizedDistance = distance / Vars._cachedControlAreaThreshold;
            var baseSpeedFactor = Mathf.Pow(normalizedDistance, Vars._cachedControlSlowdownStrength);
            var asymptoteCalc = normalizedDistance / (normalizedDistance + Vars._cachedControlAsymptoteFactor);
            return baseSpeedFactor * asymptoteCalc;
        }
        #endregion

        #region 定量操作
        public static void QuantiHeating()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                if (Managers.Cursor.hoveredInteractiveItem != null)
                    if (Managers.Cursor.hoveredInteractiveItem.GetType() == typeof(Bellows))
                    {
                        var coalManager = Managers.Ingredient.coals;
                        Helper.UpdateFunctionsConfigCache();
                        var heatValue = Vars._cachedHeatValue;
                        Traverse.Create(coalManager).Field("_heat").SetValue(heatValue / 100f);
                        Traverse.Create(coalManager).Method("Update", Array.Empty<object>()).GetValue();
                    }
        }
        public static void QuantiGrinding()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                if (Managers.Cursor.hoveredInteractiveItem != null)
                    if (Managers.Cursor.hoveredInteractiveItem.GetType() == typeof(Pestle))
                    {
                        var mortarManager = Managers.Ingredient.mortar;
                        if (mortarManager.ContainedStack != null)
                        {
                            Helper.UpdateFunctionsConfigCache();
                            var grindValue = Vars._cachedGrindValue;
                            mortarManager.ContainedStack.overallGrindStatus = grindValue / 100f;
                        }
                    }
        }
        #endregion
    }
}
