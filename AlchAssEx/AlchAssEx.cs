using System;
using BepInEx;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssEx
{
    [BepInPlugin("AlchAssEx", "Alchemist's Assistant Extension", "4.0.1")]
    [BepInDependency("AlchAss", BepInDependency.DependencyFlags.HardDependency)]
    public class AlchAssEx : BaseUnityPlugin
    {
        #region Unity - 生命周期
        public void Awake()
        {
            Variables.QuantiHeating = Config.Bind("操作控制", "允许定量加热", true,
                "右键风箱设置热量值 | Right-click bellows to set heat value");
            Variables.QuantiGrinding = Config.Bind("操作控制", "允许定量研磨", true,
                "右键研杵设置研磨度 | Right-click pestle to set grinding level");
            Variables.enableVortexEdgeControl = Config.Bind("操作控制", "允许漩涡制动", true,
                "按'键启用漩涡边缘减速 | Press ' to enable vortex edge slowdown");
            Variables.enableClosestPointControl = Config.Bind("操作控制", "允许最近点制动", true,
                "按]键启用最近点减速 | Press ] to enable closest point slowdown");
            Variables.enableTargetProximityControl = Config.Bind("操作控制", "允许目标接近点制动", true,
                "按[键启用目标接近点减速 | Press [ to enable target proximity slowdown");
            Variables.enableGrindSpeed = Config.Bind("操作控制", "允许研磨减速", true,
                "按Z/X键减速研磨 | Press Z/X to slow down grinding");
            Variables.enableStirSpeed = Config.Bind("操作控制", "允许搅拌减速", true,
                "按Z/X键减速搅拌 | Press Z/X to slow down stirring");
            Variables.enableLadleSpeed = Config.Bind("操作控制", "允许加水减速", true,
                "按Z/X键减速加水 | Press Z/X to slow down ladling");
            Variables.enableHeatSpeed = Config.Bind("操作控制", "允许加热减速", true,
                "按Z/X键减速加热 | Press Z/X to slow down heating");
            Variables.enableMassBrewing = Config.Bind("操作控制", "允许大批炼药", true,
                "按Z/X键批量制作药剂 | Press Z/X to batch brew potions");

            Depends.UpdateFunctionsConfigCache();
            LocalizationManager.OnInitialize.AddListener(Depends.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAssEx));
            Logger.LogInfo("Alchemist's Assistant Extension 加载完成！");
        }
        public void Update()
        {
            if (Keyboard.current.xKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame)
                Depends.UpdateFunctionsConfigCache();
            if (Variables.QuantiHeating.Value)
                Functions.QuantiHeating();
            if (Variables.QuantiGrinding.Value)
                Functions.QuantiGrinding();
            if (Variables.enableVortexEdgeControl.Value)
                Functions.VortexEdgeControl();
            if (Variables.enableClosestPointControl.Value)
                Functions.ClosestPointControl();
            if (Variables.enableTargetProximityControl.Value)
                Functions.TargetProximityControl();
        }
        #endregion

        #region Harmony Patch - 慢速操作
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        public static void GrindSlowDown(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (Variables.enableGrindSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorZ;
                pestleLinearSpeed /= manualSlowdownFactor;
                pestleAngularSpeed /= manualSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        public static void StirSlowDown(ref float ___StirringValue)
        {
            if (Variables.enableStirSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorZ;
                ___StirringValue /= manualSlowdownFactor;
            }
            if (Variables.enableVortexEdgeControl.Value || Variables.enableClosestPointControl.Value || Variables.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Variables.vortexEdgeControl)
                    minSlowdownFactor = Mathf.Clamp01(Variables.vortexEdgeSpeed);
                if (Variables.closestPointControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variables.closestPointspeed[0]));
                if (Variables.targetProximityControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variables.targetProximitySpeed[1]));
                ___StirringValue *= minSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        public static void LadleSlowDown(ref float __result)
        {
            if (Variables.enableLadleSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorZ;
                __result /= manualSlowdownFactor;
            }
            if (Variables.enableVortexEdgeControl.Value || Variables.enableClosestPointControl.Value || Variables.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Variables.vortexEdgeControl)
                    minSlowdownFactor = Mathf.Clamp01(Variables.vortexEdgeSpeed);
                if (Variables.closestPointControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variables.closestPointspeed[1]));
                if (Variables.targetProximityControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variables.targetProximitySpeed[0]));
                __result *= minSlowdownFactor;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void HeatSlowDown(ref float __state)
        {
            var vortexSettings = Settings<RecipeMapManagerVortexSettings>.Asset;
            __state = vortexSettings.vortexMovementSpeed;
            if (Variables.enableHeatSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Variables._cachedSlowdownFactorZ;
                vortexSettings.vortexMovementSpeed /= manualSlowdownFactor;
            }
            if (Variables.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Variables.targetProximityControl)
                    minSlowdownFactor = Mathf.Clamp01(Variables.targetProximitySpeed[2]);
                vortexSettings.vortexMovementSpeed *= minSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void HeatSlowDownEnd(float __state)
        {
            if (__state != 0f)
                Settings<RecipeMapManagerVortexSettings>.Asset.vortexMovementSpeed = __state;
        }
        #endregion

        #region Harmony Patch - 批量调制
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeBookRecipeBrewController), "BrewRecipe")]
        public static void MassBrewing(ref int count, IRecipeBookPageContent recipePageContent)
        {
            if (Variables.enableMassBrewing.Value)
                if (count > 1)
                {
                    var brewingTimes = 1;
                    if (Keyboard.current.xKey.isPressed)
                        brewingTimes = Variables._cachedBrewingMultiplierX;
                    else if (Keyboard.current.zKey.isPressed)
                        brewingTimes = Variables._cachedBrewingMultiplierZ;
                    if (Depends.CanBrewTimes(recipePageContent, count, brewingTimes))
                        count *= brewingTimes;
                }
        }
        #endregion

        #region Harmony Patch - 制动功能
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void PotionStatus()
        {
            Functions.UpdateVortexEdgeControl();
            Functions.UpdateClosestPointControl();
            Functions.UpdateTargetProximityControl();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerEnter2D")]
        public static void VortexDistanceEnter()
        {
            if (Variables.resetWhenLoading)
            {
                Variables.resetWhenLoading = false;
                return;
            }
            if (Variables.vortexEdgeControl)
                Variables.vortexEdgeSpeed = float.MinValue;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerExit2D")]
        public static void VortexDistanceExit()
        {
            if (Variables.vortexEdgeControl)
                Variables.vortexEdgeSpeed = float.MaxValue;
        }
        #endregion

        #region Harmony Patch - 工具方法
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetWhenLoading()
        {
            Variables.resetWhenLoading = true;
        }
        #endregion
    }
}
