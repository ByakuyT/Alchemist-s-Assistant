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
    public static class Functions
    {
        #region 自动制动
        public static void VortexEdgeControl()
        {
            if (Keyboard.current.quoteKey.wasPressedThisFrame)
            {
                if (Variables.vortexEdgeControl)
                {
                    Variables.vortexEdgeControl = false;
                    Variables.vortexEdgeSpeed = float.MaxValue;
                }
                else
                {
                    Depends.UpdateFunctionsConfigCache();
                    Variables.vortexEdgeControl = true;
                }
                AlchAss.Depends.SpawnMessageText(LocalizationManager.GetText("avortex") + LocalizationManager.GetText(Variables.vortexEdgeControl ? "aopen" : "aclose"));
            }
        }
        public static void ClosestPointControl()
        {
            if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
            {
                if (Variables.closestPointControl)
                {
                    Variables.closestPointControl = false;
                    Variables.closestPointspeed[0] = float.MaxValue;
                    Variables.closestPointspeed[1] = float.MaxValue;
                }
                else
                {
                    Depends.UpdateFunctionsConfigCache();
                    Variables.closestPointControl = true;
                }
                AlchAss.Depends.SpawnMessageText(LocalizationManager.GetText("aclosest") + LocalizationManager.GetText(Variables.closestPointControl ? "aopen" : "aclose"));
            }
        }
        public static void TargetProximityControl()
        {
            if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
            {
                if (Variables.targetProximityControl)
                {
                    Variables.targetProximityControl = false;
                    Variables.targetProximitySpeed[0] = float.MaxValue;
                    Variables.targetProximitySpeed[1] = float.MaxValue;
                    Variables.targetProximitySpeed[2] = float.MaxValue;
                }
                else
                {
                    Depends.UpdateFunctionsConfigCache();
                    Variables.targetProximityControl = true;
                }
                AlchAss.Depends.SpawnMessageText(LocalizationManager.GetText("aproximity") + LocalizationManager.GetText(Variables.targetProximityControl ? "aopen" : "aclose"));
            }
        }
        public static void UpdateVortexEdgeControl()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem == null || !Variables.vortexEdgeControl)
                return;
            if (Variables.vortexEdgeSpeed < 0f)
                return;
            var vortexCenter = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var vortexRadius = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius;
            var distance = (vortexCenter - indicatorPosition).magnitude;
            var maxDistance = vortexRadius + AlchAss.Variables.PotionBottleRadius;
            var distanceToEdge = maxDistance - distance;
            Variables.vortexEdgeSpeed = CalculateControlSpeedFactor(distanceToEdge);
        }
        public static void UpdateClosestPointControl()
        {
            if (!Variables.closestPointControl)
                return;
            if (!AlchAss.Variables.SharedCache.isValid)
            {
                Variables.closestPointspeed[0] = float.MaxValue;
                Variables.closestPointspeed[1] = float.MaxValue;
                return;
            }
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            for (int i = 0; i < 2; i++)
            {
                if (AlchAss.Variables.closestPoints[i].HasValue)
                {
                    var distance = Vector2.Distance(indicatorPosition, AlchAss.Variables.closestPoints[i].Value);
                    Variables.closestPointspeed[i] = CalculateControlSpeedFactor(distance);
                }
                else
                    Variables.closestPointspeed[i] = float.MaxValue;
            }
        }
        public static void UpdateTargetProximityControl()
        {
            if (!Variables.targetProximityControl)
            {
                Variables.targetProximitySpeed[0] = float.MaxValue;
                Variables.targetProximitySpeed[1] = float.MaxValue;
                Variables.targetProximitySpeed[2] = float.MaxValue;
                return;
            }
            if (!AlchAss.Variables.SharedCache.isValid)
            {
                Variables.targetProximitySpeed[0] = float.MaxValue;
                Variables.targetProximitySpeed[1] = float.MaxValue;
                Variables.targetProximitySpeed[2] = float.MaxValue;
                return;
            }
            float[] distances = [
                AlchAss.Variables.closestPointDis[0],
                AlchAss.Variables.closestPointDis[1],
                Mathf.Min(AlchAss.Variables.closestPointDis[0], AlchAss.Variables.closestPointDis[1])
            ];
            for (int i = 0; i < 3; i++)
                Variables.targetProximitySpeed[i] = CalculateControlSpeedFactor(distances[i]);
        }
        public static float CalculateControlSpeedFactor(float distance)
        {
            var normalizedDistance = distance / Variables._cachedControlAreaThreshold;
            var baseSpeedFactor = Mathf.Pow(normalizedDistance, Variables._cachedControlSlowdownStrength);
            var asymptoteCalc = normalizedDistance / (normalizedDistance + Variables._cachedControlAsymptoteFactor);
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
                        Depends.UpdateFunctionsConfigCache();
                        var heatValue = Variables._cachedHeatValue;
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
                            Depends.UpdateFunctionsConfigCache();
                            var grindValue = Variables._cachedGrindValue;
                            mortarManager.ContainedStack.overallGrindStatus = grindValue / 100f;
                        }
                    }
        }
        #endregion
    }
}
