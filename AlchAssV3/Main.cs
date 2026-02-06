using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem.Cursor;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.Mortar;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using PotionCraft.ObjectBased.UIElements;
using UnityEngine;

namespace AlchAssV3
{
    [BepInPlugin("AlchAssV3", "Alchemist's Assistant V3", "2.6.0")]
    public class Main : BaseUnityPlugin
    {
        #region Unity - 生命周期
        /// <summary>
        /// 游戏初始化时
        /// </summary>
        public void Awake()
        {
            Variable.WindowPositions[0] = Config.Bind("窗口位置", "路径信息", new Vector3(-12.50f, -1.85f, 0f));
            Variable.WindowPositions[1] = Config.Bind("窗口位置", "加水信息", new Vector3(-12.50f, -4.60f, 0f));
            Variable.WindowPositions[2] = Config.Bind("窗口位置", "移动信息", new Vector3(-4.10f, -4.60f, 0f));
            Variable.WindowPositions[3] = Config.Bind("窗口位置", "目标效果", new Vector3(-7.30f, -4.95f, 0f));
            Variable.WindowPositions[4] = Config.Bind("窗口位置", "酿造信息", new Vector3(-1.70f, -4.95f, 0f));
            Variable.WindowPositions[5] = Config.Bind("窗口位置", "效果偏离", new Vector3(-9.95f, -5.30f, 0f));
            Variable.WindowPositions[6] = Config.Bind("窗口位置", "活跃漩涡", new Vector3(7.65f, -4.60f, 0f));
            Variable.WindowPositions[7] = Config.Bind("窗口位置", "目标漩涡", new Vector3(10.05f, -4.60f, 0f));
            Variable.WindowPositions[8] = Config.Bind("窗口位置", "研磨信息", new Vector3(5.20f, -6.00f, 0f));
            Variable.WindowRectConfig = Config.Bind("窗口位置", "控制面板", new Rect(200, 200, 400, 400));

            Variable.ColorLines[0] = Config.Bind("颜色设置", "路径切向线", new Color(0.0f, 0.9f, 0.9f));
            Variable.ColorLines[1] = Config.Bind("颜色设置", "加水方向线", new Color(0.2f, 0.5f, 1.0f));
            Variable.ColorLines[2] = Config.Bind("颜色设置", "效果径向线", new Color(0.0f, 0.8f, 0.4f));
            Variable.ColorLines[3] = Config.Bind("颜色设置", "漩涡径向线", new Color(0.8f, 0.4f, 1.0f));
            Variable.ColorLines[4] = Config.Bind("颜色设置", "漩涡切向线", new Color(0.5f, 0.0f, 0.8f));
            Variable.ColorPaths[0] = Config.Bind("颜色设置", "路径曲线一", new Color(0.1f, 0.3f, 0.6f));
            Variable.ColorPaths[1] = Config.Bind("颜色设置", "路径曲线二", new Color(0.0f, 0.4f, 0.3f));
            Variable.ColorVortex = Config.Bind("颜色设置", "漩涡曲线", new Color(0.7f, 0.0f, 1.0f));
            Variable.ColorRange = Config.Bind("颜色设置", "漩涡和效果范围", new Color(0.6f, 0.3f, 0.7f));
            Variable.ColorClosest = Config.Bind("颜色设置", "最近点", new Color(0.2f, 1.0f, 0.2f));
            Variable.ColorDanger = Config.Bind("颜色设置", "危险点", new Color(1.0f, 0.5f, 0.0f));
            Variable.ColorIntersection = Config.Bind("颜色设置", "交会点", new Color(1.0f, 0.8f, 0.2f));
            Variable.ColorDefeat = Config.Bind("颜色设置", "失败点", new Color(1.0f, 0.1f, 0.1f));
            Variable.ColorCustomNormal = Config.Bind("颜色设置", "自定义方向线静态", new Color(0.3f, 0.3f, 0.3f));
            Variable.ColorCustomHover = Config.Bind("颜色设置", "自定义方向线选中", new Color(1.0f, 1.0f, 0.0f));

            Variable.KeyWindow = Config.Bind("快捷键", "开关控制面板", new KeyboardShortcut(KeyCode.F2));
            Variable.KeyEffect = Config.Bind("快捷键", "选择目标效果", new KeyboardShortcut(KeyCode.LeftControl));
            Variable.KeyVortex = Config.Bind("快捷键", "选择目标漩涡", new KeyboardShortcut(KeyCode.LeftAlt));
            Variable.KeyCustom = Config.Bind("快捷键", "自定义方向线", new KeyboardShortcut(KeyCode.LeftShift));

            Variable.LineWidth = Config.Bind("其他设置", "渲染线宽", 0.075f);
            Variable.NodeSize = Config.Bind("其他设置", "渲染点大小", 0.15f);
            Variable.WindowScale = Config.Bind("其他设置", "信息窗口缩放", 0.8f);

            Variable.WindowRect = Variable.WindowRectConfig.Value;
            Function.LoadFromBins();
            LocalizationManager.OnInitialize.AddListener(Function.FormatLocalization);
            LocalizationManager.OnLocaleChanged.AddListener(Function.SetDebugWindowTitle);
            Rendering.CreateMaterialAndSprites();

            Harmony.CreateAndPatchAll(typeof(Main));
            Logger.LogInfo("Alchemist's Assistant V3 插件已加载");
        }

