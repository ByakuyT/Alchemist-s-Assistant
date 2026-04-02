using AlchAssV3;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;

namespace AlchAssExV3
{
    [BepInPlugin("AlchAssExV3", "Alchemist's Assistant Extension V3", "2.6.0")]
    [BepInDependency("AlchAssV3", BepInDependency.DependencyFlags.HardDependency)]
    public class MainEx : BaseUnityPlugin
    {
        #region Unity - 生命周期
        /// <summary>
        /// 游戏初始化时
        /// </summary>
        public void Awake()
        {
            VariableEx.KeyManuals[0] = Config.Bind("快捷键", "手动控制配置 1", new KeyboardShortcut(KeyCode.LeftControl));
            VariableEx.KeyManuals[1] = Config.Bind("快捷键", "手动控制配置 2", new KeyboardShortcut(KeyCode.LeftAlt));
            VariableEx.KeyManuals[2] = Config.Bind("快捷键", "手动控制配置 3", new KeyboardShortcut(KeyCode.LeftShift));
            VariableEx.KeyAutoPause = Config.Bind("快捷键", "暂停自动控制", new KeyboardShortcut(KeyCode.Z));

            VariableEx.AutoThreshold = Config.Bind("自动控制参数", "距离阈值", 0.1f);
            VariableEx.AutoStrength = Config.Bind("自动控制参数", "制动强度", 1.5f);
            VariableEx.AutoCrossSpeed = Config.Bind("自动控制参数", "越界速度", 5e-4f);
            VariableEx.AutoMinSpeed = Config.Bind("自动控制参数", "最低速度", 0f);

            for (var i = 0; i < 3; i++)
            {
                VariableEx.ConfigGrindSpeed[i] = Config.Bind($"手动控制参数 {i + 1}", "研磨速度", VariableEx.Speed[i]);
                VariableEx.ConfigStirSpeed[i] = Config.Bind($"手动控制参数 {i + 1}", "搅拌速度", VariableEx.Speed[i]);
                VariableEx.ConfigLadleSpeed[i] = Config.Bind($"手动控制参数 {i + 1}", "加水速度", VariableEx.Speed[i]);
                VariableEx.ConfigHeatSpeed[i] = Config.Bind($"手动控制参数 {i + 1}", "加热速度", VariableEx.Speed[i]);
                VariableEx.ConfigBrewBulk[i] = Config.Bind($"手动控制参数 {i + 1}", "酿造倍率", VariableEx.Bulk[i]);
            }

            FunctionEx.InitInputs();
            LocalizationManager.OnInitialize.AddListener(FunctionEx.FormatLocalization);
            LocalizationManager.OnLocaleChanged.AddListener(FunctionEx.ResetLabelWidth);
            Harmony.CreateAndPatchAll(typeof(MainEx));
            Logger.LogInfo("Alchemist's Assistant Extension V3 插件已加载");
        }

        /// <summary>
        /// 每帧更新时
        /// </summary>
        public void Update()
        {
            FunctionEx.UpdateManualSpeedValue();
            FunctionEx.UpdateAutoSpeedValue();
            FunctionEx.QuantitativeHeating();
            FunctionEx.QuantitativeGrinding();
        }
        #endregion

        #region Patch - 手动减速操作
        /// <summary>
        /// 减速研磨
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        public static void SlowGrinding(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (VariableEx.EnableSlowGrinding)
            {
                pestleLinearSpeed *= VariableEx.GrindSpeed;
                pestleAngularSpeed *= VariableEx.GrindSpeed;
            }
        }

        /// <summary>
        /// 减速搅拌
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        public static void SlowStirring(ref float ___StirringValue)
        {
            if (VariableEx.EnableSlowStirring)
                ___StirringValue *= VariableEx.StirSpeed * VariableEx.AutoStirSpeed;
        }

        /// <summary>
        /// 减速加水
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        public static void SlowLadling(ref float __result)
        {
            if (VariableEx.EnableSlowLadling)
                __result *= VariableEx.LadleSpeed * VariableEx.AutoLadleSpeed;
        }

        /// <summary>
        /// 减速加热
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void SlowHeating(ref float __state)
        {
            var vortexSettings = Settings<RecipeMapManagerVortexSettings>.Asset;
            __state = vortexSettings.vortexMovementSpeed;
            if (VariableEx.EnableSlowHeating)
                vortexSettings.vortexMovementSpeed *= VariableEx.HeatSpeed * VariableEx.AutoHeatSpeed;
        }

        /// <summary>
        /// 减速加热回执
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void SpeedHeatEnd(float __state)
        {
            if (__state != 0f)
                Settings<RecipeMapManagerVortexSettings>.Asset.vortexMovementSpeed = __state;
        }
        #endregion

        #region Patch - 手动批量酿造
        /// <summary>
        /// 批量酿造
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeBookRecipeBrewController), "BrewRecipe")]
        public static void BulkBrewing(ref int count, IRecipeBookPageContent recipePageContent)
        {
            if (VariableEx.EnableBulkBrewing && count > 1 && FunctionEx.CanBrewTimes(recipePageContent, count, VariableEx.BrewBulk))
                count *= VariableEx.BrewBulk;
        }
        #endregion

        #region Patch - 自动控制
        /// <summary>
        /// 指示器更新时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void SpeedControl()
        {
            CalculationEx.GetQuantitativeSpeed();
            CalculationEx.GetEffectSpeed();
            CalculationEx.GetVortexSpeed();
            CalculationEx.GetDangerSpeed();
            CalculationEx.GetSwampSpeed();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetWhenLoading()
        {
            FunctionEx.ResetAutoSpeedValue();
        }
        #endregion

        #region Patch - 控制面板
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIWindow), "DrawExpansion")]
        public static void DrawExpansionUI()
        {
            FunctionEx.SetLabelWidth();
            UIWindowEx.DrawAutoEnables();
            UIWindowEx.DrawManualEnables();
            UIWindowEx.DrawAutoSettings();
            UIWindowEx.DrawManualSettings();
        }
        #endregion
    }
}
