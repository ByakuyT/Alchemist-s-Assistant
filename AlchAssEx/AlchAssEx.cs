using BepInEx;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.RecipeMap;
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
    [BepInPlugin("AlchAssEx", "Alchemist's Assistant Extension", "4.0.0")]
    [BepInDependency("AlchAss", BepInDependency.DependencyFlags.HardDependency)]
    public class AlchAssEx : BaseUnityPlugin
    {
        #region Unity - 生命周期
        public void Awake()
        {
            Vars.QuantiHeating = Config.Bind("操作控制", "允许定量加热", true,
                "右键风箱设置热量值 | Right-click bellows to set heat value");
            Vars.QuantiGrinding = Config.Bind("操作控制", "允许定量研磨", true,
                "右键研杵设置研磨度 | Right-click pestle to set grinding level");
            Vars.enableVortexEdgeControl = Config.Bind("操作控制", "允许漩涡制动", true,
                "按'键启用漩涡边缘减速 | Press ' to enable vortex edge slowdown");
            Vars.enableClosestPointControl = Config.Bind("操作控制", "允许最近点制动", true,
                "按]键启用最近点减速 | Press ] to enable closest point slowdown");
            Vars.enableTargetProximityControl = Config.Bind("操作控制", "允许目标接近点制动", true,
                "按[键启用目标接近点减速 | Press [ to enable target proximity slowdown");
            Vars.enableGrindSpeed = Config.Bind("操作控制", "允许研磨减速", true,
                "按Z/X键减速研磨 | Press Z/X to slow down grinding");
            Vars.enableStirSpeed = Config.Bind("操作控制", "允许搅拌减速", true,
                "按Z/X键减速搅拌 | Press Z/X to slow down stirring");
            Vars.enableLadleSpeed = Config.Bind("操作控制", "允许加水减速", true,
                "按Z/X键减速加水 | Press Z/X to slow down ladling");
            Vars.enableHeatSpeed = Config.Bind("操作控制", "允许加热减速", true,
                "按Z/X键减速加热 | Press Z/X to slow down heating");
            Vars.enableMassBrewing = Config.Bind("操作控制", "允许大批炼药", true,
                "按Z/X键批量制作药剂 | Press Z/X to batch brew potions");

            Helper.UpdateFunctionsConfigCache();
            LocalizationManager.OnInitialize.AddListener(Helper.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAssEx));
            Logger.LogInfo("Alchemist's Assistant Extension 加载完成！");
        }
        public void Update()
        {
            if (Keyboard.current.xKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame)
                Helper.UpdateFunctionsConfigCache();
            if (Vars.QuantiHeating.Value)
                Controler.QuantiHeating();
            if (Vars.QuantiGrinding.Value)
                Controler.QuantiGrinding();
            if (Vars.enableVortexEdgeControl.Value)
                Controler.VortexEdgeControl();
            if (Vars.enableClosestPointControl.Value)
                Controler.ClosestPointControl();
            if (Vars.enableTargetProximityControl.Value)
                Controler.TargetProximityControl();
        }
        #endregion

        #region Harmony Patch - 慢速操作
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        public static void GrindSlowDown(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (Vars.enableGrindSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorZ;
                pestleLinearSpeed /= manualSlowdownFactor;
                pestleAngularSpeed /= manualSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        public static void StirSlowDown(ref float ___StirringValue)
        {
            if (Vars.enableStirSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorZ;
                ___StirringValue /= manualSlowdownFactor;
            }
            if (Vars.enableVortexEdgeControl.Value || Vars.enableClosestPointControl.Value || Vars.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Vars.vortexEdgeControl)
                    minSlowdownFactor = Mathf.Clamp01(Vars.vortexEdgeSpeed);
                if (Vars.closestPointControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Vars.closestPointspeed[0]));
                if (Vars.targetProximityControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Vars.targetProximitySpeed[1]));
                ___StirringValue *= minSlowdownFactor;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        public static void LadleSlowDown(ref float __result)
        {
            if (Vars.enableLadleSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorZ;
                __result /= manualSlowdownFactor;
            }
            if (Vars.enableVortexEdgeControl.Value || Vars.enableClosestPointControl.Value || Vars.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Vars.vortexEdgeControl)
                    minSlowdownFactor = Mathf.Clamp01(Vars.vortexEdgeSpeed);
                if (Vars.closestPointControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Vars.closestPointspeed[1]));
                if (Vars.targetProximityControl)
                    minSlowdownFactor = Mathf.Min(minSlowdownFactor, Mathf.Clamp01(Vars.targetProximitySpeed[0]));
                __result *= minSlowdownFactor;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        public static void HeatSlowDown(ref float __state)
        {
            var vortexSettings = Settings<RecipeMapManagerVortexSettings>.Asset;
            __state = vortexSettings.vortexMovementSpeed;
            if (Vars.enableHeatSpeed.Value)
            {
                var manualSlowdownFactor = 1.0f;
                if (Keyboard.current.xKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorX;
                else if (Keyboard.current.zKey.isPressed)
                    manualSlowdownFactor = Vars._cachedSlowdownFactorZ;
                vortexSettings.vortexMovementSpeed /= manualSlowdownFactor;
            }
            if (Vars.enableTargetProximityControl.Value)
            {
                var minSlowdownFactor = 1.0f;
                if (Vars.targetProximityControl)
                    minSlowdownFactor = Mathf.Clamp01(Vars.targetProximitySpeed[2]);
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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void PotionStatus(float ___health)
        {
            Controler.UpdateVortexEdgeControl();
            Controler.UpdateClosestPointControl();
            Controler.UpdateTargetProximityControl();
        }
        #endregion

        #region Harmony Patch - 批量调制
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeBookRecipeBrewController), "BrewRecipe")]
        public static void MassBrewing(ref int count, IRecipeBookPageContent recipePageContent)
        {
            if (Vars.enableMassBrewing.Value)
                if (count > 1)
                {
                    var brewingTimes = 1;
                    if (Keyboard.current.xKey.isPressed)
                        brewingTimes = Vars._cachedBrewingMultiplierX;
                    else if (Keyboard.current.zKey.isPressed)
                        brewingTimes = Vars._cachedBrewingMultiplierZ;
                    if (Helper.CanBrewTimes(recipePageContent, count, brewingTimes))
                        count *= brewingTimes;
                }
        }
        #endregion

        #region Harmony Patch - 出入漩涡
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerEnter2D")]
        public static void VortexDistanceEnter()
        {
            if (Vars.vortexEdgeControl)
                Vars.vortexEdgeSpeed = float.MinValue;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VortexMapItemCollider), "OnTriggerExit2D")]
        public static void VortexDistanceExit()
        {
            if (Vars.vortexEdgeControl)
                Vars.vortexEdgeSpeed = float.MaxValue;
        }
        #endregion
    }
}
