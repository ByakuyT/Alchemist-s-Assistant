using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Bellows;
using PotionCraft.ObjectBased.Pestle;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssExV3
{
    public static class Function
    {
        #region 模式切换
        public static void VortexEdgeControl()
        {
            if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyVortexEdgeControl.Value.IsDown())
            {
                Variable.vortexEdgeControl = !Variable.vortexEdgeControl;
                //AlchAssV3.Function.SpawnMessageText($"""Vortex edge control {(Variable.vortexEdgeControl?"enabled":"disabled")}.""");
                AlchAssV3.Function.SpawnMessageText(LocalizationManager.GetText("AlchAssV3Ex_vortex_edge_control") + (Variable.vortexEdgeControl ? LocalizationManager.GetText("AlchAssV3_enabled") : LocalizationManager.GetText("AlchAssV3_disabled")) + ".");
                if (!Variable.vortexEdgeControl)
                {
                    Variable.vortexEdgeSpeed = float.MaxValue;
                }
            }
        }
        public static void ClosestPointControl()
        {
            if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyClosestPointControl.Value.IsDown())
            {
                Variable.closestPointControl = !Variable.closestPointControl;
                //AlchAssV3.Function.SpawnMessageText($""" Closest point control {(Variable.closestPointControl ? "enabled" : "disabled")}.""");
                AlchAssV3.Function.SpawnMessageText(LocalizationManager.GetText("AlchAssV3Ex_closest_point_control")+ (Variable.closestPointControl ? LocalizationManager.GetText("AlchAssV3_enabled") : LocalizationManager.GetText("AlchAssV3_disabled")) + ".");
                if (!Variable.closestPointControl)
                {
                    Variable.closestPointspeed[0] = float.MaxValue;
                    Variable.closestPointspeed[1] = float.MaxValue;
                }
            }
        }
        public static void TargetProximityControl()
        {
            if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyTargetProximityControl.Value.IsDown())
            {
                Variable.targetProximityControl = !Variable.targetProximityControl;
                //AlchAssV3.Function.SpawnMessageText($""" Target proximity control {(Variable.targetProximityControl? "enabled" : "disabled")}.""");
                AlchAssV3.Function.SpawnMessageText(LocalizationManager.GetText("AlchAssV3Ex_target_proximity_control") + (Variable.targetProximityControl ? LocalizationManager.GetText("AlchAssV3_enabled") : LocalizationManager.GetText("AlchAssV3_disabled")) + ".");
                if (!Variable.targetProximityControl)
                {
                    Variable.targetProximitySpeed[0] = float.MaxValue;
                    Variable.targetProximitySpeed[1] = float.MaxValue;
                    Variable.targetProximitySpeed[2] = float.MaxValue;
                }
            }
        }
        #endregion

        #region 自动制动
        public static void UpdateVortexEdgeControl()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem == null || !Variable.vortexEdgeControl)
                return;
            if (Variable.vortexEdgeSpeed < 0f)
                return;
            var vortexCenter = Managers.RecipeMap.CurrentVortexMapItem.thisTransform.localPosition;
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            var vortexRadius = ((CircleCollider2D)Traverse.Create(Managers.RecipeMap.CurrentVortexMapItem).Field("vortexCollider").GetValue()).radius;
            var distance = (vortexCenter - indicatorPosition).magnitude;
            var maxDistance = vortexRadius + (float)AlchAssV3.Variable.IndicatorRadius;
            var distanceToEdge = maxDistance - distance;
            Variable.vortexEdgeSpeed = Depend.CalculateControlSpeedFactor(distanceToEdge);
        }
        public static void UpdateClosestPointControl()
        {
            if (!Variable.closestPointControl)
                return;
            if (AlchAssV3.Variable.TargetEffect == null)
            {
                Variable.closestPointspeed[0] = float.MaxValue;
                Variable.closestPointspeed[1] = float.MaxValue;
                return;
            }
            var indicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            for (int i = 0; i < 2; i++)
            {
                if (!float.IsNaN(AlchAssV3.Variable.ClosestPositions[i].x))
                {
                    var distance = Vector2.Distance(indicatorPosition, AlchAssV3.Variable.ClosestPositions[i]);
                    Variable.closestPointspeed[i] = Depend.CalculateControlSpeedFactor(distance);
                }
                else
                    Variable.closestPointspeed[i] = float.MaxValue;
            }
        }
        public static void UpdateTargetProximityControl()
        {
            if (!Variable.targetProximityControl)
            {
                Variable.targetProximitySpeed[0] = float.MaxValue;
                Variable.targetProximitySpeed[1] = float.MaxValue;
                Variable.targetProximitySpeed[2] = float.MaxValue;
                return;
            }
            if (AlchAssV3.Variable.TargetEffect == null)
            {
                Variable.targetProximitySpeed[0] = float.MaxValue;
                Variable.targetProximitySpeed[1] = float.MaxValue;
                Variable.targetProximitySpeed[2] = float.MaxValue;
                return;
            }
            Vector2 targetPosition = AlchAssV3.Variable.TargetEffect.transform.localPosition;
            float dist0 = Vector2.Distance(AlchAssV3.Variable.ClosestPositions[0], targetPosition);
            float dist1 = Vector2.Distance(AlchAssV3.Variable.ClosestPositions[1], targetPosition);
            float[] distances = [dist0, dist1, Mathf.Min(dist0, dist1)];
            for (int i = 0; i < 3; i++)
                Variable.targetProximitySpeed[i] = float.IsNaN(distances[i]) ? float.MaxValue : Depend.CalculateControlSpeedFactor(distances[i]);
        }
        #endregion

        #region 定量操作
        public static void QuantiHeating()
        {
            if (AlchAssV3.Variable.KeyMode == "Normal" && Mouse.current.rightButton.wasPressedThisFrame && Managers.Cursor.hoveredInteractiveItem.GetType() == typeof(Bellows))
            {
                var coalManager = Managers.Ingredient.coals;
                if (AlchAssV3.Variable.FloatInput.Count > 0)
                {
                    Variable.HeatValue = (float)AlchAssV3.Variable.FloatInput.Dequeue();
                    Variable.HeatValue = Mathf.Clamp(Variable.HeatValue, 0f, 100f);
                }
                Traverse.Create(coalManager).Field("_heat").SetValue(Variable.HeatValue / 100f);
                Traverse.Create(coalManager).Method("Update", Array.Empty<object>()).GetValue();
            }
        }
        public static void QuantiGrinding()
        {
            if (AlchAssV3.Variable.KeyMode == "Normal" && Mouse.current.rightButton.wasPressedThisFrame && Managers.Cursor.hoveredInteractiveItem.GetType() == typeof(Pestle))
            {
                var mortarManager = Managers.Ingredient.mortar;
                if (AlchAssV3.Variable.FloatInput.Count > 0)
                {
                    Variable.GrindValue = (float)AlchAssV3.Variable.FloatInput.Dequeue();
                    Variable.GrindValue = Mathf.Clamp(Variable.GrindValue, 0f, 100f);
                }
                if (mortarManager.ContainedStack != null)
                {
                    mortarManager.ContainedStack.overallGrindStatus = Variable.GrindValue / 100f;
                }
            }
        }
        #endregion
    }
}
