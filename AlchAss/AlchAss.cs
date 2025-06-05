using BepInEx;
using HarmonyLib;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.Cursor;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using PotionCraft.ObjectBased.UIElements;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAss
{
    [BepInPlugin("AlchAss", "Alchemist's Assistant", "4.1.0")]
    public class AlchAss : BaseUnityPlugin
    {
        #region Unity - 生命周期
        public void Awake()
        {
            Variables.enableGrindStatus = Config.Bind("信息窗口", "研磨信息", true,
                "显示研磨进度 | Show grinding progress");
            Variables.enableHealthStatus = Config.Bind("信息窗口", "血量信息", true,
                "显示当前血量 | Show current health");
            Variables.enableVortexStatus = Config.Bind("信息窗口", "漩涡信息", true,
                "显示漩涡距离和角度信息 | Show vortex distance and angle information");
            Variables.enablePositionStatus = Config.Bind("信息窗口", "位置信息", true,
                "显示坐标位置和旋转量 | Show coordinate position and rotation");
            Variables.enableStirStatus = Config.Bind("信息窗口", "移动信息", true,
                "显示搅拌阶段、进度和方向 | Show stirring stage, progress and direction");
            Variables.enableZoneStatus = Config.Bind("信息窗口", "区域信息", true,
                "显示区域进入位置，按.键切换 | Show zone entry positions, press . to switch");
            Variables.enableTargetStatus = Config.Bind("信息窗口", "目标信息", true,
                "显示目标效果的位置和方向 | Show target effect position and direction");
            Variables.enableDeviationStatus = Config.Bind("信息窗口", "偏离信息", true,
                "需要目标信息 | 显示与目标的偏差 | Requires target info | Show deviation from target");
            Variables.enablePathStatus = Config.Bind("信息窗口", "路径信息", true,
                "需要目标信息 | 显示路径最近点信息 | Requires target info | Show closest path point info");
            Variables.enableLadleStatus = Config.Bind("信息窗口", "加水信息", true,
                "需要目标信息 | 显示加水最近点信息 | Requires target info | Show closest ladle point info");
            Variables.enableTooltip = Config.Bind("信息窗口", "键位表", true,
                "显示所有快捷键列表 | Show all hotkey list");
            Variables.windowScale = Config.Bind("信息窗口", "窗口缩放", 0.8f,
                "信息窗口的显示大小比例 | Display size ratio of info windows");
            Variables.enableDirectionLine = Config.Bind("辅助渲染", "允许辅助示线", true,
                "按/键显示方向线、区域标记和漩涡圆形 | Press / to show direction lines, zone markers and vortex circles");

            if (!Variables.enableTargetStatus.Value)
                Variables.enableDeviationStatus.Value = Variables.enablePathStatus.Value = Variables.enableLadleStatus.Value = false;
            Directory.CreateDirectory(Variables.ConfigDirectory);
            Depends.LoadWindowPositions();
            LocalizationManager.OnInitialize.AddListener(Depends.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAss));
            Logger.LogInfo("Alchemist's Assistant 加载完成！");
        }
        public void Update()
        {
            if (Variables.enableDirectionLine.Value)
                Functions.DirectionLine();
            if (Variables.enablePathStatus.Value)
                Functions.EndMode();
            if (Variables.enablePositionStatus.Value)
                Functions.PositionMode();
            if (Variables.enableZoneStatus.Value)
                Functions.ZoneMode();
            if (Variables.enableDirectionLine.Value)
                Functions.VortexSelection();
        }
        #endregion

        #region Harmony Patch - 窗口创建
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovableUIItem), "FixOutOfBoundsCase")]
        public static bool WindowFixOutOfBoundsCase(MovableUIItem __instance)
        {
            return __instance is not DebugWindow;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Window), "ToForeground")]
        public static bool WindowForeground(Window __instance)
        {
            if (__instance is not DebugWindow dbg)
                return true;
            if (!Variables.foreground_queue.Contains(dbg))
                return false;
            Variables.foreground_queue.Remove(dbg);
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Room), "Awake")]
        public static void InitDebugWindows(Room __instance)
        {
            if (__instance.roomIndex == RoomIndex.Laboratory)
                Variables.lab = __instance;
        }
        #endregion

        #region Harmony Patch - 信息显示
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortar), "Update")]
        public static void GrindStatus(Mortar __instance)
        {
            if (Variables.enableGrindStatus.Value && Variables.grindDebugWindow == null)
                Variables.grindDebugWindow = Depends.CreateDebugWindow("dialog_grind_status", Variables.positionGrindDebugWindow);
            Variables.grindDebugWindow?.ShowText(Functions.GrindCalc(__instance));
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void PotionStatus(float ___health)
        {
            if (Variables.enableHealthStatus.Value && Variables.healthDebugWindow == null)
                Variables.healthDebugWindow = Depends.CreateDebugWindow("dialog_health_status", Variables.positionHealthDebugWindow);
            if (Variables.enableVortexStatus.Value && Variables.vortexDebugWindow == null)
                Variables.vortexDebugWindow = Depends.CreateDebugWindow("dialog_vortex_status", Variables.positionVortexDebugWindow);
            if (Variables.enableStirStatus.Value && Variables.stirDebugWindow == null)
                Variables.stirDebugWindow = Depends.CreateDebugWindow("dialog_stir_status", Variables.positionStirDebugWindow);
            if (Variables.enablePositionStatus.Value && Variables.positionDebugWindow == null)
                Variables.positionDebugWindow = Depends.CreateDebugWindow("dialog_position_status", Variables.positionPositionDebugWindow);
            if (Variables.enableDeviationStatus.Value && Variables.deviationDebugWindow == null)
                Variables.deviationDebugWindow = Depends.CreateDebugWindow("dialog_deviation_status", Variables.positionDeviationDebugWindow);
            if (Variables.enablePathStatus.Value && Variables.pathDebugWindow == null)
                Variables.pathDebugWindow = Depends.CreateDebugWindow("dialog_path_status", Variables.positionPathDebugWindow);
            if (Variables.enableLadleStatus.Value && Variables.ladleDebugWindow == null)
                Variables.ladleDebugWindow = Depends.CreateDebugWindow("dialog_ladle_status", Variables.positionLadleDebugWindow);
            if (Variables.enableTargetStatus.Value && Variables.targetDebugWindow == null)
                Variables.targetDebugWindow = Depends.CreateDebugWindow("dialog_target_status", Variables.positionTargetDebugWindow);
            if (Variables.enableZoneStatus.Value && Variables.zoneDebugWindow == null)
                Variables.zoneDebugWindow = Depends.CreateDebugWindow("dialog_zone_status", Variables.positionZoneDebugWindow);
            if (Variables.enableTooltip.Value && Variables.tooltipDebugWindow == null)
                Variables.tooltipDebugWindow = Depends.CreateDebugWindow("dialog_tooltip", Variables.positionTooltipDebugWindow);
            Variables.healthDebugWindow?.ShowText(Functions.HealthCalc(___health));
            Variables.vortexDebugWindow?.ShowText(Functions.VortexCalc());
            Variables.stirDebugWindow?.ShowText(Functions.MoveCalc());
            Variables.positionDebugWindow?.ShowText(Functions.PositionCalc());
            Variables.targetDebugWindow?.ShowText(Functions.TargetCalc());
            Variables.deviationDebugWindow?.ShowText(Functions.DeviationCalc());
            Variables.pathDebugWindow?.ShowText(Functions.PathCalc());
            Variables.ladleDebugWindow?.ShowText(Functions.LadleCalc());
            Variables.zoneDebugWindow?.ShowText(Functions.ZoneCalc());
            Variables.tooltipDebugWindow?.ShowText(LocalizationManager.GetText("tooltip"));
            if (Variables.solventDirectionHint != null)
                Traverse.Create(Variables.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
        }
        #endregion

        #region Harmony Patch - 示线渲染
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SolventDirectionHint), "OnPositionOnMapChanged")]
        public static void DirectionLineUpdate(SolventDirectionHint __instance, SpriteRenderer ___spriteRenderer)
        {
            if (!Variables.enableDirectionLine.Value)
                return;
            if (Variables.solventDirectionHint == null)
                Variables.solventDirectionHint = __instance;
            if (Variables.texture == null)
            {
                Variables.texture = new Texture2D(1, 1);
                Variables.texture.SetPixels([Color.white]);
                Variables.texture.Apply();
            }
            Depends.InitializeLineRenderers(__instance, ___spriteRenderer);
            Depends.HandleDirectionLineDisplay(__instance, ___spriteRenderer);
        }
        #endregion

        #region Harmony Patch - 工具方法
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "SaveProgressToPool")]
        public static void SaveWindowsPosition()
        {
            var windowConfigPath = Path.Combine(Variables.ConfigDirectory, Variables.windowPath);
            string windowPositions = $@"{Variables.grindDebugWindow?.transform.position}
{Variables.healthDebugWindow?.transform.position}
{Variables.vortexDebugWindow?.transform.position}
{Variables.stirDebugWindow?.transform.position}
{Variables.positionDebugWindow?.transform.position}
{Variables.deviationDebugWindow?.transform.position}
{Variables.pathDebugWindow?.transform.position}
{Variables.ladleDebugWindow?.transform.position}
{Variables.targetDebugWindow?.transform.position}
{Variables.tooltipDebugWindow?.transform.position}
{Variables.zoneDebugWindow?.transform.position}";
            File.WriteAllText(windowConfigPath, windowPositions);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetWhenLoading()
        {
            Variables.resetWhenLoading = true;
        }
        [HarmonyPatch(typeof(CursorManager), "UpdateDebugWindow")]
        [HarmonyPostfix]
        public static void UpdateDebugWindowPostfix(InteractiveItem ___hoveredInteractiveItem)
        {
            if (Variables.enableTargetStatus.Value)
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    Variables.hoveredItemName = ___hoveredInteractiveItem != null ? ___hoveredInteractiveItem.transform.name : null;
        }
        #endregion
    }
}
