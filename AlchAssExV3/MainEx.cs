using AlchAssV3;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;

namespace AlchAssExV3
{
    [BepInPlugin("AlchAssExV3", "Alchemist's Assistant Extension V3", "2.5.0")]
    [BepInDependency("AlchAssV3", BepInDependency.DependencyFlags.HardDependency)]
    public class MainEx : BaseUnityPlugin
    {
        #region Unity - 生命周期
        /// <summary>
        /// 游戏初始化时
        /// </summary>
        public void Awake()
        {
            VariableEx.KeyLevels[0] = Config.Bind("快捷键", "配置一", new KeyboardShortcut(KeyCode.LeftControl));
            VariableEx.KeyLevels[1] = Config.Bind("快捷键", "配置二", new KeyboardShortcut(KeyCode.LeftAlt));
            VariableEx.KeyLevels[2] = Config.Bind("快捷键", "配置三", new KeyboardShortcut(KeyCode.LeftShift));
            VariableEx.KeyRelease = Config.Bind("快捷键", "暂停制动", new KeyboardShortcut(KeyCode.Z));

            VariableEx.ControlThreshold = Config.Bind("制动配置", "制动阈值", 0.1f);
            VariableEx.ControlStrength = Config.Bind("制动配置", "制动强度", 1.5f);
            VariableEx.ControlEnterSpeed = Config.Bind("制动配置", "进入速度", 1e-3f);
            VariableEx.ControlMinSpeed = Config.Bind("制动配置", "最低速度", 0f);

            float[] set = [100f, 0f, 50f], speed = [10f, 1f, 0.1f]; int[] mass = [10, 20, 50];
            for (int i = 0; i < 3; i++)
            {
                VariableEx.ConfigGrindSet[i] = Config.Bind("操作控制", $"定量研磨 {i + 1}", set[i]);
                VariableEx.ConfigHeatSet[i] = Config.Bind("操作控制", $"定量加热 {i + 1}", set[i]);
                VariableEx.ConfigGrindSpeed[i] = Config.Bind("操作控制", $"研磨速度 {i + 1}", speed[i]);
                VariableEx.ConfigStirSpeed[i] = Config.Bind("操作控制", $"搅拌速度 {i + 1}", speed[i]);
                VariableEx.ConfigLadleSpeed[i] = Config.Bind("操作控制", $"加水速度 {i + 1}", speed[i]);
                VariableEx.ConfigHeatSpeed[i] = Config.Bind("操作控制", $"加热速度 {i + 1}", speed[i]);
                VariableEx.ConfigBrewMassive[i] = Config.Bind("操作控制", $"批量酿造 {i + 1}", mass[i]);
            }

            FunctionEx.InitInputs();
            LocalizationManager.OnInitialize.AddListener(FunctionEx.FormatLocalization);
            LocalizationManager.OnLocaleChanged.AddListener(FunctionEx.ClearLabelWidth);
            Harmony.CreateAndPatchAll(typeof(MainEx));
            Logger.LogInfo("Alchemist's Assistant Extension V3 插件已加载");
        }

        /// <summary>
        /// 每帧更新时
        /// </summary>
        public void Update()
        {
            FunctionEx.UpdateManualValue();
            FunctionEx.SetGrind();
            FunctionEx.SetHeat();
        }
        #endregion

        #region Patch - 减速操作
        /// <summary>
        /// 减速研磨
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        public static void SpeedGrind(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (VariableEx.EnableGrindSpeed)
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
        public static void SpeedStir(ref float ___StirringValue)
        {
            if (VariableEx.EnableStirSpeed)
                ___StirringValue *= VariableEx.StirSpeed;
        }

        /// <summary>
        /// 减速加水
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        public static void SpeedLadle(ref float __result)
        {
            if (VariableEx.EnableLadleSpeed)
                __result *= VariableEx.LadleSpeed;
        }

        /// <summary>
        /// 减速加热
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void SpeedHeat(ref float __state)
        {
            var vortexSettings = Settings<RecipeMapManagerVortexSettings>.Asset;
            __state = vortexSettings.vortexMovementSpeed;
            if (VariableEx.EnableHeatSpeed)
                vortexSettings.vortexMovementSpeed *= VariableEx.HeatSpeed;
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

        #region Patch - 批量酿造
        /// <summary>
        /// 批量酿造
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeBookRecipeBrewController), "BrewRecipe")]
        public static void MassBrewing(ref int count, IRecipeBookPageContent recipePageContent)
        {
            if (VariableEx.EnableBrewMassive && count > 1 && FunctionEx.CanBrewTimes(recipePageContent, count, VariableEx.BrewMassive))
                count *= VariableEx.BrewMassive;
        }
        #endregion

        #region Patch - 制动功能
        /// <summary>
        /// 指示器更新时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void SpeedControl()
        {
            CalculationEx.GetEdgeControl();
            CalculationEx.GetClosestControl();
            CalculationEx.GetProximityControl();
            CalculationEx.GetStirSet();
            CalculationEx.GetLadleSet();
        }

        /// <summary>
        /// 进入漩涡时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerEnter2D")]
        public static void VortexDistanceEnter()
        {
            if (VariableEx.LoadReset)
                VariableEx.LoadReset = false;
            else if (VariableEx.EnableEdgeControl)
            {
                VariableEx.EdgeSpeed = float.MinValue;
                VariableEx.EnterPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset;
            }
        }

        /// <summary>
        /// 离开漩涡时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerExit2D")]
        public static void VortexDistanceExit()
        {
            if (VariableEx.EnableEdgeControl)
                VariableEx.EdgeSpeed = float.MaxValue;
        }

        /// <summary>
        /// 读档时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetWhenLoading()
        {
            if (Managers.RecipeMap.CurrentVortexMapItem != null)
                VariableEx.LoadReset = true;
        }
        #endregion

        #region Patch - 控制面板
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIWindow), "DrawExpansion")]
        public static void DrawExpansionUI()
        {
            UIWindowEx.GetLabelWidth();
            UIWindowEx.DrawEnables();
            UIWindowEx.DrawSets();
            UIWindowEx.DrawLevels("默认数值配置一", 0, ref VariableEx.L1Expand);
            UIWindowEx.DrawLevels("默认数值配置二", 1, ref VariableEx.L2Expand);
            UIWindowEx.DrawLevels("默认数值配置三", 2, ref VariableEx.L3Expand);
        }
        #endregion
    }
}
