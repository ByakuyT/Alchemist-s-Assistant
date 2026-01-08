using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Cursor;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ManagersSystem.Npc;
using PotionCraft.QuestSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using PotionCraft.ObjectBased.UIElements;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace AlchAssV3
{
    [BepInPlugin("AlchAssV3", "Alchemist's Assistant V3", "1.1.0")]
    public class Main : BaseUnityPlugin
    {
        #region Constants

        public static void BindSwitchKeyInfo(Variable.SwitchDictionaryKey key, out string info, out KeyboardShortcut defaultKeyShortcut)
        {
            switch (key)
            {
                case Variable.SwitchDictionaryKey.PathLine:
                    info = "路径方向线";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.LadleLine:
                    info = "加水方向线";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.TargetLine:
                    info = "目标方向线";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.VortexLine:
                    info = "漩涡方向线";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.PathCurve:
                    info = "路径曲线";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.VortexCurve:
                    info = "漩涡曲线";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.TargetRange:
                    info = "目标范围";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.VortexRange:
                    info = "漩涡范围";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.AreaTracking:
                    info = "区域追踪";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.SwampScaling:
                    info = "沼泽收缩";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.Transparency:
                    info = "透明瓶身";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.BackQuote, KeyCode.LeftControl);
                    break;
                case Variable.SwitchDictionaryKey.PolarMode:
                    info = "极坐标模式";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftAlt);
                    break;
                case Variable.SwitchDictionaryKey.SaltDegreeMode:
                    info = "盐度模式";
                    defaultKeyShortcut = new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftAlt);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
        #endregion

        #region Unity - 生命周期
        /// <summary>
        /// 游戏初始化时
        /// </summary>
        public void Awake()
        {
            Variable.EnableWindows[0] = Config.Bind("窗口设置", "路径信息", true);
            Variable.EnableWindows[1] = Config.Bind("窗口设置", "加水信息", true);
            Variable.EnableWindows[2] = Config.Bind("窗口设置", "移动信息", true);
            Variable.EnableWindows[3] = Config.Bind("窗口设置", "目标信息", true);
            Variable.EnableWindows[4] = Config.Bind("窗口设置", "位置信息", true);
            Variable.EnableWindows[5] = Config.Bind("窗口设置", "偏离信息", true);
            Variable.EnableWindows[6] = Config.Bind("窗口设置", "漩涡信息", true);
            Variable.EnableWindows[7] = Config.Bind("窗口设置", "血量信息", true);
            Variable.EnableWindows[8] = Config.Bind("窗口设置", "研磨信息", true);
            Variable.EnableWindows[9] = Config.Bind("窗口设置", "调试信息", true);
            Variable.EnableWindows[10] = Config.Bind("窗口设置", "IO信息", true);
            Variable.EnableWindows[11] = Config.Bind("窗口设置", "快捷键信息", true);

            Variable.Colors[0] = Config.Bind("颜色设置", "路径方向线", new Color(0.1f, 0.8f, 0.1f));
            Variable.Colors[1] = Config.Bind("颜色设置", "加水方向线", new Color(0.1f, 0.1f, 0.8f));
            Variable.Colors[2] = Config.Bind("颜色设置", "目标方向线", new Color(0.8f, 0.1f, 0.1f));
            Variable.Colors[3] = Config.Bind("颜色设置", "漩涡方向线", new Color(0.4f, 0.4f, 0.1f));
            Variable.Colors[4] = Config.Bind("颜色设置", "路径曲线一", new Color(0.0f, 0.9f, 1.0f));
            Variable.Colors[5] = Config.Bind("颜色设置", "路径曲线二", new Color(1.0f, 0.0f, 0.9f));
            Variable.Colors[6] = Config.Bind("颜色设置", "漩涡曲线", new Color(0.5f, 0.2f, 0.5f));
            Variable.Colors[7] = Config.Bind("颜色设置", "区域范围", new Color(0.5f, 0.1f, 0.5f));
            Variable.Colors[8] = Config.Bind("颜色设置", "最近点", new Color(0.8f, 0.3f, 0.8f));
            Variable.Colors[9] = Config.Bind("颜色设置", "危险点", new Color(0.2f, 0.6f, 0.6f));
            Variable.Colors[10] = Config.Bind("颜色设置", "交会点", new Color(0.1f, 0.6f, 0.6f));
            Variable.Colors[11] = Config.Bind("颜色设置", "失败点", new Color(0.8f, 0.0f, 0.2f));

            // switch keys
            foreach (Variable.SwitchDictionaryKey key in Enum.GetValues(typeof(Variable.SwitchDictionaryKey)))
            {
                BindSwitchKeyInfo(key, out string info, out KeyboardShortcut defaultKeyShortcut);
                Variable.SwitchKeyShortcuts[key] = Config.Bind("快捷键设置", info, defaultKeyShortcut);
            }

            // non-switch keys
            Variable.PrevVortexKeyShortcut = Config.Bind("快捷键设置", "上一个漩涡", new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftAlt));
            Variable.NextVortexKeyShortcut = Config.Bind("快捷键设置", "下一个漩涡", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftAlt));
            Variable.NearestVortexKeyShortcut = Config.Bind("快捷键设置", "选择最近漩涡", new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftAlt));
            Variable.UnselectVortexKeyShortcut = Config.Bind("快捷键设置", "取消漩涡选择", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftAlt));
            Variable.SelectEffectKeyShortcut = Config.Bind("快捷键设置", "选择效果", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftAlt));

            Variable.LineWidth = Config.Bind("其他设置", "渲染线宽", 0.075);
            Variable.NodeSize = Config.Bind("其他设置", "渲染点大小", 0.15);
            Variable.WindowScale = Config.Bind("其他设置", "信息窗口缩放", 0.8);

            Directory.CreateDirectory(Variable.ConfigPath);
            Function.LoadDebugWindowPos();
            Function.LoadFromBin();
            Rendering.CreateMaterialAndSprites();
            LocalizationManager.OnInitialize.AddListener(Localization.SetAllLocalizations);
            Harmony.CreateAndPatchAll(typeof(Main));
            Logger.LogInfo("Alchemist's Assistant V3 插件已加载");
        }

        /// <summary>
        /// 每帧更新时
        /// </summary>
        public void Update()
        {
            Function.UpdateKeyMode();
            Function.UpdateFloatInput();
            Function.UpdateSwitches();
            Function.UpdateSelectVortex();
        }
        #endregion

        #region Patch - 状态更新
        /// <summary>
        /// 研钵更新时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortar), "Update")]
        public static void MortarUpdate(Mortar __instance)
        {
            Variable.DebugWindows[8]?.ShowText(Calculation.CalculateGrind(__instance));
        }

        /// <summary>
        /// 地图更新时
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void MapUpdate(float ___health)
        {
            Variable.IndicatorPosition = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
            Variable.IndicatorRotation = Managers.RecipeMap.indicatorRotation.Value;
            Variable.CurrentMapID = Managers.RecipeMap.currentMap.potionBase.name;

            Calculation.InitPathCurve(out Variable.PathPhysical, out Variable.PathGraphical, out Variable.SwampPositions);
            Calculation.InitVortexCurve(out Variable.VortexParameters, out Variable.VortexGraphical);
            Calculation.InitPoints(___health, out Variable.ClosestPositions, out Variable.DefeatPositions, out Variable.DangerPositions, out Variable.IntersectionPositions, out Variable.DangerDistance);

            Variable.LineDirections[0] = Variable.SwitchPathDirection.getState() ? Calculation.GetPathLineDirection() : double.NaN;
            Variable.LineDirections[1] = Variable.SwitchLadleLine.getState() ? Calculation.GetLadleLineDirection() : double.NaN;
            Variable.LineDirections[2] = Variable.SwitchTargetLine.getState() ? Calculation.GetTargetLineDirection() : double.NaN;
            Variable.LineDirections[3] = Variable.SwitchVortexLine.getState() ? Calculation.GetVortexLineDirection() : double.NaN;
            Variable.LineDirections[4] = Variable.SwitchVortexCurve.getState() ? Calculation.GetVortexMoveDirection() : double.NaN;

            Variable.DebugWindows[0]?.ShowText(Calculation.CalculatePath());
            Variable.DebugWindows[1]?.ShowText(Calculation.CalculateLadle());
            Variable.DebugWindows[2]?.ShowText(Calculation.CalculateMove());
            Variable.DebugWindows[3]?.ShowText(Calculation.CalculateTarget());
            Variable.DebugWindows[4]?.ShowText(Calculation.CalculatePosition());
            Variable.DebugWindows[5]?.ShowText(Calculation.CalculateDeviation());
            Variable.DebugWindows[6]?.ShowText(Calculation.CalculateVortex());
            Variable.DebugWindows[7]?.ShowText(Calculation.CalculateHealth(___health));

            Variable.DebugWindows[9]?.ShowText(Calculation.CalculateDebug());
            Variable.DebugWindows[10]?.ShowText(Calculation.CalculateIO());
            Variable.DebugWindows[11]?.ShowText(Calculation.CalculateHotkey());

            Rendering.SetTransparency();
            Rendering.SetNodeRenderers();
            Rendering.SetLineRenderers();
            Rendering.SetCurveRenderers();
            Rendering.SetRangeRenderers();
        }
        #endregion

        #region Patch - 信息保存
        /// <summary>
        /// 获取右键点击的目标效果
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CursorManager), "UpdateDebugWindow")]
        public static void UpdateSelectEffect(InteractiveItem ___hoveredInteractiveItem)
        {
            Function.UpdateSelectEffect(___hoveredInteractiveItem);
        }

        /// <summary>
        /// 获取基础渲染器
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SolventDirectionHint), "OnPositionOnMapChanged")]
        public static void GetBaseRenderer(SolventDirectionHint __instance, SpriteRenderer ___spriteRenderer)
        {
            Variable.BaseRenderPosition = __instance.transform.position;
            Variable.BaseLadleRenderer = ___spriteRenderer;
        }

        /// <summary>
        /// 保存窗口位置
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadManager), "SaveProgressToPool")]
        public static void SaveDebugWindowPos()
        {
            Function.SaveDebugWindowPos();
        }
        #endregion

        #region Patch - 窗口辅助
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
            if (!Variable.ActiveDebugWindows.Contains(dbg))
                return false;
            Variable.ActiveDebugWindows.Remove(dbg);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Room), "Awake")]
        public static void InitDebugWindows(Room __instance)
        {
            if (__instance.roomIndex == RoomIndex.Laboratory)
                for (int i = 0; i < Variable.DebugWindows.Length; i++)
                    Function.InitDebugWindow(i, __instance);
        }
        #endregion

        //PotionCraft.Settings.Settings<int>
    }
}
