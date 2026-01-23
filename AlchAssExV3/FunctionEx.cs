using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ObjectBased.Bellows;
using PotionCraft.ObjectBased.Pestle;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssExV3
{
    public static class FunctionEx
    {
        #region 数据计算
        /// <summary>
        /// 更新手动变速
        /// </summary>
        public static void UpdateManualValue()
        {
            var sel = -1;
            for (var i = 0; i < 3; i++)
                if (VariableEx.KeyLevels[i].Value.IsPressed())
                    sel = i;

            VariableEx.GrindSet = sel < 0 ? -1f : VariableEx.ConfigGrindSet[sel].Value / 100f;
            VariableEx.HeatSet = sel < 0 ? -1f : VariableEx.ConfigHeatSet[sel].Value / 100f;
            VariableEx.GrindSpeed = sel < 0 ? 1f : VariableEx.ConfigGrindSpeed[sel].Value / 100f;
            VariableEx.StirSpeed = sel < 0 ? 1f : VariableEx.ConfigStirSpeed[sel].Value / 100f;
            VariableEx.LadleSpeed = sel < 0 ? 1f : VariableEx.ConfigLadleSpeed[sel].Value / 100f;
            VariableEx.HeatSpeed = sel < 0 ? 1f : VariableEx.ConfigHeatSpeed[sel].Value / 100f;
            VariableEx.BrewMassive = sel < 0 ? 1 : VariableEx.ConfigBrewMassive[sel].Value;

            if (!VariableEx.KeyRelease.Value.IsPressed())
            {
                VariableEx.StirSpeed *= Mathf.Clamp01(Mathf.Min(VariableEx.EdgeSpeed, VariableEx.ClosestSpeed[0], VariableEx.ProximitySpeed[0], VariableEx.StirSetSpeed));
                VariableEx.LadleSpeed *= Mathf.Clamp01(Mathf.Min(VariableEx.EdgeSpeed, VariableEx.ClosestSpeed[1], VariableEx.ProximitySpeed[1], VariableEx.LadleSetSpeed));
                VariableEx.HeatSpeed *= Mathf.Clamp01(Mathf.Min(VariableEx.ProximitySpeed));
            }
        }

        /// <summary>
        /// 计算制动速度
        /// </summary>
        public static float GetControlSpeed(float distance)
        {
            var normalizedDistance = distance / VariableEx.ControlThreshold.Value;
            var speedFactor = Mathf.Pow(normalizedDistance, VariableEx.ControlStrength.Value);
            return Mathf.Max(speedFactor, VariableEx.ControlMinSpeed.Value);
        }

        /// <summary>
        /// 判断是否能够进行批量炼药
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
        #endregion

        #region 操作控制
        /// <summary>
        /// 定量研磨
        /// </summary>
        public static void SetGrind()
        {
            if (VariableEx.GrindSet >= 0f && VariableEx.EnableGrindSet && Mouse.current.rightButton.wasPressedThisFrame && Managers.Cursor.hoveredInteractiveItem?.GetType() == typeof(Pestle))
                Managers.Ingredient.mortar.ContainedStack?.overallGrindStatus = VariableEx.GrindSet;
        }

        /// <summary>
        /// 定量加热
        /// </summary>
        public static void SetHeat()
        {
            if (VariableEx.HeatSet >= 0f && VariableEx.EnableHeatSet && Mouse.current.rightButton.wasPressedThisFrame && Managers.Cursor.hoveredInteractiveItem?.GetType() == typeof(Bellows))
            {
                var coal = Managers.Ingredient.coals;
                Traverse.Create(coal).Field("_heat").SetValue(VariableEx.HeatSet);
                Traverse.Create(coal).Method("Update", Array.Empty<object>()).GetValue();
            }
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化输入框
        /// </summary>
        public static void InitInputs()
        {
            for (int i = 0; i < 3; i++)
            {
                VariableEx.InputGrindSet[i] = ($"{VariableEx.ConfigGrindSet[i].Value}", false);
                VariableEx.InputHeatSet[i] = ($"{VariableEx.ConfigHeatSet[i].Value}", false);
                VariableEx.InputGrindSpeed[i] = ($"{VariableEx.ConfigGrindSpeed[i].Value}", false);
                VariableEx.InputStirSpeed[i] = ($"{VariableEx.ConfigStirSpeed[i].Value}", false);
                VariableEx.InputLadleSpeed[i] = ($"{VariableEx.ConfigLadleSpeed[i].Value}", false);
                VariableEx.InputHeatSpeed[i] = ($"{VariableEx.ConfigHeatSpeed[i].Value}", false);
                VariableEx.InputBrewMassive[i] = ($"{VariableEx.ConfigBrewMassive[i].Value}", false);
            }
        }
        #endregion
    }
}
