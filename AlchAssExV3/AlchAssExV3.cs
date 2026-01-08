using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
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

namespace AlchAssExV3
{
    [BepInPlugin("AlchAssExV3", "Alchemist's Assistant Extension V3", "1.0.0")]
    [BepInDependency("AlchAssV3", BepInDependency.DependencyFlags.HardDependency)]
    public class AlchAssEx : BaseUnityPlugin
    {
        #region Unity - 生命周期
        public void Awake()
        {
            Variable.QuantiHeating = Config.Bind("操作控制", "允许定量加热", true,
                "右键风箱设置热量值 | Right-click bellows to set heat value");
            Variable.QuantiGrinding = Config.Bind("操作控制", "允许定量研磨", true,
                "右键研杵设置研磨度 | Right-click pestle to set grinding level");
            Variable.enableVortexEdgeControl = Config.Bind("操作控制", "允许漩涡制动", true,
                "允许漩涡边缘减速 | Allow vortex edge slowdown");
            Variable.enableClosestPointControl = Config.Bind("操作控制", "允许最近点制动", true,
                "允许最近点减速 | Allow closest point slowdown");
            Variable.enableTargetProximityControl = Config.Bind("操作控制", "允许目标接近点制动", true,
                "允许目标接近点减速 | Allow target proximity slowdown");
            Variable.enableGrindSpeed = Config.Bind("操作控制", "允许研磨减速", true,
                "允许进行轻度和重度的研磨减速 | Allow minor and major slowdown for grinding.");
            Variable.enableStirSpeed = Config.Bind("操作控制", "允许搅拌减速", true,
                "允许进行轻度和重度的搅拌减速 | Allow minor and major slowdown for stirring.");
            Variable.enableLadleSpeed = Config.Bind("操作控制", "允许加水减速", true,
                "允许进行轻度和重度的加水减速 | Allow minor and major slowdown for pouring solvent.");
            Variable.enableHeatSpeed = Config.Bind("操作控制", "允许加热减速", true,
                "允许进行轻度和重度的加热减速 | Allow minor and major slowdown for heating.");
            Variable.enableMassBrewing = Config.Bind("操作控制", "允许大批炼药", true,
                "允许进行小批量和大批量的批量炼药 | Allow minor and major batch brewing.");

            Variable.KeyVortexEdgeControl = Config.Bind("快捷键", "漩涡制动键", new KeyboardShortcut(KeyCode.Quote),
                "启用/禁用漩涡边缘制动的快捷键 | Hotkey to toggle vortex edge control");
            Variable.KeyClosestPointControl = Config.Bind("快捷键", "最近点制动键", new KeyboardShortcut(KeyCode.LeftBracket),
                "启用/禁用最近点制动的快捷键 | Hotkey to toggle closest point control");
            Variable.KeyTargetProximityControl = Config.Bind("快捷键", "目标接近点制动键", new KeyboardShortcut(KeyCode.RightBracket),
                "启用/禁用目标接近点制动的快捷键 | Hotkey to toggle target proximity control");
            Variable.KeyMinorSlowDown = Config.Bind("快捷键", "轻度减速和小批量制作键", new KeyboardShortcut(KeyCode.Z),
                "进行轻度手动减速和小批量制作的快捷键 | Hotkey for minor manual slowdown and minor batch brewing");
            Variable.KeyMajorSlowDown = Config.Bind("快捷键", "重度减速和大批量制作键", new KeyboardShortcut(KeyCode.X),
                "进行重度手动减速和大批量制作的快捷键 | Hotkey for major manual slowdown and major batch brewing");

            Variable.ControlAreaThreshold = Config.Bind("数值设置", "控制区域半径", 0.05f,
                "设置制动控制区域的半径 (数值越大控制区域越大) | Control area threshold (higher = larger control area)");
            Variable.ControlSlowdownStrength = Config.Bind("数值设置", "控制减速强度", 1.35f,
                "设置制动控制的减速强度 (数值越大减速越剧烈) | Control slowdown strength (higher = more aggressive slowdown)");
            Variable.ControlAsymptoteFactor = Config.Bind("数值设置", "控制减速渐近因子", 0.002f,
                "设置制动控制的减速渐近因子 (数值越大越平滑) | Control slowdown asymptote factor (higher = smoother)");
            Variable.MajorSlowdownFactor = Config.Bind("数值设置", "显著减速倍数", 100f,
                "设置显著减速的倍数 (数值越大越慢) | Major Slowdown factor (higher = slower)");
            Variable.MinorSlowdownFactor = Config.Bind("数值设置", "轻微减速倍数", 10f,
                "设置轻微减速的倍数 (数值越大越慢) | Minor Slowdown factor (higher = slower)");
            Variable.MajorBrewingMultiplier = Config.Bind("数值设置", "大批量制作倍数", 100,
                "设置大批量炼药倍数 | Major Brewing multiplier");
            Variable.MinorBrewingMultiplier = Config.Bind("数值设置", "小批量制作倍数", 10,
                "设置小批量炼药倍数 | Minor Brewing multiplier");
            Harmony.CreateAndPatchAll(typeof(AlchAssEx));
            LocalizationManager.OnInitialize.AddListener(Localization.SetAllLocalizations);
            Logger.LogInfo("Alchemist's Assistant Extension 加载完成！");
        }
        public void Update()
        {
            if (Variable.QuantiHeating.Value)
                Function.QuantiHeating();
            if (Variable.QuantiGrinding.Value)
                Function.QuantiGrinding();
            if (Variable.enableVortexEdgeControl.Value)
                Function.VortexEdgeControl();
            if (Variable.enableClosestPointControl.Value)
                Function.ClosestPointControl();
            if (Variable.enableTargetProximityControl.Value)
                Function.TargetProximityControl();
        }
        #endregion

