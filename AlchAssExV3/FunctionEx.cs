using AlchAssV3;
using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ObjectBased.Bellows;
using PotionCraft.ObjectBased.Pestle;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssExV3
{
    public static class FunctionEx
    {
        #region 初始化
        /// <summary>
        /// 初始化输入框
        /// </summary>
        public static void InitInputs()
        {
            for (int i = 0; i < 3; i++)
            {
                VariableEx.InputGrindSpeed[i] = ($"{VariableEx.ConfigGrindSpeed[i].Value}", false);
                VariableEx.InputStirSpeed[i] = ($"{VariableEx.ConfigStirSpeed[i].Value}", false);
                VariableEx.InputLadleSpeed[i] = ($"{VariableEx.ConfigLadleSpeed[i].Value}", false);
                VariableEx.InputHeatSpeed[i] = ($"{VariableEx.ConfigHeatSpeed[i].Value}", false);
                VariableEx.InputBrewBulk[i] = ($"{VariableEx.ConfigBrewBulk[i].Value}", false);
            }
            VariableEx.InputEffectDeviation = ($"{VariableEx.EffectDeviation}", false);
            VariableEx.InputHealthThreshold = ($"{VariableEx.HealthThreshold}", false);
            VariableEx.InputStirringLength = ($"{VariableEx.StirringLength}", false);
            VariableEx.InputLadlingLength = ($"{VariableEx.LadlingLength}", false);
            VariableEx.InputRestoringAngle = ($"{VariableEx.RestoringAngle}", false);
            VariableEx.InputGrindingTarget = ($"{VariableEx.GrindingTarget}", false);
            VariableEx.InputHeatingTarget = ($"{VariableEx.HeatingTarget}", false);
        }

        /// <summary>
        /// 格式化本地化文本
        /// </summary>
        public static void FormatLocalization()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Localization.RegisterLocalization("AlchAssExV3.Locs.Label.json", assembly);
            Localization.RegisterLocalization("AlchAssExV3.Locs.Button.json", assembly);
        }

        /// <summary>
        /// 重置标签长度
        /// </summary>
        public static void ResetLabelWidth()
        {
            VariableEx.LabelWidthAutos = float.NaN;
            VariableEx.LabelWidthManuals = float.NaN;
        }

        public static void SetLabelWidth()
        {
            if (float.IsNaN(VariableEx.LabelWidthAutos))
                VariableEx.LabelWidthAutos = Localization.GetLabelWidth(["效果偏离", "血量阈值", "搅拌长度", "研磨目标", "加热目标", "加水长度", "回正角度"], true);
            if (float.IsNaN(VariableEx.LabelWidthManuals))
                VariableEx.LabelWidthManuals = Localization.GetLabelWidth(["搅拌速度", "加水速度", "研磨速度", "加热速度", "酿造倍率"], true);
        }
        #endregion

        #region 定量操作
        /// <summary>
        /// 定量研磨
        /// </summary>
        public static void QuantitativeGrinding()
        {
            if (VariableEx.EnableQuantitativeGrinding && Mouse.current.rightButton.wasPressedThisFrame && Managers.Cursor.hoveredInteractiveItem?.GetType() == typeof(Pestle))
                Managers.Ingredient.mortar.ContainedStack?.overallGrindStatus = VariableEx.GrindingTarget / 100f;
        }

        /// <summary>
        /// 定量加热
        /// </summary>
        public static void QuantitativeHeating()
        {
            if (VariableEx.EnableQuantitativeHeating && Mouse.current.rightButton.wasPressedThisFrame && Managers.Cursor.hoveredInteractiveItem?.GetType() == typeof(Bellows))
            {
                var coal = Managers.Ingredient.coals;
                Traverse.Create(coal).Field("_heat").SetValue(VariableEx.HeatingTarget / 100f);
                Traverse.Create(coal).Method("Update", Array.Empty<object>()).GetValue();
            }
        }
        #endregion

        #region 数据计算
        /// <summary>
        /// 更新手动变速
        /// </summary>
        public static void UpdateManualSpeedValue()
        {
            var sel = -1;
            for (var i = 0; i < 3; i++)
                if (VariableEx.KeyManuals[i].Value.IsPressed())
                    sel = i;

            VariableEx.GrindSpeed = sel < 0 ? 1f : VariableEx.ConfigGrindSpeed[sel].Value / 100f;
            VariableEx.StirSpeed = sel < 0 ? 1f : VariableEx.ConfigStirSpeed[sel].Value / 100f;
            VariableEx.LadleSpeed = sel < 0 ? 1f : VariableEx.ConfigLadleSpeed[sel].Value / 100f;
            VariableEx.HeatSpeed = sel < 0 ? 1f : VariableEx.ConfigHeatSpeed[sel].Value / 100f;
            VariableEx.BrewBulk = sel < 0 ? 1 : VariableEx.ConfigBrewBulk[sel].Value;
        }

        /// <summary>
        /// 更新自动变速
        /// </summary>
        public static void UpdateAutoSpeedValue()
        {
            if (!VariableEx.KeyAutoPause.Value.IsPressed())
            {
                float[] stirSpeed = [VariableEx.SpeedEffect, VariableEx.SpeedVortex,VariableEx.SpeedDanger,VariableEx.SpeedSwamp,
                    VariableEx.StirSpeedEffect, VariableEx.StirSpeedVortex,VariableEx.StirSpeedDanger,
                    VariableEx.StirSpeedQuantitative];
                float[] ladleSpeed = [VariableEx.SpeedEffect, VariableEx.SpeedVortex,VariableEx.SpeedDanger,
                    VariableEx.LadleSpeedEffect, VariableEx.LadleSpeedVortex,VariableEx.LadleSpeedDanger,VariableEx.LadleSpeedSwamp,
                    VariableEx.LadleSpeedQuantitative, VariableEx.LadleSpeedRestoring];
                float[] heatSpeed = [VariableEx.SpeedEffect,VariableEx.SpeedDanger,
                    VariableEx.HeatSpeedEffect, VariableEx.HeatSpeedVortex,VariableEx.HeatSpeedDanger,VariableEx.HeatSpeedSwamp];

                VariableEx.AutoStirSpeed = Mathf.Clamp01(Mathf.Min(stirSpeed));
                VariableEx.AutoLadleSpeed = Mathf.Clamp01(Mathf.Min(ladleSpeed));
                VariableEx.AutoHeatSpeed = Mathf.Clamp01(Mathf.Min(heatSpeed));
            }
            else
                ResetAutoSpeedValue();
        }

        /// <summary>
        /// 重置自动变速
        /// </summary>
        public static void ResetAutoSpeedValue()
        {
            VariableEx.AutoStirSpeed = 1f;
            VariableEx.AutoLadleSpeed = 1f;
            VariableEx.AutoHeatSpeed = 1f;

            VariableEx.EffectIntersectionPrevious = false;
            VariableEx.VortexIntersectionPrevious = false;
            VariableEx.DangerIntersectionPrevious = false;
            VariableEx.SwampIntersectionPrevious = false;
            VariableEx.EffectAlignmentStirPrevious = false;
            VariableEx.VortexAlignmentStirPrevious = false;
            VariableEx.DangerAlignmentStirPrevious = false;
            VariableEx.SwampAlignmentStirPrevious = false;
            VariableEx.EffectAlignmentLadlePrevious = false;
            VariableEx.VortexAlignmentLadlePrevious = false;
            VariableEx.DangerAlignmentLadlePrevious = false;
            VariableEx.DangerAlignmentHeatPrevious = false;
            VariableEx.DangerOutStirPrevious = false;
            VariableEx.DangerOutLadlePrevious = false;
            VariableEx.DangerOutHeatPrevious = false;
            VariableEx.SwampOutStirPrevious = false;
        }

        /// <summary>
        /// 计算自动控制速度
        /// </summary>
        public static float GetAutoControlSpeed(float dis)
        {
            if (dis <= 0) return 0f;
            var normDis = dis / VariableEx.AutoThreshold.Value;
            var speed = Mathf.Pow(normDis, VariableEx.AutoStrength.Value);
            return Mathf.Max(speed, VariableEx.AutoMinSpeed.Value);
        }

        /// <summary>
        /// 判断是否能够进行批量酿造
        /// </summary>
        public static bool CanBrewTimes(IRecipeBookPageContent recipe, int count, int times)
        {
            var require = RecipeBookRecipeBrewController.GetUsedDuringBrewingIngredientsAmount(
                recipe.GetComponentsToUseInBrewWithPreparedIngredients(),
                recipe.GetComponentsToUseInBrewWithoutPreparedIngredients(),
                count * times, true);
            foreach (var item in require)
                if (item.Type == AlchemySubstanceComponentType.InventoryItem)
                {
                    var invItem = item.Component as InventoryItem;
                    if (invItem == null) continue;
                    var amount = Managers.Player.Inventory.GetItemCount(invItem);
                    if (amount < item.Amount) return false;
                }
            return true;
        }

        /// <summary>
        /// 映射 0-1 的范围到数值
        /// </summary>
        public static float NormalizedToValue(float t, float[] pars)
        {
            var l = pars.Length - 1;
            for (var i = 1; i < l; i++)
                if (t <= (float)i / l)
                    return Mathf.Lerp(pars[i - 1], pars[i], (t - (float)(i - 1) / l) * l);
            return Mathf.Lerp(pars[l - 1], pars[l], (t - (float)(l - 1) / l) * l);
        }

        /// <summary>
        /// 映射数值到 0-1 的范围
        /// </summary>
        public static float ValueToNormalized(float v, float[] pars)
        {
            var l = pars.Length - 1;
            for (var i = 1; i < l; i++)
                if (v <= pars[i])
                    return ((float)(i - 1) / l) + Mathf.InverseLerp(pars[i - 1], pars[i], v) * (1f / l);
            return ((float)(l - 1) / l) + Mathf.InverseLerp(pars[l - 1], pars[l], v) * (1f / l);
        }
    }
    #endregion
}