        /// <summary>
        /// 每帧更新时
        /// </summary>
        public void Update()
        {
            Function.UpdateWindow();
            Function.UpdateSelectVortex();
            Function.UpdateCustomLines();
        }

        /// <summary>
        /// GUI 更新时
        /// </summary>
        public void OnGUI()
        {
            if (Variable.ShowWindow)
            {
                if (Variable.WindowStyle == null)
                    UIWindow.InitStyles();
                Variable.WindowRect = GUILayout.Window(0, Variable.WindowRect, UIWindow.DrawWindow, LocalizationManager.GetText("功能面板"), Variable.WindowStyle);
            }
        }
        #endregion

        #region Patch - 游戏更新
        /// <summary>
        /// 计算研磨信息
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortar), "Update")]
        public static void MortarUpdate(Mortar __instance)
        {
            Variable.DebugWindows[8]?.ShowText(Calculation.CalculateGrind(__instance));
        }

        /// <summary>
        /// 计算其他窗口信息和渲染元素
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndicatorMapItem), "UpdateByCollection")]
        public static void IndicatorUpdate(float ___health)
        {
            Calculation.UpdateOffset();

            Calculation.InitPathCurve();
            Calculation.InitVortexCurve();
            Calculation.InitPoints(___health);

            Calculation.GetPathLineDirection();
            Calculation.GetLadleLineDirection();
            Calculation.GetTargetLineDirection();
            Calculation.GetVortexLineDirection();
            Calculation.GetTangentLineDirection();

            Rendering.SetNodeRenderers();
            Rendering.SetLineRenderers();
            Rendering.SetCurveRenderers();
            Rendering.SetRangeRenderers();
            Rendering.SetCustomLineRenderers();
            Rendering.SetIndicatorRenderers();

            Variable.DebugWindows[0]?.ShowText(Calculation.CalculatePath());
            Variable.DebugWindows[1]?.ShowText(Calculation.CalculateLadle());
            Variable.DebugWindows[2]?.ShowText(Calculation.CalculateMove());
            Variable.DebugWindows[3]?.ShowText(Calculation.CalculateEffect());
            Variable.DebugWindows[4]?.ShowText(Calculation.CalculateBrewing(___health));
            Variable.DebugWindows[5]?.ShowText(Calculation.CalculateDeviation());
            Variable.DebugWindows[6]?.ShowText(Calculation.CalculateVortex());
            Variable.DebugWindows[7]?.ShowText(Calculation.CalculateTargetVortex());
        }

        /// <summary>
        /// 获取右键点击的目标效果
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CursorManager), "UpdateDebugWindow")]
        public static void UpdateSelectEffect(InteractiveItem ___hoveredInteractiveItem)
        {
            if (___hoveredInteractiveItem != null)
                Function.UpdateSelectEffect(___hoveredInteractiveItem);
        }
        #endregion

        #region Patch - 信息获取
        /// <summary>
        /// 生成调试窗口
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Room), "Awake")]
        public static void InitDebugWindows(Room __instance)
        {
            if (__instance.roomIndex == RoomIndex.Laboratory)
                for (int i = 0; i < Variable.DebugWindows.Length; i++)
                    Function.InitDebugWindow(i, __instance);
        }

        /// <summary>
        /// 获取基础加水渲染器
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SolventDirectionHint), "Awake")]
        public static void GetBaseRenderer(SpriteRenderer ___spriteRenderer)
        {
            Variable.BaseLadleRenderer = ___spriteRenderer;
        }
        #endregion

        #region Patch - 窗口辅助
        /// <summary>
        /// 不知道为什么要这么做，但不这样窗口会有问题
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovableUIItem), "FixOutOfBoundsCase")]
        public static bool WindowFixOutOfBoundsCase(MovableUIItem __instance)
        {
            return __instance is not DebugWindow;
        }

        /// <summary>
        /// 同样不知道为什么要这么做，但不这样窗口也会有问题
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Window), "ToForeground")]
        public static bool WindowForeground(Window __instance)
        {
            return __instance is not DebugWindow;
        }
        #endregion
    }
}
