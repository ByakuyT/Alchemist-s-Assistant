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
    [BepInPlugin("AlchAss", "Alchemist's Assistant", "4.0.0")]
    public class AlchAss : BaseUnityPlugin
    {
        #region Unity - 生命周期
        public void Awake()
        {
            Vars.enableGrindStatus = Config.Bind("信息窗口", "研磨信息", true,
                "显示研磨进度 | Show grinding progress");
            Vars.enableHealthStatus = Config.Bind("信息窗口", "血量信息", true,
                "显示当前血量 | Show current health");
            Vars.enableVortexStatus = Config.Bind("信息窗口", "漩涡信息", true,
                "显示漩涡距离和角度信息 | Show vortex distance and angle information");
            Vars.enablePositionStatus = Config.Bind("信息窗口", "位置信息", true,
                "显示坐标位置和旋转量 | Show coordinate position and rotation");
            Vars.enableStirStatus = Config.Bind("信息窗口", "移动信息", true,
                "显示搅拌阶段、进度和方向 | Show stirring stage, progress and direction");
            Vars.enableZoneStatus = Config.Bind("信息窗口", "区域信息", true,
                "显示区域进入位置，按.键切换 | Show zone entry positions, press . to switch");
            Vars.enableTargetStatus = Config.Bind("信息窗口", "目标信息", true,
                "显示目标效果的位置和方向 | Show target effect position and direction");
            Vars.enableDeviationStatus = Config.Bind("信息窗口", "偏离信息", true,
                "需要目标信息 | 显示与目标的偏差 | Requires target info | Show deviation from target");
            Vars.enablePathStatus = Config.Bind("信息窗口", "路径信息", true,
                "需要目标信息 | 显示路径最近点信息 | Requires target info | Show closest path point info");
            Vars.enableLadleStatus = Config.Bind("信息窗口", "加水信息", true,
                "需要目标信息 | 显示加水最近点信息 | Requires target info | Show closest ladle point info");
            Vars.enableTooltip = Config.Bind("信息窗口", "键位表", true,
                "显示所有快捷键列表 | Show all hotkey list");
            Vars.windowScale = Config.Bind("信息窗口", "窗口缩放", 1f,
                "信息窗口的显示大小比例 | Display size ratio of info windows");
            Vars.enableDirectionLine = Config.Bind("操作控制", "允许辅助示线", true,
                "按/键显示方向线和区域标记 | Press / to show direction lines and zone markers");

            if (!Vars.enableTargetStatus.Value)
                Vars.enableDeviationStatus.Value = Vars.enablePathStatus.Value = Vars.enableLadleStatus.Value = false;
            Directory.CreateDirectory(Vars.ConfigDirectory);
            Helper.LoadWindowPositions();
            LocalizationManager.OnInitialize.AddListener(Helper.SetModLocalization);
            Harmony.CreateAndPatchAll(typeof(AlchAss));
            Logger.LogInfo("Alchemist's Assistant 加载完成！");
        }
        public void Update()
        {
            if (Vars.enableDirectionLine.Value)
                Controler.DirectionLine();
            if (Vars.enablePathStatus.Value)
                Controler.EndMode();
            if (Vars.enablePositionStatus.Value)
                Controler.PositionMode();
            if (Vars.enableZoneStatus.Value)
                Controler.ZoneMode();
        }
        #endregion

        #region Harmony Patch - 窗口创建
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "SaveProgressToPool")]
        public static void SaveWindowsPosition()
        {
            var windowConfigPath = Path.Combine(Vars.ConfigDirectory, Vars.windowPath);
            string windowPositions = $@"{Vars.grindDebugWindow?.transform.position}
{Vars.healthDebugWindow?.transform.position}
{Vars.vortexDebugWindow?.transform.position}
{Vars.stirDebugWindow?.transform.position}
{Vars.positionDebugWindow?.transform.position}
{Vars.deviationDebugWindow?.transform.position}
{Vars.pathDebugWindow?.transform.position}
{Vars.ladleDebugWindow?.transform.position}
{Vars.targetDebugWindow?.transform.position}
{Vars.tooltipDebugWindow?.transform.position}
{Vars.zoneDebugWindow?.transform.position}";
            File.WriteAllText(windowConfigPath, windowPositions);
        }
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
            if (!Vars.foreground_queue.Contains(dbg))
                return false;
            Vars.foreground_queue.Remove(dbg);
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Room), "Awake")]
        public static void InitDebugWindows(Room __instance)
        {
            if (__instance.roomIndex == RoomIndex.Laboratory)
                Vars.lab = __instance;
        }
        #endregion

        #region Harmony Patch - 信息显示
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortar), "Update")]
        public static void GrindStatus(Mortar __instance)
        {
            if (Vars.enableGrindStatus.Value && Vars.grindDebugWindow == null)
                Vars.grindDebugWindow = Helper.CreateDebugWindow("dialog_grind_status", Vars.positionGrindDebugWindow);
            Vars.grindDebugWindow?.ShowText(InfoCalc.GrindCalc(__instance));
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void PotionStatus(float ___health)
        {
            Helper.UpdateLineDirections();
            Helper.UpdateZonePositions();
            if (Vars.enableHealthStatus.Value && Vars.healthDebugWindow == null)
                Vars.healthDebugWindow = Helper.CreateDebugWindow("dialog_health_status", Vars.positionHealthDebugWindow);
            if (Vars.enableVortexStatus.Value && Vars.vortexDebugWindow == null)
                Vars.vortexDebugWindow = Helper.CreateDebugWindow("dialog_vortex_status", Vars.positionVortexDebugWindow);
            if (Vars.enableStirStatus.Value && Vars.stirDebugWindow == null)
                Vars.stirDebugWindow = Helper.CreateDebugWindow("dialog_stir_status", Vars.positionStirDebugWindow);
            if (Vars.enablePositionStatus.Value && Vars.positionDebugWindow == null)
                Vars.positionDebugWindow = Helper.CreateDebugWindow("dialog_position_status", Vars.positionPositionDebugWindow);
            if (Vars.enableDeviationStatus.Value && Vars.deviationDebugWindow == null)
                Vars.deviationDebugWindow = Helper.CreateDebugWindow("dialog_deviation_status", Vars.positionDeviationDebugWindow);
            if (Vars.enablePathStatus.Value && Vars.pathDebugWindow == null)
                Vars.pathDebugWindow = Helper.CreateDebugWindow("dialog_path_status", Vars.positionPathDebugWindow);
            if (Vars.enableLadleStatus.Value && Vars.ladleDebugWindow == null)
                Vars.ladleDebugWindow = Helper.CreateDebugWindow("dialog_ladle_status", Vars.positionLadleDebugWindow);
            if (Vars.enableTargetStatus.Value && Vars.targetDebugWindow == null)
                Vars.targetDebugWindow = Helper.CreateDebugWindow("dialog_target_status", Vars.positionTargetDebugWindow);
            if (Vars.enableZoneStatus.Value && Vars.zoneDebugWindow == null)
                Vars.zoneDebugWindow = Helper.CreateDebugWindow("dialog_zone_status", Vars.positionZoneDebugWindow);
            if (Vars.enableTooltip.Value && Vars.tooltipDebugWindow == null)
                Vars.tooltipDebugWindow = Helper.CreateDebugWindow("dialog_tooltip", Vars.positionTooltipDebugWindow);
            Vars.healthDebugWindow?.ShowText(InfoCalc.HealthCalc(___health));
            Vars.vortexDebugWindow?.ShowText(InfoCalc.VortexCalc());
            Vars.stirDebugWindow?.ShowText(InfoCalc.MoveCalc());
            Vars.positionDebugWindow?.ShowText(InfoCalc.PositionCalc());
            Vars.targetDebugWindow?.ShowText(InfoCalc.TargetCalc());
            Vars.deviationDebugWindow?.ShowText(InfoCalc.DeviationCalc());
            Vars.pathDebugWindow?.ShowText(InfoCalc.PathCalc());
            Vars.ladleDebugWindow?.ShowText(InfoCalc.LadleCalc());
            Vars.zoneDebugWindow?.ShowText(InfoCalc.ZoneCalc());
            Vars.tooltipDebugWindow?.ShowText(LocalizationManager.GetText("tooltip"));
            if (Vars.solventDirectionHint != null)
                Traverse.Create(Vars.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
        }
        [HarmonyPatch(typeof(CursorManager), "UpdateDebugWindow")]
        [HarmonyPostfix]
        public static void UpdateDebugWindowPostfix(InteractiveItem ___hoveredInteractiveItem)
        {
            if (Vars.enableTargetStatus.Value)
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    Vars.hoveredItemName = ___hoveredInteractiveItem != null ? ___hoveredInteractiveItem.transform.name : null;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "LoadProgressState")]
        public static void ResetZonePoint()
        {
            Vars.resetZone = true;
        }
        #endregion

        #region Harmony Patch - 示线渲染
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SolventDirectionHint), "OnPositionOnMapChanged")]
        public static void DirectionLineUpdate(SolventDirectionHint __instance, SpriteRenderer ___spriteRenderer)
        {
            if (!Vars.enableDirectionLine.Value)
                return;
            if (Vars.solventDirectionHint == null)
                Vars.solventDirectionHint = __instance;
            if (Vars.texture == null)
            {
                Vars.texture = new Texture2D(1, 1);
                Vars.texture.SetPixels([Color.white]);
                Vars.texture.Apply();
            }
            Helper.InitializeLineRenderers(__instance, ___spriteRenderer);
            Helper.HandleDirectionLineDisplay(__instance, ___spriteRenderer);
        }
        #endregion
    }
}