        #region Harmony Patch - 慢速操作
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        public static void GrindSlowDown(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (Variable.enableGrindSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMajorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MajorSlowdownFactor.Value;
                else if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMinorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MinorSlowdownFactor.Value;
                pestleLinearSpeed /= manualSlowdownFactor;
                pestleAngularSpeed /= manualSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        public static void StirSlowDown(ref float ___StirringValue)
        {
            if (Variable.enableStirSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMajorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MajorSlowdownFactor.Value;
                else if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMinorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MinorSlowdownFactor.Value;
                ___StirringValue /= manualSlowdownFactor;
            }
            if (Variable.enableVortexEdgeControl.Value || Variable.enableClosestPointControl.Value || Variable.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Variable.vortexEdgeControl)
                    minSlowdownFactor = Mathf.Clamp01(Variable.vortexEdgeSpeed);
                if (Variable.closestPointControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variable.closestPointspeed[0]));
                if (Variable.targetProximityControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variable.targetProximitySpeed[1]));
                ___StirringValue *= minSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        public static void LadleSlowDown(ref float __result)
        {
            if (Variable.enableLadleSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMajorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MajorSlowdownFactor.Value;
                else if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMinorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MinorSlowdownFactor.Value;
                __result /= manualSlowdownFactor;
            }
            if (Variable.enableVortexEdgeControl.Value || Variable.enableClosestPointControl.Value || Variable.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Variable.vortexEdgeControl)
                    minSlowdownFactor = Mathf.Clamp01(Variable.vortexEdgeSpeed);
                if (Variable.closestPointControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variable.closestPointspeed[1]));
                if (Variable.targetProximityControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Variable.targetProximitySpeed[0]));
                __result *= minSlowdownFactor;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void HeatSlowDown(ref float __state)
        {
            var vortexSettings = Settings<RecipeMapManagerVortexSettings>.Asset;
            __state = vortexSettings.vortexMovementSpeed;
            if (Variable.enableHeatSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMajorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MajorSlowdownFactor.Value;
                else if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMinorSlowDown.Value.IsPressed())
                    manualSlowdownFactor = Variable.MinorSlowdownFactor.Value;
                vortexSettings.vortexMovementSpeed /= manualSlowdownFactor;
            }
            if (Variable.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Variable.targetProximityControl)
                    minSlowdownFactor = Mathf.Clamp01(Variable.targetProximitySpeed[2]);
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
            if (Variable.enableMassBrewing.Value)
                if (count > 1)
                {
                    var brewingTimes = 1;
                    if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMajorSlowDown.Value.IsPressed())
                        brewingTimes = Variable.MajorBrewingMultiplier.Value;
                    else if (AlchAssV3.Variable.KeyMode == "Normal" && Variable.KeyMinorSlowDown.Value.IsPressed())
                        brewingTimes = Variable.MinorBrewingMultiplier.Value;
                    if (Depend.CanBrewTimes(recipePageContent, count, brewingTimes))
                        count *= brewingTimes;
                }
        }
        #endregion

        #region Harmony Patch - 制动功能
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void PotionStatus()
        {
            Function.UpdateVortexEdgeControl();
            Function.UpdateClosestPointControl();
            Function.UpdateTargetProximityControl();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerEnter2D")]
        public static void VortexDistanceEnter()
        {
            if (Variable.resetWhenLoading)
            {
                Variable.resetWhenLoading = false;
                return;
            }
            if (Variable.vortexEdgeControl)
                Variable.vortexEdgeSpeed = float.MinValue;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerExit2D")]
        public static void VortexDistanceExit()
        {
            if (Variable.vortexEdgeControl)
                Variable.vortexEdgeSpeed = float.MaxValue;
        }
        #endregion

        #region Harmony Patch - 工具方法
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetWhenLoading()
        {
            Variable.resetWhenLoading = true;
        }
        #endregion
    }
}
