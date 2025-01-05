using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAss
{
    [BepInPlugin("AlchAss", "Alchemist's Assistant", "2.0.1")]
    public class AlchAss : BaseUnityPlugin
    {
        private static ConfigEntry<bool> enableGrindStatus;
        private static ConfigEntry<bool> enableHealthStatus;
        private static ConfigEntry<bool> enableVortexStatus;
        private static ConfigEntry<bool> enableStirStatus;
        private static ConfigEntry<bool> enablePositionStatus;
        private static ConfigEntry<bool> enableDeviationStatus;
        private static ConfigEntry<bool> enableClosestPathStatus;
        private static ConfigEntry<bool> enableClosestLadleStatus;

        private static ConfigEntry<bool> enableShuttingDown;
        private static ConfigEntry<bool> enableGrindSpeed;
        private static ConfigEntry<bool> enableStirSpeed;
        private static ConfigEntry<bool> enableLadleSpeed;
        private static ConfigEntry<bool> enableHeatSpeed;
        private static ConfigEntry<bool> enableBrewMore;

        private static ConfigEntry<Vector2> positionGrindDebugWindow;
        private static ConfigEntry<Vector2> positionHealthDebugWindow;
        private static ConfigEntry<Vector2> positionVortexDebugWindow;
        private static ConfigEntry<Vector2> positionStirDebugWindow;
        private static ConfigEntry<Vector2> positionPositionDebugWindow;
        private static ConfigEntry<Vector2> positionDeviationDebugWindow;
        private static ConfigEntry<Vector2> positionClosestPathDebugWindow;
        private static ConfigEntry<Vector2> positionClosestLadleDebugWindow;
        private static ConfigEntry<Vector3> speedControl;
        private static ConfigEntry<Vector3> brewControl;

        private static DebugWindow grindDebugWindow;
        private static DebugWindow healthDebugWindow;
        private static DebugWindow vortexDebugWindow;
        private static DebugWindow stirDebugWindow;
        private static DebugWindow positionDebugWindow;
        private static DebugWindow deviationDebugWindow;
        private static DebugWindow closestPathDebugWindow;
        private static DebugWindow closestLadleDebugWindow;

        public static Room lab;
        public static bool windowsPosition = false;
        public static readonly List<DebugWindow> foreground_queue = new();

        void Awake()
        {
            enableGrindStatus = Config.Bind("信息窗口", "研磨信息", true, "开启后，显示研磨进度.");
            enableHealthStatus = Config.Bind("信息窗口", "血量信息", true, "开启后，显示血量.");
            enableVortexStatus = Config.Bind("信息窗口", "漩涡信息", true, "开启后，显示漩涡中心方向和夹角.");
            enableStirStatus = Config.Bind("信息窗口", "搅拌信息", true, "开启后，显示搅拌阶段、搅拌进度和搅拌方向.");
            enablePositionStatus = Config.Bind("信息窗口", "位置信息", true, "开启后，显示位置、方向、旋转和折算盐量，以及与所接触效果的差值.");
            enableDeviationStatus = Config.Bind("信息窗口", "偏离信息", true, "开启后，显示与所接触效果的总体、位置和旋转偏差度.");
            enableClosestPathStatus = Config.Bind("信息窗口", "路径信息", true, "开启后，显示路径最近点的目标效果和偏差度.");
            enableClosestLadleStatus = Config.Bind("信息窗口", "加水信息", true, "开启后，显示加水最近点的目标效果和偏差度.");

            enableShuttingDown = Config.Bind("操作控制", "允许漩涡急停", true, "开启后，右键点击风箱把手将使药水瞬间冷却.");
            enableGrindSpeed = Config.Bind("操作控制", "允许研磨减速", true, "开启后，按住 Z, X 或 Z + X 键将使研磨减速至相应比例.");
            enableStirSpeed = Config.Bind("操作控制", "允许搅拌减速", true, "开启后，按住 Z, X 或 Z + X 键将使搅拌减速至相应比例.");
            enableLadleSpeed = Config.Bind("操作控制", "允许加水减速", true, "开启后，按住 Z, X 或 Z + X 键将使加水减速至相应比例.");
            enableHeatSpeed = Config.Bind("操作控制", "允许加热减速", true, "开启后，按住 Z, X 或 Z + X 键将使加热减速至相应比例.");
            enableBrewMore = Config.Bind("操作控制", "允许大批炼药", true, "开启后，按住 Z, X 或 Z + X 键将使炼药数量增加至相应比例.");
            speedControl = Config.Bind("操作控制", "减速比例", new Vector3(10f, 100f, 1000f), "按住 Z, X 或 Z + X 键减速操作至 1/x*, 1/y*, 1/z*");
            brewControl = Config.Bind("操作控制", "研磨信息", new Vector3(5, 10, 20), "按住 Z, X 或 Z + X 键增加炼药数量至 x*, y*, z*");

            positionGrindDebugWindow = Config.Bind("窗口位置", "研磨信息", new Vector2(8.5f, -4.5f), "调整研磨信息窗口坐标.");
            positionHealthDebugWindow = Config.Bind("窗口位置", "血量信息", new Vector2(5.5f, -4.5f), "调整血量信息窗口坐标.");
            positionVortexDebugWindow = Config.Bind("窗口位置", "漩涡信息", new Vector2(2.5f, -4.5f), "调整漩涡信息窗口坐标.");
            positionStirDebugWindow = Config.Bind("窗口位置", "搅拌信息", new Vector2(-0.5f, -4.5f), "调整搅拌信息窗口坐标.");
            positionPositionDebugWindow = Config.Bind("窗口位置", "位置信息", new Vector2(-3.5f, -4.5f), "调整位置信息窗口坐标.");
            positionDeviationDebugWindow = Config.Bind("窗口位置", "偏离信息", new Vector2(-6.5f, -4.5f), "调整偏离信息窗口坐标.");
            positionClosestPathDebugWindow = Config.Bind("窗口位置", "路径信息", new Vector2(-9.5f, -4.5f), "调整路径信息窗口坐标.");
            positionClosestLadleDebugWindow = Config.Bind("窗口位置", "加水信息", new Vector2(-12.5f, -4.5f), "调整加水信息窗口坐标.");

            LocalizationManager.OnInitialize.AddListener(Helper.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAss));
            Logger.LogInfo("插件加载完成！");
        }

        void Update()
        {
            Helper.WindowsPosition();
            if (enableShuttingDown.Value)
                InfoCalc.CoolDown();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Window), "ToForeground")]
        private static bool WindowForeground(Window __instance)
        {
            if (__instance is not DebugWindow dbg)
                return true;
            if (!foreground_queue.Contains(dbg))
                return false;
            foreground_queue.Remove(dbg);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Room), "Awake")]
        private static void InitDebugWindows(Room __instance)
        {
            if (__instance.roomIndex == RoomIndex.Laboratory)
                lab = __instance;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubstanceGrinding), "TryToGrind")]
        private static void GrindSlowDown(ref float pestleLinearSpeed, ref float pestleAngularSpeed)
        {
            if (enableGrindSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed && Keyboard.current.zKey.isPressed)
                    InfoCalc.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, speedControl.Value.z);
                else if (Keyboard.current.xKey.isPressed)
                    InfoCalc.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, speedControl.Value.y);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.GrindSlowDown(ref pestleLinearSpeed, ref pestleAngularSpeed, speedControl.Value.x);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Cauldron), "UpdateStirringValue")]
        private static void StirSlowDown(ref float ___StirringValue)
        {
            if (enableStirSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed && Keyboard.current.zKey.isPressed)
                    InfoCalc.StirSlowDown(ref ___StirringValue, speedControl.Value.z);
                else if (Keyboard.current.xKey.isPressed)
                    InfoCalc.StirSlowDown(ref ___StirringValue, speedControl.Value.y);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.StirSlowDown(ref ___StirringValue, speedControl.Value.x);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "GetSpeedOfMovingTowardsBase")]
        private static void LadleSlowDown(ref float __result)
        {
            if (enableLadleSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed && Keyboard.current.zKey.isPressed)
                    InfoCalc.LadleSlowDown(ref __result, speedControl.Value.z);
                else if (Keyboard.current.xKey.isPressed)
                    InfoCalc.LadleSlowDown(ref __result, speedControl.Value.y);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.LadleSlowDown(ref __result, speedControl.Value.x);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        private static void HeatSlowDown(ref float __state)
        {
            if (enableHeatSpeed.Value)
            {
                if (Keyboard.current.xKey.isPressed && Keyboard.current.zKey.isPressed)
                    InfoCalc.HeatSlowDown(ref __state, speedControl.Value.z);
                else if (Keyboard.current.xKey.isPressed)
                    InfoCalc.HeatSlowDown(ref __state, speedControl.Value.y);
                else if (Keyboard.current.zKey.isPressed)
                    InfoCalc.HeatSlowDown(ref __state, speedControl.Value.x);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeMapManager), "MoveIndicatorTowardsVortex")]
        private static void HeatSlowDownEnd(float __state)
        {
            if (enableHeatSpeed.Value)
                if (__state != 0f)
                    if (Keyboard.current.xKey.isPressed || Keyboard.current.zKey.isPressed)
                        Settings<RecipeMapManagerVortexSettings>.Asset.vortexMovementSpeed = __state;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortar), "Update")]
        private static void GrindStatus(Mortar __instance)
        {
            if (enableGrindStatus.Value)
                if (grindDebugWindow == null)
                    grindDebugWindow = Helper.CreateDebugWindow("#mod_dialog_grind_status", positionGrindDebugWindow.Value);

            if (grindDebugWindow != null)
            {
                if (!windowsPosition)
                    grindDebugWindow.ShowText(InfoCalc.GrindCalc(__instance));
                else
                    grindDebugWindow.ShowText(grindDebugWindow.transform.position.ToString());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        private static void PotionStatus(IndicatorMapItem __instance)
        {
            if (enableHealthStatus.Value)
                if (healthDebugWindow == null)
                    healthDebugWindow = Helper.CreateDebugWindow("#mod_dialog_health_status", positionHealthDebugWindow.Value);
            if (enableVortexStatus.Value)
                if (vortexDebugWindow == null)
                    vortexDebugWindow = Helper.CreateDebugWindow("#mod_dialog_vortex_status", positionVortexDebugWindow.Value);
            if (enableStirStatus.Value)
                if (stirDebugWindow == null)
                    stirDebugWindow = Helper.CreateDebugWindow("#mod_dialog_stir_status", positionStirDebugWindow.Value);
            if (enablePositionStatus.Value)
                if (positionDebugWindow == null)
                    positionDebugWindow = Helper.CreateDebugWindow("#mod_dialog_position_status", positionPositionDebugWindow.Value);
            if (enableDeviationStatus.Value)
                if (deviationDebugWindow == null)
                    deviationDebugWindow = Helper.CreateDebugWindow("#mod_dialog_deviation_status", positionDeviationDebugWindow.Value);
            if (enableClosestPathStatus.Value)
                if (closestPathDebugWindow == null)
                    closestPathDebugWindow = Helper.CreateDebugWindow("#mod_dialog_path_status", positionClosestPathDebugWindow.Value);
            if (enableClosestLadleStatus.Value)
                if (closestLadleDebugWindow == null)
                    closestLadleDebugWindow = Helper.CreateDebugWindow("#mod_dialog_ladle_status", positionClosestLadleDebugWindow.Value);

            if (healthDebugWindow != null)
            {
                if (!windowsPosition)
                    healthDebugWindow.ShowText(InfoCalc.HealthCalc(__instance));
                else
                    healthDebugWindow.ShowText(healthDebugWindow.transform.position.ToString());
            }
            if (vortexDebugWindow != null)
            {
                if (!windowsPosition)
                    vortexDebugWindow.ShowText(InfoCalc.VortexCalc());
                else
                    vortexDebugWindow.ShowText(vortexDebugWindow.transform.position.ToString());
            }
            if (stirDebugWindow != null)
            {
                if (!windowsPosition)
                    stirDebugWindow.ShowText(InfoCalc.StirCalc());
                else
                    stirDebugWindow.ShowText(stirDebugWindow.transform.position.ToString());
            }
            if (positionDebugWindow != null || deviationDebugWindow != null)
            {
                var posdev = InfoCalc.PositionDeviationCalc();
                if (positionDebugWindow != null)
                {
                    if (!windowsPosition)
                        positionDebugWindow.ShowText(posdev.Item1);
                    else
                        positionDebugWindow.ShowText(positionDebugWindow.transform.position.ToString());
                }
                if (deviationDebugWindow != null)
                {
                    if (!windowsPosition)
                        deviationDebugWindow.ShowText(posdev.Item2);
                    else
                        deviationDebugWindow.ShowText(deviationDebugWindow.transform.position.ToString());
                }
            }
            if (closestPathDebugWindow != null)
            {
                if (!windowsPosition)
                    closestPathDebugWindow.ShowText(InfoCalc.PathCalc());
                else
                    closestPathDebugWindow.ShowText(closestPathDebugWindow.transform.position.ToString());
            }
            if (closestLadleDebugWindow != null)
            {
                if (!windowsPosition)
                    closestLadleDebugWindow.ShowText(InfoCalc.LadleCalc());
                else
                    closestLadleDebugWindow.ShowText(closestLadleDebugWindow.transform.position.ToString());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeBookRecipeBrewController), "BrewRecipe")]
        private static void BrewMore(ref int count, IRecipeBookPageContent recipePageContent)
        {
            if (enableBrewMore.Value)
                if (count > 1)
                {
                    if (Keyboard.current.xKey.isPressed && Keyboard.current.zKey.isPressed)
                        InfoCalc.BrewRecipe(ref count, recipePageContent, (int)brewControl.Value.z);
                    else if (Keyboard.current.xKey.isPressed)
                        InfoCalc.BrewRecipe(ref count, recipePageContent, (int)brewControl.Value.y);
                    else if (Keyboard.current.zKey.isPressed)
                        InfoCalc.BrewRecipe(ref count, recipePageContent, (int)brewControl.Value.x);
                }
        }
    }
}
